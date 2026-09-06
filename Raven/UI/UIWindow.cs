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
using Raven.Graphics;
using Raven.Graphics.Drawing2D;
using Raven.Graphics.InterpolatedTypes;

namespace Raven.UI {
    public partial class UIWindow : IUIForm {
        public UIWindowManager window_manager;

        private MouseWatcher mouse => window_manager.mouse;
        
        public Vector2i client_top_left => (Vector2i.UnitY * top_bar_height);
        public Vector2i client_size => size - (Vector2i.UnitY * top_bar_height);
        public Vector2i client_bottom_right => client_top_left + client_size;

        private Vector2i old_size = Vector2i.Zero;
        
        public Vector2i top_bar_size => new Vector2i(client_size.X, top_bar_height);

        public Vector2i min_window_size = new Vector2i(40, 40);
        public Vector2i max_window_size = new Vector2i(600, 600);

        int top_bar_height = 17;

        RenderTarget2D top_bar_render_target;
        
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

        bool _hide_mouse_over = false;
        bool _top_bar_mouse_over = false;
        bool _hide_mouse_down = false;
        bool _top_bar_mouse_down = false;
        private Lerper _hide_mouse_over_fade = new Lerper(0f, 1f, 200);
        
        bool _grabbed_bar = false;
        Vector2 _bar_mouse_offset = Vector2.Zero;

        public bool allow_resize = true;
        public bool show_hide_button = true;
        
        public bool BeingMoved => _grabbed_bar;
        public bool BeingResized => _resize_handle_B_grabbed || _resize_handle_R_grabbed;

        bool mdown = false;
        bool mid_mdown = false;
        bool mdown_p = false;
        bool mid_mdown_p = false;
        
        Vector2i last_mouse_pos = Vector2i.Zero;

        int resize_handle_thickness = 10;

        static Collision2D.Shape2D _mouse_coll_obj_child;
        Vector2i parent_pos => parent_form.position;

        public Action? start_of_update;
        public Action? end_of_update;

        public Action? on_show;
        public Action? on_hide;
        
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

        public virtual void setup() {
            _collision.Add("form", new BoundingBox2D(Vector2i.Zero, size));
            _collision.Add("top_bar", new BoundingBox2D(position, position + (Vector2i.UnitX * size.X) + (Vector2i.UnitY * (top_bar_height+1))));

            _collision.Add("resize_handle_R", new BoundingBox2D(
                position + (size - (Vector2i.UnitX * 6)) - (Vector2i.UnitY * size.Y) + (Vector2i.UnitX * 3),
                bottom_right + (Vector2i.One * 3)));
            _collision.Add("resize_handle_B", new BoundingBox2D(
                position + (size - (Vector2i.UnitY * 6)) - (Vector2i.UnitX * size.X) + (Vector2i.UnitY * 3),
                bottom_right + (Vector2i.One * 3)));

            _collision.Add("hide", new BoundingBox2D(
                position + top_bar_size - (Vector2i.One * top_bar_height), 
                position + top_bar_size
                ));
            
            _client_area = RenderTargetEx.create(client_size.X, client_size.Y);
            top_bar_render_target = RenderTargetEx.create(top_bar_size.X, top_bar_size.Y);
            
            change_text(text);
        }

        public virtual void update() {
            //test_mouse();
            
            start_of_update?.Invoke();
            
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
                
                ((BoundingBox2D)_collision["hide"]).set(
                    (position + parent_form.client_top_left)+ top_bar_size - (Vector2i.One * top_bar_height), 
                    (position + parent_form.client_top_left) + top_bar_size
                );

                mdown = mouse.is_pressed(MouseWatcher.MouseButtons.Left) ;
                mid_mdown = mouse.is_pressed(MouseWatcher.MouseButtons.Middle);
                
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

                ((BoundingBox2D)_collision["hide"]).set(
                    position + top_bar_size - (Vector2i.One * top_bar_height), 
                    position + top_bar_size
                );

                mdown = mouse.is_pressed(MouseWatcher.MouseButtons.Left) && State.is_active && MouseWatcher.mouse_in_bounds;
                mid_mdown = mouse.is_pressed(MouseWatcher.MouseButtons.Middle) && State.is_active && MouseWatcher.mouse_in_bounds;

                _resize_handle_R_mo = Collision2D.GJK2D.test_shapes_simple(_collision["resize_handle_R"], MouseWatcher.MouseCollisionObject, out _);
                _resize_handle_B_mo = Collision2D.GJK2D.test_shapes_simple(_collision["resize_handle_B"], MouseWatcher.MouseCollisionObject, out _);
                
                _hide_mouse_over = top_of_mouse_stack && Collision2D.GJK2D.test_shapes_simple(_collision["hide"], MouseWatcher.MouseCollisionObject, out _);
                
                _top_bar_mouse_over = top_of_mouse_stack && Collision2D.GJK2D.test_shapes_simple(_collision["top_bar"], MouseWatcher.MouseCollisionObject, out _);
            }

            if (gvars.get_bool("ui_window_middle_click_close")) {
                if (mid_mdown && !mid_mdown_p && top_of_mouse_stack && _top_bar_mouse_over) _top_bar_mouse_down = true;
                if (!mid_mdown && mid_mdown_p && !_top_bar_mouse_over) _top_bar_mouse_down = false;
                if (!mid_mdown && mid_mdown_p && top_of_mouse_stack && _top_bar_mouse_over) hide();
            }

            if (show_hide_button) {
                if (mdown && !mdown_p && top_of_mouse_stack && _hide_mouse_over) // just clicked hide
                    _hide_mouse_down = true;
                if (!mdown && mdown_p && top_of_mouse_stack && _hide_mouse_down && !_hide_mouse_over) // just released click, but not while over hide
                    _hide_mouse_down = false;
                if (!mdown && mdown_p && top_of_mouse_stack && _hide_mouse_down && _hide_mouse_over) { // released click, over hide. hide window
                    _hide_mouse_over_fade.reset(0f);
                    hide(); 
                    _hide_mouse_down = false;
                }
                
                //skip to the end if the mouse is doing anythang with the hide button 
                if (_hide_mouse_down) goto end;
            }

            //resizing
            //mouse just clicked
            if (mdown && !mdown_p && top_of_mouse_stack && allow_resize) {
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

                foreach (var subform in subforms) {
                    subform.parent_size_changed(client_size);
                }
            }

            if (_resize_handle_R_grabbed || _resize_handle_B_grabbed) {
                last_mouse_pos = MouseWatcher.Position;
                mdown_p = mdown;
                
                return;
            }
            
            
            //window movement
            //mouse just clicked
            if (mdown && !mdown_p && top_of_mouse_stack) {
                //if clicking top bar, grab the window
                if (Collision2D.GJK2D.test_shapes_simple(_collision["top_bar"], MouseWatcher.MouseCollisionObject, out _))
                    _grabbed_bar = true;
            }

            //mouse down and bar grabbed, position needs to change according to mouse delta
            if (mdown && _grabbed_bar) {
                this.position += mouse.MouseDelta;
            }

            //mouse released, release bar
            if ((!mdown && mdown_p) || !visible) {
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
            
            end:
            //subform updates
            update_all_subforms();

            last_mouse_pos = MouseWatcher.Position;
            
            mdown_p = mdown;
            mid_mdown_p = mid_mdown;

            if (size != old_size) _render_targets_need_resize = true;
            
            if (_render_targets_need_resize) {
                top_bar_render_target = RenderTargetEx.create(top_bar_size.X, top_bar_size.Y);
                _client_area = RenderTargetEx.create(client_size.X, client_size.Y);
                _render_targets_need_resize = false;
            }

            old_size = size;
            
            end_of_update?.Invoke();
        }

        public Action internal_draw_action;
        public Action end_of_draw_action;
        public Action start_of_draw_action;

        private Color border => Draw2D.ColorInterpolate(UIColors.Foreground.multiply_color(UIColors.focus_fade), UIColors.Foreground, focus_lerp.Value);
        private Color title_bar => Draw2D.ColorInterpolate(UIColors.Foreground75Percent.multiply_color(UIColors.focus_fade), UIColors.Foreground75Percent, focus_lerp.Value);
        private Color title_text => Draw2D.ColorInterpolate(UIColors.Foreground.multiply_color(UIColors.focus_fade), UIColors.Foreground25Percent, focus_lerp.Value);

        private float _text_side_gap = 4f;
            
        public void render_internal() {
            if (!_update_render_targets || !_visible) return;

            //DRAW TOP BAR
            State.graphics_device.SetRenderTarget(top_bar_render_target);
            State.graphics_device.Clear(title_bar);
            Draw2D.begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None);
            
            Draw2D.fill_rect_dither(Vector2i.One , top_bar_size - (Vector2.One * 2), 
                UIColors.Foreground75Percent.multiply_color(UIColors.focus_fade), 
                Draw2D.ColorInterpolate(UIColors.Foreground.multiply_color(UIColors.focus_fade), UIColors.Foreground, focus_lerp.Value), 
                (int)(top_bar_height / 5f));

            // TITLE
            var text_background_min = (Vector2i.Right * ((top_bar_size.X / 2f) - (text_size.X / 2f) - (_text_side_gap)));
            var text_background_max = text_background_min + (text_size.X + (_text_side_gap * 2)).ToV2X() + top_bar_height.ToV2Y();
            
            Draw2D.fill_rect(text_background_min + Vector2i.UnitY, text_background_max - (Vector2i.UnitY * 2), Draw2D.ColorInterpolate(UIColors.Background, UIColors.Foreground, focus_lerp.Value));
            Draw2D.line(text_background_min, text_background_min + (Vector2.UnitY * top_bar_height), title_bar, 1f);
            Draw2D.line(text_background_max, text_background_max - (Vector2.UnitY * top_bar_height), title_bar, 1f);
            Draw2D.text("profont", text, (Vector2i.Right * ((top_bar_size.X / 2f) - (text_size.X / 2f))) + (Vector2.UnitY * ((top_bar_height / 2f) - (text_size.Y / 2f) - 0.5f)), title_text);
            
            Draw2D.end();

            if (show_hide_button) {
                // HIDE BUTTON
                if (_hide_mouse_over) _hide_mouse_over_fade.Lerp();
                else _hide_mouse_over_fade.LerpReverse();
                
                Vector2i x_offset = Vector2i.Left;

                if (!_hide_mouse_down) {
                    // BACKGROUND
                    Draw2D.fill_rect(
                        top_bar_size - (Vector2i.One * top_bar_height),
                        top_bar_size,

                        Draw2D.ColorInterpolate(
                            Draw2D.ColorInterpolate(
                                UIColors.Background,
                                UIColors.Foreground,
                                focus_lerp.Value),

                            Draw2D.ColorInterpolate(
                                title_bar,
                                UIColors.Background,
                                focus_lerp.Value),
                            _hide_mouse_over_fade.Value
                        )
                    );

                    // LINE
                    Draw2D.line(
                        top_bar_size + Vector2i.Left + (Vector2i.Up * 6) + (Vector2i.Left * top_bar_height * 0.3f),
                        top_bar_size + Vector2i.Left + (Vector2i.Up * 6) + (Vector2i.Left * top_bar_height * 0.7f),

                        Draw2D.ColorInterpolate(
                            Draw2D.ColorInterpolate(
                                title_bar,
                                UIColors.Background,
                                focus_lerp.Value),
                            Draw2D.ColorInterpolate(
                                UIColors.Background,
                                UIColors.Foreground,
                                focus_lerp.Value),
                            _hide_mouse_over_fade.Value
                        ),
                        2f);

                } else {
                    // HIDE BUTTON BACKGROUND WITH MOUSE DOWN
                    Draw2D.fill_rect(
                        top_bar_size - (Vector2i.One * top_bar_height),
                        top_bar_size,

                        Draw2D.ColorInterpolate(
                            Draw2D.ColorInterpolate(
                                title_bar,
                                UIColors.Background,
                                focus_lerp.Value),

                            Draw2D.ColorInterpolate(
                                UIColors.Background,
                                UIColors.Foreground,
                                focus_lerp.Value),
                            _hide_mouse_over_fade.Value
                        )
                    );

                    // LINE WITH MOUSE DOWN
                    Draw2D.line(
                        top_bar_size + Vector2i.Left + (Vector2i.Up * 6) + (Vector2i.Left * top_bar_height * 0.3f),
                        top_bar_size + Vector2i.Left + (Vector2i.Up * 6) + (Vector2i.Left * top_bar_height * 0.7f),

                        Draw2D.ColorInterpolate(
                            Draw2D.ColorInterpolate(
                                UIColors.Background,
                                UIColors.Foreground,
                                focus_lerp.Value),
                            Draw2D.ColorInterpolate(
                                title_bar,
                                UIColors.Background,
                                focus_lerp.Value),
                            _hide_mouse_over_fade.Value
                        ),
                        2f);
                }

                // HIDE BUTTON OUTLINE
                Draw2D.rect(
                    top_bar_size - (Vector2i.One * top_bar_height - 1),
                    top_bar_size - Vector2i.One,
                    title_bar, 1f
                );
            }
            
            render_all_subform_internals();

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
                8);
            
            Draw2D.begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None);

            draw_all_subforms();

            internal_draw_action?.Invoke();

            Draw2D.end();
        }

        
        public void draw() {
            if (!_visible) return;

            start_of_draw_action?.Invoke();
            
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
                    UIColors.Foreground50Percent);
                Draw2D.fill_rect(absolute_position + client_top_left, absolute_position + client_top_left + client_size, 
                    UIColors.Background50Percent);
                
                //draw subform outlines
                foreach (IUIForm subform in subforms) {
                    Draw2D.rect(absolute_position + client_top_left + subform.position,absolute_position +  client_top_left + subform.position + subform.size, UIColors.Foreground.multiply_alpha(0.5f), 1f);    
                }
                
                //draw window border
                Draw2D.rect(top_left, bottom_right, UIColors.Foreground50Percent, 1f);
                Draw2D.rect(top_left, top_left + top_bar_size, UIColors.Foreground50Percent, 1f);
            }

            if (allow_resize) {
                if ((_resize_handle_R_mo || _resize_handle_R_grabbed) &&
                    (!_resize_handle_B_grabbed || _resize_handle_both_grabbed) && top_of_mouse_stack) {
                    Draw2D.line(top_right - Vector2i.One, bottom_right - Vector2i.UnitX, UIColors.Foreground, 2f);
                }

                if ((_resize_handle_B_mo || _resize_handle_B_grabbed) &&
                    (!_resize_handle_R_grabbed || _resize_handle_both_grabbed) && top_of_mouse_stack) {
                    Draw2D.line(bottom_left - Vector2i.One, bottom_right - Vector2i.UnitY, UIColors.Foreground, 2f);
                }
            }

            end_of_draw_action?.Invoke();
        }

        public void parent_size_changed(Vector2i new_size) {
            
        }

    }
}
