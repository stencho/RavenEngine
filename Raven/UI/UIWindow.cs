using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Raven.Console;
using Raven.Engine;
using Raven.Engine.Collision;
using Raven.Engine.Collision.Shapes2D;
using Raven.Engine.Controls;
using Raven.Graphics.Drawing2D;
using Raven.Graphics.InterpolatedTypes;

namespace Raven.UI {
    public partial class UIWindow : IUIForm {
        public UIWindowManager window_manager;

        private MouseWatcher mouse => window_manager.mouse;
        
        public Vector2i client_top_left => (Vector2i.UnitY * top_bar_height);
        public Vector2i client_size => size - (Vector2i.UnitY * top_bar_height);
        public Vector2i client_bottom_right => client_top_left + client_size;

        public Vector2i top_bar_size => new Vector2i(client_size.X, top_bar_height);

        public Vector2i min_window_size = new Vector2i(40, 40);
        public Vector2i max_window_size = new Vector2i(600, 600);

        int top_bar_height = 13;

        RenderTarget2D top_bar_render_target;


        bool _draw_collision = false;

        bool _update_render_targets = true;
        bool _draw_render_targets = true;
        public bool RenderTargetsHidden => !_draw_render_targets;
        bool _render_targets_need_resize = false;

        bool _resize_handle_R_mo = false;
        bool _resize_handle_B_mo = false;
        bool _resize_handle_both_mo => _resize_handle_R_mo && _resize_handle_B_mo;

        bool _resize_handle_R_grabbed = false;
        bool _resize_handle_B_grabbed = false;
        bool _resize_handle_both_grabbed => _resize_handle_R_grabbed && _resize_handle_B_grabbed;

        bool _grabbed_bar = false;
        Vector2 _bar_mouse_offset = Vector2.Zero;

        public bool BeingMoved => _grabbed_bar;
        public bool BeingResized => _resize_handle_B_grabbed || _resize_handle_R_grabbed;

        bool mdown = false;
        bool mdown_p = false;
        Vector2i last_mouse_pos = Vector2i.Zero;

        int resize_handle_thickness = 10;


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

        public void setup() {
            _collision.Add("form", new BoundingBox2D(Vector2i.Zero, size));
            _collision.Add("top_bar", new BoundingBox2D(position, position + (Vector2i.UnitX * size.X) + (Vector2i.UnitY * (top_bar_height+1))));

            _collision.Add("resize_handle_R", new BoundingBox2D(
                position + (size - (Vector2i.UnitX * 6)) - (Vector2i.UnitY * size.Y) + (Vector2i.UnitX * 3),
                bottom_right + (Vector2i.One * 3)));
            _collision.Add("resize_handle_B", new BoundingBox2D(
                position + (size - (Vector2i.UnitY * 6)) - (Vector2i.UnitX * size.X) + (Vector2i.UnitY * 3),
                bottom_right + (Vector2i.One * 3)));

            _client_area = new RenderTarget2D(State.graphics_device, client_size.X, client_size.Y);
            top_bar_render_target = new RenderTarget2D(State.graphics_device, top_bar_size.X, top_bar_size.Y);
            
            change_text(text);
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


                mdown = mouse.is_pressed(MouseWatcher.MouseButtons.Left) ;
                
                _mouse_coll_obj_child = new Point2D(MouseWatcher.Position.ToVector2());


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


                mdown = mouse.is_pressed(MouseWatcher.MouseButtons.Left) && State.is_active && MouseWatcher.mouse_in_bounds;

                _resize_handle_R_mo = Collision2D.GJK2D.test_shapes_simple(_collision["resize_handle_R"], MouseWatcher.MouseCollisionObject, out _);
                _resize_handle_B_mo = Collision2D.GJK2D.test_shapes_simple(_collision["resize_handle_B"], MouseWatcher.MouseCollisionObject, out _);
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
                var size_change = mouse.MouseDelta;

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

                if (MouseWatcher.Position.X > State.resolution.X)
                    tmpX = State.resolution.X - top_left.X;

                if (MouseWatcher.Position.Y > State.resolution.Y)
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
                last_mouse_pos = MouseWatcher.Position;
                mdown_p = mdown;
                
                return;
            }


            //below here is window movement
            //mouse just clicked
            if (mdown && !mdown_p && top_of_mouse_stack) {
                //if clicking top bar, grab the top bad
                if (Collision2D.GJK2D.test_shapes_simple(_collision["top_bar"], MouseWatcher.MouseCollisionObject, out _))
                    _grabbed_bar = true;
            }

            //mouse down and bar grabbed, position needs to change according to mouse delta
            if (mdown && _grabbed_bar) {
                this.position += mouse.MouseDelta;
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

            last_mouse_pos = MouseWatcher.Position;
            
            mdown_p = mdown;

            if (_render_targets_need_resize) {
                top_bar_render_target = new RenderTarget2D(State.graphics_device, top_bar_size.X, top_bar_size.Y);
                _client_area = new RenderTarget2D(State.graphics_device, client_size.X, client_size.Y);
                _render_targets_need_resize = false;
            }
        }

        public Action internal_draw_action;
        public Action draw_action;

        public FloatLerperManual focus_lerp = new FloatLerperManual(0f, 1f, 200);

        private Color foreground => Draw2D.ColorInterpolate(UIColors.Foreground.multiply_color(UIColors.focus_fade), UIColors.Foreground, focus_lerp.Value);
        private Color background => Draw2D.ColorInterpolate(UIColors.Background.multiply_color(UIColors.focus_fade), UIColors.Background, focus_lerp.Value);
        private Color border => Draw2D.ColorInterpolate(UIColors.Foreground.multiply_color(UIColors.focus_fade), UIColors.Foreground, focus_lerp.Value);
        private Color title_bar => Draw2D.ColorInterpolate(UIColors.Background.multiply_color(UIColors.focus_fade), UIColors.Foreground, focus_lerp.Value);
        private Color title_text => Draw2D.ColorInterpolate(UIColors.Foreground.multiply_color(UIColors.focus_fade), UIColors.Background, focus_lerp.Value);

        private float _text_side_gap = 4f;
        
        public void render_internal() {
            if (!_update_render_targets || !_visible) return;

            //DRAW TOP BAR
            State.graphics_device.SetRenderTarget(top_bar_render_target);
            State.graphics_device.Clear(title_bar);
            Draw2D.begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None);

            Draw2D.fill_rect_dither(Vector2i.Zero , top_bar_size - Vector2i.UnitY, 
                UIColors.Foreground.multiply_color(0.9f).multiply_color(UIColors.focus_fade), 
                Draw2D.ColorInterpolate(UIColors.Foreground.multiply_color(UIColors.focus_fade), UIColors.Foreground, focus_lerp.Value), 
                (int)(top_bar_height / 3f));

            var text_background_min = (Vector2i.Right * ((top_bar_size.X / 2f) - (text_size.X / 2f) - (_text_side_gap)));
            var text_background_max = text_background_min + (text_size.X + (_text_side_gap * 2)).ToV2X() + top_bar_height.ToV2Y();
            
            Draw2D.fill_rect(text_background_min, text_background_max, Draw2D.ColorInterpolate(UIColors.Background, UIColors.Foreground, focus_lerp.Value));
            
            Draw2D.text("profont", text, (Vector2i.Right * ((top_bar_size.X / 2f) - (text_size.X / 2f))) + (Vector2.UnitY * ((top_bar_height / 2f) - (text_size.Y / 2f))), title_text);
            
            //Draw2D.line(size.X - 45, 0, size.X - 45, size.Y, 1f, Color.Black);
            //Draw2D.line(size.X - 30, 0, size.X - 30, size.Y, 1f, Color.Black);
            //Draw2D.line(size.X - 15, 0, size.X - 15, size.Y, 1f, Color.Black);
            
            Draw2D.end();

            //PRE-DRAW SUBFORMS
            foreach (IUIForm subform in subforms) {
                State.graphics_device.SetRenderTarget(subform.client_area);
                subform.render_internal();
            }

            //RENDER MAIN CLIENT AREA
            State.graphics_device.SetRenderTarget(client_area);
            State.graphics_device.Clear(UIColors.Background);
            
            
            Draw2D.fill_rect_dither(Vector2i.Zero, client_size, 
                Draw2D.ColorInterpolate(
                    UIColors.Background.multiply_color(0.8f).multiply_color(UIColors.focus_fade), //unfocused
                    UIColors.Background.multiply_color(0.95f), //focused
                    focus_lerp.Value), 
                Draw2D.ColorInterpolate(
                    UIColors.Background.multiply_color(UIColors.focus_fade), //unfocused
                    UIColors.Background, //focused
                    focus_lerp.Value), 
                16);
            
            //Draw2D.rect();
            
            Draw2D.begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None);

            foreach (IUIForm subform in subforms) {
                subform.draw();
            }

            internal_draw_action?.Invoke();

            Draw2D.end();
        }

        
        public void draw() {
            if (!_visible) return;
            
            if (has_focus) focus_lerp.Lerp();
            else focus_lerp.LerpReverse();

            //Draw the window contents if _draw_render_targets is on (this is used alongside resizing windows to prevent issues w/ resizing render targets a bunch)
            if (_draw_render_targets) {
                //Draw title bar
                Draw2D.image(top_bar_render_target, top_left, top_bar_size, Color.White);
                
                //draw client area contents
                Draw2D.image(client_area, absolute_position + client_top_left, Color.White);
                
                //draw window border
                Draw2D.rect(top_left, bottom_right, border, 1f);
                Draw2D.rect(top_left, top_left + top_bar_size, border, 1f);
                
            //Draw a transparent basic version of the window while resizing (to avoid stretching contents)                
            } else {
                Draw2D.fill_rect(top_left, top_left + top_bar_size,
                    UIColors.Foreground.multiply_alpha(0.5f));
                Draw2D.fill_rect(client_top_left, client_top_left + client_size, 
                    UIColors.Background.multiply_alpha(0.5f));
                
                //draw subform outlines
                foreach (IUIForm subform in subforms) {
                    Draw2D.rect(client_top_left + subform.position, client_top_left + subform.position + subform.size, UIColors.Foreground.multiply_alpha(0.5f), 1f);    
                }
                
                //draw window border
                Draw2D.rect(top_left, bottom_right, UIColors.Foreground.multiply_alpha(0.5f), 1f);
                Draw2D.rect(top_left, top_left + top_bar_size, UIColors.Foreground.multiply_alpha(0.5f), 1f);
            }
            
            if ((_resize_handle_R_mo || _resize_handle_R_grabbed) && (!_resize_handle_B_grabbed || _resize_handle_both_grabbed) && top_of_mouse_stack) {
                Draw2D.line(top_right - Vector2i.One, bottom_right - Vector2i.UnitX, UIColors.Foreground, 2f);
            }

            if ((_resize_handle_B_mo || _resize_handle_B_grabbed) && (!_resize_handle_R_grabbed || _resize_handle_both_grabbed) && top_of_mouse_stack) {
                Draw2D.line(bottom_left - Vector2i.One, bottom_right - Vector2i.UnitY, UIColors.Foreground, 2f);
            }

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
