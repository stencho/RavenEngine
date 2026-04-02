

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Engine.Collision;
using Raven.Graphics.Drawing2D;
using Raven.UI;

namespace Raven.Console {
    public class ConsoleWindow : UIWindow {

        public ConsoleWindow(IUIForm parent_form = null) : base(parent_form) {
            subforms.Add(new ConsoleUI(3, 3, client_size.X - 3, client_size.Y - 3, this));
            change_text("console");
            change_name("console");
        }

        public ConsoleWindow(Vector2i position, Vector2i size, IUIForm parent_form = null) : base(position, size, parent_form) {
            subforms.Add(new ConsoleUI(3, 3, client_size.X - 3, client_size.Y - 3, this));
            change_text("console");
            change_name("console");
        }

        public override void update() {
            base.update();

            subforms[0].size = client_size - 3;
        }
    }

    public class ConsoleUI : IUIForm {
        public string name => "text_display";
        public string text => _text;
        string _text = "";//@"According to all known laws of #c:Red# aviation, there is no way that a bee should be #c:128,128,128# able to fly. Its wings are too small to get its fat little body off the ground. The bee, of course, flies anyway. Because bees don’t care what humans think is impossible.” SEQ. 75 - “INTRO TO BARRY” INT. BENSON HOUSE - DAY ANGLE ON: Sneakers on the ground. Camera PANS UP to reveal BARRY BENSON’S BEDROOM ANGLE ON: Barry’s hand flipping through different sweaters in his closet. BARRY Yellow black, yellow black, yellow black, yellow black, yellow black, yellow black...oohh, black and yellow... ANGLE ON: Barry wearing the sweater he picked, looking in the mirror. BARRY (CONT’D) Yeah, let’s shake it up a little. He picks the black and yellow one. He then goes to the sink, takes the top off a CONTAINER OF HONEY, and puts some honey into his hair. He squirts some in his mouth and gargles. Then he takes the lid off the bottle, and rolls some on like deodorant. CUT TO: INT. BENSON HOUSE KITCHEN - CONTINUOUS Barry’s mother, JANET BENSON, yells up at Barry. JANET BENSON Barry, break";
        string _buffer = "";

        public bool has_focus { get; set; } = false;
        public bool visible => _visible;
        bool _visible = true;

        public Vector2i position { get; set; } = Vector2i.Zero;
        public Vector2i absolute_position => parent_form.absolute_position + position;
        public Vector2i size { get; set; } = Vector2i.One * 10;

        public Vector2i top_left => position;
        public Vector2i bottom_right => position + size;
        public Vector2i bottom_left => position + (Vector2i.UnitY * client_size.Y);
        public Vector2i top_right => position + (Vector2i.UnitX * client_size.X);

        public Vector2i client_top_left => top_left;
        public Vector2i client_size => size;
        public Vector2i client_bottom_right => bottom_right;
        public bool use_internal_rendering => false;
        public RenderTarget2D client_area { get; }

        public bool mouse_over => (mouse_interactions.Count > 0);

        public int top_hit_subform { get; set; } = -1;
        public bool top_of_mouse_stack { get; set; } = false;

        public List<string> mouse_interactions => _mouse_interactions;
        List<string> _mouse_interactions = new List<string>();

        public List<IUIForm> subforms { get; set; } = new List<IUIForm>();

        public Dictionary<string, Collision2D.Shape2D> collision => _collision;

        public ui_layer_state layer_state => ui_layer_state.floating;
        public FormAnchor anchor { get; set; }

        Dictionary<string, Collision2D.Shape2D> _collision = new Dictionary<string, Collision2D.Shape2D>();

        public float text_line_gap = 1;
        public float message_gap = 3;

        public Color text_color = Color.White;
        public Color text_shadow_color = Color.Black;

        public bool text_shadow = false;

        public int scroll_bar_width = 15;

        public int text_box_height = 18;
        public int text_box_edge_margin = 4;

        public int bottom_scroll_pos = 25;

        public IUIForm parent_form { get; set; }

        ConsoleInputHandler cih;

        public ConsoleUI(int X, int Y, int width, int height, IUIForm parent_form = null) {
            this.parent_form = parent_form;

            position = new Vector2i(X, Y);
            size = new Vector2i(width, height);

            cih = new ConsoleInputHandler(110, 10, width - 1);

            //Logging.log_with_namespace("test", this.GetType());
            //Logging.log(Logging.log_level.ERROR, "test 2");
            //Logging.log(Logging.log_level.WARNING, "test 3");
            //Logging.log("be #c:128,128,128# jhgjkhfg", log_data.default_format_text_header_source);
            Log.log("I am farting #b_on#very#b_off# hard in this moment #b_on#so#b_off# hard in fact that my cheeks of ass are sort of lioke this: (#c:SaddleBrown#,(#c# ) !!", "FARTING!!", this.GetType().ToString(), Log.log_data.default_format_text_header_source,Log.log_level.CUSTOM, "LightGray", "SaddleBrown", "SaddleBrown", "SaddleBrown");

        }

        public void change_text(string text) {
            _text = text;
        }

        public bool test_mouse() {
            return UIStandard.test_mouse(ref _collision, ref _mouse_interactions);
        }

        public void update() {

            cih.has_focus = parent_form.has_focus;

            cih.update(cih.has_focus, parent_form.client_top_left);

            //_text = Logging.last_n_messages(200);

        }

        public void update_collision() {
            
        }

        public void render_internal() {

        }

        float default_x_position = 4;
        float notch_width = 3;

        public Color default_color = Color.LightGray;

        float scroll_position = 0;

        Vector2 pos = Vector2.Zero;
        public void draw() {
            Color current_color = default_color;

            bool bold = false;

            float msx = Math2D.measure_string_x("profont", "a");
            float msx_single = Math2D.measure_string_x("profont", "a");

            float msy = Math2D.measure_string_y("profont", "a");

            float msy_overall = 0;

            var c = 0;
            var x = 0;
            float last_msy = 0;

            bool skipdraw = false;

            //Draw2D.fill_rect(0, 0, size.X, size.Y, Color.FromNonPremultiplied(25, 25, 25, 255));

            for (int d = 0; d < Log.data.Count; d++) {
                x = Log.data.Count - 1 - c;

                if (x < 0) break;

                skipdraw = false;

                string edited = Log.data[x].print();

                edited = edited.Replace("\n", " \\n ");

                var rm = Regex.Match(edited, UIStandard.color_string_pattern);

                edited = Regex.Replace(edited, UIStandard.color_string_pattern, " $1 ");
                edited = Regex.Replace(edited, UIStandard.color_string_reset_pattern, " $1 ");
                edited = Regex.Replace(edited, UIStandard.color_string_RGB_pattern, " $1 ");

                edited = Regex.Replace(edited, UIStandard.bold_string_disable_pattern, " $1 ");
                edited = Regex.Replace(edited, UIStandard.bold_string_enable_pattern, " $1 ");
                edited = Regex.Replace(edited, UIStandard.bold_string_toggle_pattern, " $1 ");

                var split = edited.Split(' ');


                pos = Vector2.Zero;
                pos.X = default_x_position;

                for (int i = 0; i < split.Length; i++) {
                    skipdraw = false;
                    // TURN ALL THIS INTO A UISTANDARD FUNCTION
                    // THEN MAKE A SIMPLE Vector2i MEASURE_STRING_FANCY(string font, string text, int max_width);
                    // then that can just run all this shit on a string, act like it's inside a text box, and measure the entire list that way
                    // from there it's not too hard to add scrolling, and from there it's also not too hard to add keep-scroll-at-bottom

                    if (Regex.IsMatch(split[i], UIStandard.color_string_pattern)) {
                        var k = split[i].Replace("#", "").Replace("c:", "");
                        if (Draw2D.string_colors.ContainsKey(k)) {
                            current_color = Draw2D.string_colors[k];
                        } else {
                            current_color = Color.White;
                        }

                        pos.X -= msx_single;
                        skipdraw = true;

                    } else if (Regex.IsMatch(split[i], UIStandard.color_string_reset_pattern)) {
                        current_color = default_color;

                        pos.X -= msx_single;
                        skipdraw = true;


                    } else if (Regex.IsMatch(split[i], UIStandard.color_string_RGB_pattern)) {

                        var s = Regex.Split(split[i].Replace("#", "").Replace("c:", ""), "[,/:;-\\|x]");
                        int r, g, b;

                        if (int.TryParse(s[0], out r) && int.TryParse(s[1], out g) && int.TryParse(s[2], out b)) {
                            current_color = Color.FromNonPremultiplied(r, g, b, 255);
                        } else {
                            current_color = default_color;
                        }

                        pos.X -= msx_single;
                        skipdraw = true;

                    } else if (Regex.IsMatch(split[i], UIStandard.bold_string_enable_pattern)) {
                        bold = true;
                        pos.X -= msx_single;
                        skipdraw = true;
                    } else if (Regex.IsMatch(split[i], UIStandard.bold_string_disable_pattern)) {
                        bold = false;
                        pos.X -= msx_single;
                        skipdraw = true;
                    } else if (Regex.IsMatch(split[i], UIStandard.bold_string_toggle_pattern)) {
                        bold = !bold;
                        pos.X -= msx_single;
                        skipdraw = true;
                    }

                    if (skipdraw) continue;

                    msx = Math2D.measure_string_x("profont", split[i]);

                    if (msx + pos.X + scroll_bar_width >= client_size.X || split[i] == "\\n") {
                        pos.X = default_x_position;
                        pos.Y += (msy + text_line_gap);

                        if (split[i] == "\\n")
                            skipdraw = true;
                    }

                    if (pos.Y > msy_overall) break;
                    
                    msx += msx_single;

                    if (skipdraw) continue;

                    if (text_shadow)
                        Draw2D.text("profont", split[i], top_left + Vector2i.One + pos + (Vector2i.UnitY * msy_overall), text_shadow_color);

                    Draw2D.text("profont", split[i], top_left + pos + (Vector2i.UnitY * msy_overall), current_color);

                    if (bold)
                        Draw2D.text("profont", split[i], top_left + pos + (Vector2i.UnitY * msy_overall) + Vector2i.UnitX, current_color);

                    pos.X += msx;
                }

                last_msy = msy_overall;
                msy_overall += pos.Y + msy + message_gap + 2;


                //Draw2D.line((int)pos.X, (int)msy_overall, client_size.X, (int)msy_overall, 1f, Color.Red);
                lock(Log.data)
                    Draw2D.fill_rect(new Vector2i(1, last_msy + 1), new Vector2i(notch_width, msy_overall - last_msy - 1), Log.data[x].side_notch_color);

                c++;
            }


            //Console.WriteLine(edited);

            //scroll bar
            // Draw2D.fill_rect(new Vector2i(client_size.X - scroll_bar_width, 0), new Vector2i(client_size.X, client_size.Y - (text_box_edge_margin) - text_box_height), Color.Red);

            //text box
            Draw2D.image(Resources.GetTexture("gradient_vertical"), new Vector2i(0, client_size.Y - (text_box_height * 1.8f)), new Vector2i(client_size.X, (text_box_height * 2f)), Color.Black);
            //Draw2D.fill_rect(new Vector2i(text_box_edge_margin/2, client_size.Y - text_box_height), new Vector2i(client_size.X - (text_box_edge_margin/2), text_box_height), Color.Red);

            cih.set_width_lock_to_char_width(this.size.X - (text_box_edge_margin * 2) - (cih.Width / 40 / 2));
            cih.set_position(new Vector2i((this.size.X / 2) - (cih.Width / 2) + 1, client_size.Y - text_box_height + 3));
            cih.draw();


            //bottom scroll marker
            //Draw2D.line(0, client_size.Y - bottom_scroll_pos, client_size.X, client_size.Y - bottom_scroll_pos, 1f, (msy_overall > client_size.Y - bottom_scroll_pos) ? Color.HotPink : Color.Purple);
        }

        public void recurse_all_subforms(Action<IUIForm> run_on_all_subforms) {
            
        }

        public string list_subforms() {
            return UIStandard.list_subforms(subforms);
        }

        public int get_form_depth() {
            return 0;
        }
    }

}
