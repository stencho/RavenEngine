using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ProjectRaven.Engine;
using ProjectRaven.Engine.Collision;
using ProjectRaven.Engine.Collision.Shapes2D;
using ProjectRaven.Engine.Controls;
using ProjectRaven.Graphics.Drawing2D;

namespace ProjectRaven.UI {
    public class UIWindow : IUIForm {
        public string name => _name; 
        string _name = "window";
        public void change_name(string name) { _name = name; }

        public string text => _text;
        string _text = "a window";
        public void change_text(string text) { _text = text; }

        public ui_layer_state layer_state => ui_layer_state.floating;

        public Vector2i position { get; set; } = Vector2i.Zero;
        public Vector2i size { get; set; } = Vector2i.One * 250;

        public Vector2i top_left => position;
        public Vector2i bottom_right => position + size;
        public Vector2i top_right => position + (Vector2i.UnitX * size.X);
        public Vector2i bottom_left => position + (Vector2i.UnitY * size.Y);

        public Vector2i client_top_left => position + (Vector2i.UnitY * top_bar_height);
        public Vector2i client_size => size - (Vector2i.UnitY * top_bar_height);
        public Vector2i client_bottom_right => client_top_left + client_size;

        public Vector2i top_bar_size => new Vector2i(client_size.X, top_bar_height);

        public Vector2i min_window_size = new Vector2i(40, 40);
        public Vector2i max_window_size = new Vector2i(600, 600);

        float top_bar_height = 12f;

        public List<IUIForm> subforms { get; set; } = new List<IUIForm>();

        public Dictionary<string, Collision2D.Shape2D> collision => _collision;
        Dictionary<string, Collision2D.Shape2D> _collision = new Dictionary<string, Collision2D.Shape2D>();

        RenderTarget2D client_render_target;
        RenderTarget2D top_bar_render_target;

        public bool mouse_over => (mouse_interactions.Count > 0);

        public bool has_focus { get; set; } = true;

        bool _draw_collision = false;

        public bool visible => _visible;
        bool _visible = true;

        bool _update_render_targets = true;
        bool _draw_render_targets = true;
        bool _render_targets_need_resize = false;

        public int top_hit_subform { get; set; } = -1;
        public bool top_of_mouse_stack { get; set; } = false;

        public List<string> mouse_interactions => _mouse_interactions;
        List<string> _mouse_interactions = new List<string>();

        bool _resize_handle_R_mo = false;
        bool _resize_handle_B_mo = false;
        bool _resize_handle_both_mo => _resize_handle_R_mo && _resize_handle_B_mo;

        bool _resize_handle_R_grabbed = false;
        bool _resize_handle_B_grabbed = false;
        bool _resize_handle_both_grabbed => _resize_handle_R_grabbed && _resize_handle_B_grabbed;

        bool _grabbed_bar = false;
        Vector2 _bar_mouse_offset = Vector2.Zero;

        bool mdown = false;
        bool mdown_p = false;
        Vector2i last_mouse_pos = Vector2i.Zero;

        int resize_handle_thickness = 10;

        public IUIForm parent_form { get; set; }

        bool is_child => parent_form != null;

        public string list_subforms() {
            return UIStandard.list_subforms(subforms);
        }

        public void add_subform(IUIForm subform) {
            subform.parent_form = this;
            
            subforms.Add(subform);
        }

        public UIWindow(IUIForm parent_form = null) {
            parent_form = parent_form;
            setup();
        }

        public UIWindow(Vector2i position, Vector2i size, IUIForm parent_form = null) {
            this.position = position;
            this.size = size;
            parent_form = parent_form;

            setup();
        }

        public void hide() { _visible = false; }
        public void show() { _visible = true; }
        public void toggle_vis() { _visible = !_visible; }
        public void toggle_vis(bool toggle) { _visible = toggle; }

        public void setup() {
            _collision.Add("form", new BoundingBox2D(Vector2i.Zero, size));
            _collision.Add("top_bar", new BoundingBox2D(position, position + (Vector2i.UnitX * size.X) + (Vector2i.UnitY * top_bar_height)));

            _collision.Add("resize_handle_R", new BoundingBox2D(
                position + (size - (Vector2i.UnitX * 6)) - (Vector2i.UnitY * size.Y) + (Vector2i.UnitX * 3),
                bottom_right + (Vector2i.One * 3)));
            _collision.Add("resize_handle_B", new BoundingBox2D(
                position + (size - (Vector2i.UnitY * 6)) - (Vector2i.UnitX * size.X) + (Vector2i.UnitY * 3),
                bottom_right + (Vector2i.One * 3)));

            client_render_target = new RenderTarget2D(State.graphics_device, client_size.X, client_size.Y);
            top_bar_render_target = new RenderTarget2D(State.graphics_device, top_bar_size.X, top_bar_size.Y);
        }

        public bool test_mouse() {
            return UIStandard.test_mouse(ref _collision, ref _mouse_interactions);
        }


        static Collision2D.Shape2D _mouse_coll_obj_child;
        Vector2i parent_pos => parent_form.position;

        public virtual void update() {
            test_mouse();

            if (is_child) {
                ((BoundingBox2D)_collision["form"]).position = (position + parent_form.client_top_left).ToVector2();
                ((BoundingBox2D)_collision["form"]).SetSize(size.ToVector2());

                ((BoundingBox2D)_collision["top_bar"]).position = (position + parent_form.client_top_left).ToVector2();
                ((BoundingBox2D)_collision["top_bar"]).SetSize((Vector2.UnitX * size.X) + (Vector2.UnitY * top_bar_height));

                ((BoundingBox2D)_collision["resize_handle_R"]).set(
                    ((position + parent_form.client_top_left) + (size - (Vector2i.UnitX * resize_handle_thickness)) - (Vector2i.UnitY * size.Y) + (Vector2i.UnitX * (resize_handle_thickness / 2))),
                    bottom_right + parent_form.client_top_left + (Vector2i.One * (resize_handle_thickness / 2)).ToVector2());

                ((BoundingBox2D)_collision["resize_handle_B"]).set(
                    ((position + parent_form.client_top_left) + (size - (Vector2i.UnitY * resize_handle_thickness)) - (Vector2i.UnitX * size.X) + (Vector2i.UnitY * (resize_handle_thickness / 2))),
                    bottom_right + parent_form.client_top_left + (Vector2i.One * (resize_handle_thickness / 2)).ToVector2());


                mdown = State.input_main_thread.is_pressed(Input.MouseButtons.Left) && State.is_active && State.input_main_thread.mouse_in_bounds;

                _mouse_coll_obj_child = new Circle2D(State.input_main_thread.mouse_position_float, 1f);


                _resize_handle_R_mo = Collision2D.GJK2D.test_shapes_simple(_collision["resize_handle_R"], _mouse_coll_obj_child, out _);
                _resize_handle_B_mo = Collision2D.GJK2D.test_shapes_simple(_collision["resize_handle_B"], _mouse_coll_obj_child, out _);

            } else {
                ((BoundingBox2D)_collision["form"]).position = (position).ToVector2();
                ((BoundingBox2D)_collision["form"]).SetSize(size.ToVector2());

                ((BoundingBox2D)_collision["top_bar"]).position = position.ToVector2();
                ((BoundingBox2D)_collision["top_bar"]).SetSize((Vector2.UnitX * size.X) + (Vector2.UnitY * top_bar_height));

                ((BoundingBox2D)_collision["resize_handle_R"]).set(
                    (position + (size - (Vector2i.UnitX * resize_handle_thickness)) - (Vector2i.UnitY * size.Y) + (Vector2i.UnitX * (resize_handle_thickness / 2))),
                    bottom_right + (Vector2i.One * (resize_handle_thickness / 2)).ToVector2());

                ((BoundingBox2D)_collision["resize_handle_B"]).set(
                    (position + (size - (Vector2i.UnitY * resize_handle_thickness)) - (Vector2i.UnitX * size.X) + (Vector2i.UnitY * (resize_handle_thickness / 2))),
                    bottom_right + (Vector2i.One * (resize_handle_thickness / 2)).ToVector2());


                mdown = State.input_main_thread.is_pressed(Input.MouseButtons.Left) && State.is_active && State.input_main_thread.mouse_in_bounds;

                _resize_handle_R_mo = Collision2D.GJK2D.test_shapes_simple(_collision["resize_handle_R"], State.input_main_thread.mouse_collision_object, out _);
                _resize_handle_B_mo = Collision2D.GJK2D.test_shapes_simple(_collision["resize_handle_B"], State.input_main_thread.mouse_collision_object, out _);
            }

            //do resize stuff here
            //mouse just clicked
            if (mdown && !mdown_p && top_of_mouse_stack) {
                //switch from mouseover to grabbed
                if (_resize_handle_R_mo && _resize_handle_B_mo) {
                    _resize_handle_R_grabbed = true;
                    _resize_handle_B_grabbed = true;
                } else if (_resize_handle_R_mo) {
                    _resize_handle_R_grabbed = true;
                } else if (_resize_handle_B_mo) {
                    _resize_handle_B_grabbed = true;
                }

                if (_resize_handle_R_mo || _resize_handle_B_mo) {
                    State.game.IsMouseVisible = false;
                }

            }

            //mouse down, something held
            if (mdown && (_resize_handle_R_grabbed || _resize_handle_B_grabbed || _resize_handle_both_grabbed)) {
                //disable drawing while resizing
                _draw_render_targets = false;

                //size change is basically just mouse delta
                var size_change = State.input_main_thread.mouse_delta;

                var sizefit = size;
                if (size.X > State.resolution.X)
                    sizefit = new Vector2i(State.resolution.X, sizefit.Y);
                if (size.Y > State.resolution.Y)
                    sizefit = new Vector2i(sizefit.X, State.resolution.Y);
                size = sizefit;

                if (_resize_handle_both_grabbed) {
                    size += size_change;
                } else if (_resize_handle_R_grabbed) {
                    size += (Vector2.UnitX * size_change.X);
                } else if (_resize_handle_B_grabbed) {
                    size += (Vector2.UnitY * size_change.Y);
                }

                float tmpX = size.X;
                float tmpY = size.Y;

                if (State.input_main_thread.mouse_position.X > State.resolution.X)
                    tmpX = State.resolution.X - top_left.X;

                if (State.input_main_thread.mouse_position.Y > State.resolution.Y)
                    tmpY = State.resolution.Y - top_left.Y;

                size = new Vector2i(tmpX, tmpY);
            }

            if (!mdown && mdown_p && (_resize_handle_R_grabbed || _resize_handle_B_grabbed)) {
                _render_targets_need_resize = true;
                _draw_render_targets = true;
                _resize_handle_R_grabbed = false;
                _resize_handle_B_grabbed = false;
                State.game.IsMouseVisible = true;
            }


            if (_resize_handle_R_grabbed || _resize_handle_B_grabbed) {
                last_mouse_pos = State.input_main_thread.mouse_position;
                mdown_p = mdown;
                return;
            }




            //below here is window movement
            //mouse just clicked
            if (mdown && !mdown_p && top_of_mouse_stack) {
                //if clicking top bar, grab the top bad
                if (Collision2D.GJK2D.test_shapes_simple(_collision["top_bar"], State.input_main_thread.mouse_collision_object, out _))
                    _grabbed_bar = true;
            }

            //mouse down and bar grabbed, position needs to change according to mouse delta
            if (mdown && _grabbed_bar) {
                this.position += State.input_main_thread.mouse_delta;
            }

            //mouse released, release bar
            if (!mdown && mdown_p) {
                _grabbed_bar = false;

                var tmp = this.position;

                if (this.top_left.X < 0)
                    tmp = new Vector2i(0, tmp.Y);
                if (this.top_left.Y < 0)
                    tmp = new Vector2i(tmp.X, 0);

                if (this.bottom_right.X > State.resolution.X)
                    tmp = new Vector2i(State.resolution.X - size.X, tmp.Y);
                if (this.bottom_right.Y > State.resolution.Y)
                    tmp = new Vector2i(tmp.X, State.resolution.Y - size.Y);

                position = tmp;
            }


            //subform updates
            for (int i = 0; i < subforms.Count; i++) {
                subforms[i].update();
            }

            last_mouse_pos = State.input_main_thread.mouse_position;
            mdown_p = mdown;

            if (_render_targets_need_resize) {
                top_bar_render_target = new RenderTarget2D(State.graphics_device, top_bar_size.X, top_bar_size.Y);
                client_render_target = new RenderTarget2D(State.graphics_device, client_size.X, client_size.Y);
                _render_targets_need_resize = false;
            }

        }



        public Color color_borders = Color.HotPink;
        public Color color_bg = Color.FromNonPremultiplied(25,25,25,255);
        public Color color_header = Color.FromNonPremultiplied(25,25,25,255);
        public Color color_header_selected = Color.HotPink;
        public Color color_header_text = Color.HotPink;
        public Color color_header_text_selected = Color.FromNonPremultiplied(25,25,25,255);
        public Color color_drag_border = Color.DeepPink;

        public Action internal_draw_action;
        public Action draw_action;

        public void render_internal() {
            if (!_update_render_targets || !_visible) return;

            State.graphics_device.SetRenderTarget(top_bar_render_target);

            if (has_focus)
                State.graphics_device.Clear(color_header_selected);
            else
                State.graphics_device.Clear(color_header);

            Draw2D.begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None);

            if (has_focus)
                Draw2D.text("profont", text, (Vector2i.Right * 4f), color_header_text_selected);
            else
                Draw2D.text("profont", text, (Vector2i.Right * 4f), color_header_text);

            //Draw2D.line(size.X - 45, 0, size.X - 45, size.Y, 1f, Color.Black);
            //Draw2D.line(size.X - 30, 0, size.X - 30, size.Y, 1f, Color.Black);
            //Draw2D.line(size.X - 15, 0, size.X - 15, size.Y, 1f, Color.Black);
            
            Draw2D.end();

            foreach (IUIForm subform in subforms) {
                subform.render_internal();
            }

            State.graphics_device.SetRenderTarget(client_render_target);
            State.graphics_device.Clear(color_bg);

            Draw2D.begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None);

            foreach (IUIForm subform in subforms) {
                lock(subform)
                    subform.draw();
            }

            internal_draw_action?.Invoke();

            Draw2D.end();
        }

        public void draw() {
            if (!_visible) return;
            Draw2D.fill_rect(top_left, top_left + top_bar_size, Color.FromNonPremultiplied(color_header.R, color_header.G, color_header.B, 128));
            Draw2D.fill_rect(client_top_left, client_top_left + client_size, Color.FromNonPremultiplied(color_bg.R, color_bg.G, color_bg.B, 128));

            if (_draw_render_targets) {
                Draw2D.image(top_bar_render_target, top_left, top_bar_size, Color.White);
                Draw2D.image(client_render_target, client_top_left, Color.White);
            }

            Draw2D.rect(top_left, bottom_right, color_borders, 1f);
            Draw2D.rect(top_left, top_left + top_bar_size, color_borders, 1f);

            if ((_resize_handle_R_mo || _resize_handle_R_grabbed) && top_of_mouse_stack) { Draw2D.line(top_right, bottom_right, color_drag_border, 3f); }
            if ((_resize_handle_B_mo || _resize_handle_B_grabbed) && top_of_mouse_stack) { Draw2D.line(bottom_left, bottom_right, color_drag_border, 3f); }

            draw_action?.Invoke();

            /*
            if (_draw_collision) {
                foreach (string is2d in collision.Keys) {
                    _collision[is2d].Draw(_mouse_interactions.Contains(is2d) ? Color.MediumVioletRed : Color.MediumPurple);
                }
            }

            foreach (IUIForm subform in subforms) {

                foreach (string is2d in subform.collision.Keys) {
                    subform.collision[is2d].Draw(_mouse_interactions.Contains(is2d) ? Color.MediumVioletRed : Color.MediumPurple);
                }
            }
            */
        }

    }
}
