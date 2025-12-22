using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectRaven.Graphics.Drawing2D;

namespace ProjectRaven.Console {
    public static class Log {
        public enum log_level {
            NORMAL,
            CUSTOM,
            WARNING,
            ERROR
        }

        public struct log_data {
            public log_level level;

            public string text;
            public string header;
            public string source;

            public const string default_format_text = "$text";
            //public const string default_format_text_source_header = "[$c_source$source$c_reset::$c_header$header$c_reset]$c_text $text$c_reset";
            public const string default_format_text_header_source = "[$c_header$header$c_reset::$c_source$source$c_reset]$c_text $text$c_reset";

            public string format;

            public Color color_source;
            public Color color_header;
            public Color color_text;

            public Color side_notch_color;

            public string print() {
                string s = format
                    .Replace("$text", text)
                    .Replace("$source", source)
                    .Replace("$header", header)
                    .Replace("$c_header", "#c:" + Draw2D.string_from_color(color_header) + "#")
                    .Replace("$c_source", "#c:" + Draw2D.string_from_color(color_source) + "#")
                    .Replace("$c_text", "#c:" + Draw2D.string_from_color(color_text) + "#")
                    .Replace("$c_reset", "#c#")
                    .Replace("$log_level", typeof(log_level).GetEnumName(level))
                    ;

                return s;
            }

            public log_data(string text = "", string header = "", string source = "", string format = default_format_text_header_source, log_level level = log_level.CUSTOM,
                string color_text = "LightGray", string color_source = "White", string color_header = "White", string side_notch_color = "White") {

                this.level = level;
                this.text = text;
                this.header = header;
                this.source = source;

                this.format = format;

                this.color_source = Draw2D.string_colors[color_source];
                this.color_header = Draw2D.string_colors[color_header];
                this.color_text = Draw2D.string_colors[color_text];
                this.side_notch_color = Draw2D.string_colors[side_notch_color];
            }

            public log_data(string text, string header) {
                this.level = log_level.CUSTOM;
                this.text = text;
                this.source = "";

                this.header = header;

                this.format = default_format_text;

                color_source = Color.White;
                color_header = Color.White;
                color_text = Color.LightGray;
                side_notch_color = Color.White;
            }

            public log_data(string text, string header, string source) {
                this.level = log_level.CUSTOM;
                this.text = text;
                this.source = source;

                this.header = header;

                this.format = default_format_text_header_source;

                color_source = Color.White;
                color_header = Color.White;
                color_text = Color.LightGray;
                side_notch_color = Color.White;
            }

            public log_data(log_level level, string text) {
                this.level = level;
                this.text = text;
                this.source = "";

                this.header = "";

                this.format = default_format_text;

                color_source = Color.White;
                color_header = Color.White;
                color_text = Color.LightGray;
                side_notch_color = Color.White;
            }

            public log_data(log_level level, string text, string header) {
                this.level = level;
                this.text = text;
                this.source = "";

                this.header = header;

                this.format = default_format_text;

                color_source = Color.White;
                color_header = Color.White;
                color_text = Color.LightGray;
                side_notch_color = Color.White;
            }
        }

        public static volatile List<log_data> data = new List<log_data>();

        public static void screenshot() {
            //Scene.screenshot();
        }

        public static void clear() {
            data = new List<log_data>();
        }

        public static void write(log_level level, string text) {
            log(level, text);
        }
        public static void log(log_level level, string text) {
            data.Insert(0, new log_data(level, text));
        }

        public static void write(string text = "", string header = "", string source = "", string format = log_data.default_format_text_header_source, log_level level = log_level.CUSTOM,
                     string color_text = "LightGray", string color_source = "White", string color_header = "White", string side_notch_color = "Transparent") {
            log(text, header, source, format, level, color_text, color_source, color_header, side_notch_color);
        }
        public static void log(string text = "", string header = "", string source = "", string format = log_data.default_format_text_header_source, log_level level = log_level.CUSTOM,
                string color_text = "LightGray", string color_source = "White", string color_header = "White", string side_notch_color = "Transparent") {
            data.Insert(0, new log_data(text, header, source, format, level, color_text, color_source, color_header, side_notch_color));
        }

        public static void write(string text) {
            log(text);
        }
        public static void log(string text) {
            data.Insert(0, new log_data(log_level.NORMAL, text));
        }

        public static void write(string text, string format) {
            log(text, format);
        }
        public static void log(string text, string format) {
            data.Insert(0, new log_data(log_level.NORMAL, text) { format = format });
        }

        public static void write(string text, Type type) {
            log(text, type);
        }
        public static void log(string text, Type type) {
            data.Insert(0, new log_data(text, "test", type.ToString().Replace("Magpie.","")));
        }

        public static string last_n_messages(int n) {
            if (n > data.Count - 1) n = data.Count - 1;
            if (n < 0) n = 0;

            if (data.Count == 0) return "";

            string s = "";
            for (int i = n; i >= 0; i--) {
                s += data[i].print() + "\n";
            }

            return s;
        }


        public static void update() {

        }

    }
}
