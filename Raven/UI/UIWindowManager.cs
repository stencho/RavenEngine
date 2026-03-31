

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        on_bottom
    }

    //TODO make this instantiated so that different UIWindowManagers can be themed differently
    public static class UIColors {
        public static Color ForegroundDark { get; set; } = Color.FromNonPremultiplied(242, 124, 248, 255);
        public static Color Foreground { get; set; } = Color.FromNonPremultiplied(255, 204, 250, 255);
        public static Color ForegroundLight {get;set;} = Color.FromNonPremultiplied(230,230,230,255);
        
        public static Color BackgroundDark { get; set; } = Color.Black;
        public static Color Background {get;set;} = Color.FromNonPremultiplied(20,20,20,255);
        public static Color BackgroundLight { get; set; } = Color.FromNonPremultiplied(35,35,35,255);

        public static Color BackgroundForegroundMix { get; set; } = Color.FromNonPremultiplied(80, 27, 75, 255);
        
        public static Color QuarterGrey => Color.FromNonPremultiplied(63, 63, 63, 255);
        public static Color MiddleGrey => Color.FromNonPremultiplied(127, 127, 127, 255);
    }
    
    public class UIWindowManager {
        public List<IUIForm> windows = new List<IUIForm>();
        ConsoleWindow console;

        public bool focus_follows_mouse = true;
        
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

        int top_hit_subform = 0;

        bool mouse_button_just_pressed => (mouse.just_pressed(MouseWatcher.MouseButtons.Left) || mouse.just_pressed(MouseWatcher.MouseButtons.Right));

        public void screenshot_top_window(int index) {

        }

        void defocus_all_windows() {
            for (int o = 0; o < windows.Count; o++) {
                windows[o].has_focus = false;
            }
        }

        public void toggle_window(UIWindow window) {
            window.toggle_visibility();

            if (window.visible) {
                if (MouseWatcher.MouseLocked) return;
                windows.BringToFront(window);
                force_focus(window);
                BindWatcher.global_enable = false;
            } else {
                BindWatcher.global_enable = true;
                defocus_all_windows();
            }
            
        }
        
        public void update() {
            //Clock.frame_probe.set("wm_update");
            mouse.UpdateDeltas();            
            
            highest_hit = -1;
            handled = false;

            top_hit_subform = -1;
            bool hit_any = false;

            //if (Controls.just_pressed(Keys.OemTilde)) {
            if (State.engine_binds.just_pressed("toggle_console")) {
                if (console.visible && !console.has_focus && !focus_follows_mouse) {
                    defocus_all_windows();
                    windows.BringToFront(console);
                    force_focus(console);
                    BindWatcher.global_enable = false;
                    return;
                }

                console.toggle_visibility();

                if (console.visible && !MouseWatcher.MouseLocked) {
                    windows.BringToFront(console);
                    force_focus(console);
                    BindWatcher.global_enable = false;
                } else {
                    BindWatcher.global_enable = true;
                    defocus_all_windows();
                }
            }

            if (MouseWatcher.MouseLocked && !MouseWatcher.MouseLockedPrevious) {
                
            }
            
            if (!State.is_active || MouseWatcher.MouseLocked) return;

            for (int i = windows.Count-1; i >= 0; i--) {
                windows[i].update();

                if (windows[i].visible == false) {
                    windows[i].has_focus = false;
                    continue;
                }

                if (highest_hit == -1 && windows[i].mouse_interactions.Count > 0 && mouse_button_just_pressed) {
                    windows.BringToFront(windows[i]);
                    highest_hit = i;
                }

                for (int o = 0; o < windows[i].subforms.Count; o++) {
                    windows[i].subforms[o].top_of_mouse_stack = false;
                }

                windows[i].top_of_mouse_stack = false;

                if (windows[i].mouse_interactions.Count > 0) {
                    if (!handled) {
                        windows[i].top_of_mouse_stack = true;

                        for (int o = windows[i].subforms.Count - 1; o >= 0; o--) {
                            IUIForm sub = (IUIForm)windows[i].subforms[o];

                            if (sub.mouse_interactions.Count > 0) {
                                if (top_hit_subform == -1) {
                                    windows[i].subforms[o].top_of_mouse_stack = true;

                                    if (mouse_button_just_pressed) {
                                        windows[i].subforms.BringToFront(windows[i].subforms[o]);
                                    }

                                    top_hit_subform = o;
                                } else {
                                    windows[i].subforms[o].top_of_mouse_stack = false;
                                }
                            }
                        }


                    } else {
                        windows[i].top_of_mouse_stack = false;
                    }

                    handled = true;
                }
                windows[i].subforms.SortWindows();
            }

            if (focus_follows_mouse) {
                if (mouse_over_UI()) {
                    BindWatcher.global_enable = false;

                    for (int i = windows.Count - 1; i >= 0; i--) {
                        windows[i].has_focus = windows[i].mouse_over && windows[i].top_of_mouse_stack;
                    }

                    for (int i = windows.Count - 1; i >= 0; i--) {

                        for (int i1 = 0; i1 < windows[i].subforms.Count; i1++) {
                            windows[i].subforms[i1].has_focus = false;
                            windows[i].subforms[i1].top_of_mouse_stack = false;
                        }
                    }
                } else {
                    for (int i = windows.Count - 1; i >= 0; i--) {
                        windows[i].has_focus = false;
                        windows[i].top_of_mouse_stack = false;

                        for (int i1 = 0; i1 < windows[i].subforms.Count; i1++) {
                            windows[i].subforms[i1].has_focus = false;
                            windows[i].subforms[i1].top_of_mouse_stack = false;
                        }
                    }
                    BindWatcher.global_enable = true;
                }
            } else {
                if (mouse_button_just_pressed) {
                    if (!mouse_over_UI()) {
                        for (int i = windows.Count - 1; i >= 0; i--) {
                            windows[i].has_focus = false;
                            windows[i].top_of_mouse_stack = false;

                            for (int i1 = 0; i1 < windows[i].subforms.Count; i1++) {
                                windows[i].subforms[i1].has_focus = false;
                                windows[i].subforms[i1].top_of_mouse_stack = false;
                            }
                        }

                        BindWatcher.global_enable = true;
                    } else {
                        BindWatcher.global_enable = false;
                    }
                }
            }
            
        }
        
        public void render_window_internals() {
            //Clock.frame_probe.set("draw_wm_internals");
            
            foreach (IUIForm window in windows) {
                //lock (window)
                    if (window.visible)
                        window.render_internal();
            }
        }

        public void draw() {
            //Clock.frame_probe.set("draw_window_manager");
            Draw2D.begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default);
            
            foreach (IUIForm window in windows) {
                if (window.visible)
                    window.draw();
            }
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
            string s = "";

            foreach (IUIForm subform in subforms) {
                s += (subform.has_focus ? "[f]" : "   ") + " [subform] " + subform.name + " \"" + subform.text + "\"" + (subform.top_of_mouse_stack ? " <-" : "") + "\n";

                if (subform.subforms.Count > 0) {
                    string a = subform.list_subforms();

                    using (StringReader sr = new StringReader(a)) {
                        while (sr.Peek() > -1) {
                            s += "        " + sr.ReadLine() + "\n";
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
                    t = true;

                    mouse_interactions.Add(is2d);
                }
            }
            return t;
        }
    }

}
