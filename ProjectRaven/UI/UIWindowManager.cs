

using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using ProjectRaven.Console;
using ProjectRaven.Engine;
using ProjectRaven.Engine.Collision;
using ProjectRaven.Engine.Controls;
using ProjectRaven.Graphics.Drawing2D;
using RavenRP;

namespace ProjectRaven.UI  {
    public enum ui_layer_state {
        floating,
        on_top,
        on_bottom
    }

    public class UIWindowManager {
        public List<IUIForm> windows = new List<IUIForm>();
        ConsoleWindow console;
        public UIWindowManager() {
            console = new ConsoleWindow(new Vector2i(200, 200), new Vector2i(400, 230));
            console.hide();
            add_window((IUIForm)console);
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
    
                if (sindex >= 0)
                    windows[windex].subforms[sindex].has_focus = true;
            }
             
        }

        int top_hit_subform = 0;

        bool mouse_button_just_pressed => (State.input_main_thread.just_pressed(Input.MouseButtons.Left) || State.input_main_thread.just_pressed(Input.MouseButtons.Right));

        public void screenshot_top_window(int index) {

        }

        public void update() {
            //Clock.frame_probe.set("wm_update");
                        
            highest_hit = -1;
            handled = false;

            top_hit_subform = -1;
            bool hit_any = false;

            //if (Controls.just_pressed(Keys.OemTilde)) {
            if (State.binds.just_pressed("toggle_console")) {
                if (console.visible && !console.has_focus) {
                    for (int o = 0; o < windows.Count; o++) {
                        windows[o].has_focus = false;
                    }

                    windows.BringToFront(console);
                    force_focus(console);

                    State.binds.global_enable = false;
                    return;
                }

                console.toggle_vis();

                if (console.visible) {
                    windows.BringToFront(console);
                    force_focus(console);

                    State.binds.global_enable = false;
                } else {
                    State.binds.global_enable = true;

                    for (int o = 0; o < windows.Count; o++) {
                        windows[o].has_focus = false;
                    }
                }
            }

            if (!State.is_active || Input.mouse_lock) return;

            for (int i = windows.Count-1; i >= 0; --i) {
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
                    State.binds.global_enable = true;
                } else {
                    State.binds.global_enable = false;
                }
            }

        }
        
        public void render_window_internals() {
            //Clock.frame_probe.set("draw_wm_internals");
            
            foreach (IUIForm window in windows) {
                lock (window)
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
            s += "\n";
            return s;
        }

        public static bool test_mouse(ref Dictionary<string, Collision2D.Shape2D> collision, ref List<string> mouse_interactions) {
            mouse_interactions.Clear();
            bool t = false;

            foreach (string is2d in collision.Keys) {
                if (Collision2D.GJK2D.test_shapes_simple(collision[is2d], State.input_main_thread.mouse_collision_object, out _)) {
                    t = true;

                    mouse_interactions.Add(is2d);
                }
            }
            return t;
        }
    }

}
