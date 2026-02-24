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
    }
}
