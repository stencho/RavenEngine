using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Raven.Engine;
using Raven.Engine.Collision;
using Raven.Engine.Collision.Shapes2D;
using Raven.Engine.Controls;
using Raven.Graphics.Drawing2D;

namespace Raven.UI {
    public interface IUIForm {
        string name { get; }
        string text { get; }

        ui_layer_state layer_state { get; }

        Vector2i position { get; set; }
        Vector2i size { get; set; }

        Vector2i client_top_left { get; }
        Vector2i client_size { get; }
        Vector2i client_bottom_right { get; }

        bool mouse_over { get; } 
        bool has_focus { get; set; }
        bool top_of_mouse_stack { get; set; }
        bool visible { get; }

        List<IUIForm> subforms { get; set; }
        Dictionary<string, Collision2D.Shape2D> collision { get; }

        IUIForm parent_form { get; set; }

        List<string> mouse_interactions { get; }

        bool test_mouse();

        void update();
        void render_internal();
        void draw();

        string list_subforms();

    }

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

        Color _text_color                   = Color.White;
        Color _border_color                 = Color.White;
        Color _background_color             = Color.Black;

        Color _hover_text_color             = Color.HotPink;
        Color _hover_border_color           = Color.HotPink;
        Color _hover_background_color       = Color.Black;

        Color _clicked_text_color           = Color.White;
        Color _clicked_border_color         = Color.White;
        Color _clicked_background_color     = Color.Black;

        float molerp = 0;
        
        public IUIForm parent_form { get; set; }

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
            if (mo && (top_of_mouse_stack && is_child)) {
                current_flags = button_mouse_status.mouse_over;

                if (State.engine_binds.Mouse.is_pressed(MouseWatcher.MouseButtons.Left)) {
                    current_flags = button_mouse_status.mouse_over | button_mouse_status.mouse_down;
                }

                
                if (molerp < 1) molerp += 0.05f;
                else if (molerp > 1) molerp = 1;
                

                if (current_flags.HasFlag(button_mouse_status.mouse_down) && !previous_flags.HasFlag(button_mouse_status.mouse_down)) {
                    //mouse just clicked
                }

            } else if (!mo || !(top_of_mouse_stack && is_child)) {
                if (molerp > 0) molerp -= 0.05f;
                else if (molerp < 0) molerp = 0;

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
            Draw2D.fill_rect(top_left, bottom_right, Draw2D.ColorInterpolate(_background_color, _hover_background_color, molerp));
            Draw2D.text("profont", text, position + (size / 2) - (ms / 2), Draw2D.ColorInterpolate(_text_color, _hover_text_color, molerp));
            Draw2D.rect(top_left, bottom_right - Vector2i.One, Draw2D.ColorInterpolate(_border_color, _hover_border_color, molerp), 2);
        }

    }
}
