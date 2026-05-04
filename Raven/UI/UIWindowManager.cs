

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Console;
using Raven.Engine;
using Raven.Engine.Collision;
using Raven.Engine.Controls;
using Raven.Graphics.Drawing2D;

namespace Raven.UI  {
    public enum ui_layer_state {
        floating,
        on_top,
        normal
    }

    //TODO make this instantiated so that different UIWindowManagers can be themed differently
    public static class UIColors {
        public static Color Foreground { get; set; } = Color.FromNonPremultiplied(242, 124, 248, 255);
        public static Color Background { get; set; } = Color.FromNonPremultiplied(45,30,45,255);
        
        public static Color Foreground75Percent => Foreground.multiply_color(.75f);
        public static Color Foreground50Percent => Foreground.multiply_color(.5f);
        public static Color Foreground25Percent => Foreground.multiply_color(.25f);
        
        public static Color Background75Percent => Background.multiply_color(.75f);
        public static Color Background50Percent => Background.multiply_color(.5f);
        public static Color Background25Percent => Background.multiply_color(.25f);
        
        public static Color QuarterGrey => Color.FromNonPremultiplied(63, 63, 63, 255);
        public static Color MiddleGrey => Color.FromNonPremultiplied(127, 127, 127, 255);

        public static Color MultAlpha(Color color, float multi) {
            return Color.FromNonPremultiplied(color.R, color.G, color.B, (int)(color.A * multi));
        }

        public static float focus_fade => 0.7f;
    }
    
    public class UIWindowManager {
        public List<IUIForm> windows = new List<IUIForm>();
        ConsoleWindow console;

        public bool focus_follows_mouse = gvars.get_bool("ui_focus_follows_mouse");
        public bool window_shadows => gvars.get_bool("ui_window_shadows");
        public Vector2i shadow_offset = (Vector2i.One * 3) + Vector2i.Down;
        
        internal MouseWatcher mouse = new MouseWatcher();
        
        public UIWindowManager() {
            console = new ConsoleWindow(new Vector2i(200, 200), new Vector2i(400, 230));
            console.hide();
            add_window(console);
        }

        public string list_windows() {
            var s = "";

            foreach (IUIForm window in windows) {
                s += "\n  [" + window.name + "]\n   [focused] " + window.has_focus + "\n   [visible] " + window.visible + "\n   [mouseover] " + window.top_of_mouse_stack + "\n   [pos] " + Vector2i.simple_string(window.position) + "\n   [size] " + Vector2i.simple_string(window.size) + "\n";
                if (window.subforms.Count > 0)
                    s += window.list_subforms();
                // s += "[sub] " + subform.name + " :: top of mouse stack: " + subform.top_of_mouse_stack + "\n";

            }

            return s;
        }

        bool exists = false;
        public void add_window(IUIForm window) {
            exists = false;
            
            foreach (IUIForm w in windows) {
                if (w == window) {
                    exists = true;
                    break;
                }
            }

            if (!exists) {
                if (window is UIWindow) (window as UIWindow).window_manager = this;
                
                windows.Add(window);
                windows.BringToFront(window);
                focus_last_added();
            }
        }

        void focus_last_added() {
            for (int o = 0; o < windows.Count; o++) {
                windows[o].has_focus = false;

                for (int s = 0; s < windows[o].subforms.Count; s++) 
                    windows[o].subforms[s].has_focus = false;
                
            }

            windows[windows.Count - 1].has_focus = true;
        }

        public bool handled = false;
        public int highest_hit = -1;

        public bool mouse_over_UI() {
            foreach (IUIForm win in windows)
                if (win.visible)
                    if (win.mouse_interactions.Count > 0) return true;
            return false;
        }

        void force_focus(IUIForm form) {
            int windex = -1;
            int sindex = -1;
            
            for (int o = 0; o < windows.Count; o++) {
                if (windows[o] == form)
                    windex = o;

                windows[o].has_focus = false;
                windows[o].top_of_mouse_stack = false;

                for (int s = 0; s < windows[o].subforms.Count; s++) {
                    sindex = s;
                    windows[o].subforms[s].has_focus = false;
                    windows[o].subforms[s].top_of_mouse_stack = false;
                }
            }

            if (windex >= 0) {
                windows[windex].has_focus = true;
    
                if (sindex >= 0 && windows[windex].subforms.Count > 0)
                    windows[windex].subforms[sindex].has_focus = true;
            }
             
        }


        bool mouse_button_just_pressed => (mouse.just_pressed(MouseWatcher.MouseButtons.Left) || mouse.just_pressed(MouseWatcher.MouseButtons.Right));

        public void screenshot_top_window(int index) {

        }

        void defocus_all_windows() {
            for (int o = 0; o < windows.Count; o++) {
                windows[o].has_focus = false;
            }
        }

        bool is_top_visible_window(IUIForm window) {
            for (int i = windows.Count - 1; i >= 0; i--) {
                if (windows[i] != window && windows[i].visible) {
                    return false;
                } else if (!windows[i].visible) {
                    continue;
                } else if (windows[i] == window) {
                    return true;
                }
            }

            return false;
        }
        
        public void toggle_window(UIWindow window) {
            if (!focus_follows_mouse) {
                if (window.visible && !window.has_focus) {
                    if (!MouseWatcher.MouseLocked) {
                        defocus_all_windows();
                        windows.BringToFront(window);
                        force_focus(window);
                        BindWatcher.global_enable = false;
                        return;
                    }
                }
            } else {
                if (window.visible && !is_top_visible_window(window)) {
                    if (!mouse_holding_window) {
                        defocus_all_windows();
                        windows.BringToFront(window);
                        force_focus(window);
                        return;
                    }
                }                
            }

            window.toggle_visibility();

            if (window.visible && !MouseWatcher.MouseLocked) {
                if (MouseWatcher.MouseLocked) return;
                
                if (!mouse_holding_window) {
                    windows.BringToFront(window);
                    force_focus(window);
                }

                BindWatcher.global_enable = false;
            } else {
                if (window.visible) {
                    windows.BringToFront(window);
                }
                BindWatcher.global_enable = true;
                defocus_all_windows();
            }
        }

        IUIForm find_top_subform(List<IUIForm> subforms) {
            for (int i = subforms.Count - 1; i >= 0; i--) {
                subforms[i].test_mouse();
                if (subforms[i].mouse_interactions.Count > 0) {
                    return subforms[i];
                }
            }

            return null;
        }
        
        IUIForm subform_find_highest(IUIForm form) {
            if (form.subforms.Count == 0) return form;
            IUIForm current_leaf = form;
            
            while (current_leaf.subforms.Count > 0) {
                var sf = find_top_subform(current_leaf.subforms);
                if (sf == null) return current_leaf;
                current_leaf = sf;
            }

            return current_leaf;
        }
        
        IUIForm top_subform_under_mouse = null;
        IUIForm window_on_mouse = null;
        bool mouse_holding_window => window_on_mouse != null;
        
        public void update() {
            //Clock.frame_probe.set("wm_update");
            mouse.UpdateDeltas();

            highest_hit = -1;
            handled = false;

            bool hit_any = false;

            //just locked mouse
            if (MouseWatcher.MouseLocked && !MouseWatcher.MouseLockedPrevious) {
                defocus_all_windows();
            }

            if (State.engine_binds.just_pressed("toggle_console")) {
                toggle_window(console);
            }


            if (!State.is_active || MouseWatcher.MouseLocked) return;
            
            window_on_mouse = null;
            
            for (int i = windows.Count - 1; i >= 0; i--) {
                if (windows[i] is UIWindow) {
                    if ((windows[i] as UIWindow).BeingMoved || (windows[i] as UIWindow).BeingResized) {
                        window_on_mouse = windows[i];
                        break;
                    }
                }
            }
            
            
            
            //initial mouse stack and bring-to-front-on-click handling and such
            for (int i = windows.Count - 1; i >= 0; i--) {

                if (windows[i].visible == false) {
                    windows[i].has_focus = false;
                    continue;
                }

                if (highest_hit == -1 && windows[i].mouse_interactions.Count > 0 && mouse_button_just_pressed) {
                    windows.BringToFront(windows[i]);
                    highest_hit = i;
                }

                windows[i].top_of_mouse_stack = false;

                if (windows[i].mouse_interactions.Count > 0) {
                    if (!handled) {

                        windows[i].recurse_all_subforms(f => {
                            f.has_focus = false;
                            f.top_of_mouse_stack = false;
                        });

                        windows[i].top_of_mouse_stack = true;

                        if (!mouse_holding_window) {
                            top_subform_under_mouse = subform_find_highest(windows[i]);

                            if (top_subform_under_mouse != null && top_subform_under_mouse.parent_form != null) {
                                top_subform_under_mouse.top_of_mouse_stack = true;
                            }
                        }
                        
                    } else {
                        windows[i].recurse_all_subforms(f => {
                            f.has_focus = false;
                            f.top_of_mouse_stack = false;
                        });
                        windows[i].top_of_mouse_stack = false;
                    }

                    handled = true;
                }

                windows[i].subforms.SortWindows();
                windows[i].recurse_all_subforms(f => { f.subforms.SortWindows(); });
            }
            
            for (int i = windows.Count - 1; i >= 0; i--) {
                windows[i].update();
            }
            
            // handle window dragging
            // a window is being held by the mouse, so regardless of focus mode, we should force
            // it to be focused, and defocus all subforms to prevent highlighting them while
            // dragging a window with the mouse close to its edge
            if (mouse_holding_window) {
                for (int i = windows.Count - 1; i >= 0; i--) {
                    if (windows[i] != window_on_mouse) {
                        windows[i].has_focus = false;
                        windows[i].top_of_mouse_stack = false;
                        windows[i].mouse_interactions.Clear();
                    } else {
                        windows[i].has_focus = true;
                        windows[i].top_of_mouse_stack = true;
                    }
                    
                    windows[i].defocus_all_subforms();
                    break;
                }
                
                    
            } else {
                // handle focus modes
                //focus follows mouse
                if (focus_follows_mouse) {
                    // mouse is over the UI so all we really need to do is focus the window and subform under it
                    if (mouse_over_UI()) {
                        BindWatcher.global_enable = false;
                        
                        for (int i = windows.Count - 1; i >= 0; i--) {
                            windows[i].has_focus = (windows[i].mouse_over && windows[i].top_of_mouse_stack);
                        }

                        if (top_subform_under_mouse != null) {
                            if (mouse_button_just_pressed)
                                top_subform_under_mouse.has_focus = true;
                            top_subform_under_mouse.top_of_mouse_stack = true;
                        }
                        
                    } else {
                        //mouse not over UI so just disable it all
                        for (int i = windows.Count - 1; i >= 0; i--) {
                            windows[i].has_focus = false;
                            windows[i].top_of_mouse_stack = false;
                            windows[i].defocus_all_subforms();
                        }
                        
                        BindWatcher.global_enable = true;
                    }
                    
                } else {
                    //click to focus, and we just clicked
                    if (mouse_button_just_pressed) {
                        if (mouse_over_UI()) {
                            //more or less all of this is handled above
                            BindWatcher.global_enable = false;
                        } else {
                            
                            //mouse not over UI, and we clicked, so defocus all
                            BindWatcher.global_enable = true;
                            for (int i = windows.Count - 1; i >= 0; i--) {
                                windows[i].has_focus = false;
                                windows[i].top_of_mouse_stack = false;
                                windows[i].defocus_all_subforms();
                            }
                        }
                    }
                }
            }
        }

    
        
        public void render_window_internals() {
            //Clock.frame_probe.set("draw_wm_internals");
            foreach (IUIForm window in windows) {
                if (window.visible && window.use_internal_rendering) {
                    State.graphics_device.SetRenderTarget(window.client_area);
                    window.render_internal();
                }
            }
        }

        public void draw() {
            //Clock.frame_probe.set("draw_window_manager");
            Draw2D.begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default);
            
            foreach (IUIForm window in windows) {
                if (window is UIWindow && window.visible && !((window as UIWindow).RenderTargetsHidden)) {
                    Draw2D.fill_rect(window.position + shadow_offset, window.position + window.size + shadow_offset, Color.Black.multiply_alpha(.4f));//, Color.Transparent, 1);
                }
                if (window.visible)
                    window.draw();
            }
            Draw2D.end();
        }
        
    }

    public static class UIStandard {
        
        public const string color_string_pattern = "(#c:[a-zA-Z].*?#)";
        public const string color_string_reset_pattern = "(#c#)";
        public const string color_string_RGB_pattern = "(#c:[0-9]{1,3}[,/:;-\\|x][0-9]{1,3}[,/:;-\\|x][0-9]{1,3}#)";

        public const string bold_string_enable_pattern = "(#b_on#)";
        public const string bold_string_disable_pattern = "(#b_off#)";
        public const string bold_string_toggle_pattern = "(#b_toggle#)";

        public static string list_subforms(List<IUIForm> subforms) {
            string s = $"";

            if (subforms.Count > 0) {
                s += "    [Depth " + subforms[0].get_form_depth().ToString() + "]\n";
            }
            
            foreach (IUIForm subform in subforms) {
                s += (subform.has_focus ? "[f]" : "   ") + $" [subform] {subform.name} \"{subform.text}\" [layer state] {subform.layer_state} [mouse over] {subform.mouse_over} {(subform.top_of_mouse_stack ? " <-" : "")}\n";
                foreach (var collision in subform.collision) {
                    s += $"      [coll] {collision.Key} {collision.Value.origin}\n";
                }

                if (subform.subforms.Count > 0) {
                    string a = subform.list_subforms();

                    using (StringReader sr = new StringReader(a)) {
                        while (sr.Peek() > -1) {
                            s += "    " + sr.ReadLine() + "\n";
                        }
                    }
                }
            }
            //s += "\n";
            return s;
        }

        public static bool test_mouse(ref Dictionary<string, Collision2D.Shape2D> collision, ref List<string> mouse_interactions) {
            mouse_interactions.Clear();
            bool t = false;
            foreach (string is2d in collision.Keys) {
                if (Collision2D.GJK2D.test_shapes_simple(collision[is2d], MouseWatcher.MouseCollisionObject, out _)) {
                    if (t == false) t = true;
                    mouse_interactions.Add(is2d);
                }
            }
            return t;
        }
    }

}
