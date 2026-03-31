using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Engine.Collision;
using Raven.Engine.Collision.Shapes2D;
using Raven.Engine.Controls;
using Raven.Graphics.Drawing2D;
using Raven.Graphics.InterpolatedTypes;

namespace Raven.UI.Forms;

    public class UIButton : IUIForm {
        public string name => "button";
        public string text => _text;

        public ui_layer_state layer_state => ui_layer_state.on_top;

        public void change_text(string text) {
            this._text = text;
        }

        public bool has_focus { get; set; } = false;
        public bool visible => _visible;
        bool _visible = true;

        public Vector2i position { get; set; } = Vector2i.Zero;
        public Vector2i size { get; set; } = Vector2i.One * 10;

        public Vector2i top_left => position;
        public Vector2i bottom_right => position + size;

        public Vector2i client_top_left => top_left;
        public Vector2i client_size => size;
        public Vector2i client_bottom_right => bottom_right;

        public bool mouse_over => (mouse_interactions.Count > 0);

        public int top_hit_subform { get; set; } = -1;
        public bool top_of_mouse_stack { get; set; } = false;

        public List<string> mouse_interactions => _mouse_interactions;
        List<string> _mouse_interactions = new List<string>();

        public List<IUIForm> subforms { get; set; } = new List<IUIForm>();

        public Dictionary<string, Collision2D.Shape2D> collision => _collision;
        Dictionary<string, Collision2D.Shape2D> _collision = new Dictionary<string, Collision2D.Shape2D>();
        
        string _text = "";

        public string list_subforms() {
            return UIStandard.list_subforms(subforms);
        }

        FloatLerperManual molerp = new FloatLerperManual(0, 1, 200);
        FloatLerperManual mdlerp = new FloatLerperManual(0, 1, 50);
        
        public IUIForm parent_form { get; set; }

        private bool mouse_down_and_over = false;
        
        public UIButton(int X, int Y, int width, int height, string text) {
            position = new Vector2i(X, Y);
            size = new Vector2i(width, height);
            _text = text;
            
            _collision.Add("button", new BoundingBox2D(top_left, bottom_right));
        }

        public UIButton(int X, int Y, int width, int height, string text, IUIForm parent_form) {
            position = new Vector2i(X, Y);
            size = new Vector2i(width, height);
            _text = text;

            this.parent_form = parent_form;

            _collision.Add("button", new BoundingBox2D(top_left,bottom_right));            
        }
        
        public UIButton(int X, int Y, string text, IUIForm parent_form = null) {
            position = new Vector2i(X, Y);
            _text = text;

            var ms = Draw2D.measure_string_profont(text);
            var ms_ol = Draw2D.measure_string_profont("one line");
            
            size = ms.ToVector2i() + (ms_ol.Y * Vector2i.UnitX) + (ms_ol.Y * Vector2i.UnitY);

            this.parent_form = parent_form;

            _collision.Add("button", new BoundingBox2D(top_left,bottom_right));            
        }

        public bool test_mouse() {
            return UIStandard.test_mouse(ref _collision, ref _mouse_interactions);
        }


        [Flags]
        public enum button_mouse_status {
            none = 0,
            mouse_over = 1 << 0,
            mouse_down = 1 << 1,
            mouse_up = 1 << 2
        }

        button_mouse_status current_flags = button_mouse_status.none;
        button_mouse_status previous_flags = button_mouse_status.none;

        Action button_click;

        public void set_action(Action action) {
            button_click = action;
        }

        bool _clicked = false;
        public bool clicked => _clicked;
        bool mdown = false;
        bool is_child => parent_form != null;
        
        public void update() {
            if (is_child) {
                ((BoundingBox2D)_collision["button"]).set(top_left + parent_form.client_top_left, parent_form.client_top_left + bottom_right);
            } else {
                ((BoundingBox2D)_collision["button"]).set(top_left, bottom_right);
            }

            var mo = test_mouse();

            mdown = State.engine_binds.Mouse.is_pressed(MouseWatcher.MouseButtons.Left);
           // mo = Math2D.AABB_test(mouse_position.X, mouse_position.Y, parent_top_left.X + X, parent_top_left.Y + Y, width, height);
            if (mo && (top_of_mouse_stack)) {
                mouse_down_and_over = mdown;
                current_flags = button_mouse_status.mouse_over;

                if (mdown) {
                    current_flags = button_mouse_status.mouse_over | button_mouse_status.mouse_down;
                }
                
                if (current_flags.HasFlag(button_mouse_status.mouse_down) && !previous_flags.HasFlag(button_mouse_status.mouse_down)) {
                    //mouse just clicked
                }

            } else if (!mo || !(top_of_mouse_stack && is_child)) {
                if (State.engine_binds.Mouse.is_pressed(MouseWatcher.MouseButtons.Left)) {
                    current_flags = button_mouse_status.mouse_down;
                } else {
                    current_flags = button_mouse_status.none;
                }
            }
             
            if (!current_flags.HasFlag(button_mouse_status.mouse_down) && previous_flags.HasFlag(button_mouse_status.mouse_down) && !_clicked ) {
                //mouse just released
                if (mo && !mdown && previous_flags.HasFlag(button_mouse_status.mouse_down) && (top_of_mouse_stack)) {
                    //do stuff
                    if (button_click != null )
                        button_click.Invoke();

                    _clicked = true;
                }
            }

            if (_clicked) {
                _clicked = false;
            }

            
            previous_flags = current_flags;
        }
        
        public void render_internal() {}

        public void draw() {
            var ms = Math2D.measure_string("profont", text);

            if (mouse_over) {
                molerp.Lerp();
            } else {
                molerp.LerpReverse();
            }
            
            if (mdown && mouse_over) {
                mdlerp.Lerp();
            } else {
                mdlerp.LerpReverse();
            }

            var mo_offset = mouse_over ? Vector2i.One : Vector2i.Zero;
            
            Draw2D.fill_rect(top_left - mo_offset, bottom_right - mo_offset, Draw2D.ColorInterpolate(UIColors.Background, UIColors.ForegroundDark, mdlerp.Value));
            
            Draw2D.fill_rect_dither(top_left - mo_offset, bottom_right - mo_offset, 
                Draw2D.ColorInterpolate(Draw2D.ColorInterpolate(UIColors.Background, UIColors.QuarterGrey, molerp.Value), UIColors.ForegroundDark, mdlerp.Value), 
                Draw2D.ColorInterpolate(Draw2D.ColorInterpolate(UIColors.Background, UIColors.BackgroundForegroundMix, molerp.Value), UIColors.ForegroundDark, mdlerp.Value)
            );
            
            Draw2D.text("profont", text, position - mo_offset + (size / 2) - (ms / 2), Draw2D.ColorInterpolate(Draw2D.ColorInterpolate(UIColors.ForegroundDark, UIColors.Foreground, molerp.Value), UIColors.Background, mdlerp.Value));
            Draw2D.rect(top_left - mo_offset, bottom_right - mo_offset, 
                Draw2D.ColorInterpolate(Draw2D.ColorInterpolate(UIColors.ForegroundDark, UIColors.Foreground, molerp.Value), UIColors.ForegroundDark, mdlerp.Value), 
                1);
        }

    }