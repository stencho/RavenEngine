using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Raven.Engine;
using Raven.Engine.Controls;
using Raven.Graphics.Drawing2D;

namespace Raven.UI
{
    public static class IMGUI
    {
        const int shadow_dist = 2;

        const int menu_x_margin = 4;
        const int menu_y_margin = 4;

        public static void slider(int x, int y, int width, string text, ref float value, float min, float max)
        {
            var measure_string = Math2D.measure_string("profont", text);
            State.spritebatch.DrawString(Resources.GetFont("profont"), text, new Vector2(x, y), Color.White);

            Draw2D.fill_rect(x, y + measure_string.Y + 10, width, 1, Color.Red);
            Draw2D.circle(new Vector2(x + width / 2, y + measure_string.Y + 10), 8, 2, Color.MonoGameOrange);
        }

        const int list_entry_gap = 2;
        const int list_x_margin = 4;
        const int list_y_margin = 4;

        public static void list_display(int x, int y, int max_width, Color bg, Color text_color, params string[] list)
        {
            var total_height = list_y_margin;
            var total_width = 0;
            int max_c = max_width / Math2D.measure_string("profont", "a").X;

            for (int i = 0; i < list.Length; i++) {
                string s = list[i];
                var measure_string = Math2D.measure_string("profont", s);

                if (measure_string.X > total_width)
                {
                    total_width = measure_string.X;
                }

                total_width += list_x_margin * 2;
                total_height += measure_string.Y + (i < list.Length - 1 ? list_entry_gap : 0);
            }

            total_height += list_y_margin;
            var current_height = list_y_margin;

            if (total_width > max_width)
                total_width = max_width;

            // background
            Draw2D.fill_rect(x + shadow_dist, y + shadow_dist, total_width, total_height, Color.FromNonPremultiplied(0, 0, 0, 128));
            Draw2D.fill_rect(x, y, total_width, total_height, bg);
            Draw2D.rect(x, y, total_width, total_height, text_color, 1);

            for (int i = 0; i < list.Length; i++) {
                string s = list[i];
                var measure_string = Math2D.measure_string("profont", s);
                string ss = s;
                if (s.Length > max_c)
                {
                    ss = ss.Ellipsis(max_c);
                }

                // text
                State.spritebatch.DrawString(Resources.GetFont("profont"), ss, new Vector2(x + list_x_margin, y + current_height) + Vector2.One, text_color);
                State.spritebatch.DrawString(Resources.GetFont("profont"), ss, new Vector2(x + list_x_margin, y + current_height), text_color);

                current_height += (i < list.Length - 1 ? measure_string.Y + list_entry_gap : 0);
            }
        }

        public static void list_display_highlight(int x, int y, int max_width, Color bg, Color text_color, Color highlight, params (string s, bool highlight)[] list)
        {
            var total_height = list_y_margin;
            var total_width = 0;

            int max_c = (max_width - list_x_margin) / Math2D.measure_string("profont", "a").X;

            for (int i = 0; i < list.Length; i++)
            {
                (string s, bool highlight) s = list[i];
                var measure_string = Math2D.measure_string("profont", s.s);

                if (measure_string.X > total_width)
                {
                    total_width = measure_string.X;
                }

                total_width += list_x_margin * 2;
                total_height += measure_string.Y + (i < list.Length - 1 ? list_entry_gap : 0);
            }

            total_height += list_y_margin;
            var current_height = list_y_margin;

            if (total_width > max_width)
                total_width = max_width;

            // background
            Draw2D.fill_rect(x + shadow_dist, y + shadow_dist, total_width, total_height, Color.FromNonPremultiplied(0, 0, 0, 128));
            Draw2D.fill_rect(x, y, total_width, total_height, bg);
            Draw2D.rect(x, y, total_width, total_height, text_color, 1);

            for (int i = 0; i < list.Length; i++)
            {
                var measure_string = Math2D.measure_string("profont", list[i].s);
                string ss = list[i].s;
                if (list[i].s.Length > max_c)
                {
                    ss = ss.Ellipsis(max_c);
                }

                // text
                State.spritebatch.DrawString(Resources.GetFont("profont"), ss, new Vector2(x + list_x_margin, y + current_height) + Vector2.One, Color.Black);
                if (list[i].highlight)
                    State.spritebatch.DrawString(Resources.GetFont("profont"), ss, new Vector2(x + list_x_margin, y + current_height), highlight);
                else
                    State.spritebatch.DrawString(Resources.GetFont("profont"), ss, new Vector2(x + list_x_margin, y + current_height), text_color);
                
                current_height += (i < list.Length - 1 ? measure_string.Y + list_entry_gap : 0);
            }
        }

        public static void list_display_reverse(int x, int y, int max_width, Color bg, Color text_color, params string[] list)
        {
            var total_height = list_y_margin;
            var total_width = 0;
            int max_c = (max_width - list_x_margin) / Math2D.measure_string("profont", "a").X;

            for (int i = 0; i < list.Length; i++)
            {
                string s = list[i];
                var measure_string = Math2D.measure_string("profont", s);

                if (measure_string.X > total_width)
                {
                    total_width = measure_string.X;
                }

                total_width += list_x_margin * 2;
                total_height += measure_string.Y + (i < list.Length - 1 ? list_entry_gap : 0);
            }

            total_height += list_y_margin;
            var current_height = list_y_margin;

            if (total_width > max_width)
                total_width = max_width;

            // background
            Draw2D.fill_rect(x + shadow_dist, y - total_height + shadow_dist, total_width, total_height, Color.FromNonPremultiplied(0, 0, 0, 128));
            Draw2D.fill_rect(x, y - total_height, total_width, total_height, bg);
            Draw2D.rect(x, y - total_height, total_width, total_height, text_color, 1);

            for (int i = 0; i < list.Length; i++)
            {
                string s = list[i];
                var measure_string = Math2D.measure_string("profont", s);
                string ss = s;
                if (s.Length > max_c)
                {
                    ss = ss.Ellipsis(max_c);
                }

                // text
                Draw2D.text(ss, new Vector2(x + list_x_margin, y - measure_string.Y - current_height) + Vector2.One, Color.Black);
                Draw2D.text(ss, new Vector2(x + list_x_margin, y - measure_string.Y - current_height), text_color);

                current_height += measure_string.Y + (i < list.Length - 1 ? list_entry_gap : 0);
            }
        }

        public static void list_display_reverse_highlight(int x, int y, int max_width, Color bg, Color text, Color highlight_color, params (string s, bool highlight)[] list)
        {
            var total_height = list_y_margin;
            var total_width = 0;

            int max_c = (max_width - list_x_margin) / Math2D.measure_string("profont", "a").X;
            for (int i = 0; i < list.Length; i++)
            {
                (string s, bool highlight) s = list[i];
                var measure_string = Math2D.measure_string("profont", s.s);

                if (measure_string.X > total_width)
                {
                    total_width = measure_string.X;
                }

                total_width += list_x_margin * 2;
                total_height += measure_string.Y + (i < list.Length - 1 ? list_entry_gap : 0);
            }

            total_height += list_y_margin;
            var current_height = list_y_margin;

            if (total_width > max_width)
                total_width = max_width;

            // background
            Draw2D.fill_rect(x + shadow_dist, y - total_height + shadow_dist, total_width, total_height, Color.FromNonPremultiplied(0, 0, 0, 128));
            Draw2D.fill_rect(x, y - total_height, total_width, total_height, bg);
            Draw2D.rect(x, y - total_height, total_width, total_height, highlight_color, 1);

            for (int i = 0; i < list.Length; i++)
            {
                string s = list[i].s;
                var measure_string = Math2D.measure_string("profont", s);
                string ss = list[i].s;
                if (list[i].s.Length > max_c)
                {
                    ss = ss.Ellipsis(max_c);
                }

                // text
                Draw2D.text(ss, new Vector2(x + list_x_margin, y - measure_string.Y - current_height) + Vector2.One, Color.Black);
                if (list[i].highlight)
                    Draw2D.text(ss, new Vector2(x + list_x_margin, y - measure_string.Y - current_height), highlight_color);
                else
                    Draw2D.text(ss, new Vector2(x + list_x_margin, y - measure_string.Y - current_height), text);

                current_height += measure_string.Y + (i < list.Length - 1 ?  list_entry_gap : 0);
            }
        }


        public static void button(int x, int y, int width, int height, string text, Action method, Color fg, Color bg)
        {
            var mouse_over = Math2D.AABB_test
                (State.input_main_thread.mouse_position.X, State.input_main_thread.mouse_position.Y,
                x, y, width, height);
            var measure_string = Math2D.measure_string("profont", text);

            var bg_col = Color.White;
            var fg_col = Color.White;

            if (mouse_over && State.input_main_thread.just_pressed(Input.MouseButtons.Left))
            {
                method();
            }

            if (mouse_over)
            {
                if (State.input_main_thread.is_pressed(Input.MouseButtons.Left))
                {
                    bg_col = bg;
                    fg_col = fg;
                }
                else
                {
                    bg_col = fg;
                    fg_col = bg;
                }
            }
            else
            {
                bg_col = bg;
                fg_col = fg;
            }

            Draw2D.fill_rect(x, y, width, height, bg_col);
            Draw2D.text(text, new Vector2(x + width / 2, y + height / 2) - measure_string / 2, fg_col);
            Draw2D.rect(x, y, width, height, fg, 1);
        }

        public static void menu(int x, int y, Color fg, Color bg, params (string text, Action method)[] items)
        {
            var mouse_over = false;
            var widest_string = float.MinValue;
            var total_height = 0;


            foreach ((string text, Action method) item in items)
            {
                var measure_string = Math2D.measure_string("profont", item.text);

                if (widest_string < measure_string.X)
                {
                    widest_string = measure_string.X;
                }

                total_height += measure_string.Y + menu_y_margin + menu_y_margin;
            }

            var total_width = (int)(widest_string + menu_x_margin * 2);

            var current_height = 0;
            foreach ((string text, Action method) item in items)
            {
                var measure_string = Math2D.measure_string("profont", item.text);

                mouse_over = Math2D.AABB_test(State.input_main_thread.mouse_position.X, State.input_main_thread.mouse_position.Y, x, y + current_height, total_width, measure_string.Y + menu_y_margin + menu_y_margin - 1);

                if (mouse_over && State.input_main_thread.just_pressed(Input.MouseButtons.Left))
                {
                    item.method();

                }

                var bg_col = Color.White;
                var fg_col = Color.White;

                if (mouse_over)
                {
                    if (State.input_main_thread.is_pressed(Input.MouseButtons.Left))
                    {
                        bg_col = bg;
                        fg_col = fg;
                    }
                    else
                    {
                        bg_col = fg;
                        fg_col = bg;
                    }
                }
                else
                {
                    bg_col = bg;
                    fg_col = fg;
                }

                Draw2D.fill_rect(x, y + current_height, total_width, measure_string.Y + menu_y_margin * 2,
                    bg_col);

                Draw2D.text("profont", item.text,
                    new Vector2(x + menu_x_margin + widest_string / 2, y + current_height + (measure_string.Y + menu_y_margin + menu_y_margin) / 2) - measure_string / 2,
                    fg_col);

                current_height += measure_string.Y + menu_y_margin + menu_y_margin;

                Draw2D.rect(x, y + (current_height - 1), (int)widest_string + menu_x_margin * 2, 1, fg, 1);
            }

            Draw2D.rect(x, y, total_width, total_height, fg, 1);

        }

        //windows style file edit view etc
        public static void menu_menu()
        {

        }
    }
}
