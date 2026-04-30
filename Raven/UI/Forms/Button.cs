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

    public partial class UIButton : IUIForm {
        public Vector2i client_top_left => top_left;
        public Vector2i client_size => size;
        public Vector2i client_bottom_right => bottom_right;

        Lerper molerp = new Lerper(0, 1, 50);
        Lerper mdlerp = new Lerper(0, 1, 50);
        
        private bool mouse_down_and_over = false;
        
        public UIButton(int X, int Y, int width, int height, string text) {
            change_text(text);
            setup(X,Y,width,height);
        }
        
        public UIButton(int X, int Y, string text) {
            change_text(text);
            setup(X,Y,text_size.X + 4, text_size.Y + 4);
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
        private bool mouse_down_on_this = false;
        
        public void update() {
            update_collision();

            var mo = test_mouse();

            mdown = State.engine_binds.Mouse.is_pressed(MouseWatcher.MouseButtons.Left);
            
            if (mo && (top_of_mouse_stack)) {
                mouse_down_and_over = mdown;
                current_flags = button_mouse_status.mouse_over;

                if (mdown) {
                    current_flags = button_mouse_status.mouse_over | button_mouse_status.mouse_down;
                }
                
                if (State.engine_binds.Mouse.just_pressed(MouseWatcher.MouseButtons.Left)) {
                    mouse_down_on_this = true;
                }
                
            //handle moving mouse off and back on    
            } else if (!mo || !(top_of_mouse_stack && is_child)) {
                if (State.engine_binds.Mouse.is_pressed(MouseWatcher.MouseButtons.Left) && mouse_down_on_this) {
                    current_flags = button_mouse_status.mouse_down;
                } else if (State.engine_binds.Mouse.just_released(MouseWatcher.MouseButtons.Left)) {
                    mouse_down_on_this = false;
                    current_flags = button_mouse_status.none;
                } else {
                    current_flags = button_mouse_status.none;
                }
            }
             
            if (!current_flags.HasFlag(button_mouse_status.mouse_down) && previous_flags.HasFlag(button_mouse_status.mouse_down) && !_clicked ) {
                //mouse just released
                if (mo && !mdown && previous_flags.HasFlag(button_mouse_status.mouse_down) && (top_of_mouse_stack) && mouse_down_on_this) {
                    //do stuff
                    if (button_click != null )
                        button_click.Invoke();

                    _clicked = true;
                    mouse_down_on_this = false;
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
            
            if (mdown && mouse_over && mouse_down_on_this) {
                mdlerp.Lerp();
            } else {
                mdlerp.LerpReverse();
            }

            var mo_offset = (Vector2.One * 2) * (molerp.Value - mdlerp.Value);
            
            //shadow
            Draw2D.fill_rect(top_left + mo_offset/2, bottom_right + mo_offset/2, Draw2D.ColorInterpolate(Color.Transparent, UIColors.Background.multiply_color(.75f), float.Clamp(molerp.Value * 2f, 0.5f, 1f)));
            //background
            Draw2D.fill_rect(top_left - mo_offset, bottom_right - mo_offset, Draw2D.ColorInterpolate(color_background, UIColors.Foreground, mdlerp.Value));
            //text
            Draw2D.text("profont", text, position - mo_offset + (size / 2) - (ms / 2), Draw2D.ColorInterpolate(color_foreground, UIColors.Background, mdlerp.Value));
            //border
            Draw2D.rect(top_left - mo_offset, bottom_right - mo_offset, 
                Draw2D.ColorInterpolate(color_foreground, UIColors.Background, mdlerp.Value), 
                1);
        }

    }