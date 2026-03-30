using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Raven.Engine;
using Raven.Engine.Controls;
using Raven.Graphics.Drawing2D;
using Raven.UI;
using TextCopy;

namespace Raven.Console {
    public class ConsoleInputHandler {

        // input stuff
        public string current_input = "";
        public string previous_input => _previous_input;
        string _previous_input = "";
        string[] split_input;

        // history
        List<string> history = new List<string>();
        int history_index = -1;
        string stored_input = "";
        int stored_cursor_pos = 0;

        public display_direction history_display_dir = display_direction.Up;
        public enum display_direction {
            Down,
            Up,
            Hidden
        }

        // undo
        List<(string entry, int cursor_pos, int sel_A, int sel_B)> undo_states = new List<(string entry, int cursor_pos, int sel_A, int sel_B)>();
        DateTime undo_dt = DateTime.Now;
        bool undo_added = false;
        double undo_time = 0;
        double undo_timer_length = 500;
        int undo_index = 0;
        int stored_select_A, stored_select_B;

        // settings
        public bool has_focus = false;
        public bool run_input = true;
        public bool block_cursor = false;

        const int max_simultaneous_keys = 6;
        const int key_repeat_time = 20;
        const int key_time_before_repeat = 400;

        // color
        public Color color_bg = Color.FromNonPremultiplied(25,25,25,255);
        public Color color_fg_shadow = Color.Black;
        public Color color_fg = Color.White;
        public Color color_fg_selected = Color.Black;
        public Color color_accent = Color.HotPink;

        public Color color_cursor = Color.White;
        public Color color_selection = Color.HotPink;

        // layout
        int X, Y, width, height;

        public int Width => width;

        int font_width = 9;
        int font_height = 9;

        public void set_position(Vector2i xy) { X = xy.X; Y = xy.Y; }
        public void set_position(Vector2 xy) { X = (int)xy.X; Y = (int)xy.Y; }

        public void set_width_lock_to_char_width(int w) {
            width = (w / font_width);
            width *= font_width; 
        }

        public void set_width(int w) {
            width = w;
        }

        private MouseWatcher mouse = new MouseWatcher();

        // cursor
        int cursor_position = 0;

        int cursor_min => 0;
        int cursor_max => current_input.Length;

        int cursor_character_index => font_width * cursor_position;
        int cursor_char_index_x_pos => cursor_character_index * font_width;

        // view
        int crop_width = 100;
        int max_x_chars => crop_width / font_width;

        int view_left = 0;
        int view_right {
            get { return view_left + max_x_chars; }
            set { view_left = value - max_x_chars; }
        }

        string current_input_cropped(int max_width) {
            this.crop_width = max_width;
            if (current_input.Length < max_x_chars) {
                return current_input;
            }
            if (view_left + max_x_chars > current_input.Length) {
                return current_input.Substring(view_left, current_input.Length - view_left);
            } else return current_input.Substring(view_left, max_x_chars);
        }
        int cursor_view_relative => (cursor_position - view_left) * font_width;

        //selection
        int select_A = -1; int select_B = -1;
        public void deselect() { select_A = -1; select_B = -1; }

        int select_dist => select_A - select_B;
        bool selected_left => select_B < select_A;
        bool selected_right => select_A < select_B;

        bool selection => select_dist != 0;

        int selection_start => select_A < select_B ? select_A : select_B;
        int selection_end => select_A > select_B ? select_A : select_B;

        //actions
        public Action scroll_up;
        public Action scroll_down;

        public Action page_up;
        public Action page_down;

        public Action escape;

        //keyboard
        (Keys key, bool just_pressed, DateTime pressed_time, double time_since_press, double repeat_timer)[] key_states;

        //mouse
        int mouse_over_index = -1;

        bool mouse_clicked_on => mouse_clicked_on_index > -1;
        int mouse_clicked_on_index = -1;
        bool only_moving_cursor => mouse_over_index != mouse_clicked_on_index;

        bool shift => State.engine_binds.Keyboard.is_pressed(Keys.LeftShift) || State.engine_binds.Keyboard.is_pressed(Keys.RightShift);
        bool ctrl => State.engine_binds.Keyboard.is_pressed(Keys.LeftControl) || State.engine_binds.Keyboard.is_pressed(Keys.RightControl);
        /// <summary>
        /// creates a single line console text and input handler
        /// </summary>
        /// <param name="X">X Position</param>
        /// <param name="Y">Y Position</param>
        /// <param name="width">Maximum width of the prompt</param>
        /// <param name="font">The MONOSPACED font to use</param>
        public ConsoleInputHandler(int X, int Y, int width) {
            // setup
            this.X = X;
            this.Y = Y;

            var ms = Math2D.measure_string("profont", "a");
            this.font_width = (int)ms.X;
            this.font_height = (int)ms.Y;

            this.width = width + 2;
            this.height = font_height + 2;

            // create and default out the key_states array
            key_states = new (Keys key, bool just_pressed, DateTime pressed_time, double time_since_press, double repeat_timer)[max_simultaneous_keys];
            for (int i = 0; i < max_simultaneous_keys; i++) {
                key_states[i] = (Keys.F24, false, DateTime.Now, 0, 0);
            }
        }


#region cursor
        /// <summary>
        /// Moves the cursor to a specific position and, if shift is held, selects everything between the cursor and the end position
        /// </summary>
        /// <param name="index">Position to move the cursor to</param>
        /// <param name="ignore_shift">Does not select any text if true</param>
        public void set_cursor_pos(int index, bool ignore_shift) {
            // no shift, deslect
            if (!shift) deselect();


            // move cursor in bounds if something is awry
            if (index < cursor_min) index = cursor_min;
            if (index > cursor_max) index = cursor_max;

            // shift held, change or create selection
            if (shift && !ignore_shift) {
                if (!selection) {
                    select_A = cursor_position;
                    select_B = index;
                } else {
                    select_B += index - cursor_position;
                }
            }

            cursor_position = index;

            move_view_to_cursor();
        }

        /// <summary>
        /// If the cursor is past either end of the string, move it back to that end
        /// </summary>
        void force_cursor_in_bounds() {
            if (cursor_max < cursor_position)
                cursor_position = cursor_max;
            if (cursor_min > cursor_position)
                cursor_position = cursor_min;
        }

        /// <summary>
        /// Moves cursor to the right
        /// </summary>
        /// <param name="ignore_shift">Does not select any text if true</param>
        /// <param name="count"></param>
        public void move_cursor_right(bool ignore_shift, int count = 1) {
            // moving more than one position
            while (count > 0) {

                // cursor position below max
                if (cursor_position < cursor_max) {

                    // shift is held, so either start selecting or change the size of the selection
                    if (shift && !ignore_shift) {
                        if (!selection) {
                            select_A = cursor_position;
                            select_B = cursor_position + 1;
                        } else {
                            select_B += 1;
                        }

                        cursor_position++;

                        // have a selection, but not holding shift, so deselect 
                    } else if (selection) {
                        // also move cursor to the right side of the selection before deselecting
                        if (selection && cursor_position != selection_end) {
                            cursor_position = selection_end;
                        }

                        deselect();

                        // no shift, no selection, below cursor_max, just move the thing right
                    } else {
                        cursor_position++;
                    }

                    // cursor pos at right side, not pressing shift, so move the cursor to the right side
                    // of the selection and deselect
                } else if (selection && !shift && cursor_position == selection_end) {
                    cursor_position = selection_end;

                    deselect();

                    break;

                    // cursor pos at right side, do nothing
                } else
                    break;

                count--;
            }
            move_view_to_cursor();
        }

        /// <summary>
        /// Moves cursor to the left
        /// </summary>
        /// <param name="ignore_shift">Does not select any text if true</param>
        /// <param name="count"></param>
        public void move_cursor_left(bool ignore_shift, int count = 1) {
            // this all works practically the same way as move_cursor_right() above
            // just replace max with min, + with - and end with start
            while (count > 0) {
                if (cursor_position > cursor_min) {
                    if (shift && !ignore_shift) {
                        if (!selection) {
                            select_A = cursor_position;
                            select_B = cursor_position - 1;
                        } else {
                            select_B -= 1;
                        }

                        cursor_position--;

                    } else if (selection) {
                        if (selection && cursor_position != selection_start) {
                            cursor_position = selection_start;
                        }

                        deselect();

                    } else {
                        cursor_position--;
                    }

                } else if (selection && !shift && cursor_position == selection_start) {
                    cursor_position = selection_start;
                    deselect();
                    break;

                } else
                    break;

                count--;
            }
            move_view_to_cursor();
        }

        /// <summary>
        /// Moves cursor to the right by one word
        /// </summary>
        /// <param name="ignore_shift">Does not select any text if true</param>
        public void move_cursor_right_one_word(bool ignore_shift) {
            // deselect if shift isn't held
            if (!shift) deselect();

            // seek from the cursor to the end of the input,
            // find the next "end of word" char, then move to where it is
            for (int i = cursor_position; i < current_input.Length; i++) {
                if (current_input[i] == ' '
                 || current_input[i] == '('
                 || current_input[i] == ')'
                 || current_input[i] == '{'
                 || current_input[i] == '}'
                 || current_input[i] == '['
                 || current_input[i] == ']'
                 || current_input[i] == '.'
                 || current_input[i] == ','
                 || current_input[i] == ':'
                 || current_input[i] == ';'
                 || current_input[i] == '\''
                 || current_input[i] == '"'
                 || current_input[i] == '-'
                 || current_input[i] == '+'
                 || current_input[i] == '='
                 || current_input[i] == '?'
                ) {
                    if ((i - cursor_position) == 0)
                        move_cursor_right(ignore_shift);
                    else
                        move_cursor_right(ignore_shift, (i - cursor_position));

                    return;
                }
            }

            set_cursor_pos(cursor_max, ignore_shift);
        }

        /// <summary>
        /// Moves cursor to the left by one word
        /// </summary>
        /// <param name="ignore_shift">Does not select any text if true</param>
        public void move_cursor_left_one_word(bool ignore_shift) {
            if (!shift) deselect();

            for (int i = cursor_position; i > 0; i--) {
                if (current_input[i-1] == ' '
                 || current_input[i-1] == '('
                 || current_input[i-1] == ')'
                 || current_input[i-1] == '{'
                 || current_input[i-1] == '}'
                 || current_input[i-1] == '['
                 || current_input[i-1] == ']'
                 || current_input[i-1] == '.'
                 || current_input[i-1] == ','
                 || current_input[i-1] == ':'
                 || current_input[i-1] == ';'
                 || current_input[i-1] == '\''
                 || current_input[i-1] == '"'
                 || current_input[i-1] == '-'
                 || current_input[i-1] == '+'
                 || current_input[i-1] == '='
                 || current_input[i-1] == '?'
                ) {
                    if ((i - cursor_position) == 0)
                        move_cursor_left(ignore_shift);
                    else
                        move_cursor_left(ignore_shift, Math.Abs(i - cursor_position));
                    return;
                }
            }

            set_cursor_pos(cursor_min, ignore_shift);
        }

        void move_view_to_cursor() {
            if (cursor_position + 1 >= view_right)
                view_right = cursor_position + 1;
            if (cursor_position < view_left)
                view_left = cursor_position;

            if (view_right > current_input.Length) {
                if (max_x_chars > current_input.Length)
                    view_right = max_x_chars;
                else
                    view_right = current_input.Length;
            }
        }
#endregion

#region undo/redo
        /// <summary>
        /// Add a new undo state
        /// </summary>
        public void add_undo_state() {
            if (undo_index > 0) {
                while (undo_index >= 0) {
                    undo_states.RemoveAt(0);
                    undo_index--;
                }
            }

            undo_index = 0;

            if (undo_states.Count > 0) {
                if (undo_states[0].entry != current_input || undo_states[0].sel_A != select_A || undo_states[0].sel_B != select_B) {
                    undo_states.Insert(0, (current_input, cursor_position, select_A, select_B));

                    undo_added = true;
                }

            } else {
                undo_states.Insert(0, (current_input, cursor_position, select_A, select_B));
                undo_added = true;
            }
        }

        /// <summary>
        /// Goes back one undo state
        /// </summary>
        public void undo() {
            if (history_index > -1) {
                reset_history();
                return;
            }

            // we're at the current state going back,
            // so store the current selection incase we come back here
            if (undo_index == 0) {
                stored_select_A = select_A;
                stored_select_B = select_B;
            }

            if (undo_index < undo_states.Count - 1) {
                // change the undo state, replace the current input with it,
                // restore selection state from it, then make sure the cursor is good
                undo_index++;

                current_input = undo_states[undo_index].entry;

                select_A = undo_states[undo_index].sel_A;
                select_B = undo_states[undo_index].sel_B;

                if (!selection)
                    cursor_position = undo_states[undo_index].cursor_pos;
                else
                    cursor_position = select_B;

                force_cursor_in_bounds();
            }
        }

        /// <summary>
        /// Moves forward one undo state
        /// </summary>
        public void redo() {
            if (history_index > -1) {
                reset_history();
                return;
            }

            // this works more or less the same way as undo() above
            // except instead of storing select state, restore it if we hit undo index 0
            if (undo_index > 0) {
                undo_index--;

                current_input = undo_states[undo_index].entry;

                select_A = undo_states[undo_index].sel_A;
                select_B = undo_states[undo_index].sel_B;

                if (!selection)
                    cursor_position = undo_states[undo_index].cursor_pos;
                else
                    cursor_position = select_B;

                force_cursor_in_bounds();
            }

            if (undo_index == 0) {
                select_A = stored_select_A;
                select_B = stored_select_B;
            }
        }
        #endregion

#region history
        void reset_history() {
            history_index = -1;
            current_input = stored_input;
            cursor_position = stored_cursor_pos;
            stored_cursor_pos = -1;
            stored_select_A = -1;
            stored_select_B = -1;
            stored_input = "";
            stored_cursor_pos = 0;
        }


        /// <summary>
        /// Moves one up the history list
        /// </summary>
        public void history_up() {

            if (history_index == -1) {
                stored_select_A = select_A;
                stored_select_B = select_B;
                select_A = -1; select_B = -1;
                stored_input = current_input;
                stored_cursor_pos = cursor_position;
            }
            if (history_index < history.Count - 1) {
                history_index++;
                select_A = -1; select_B = -1;

                current_input = history[history_index];
                cursor_position = cursor_max;

            }

            force_cursor_in_bounds();
            move_view_to_cursor();
        }

        /// <summary>
        /// Moves one down the history list
        /// </summary>
        public void history_down() {

            if (history_index > 0) {
                history_index--;
                select_A = -1; select_B = -1;

                current_input = history[history_index];
                cursor_position = cursor_max;
            } else if (history_index == 0)
                reset_history();

            force_cursor_in_bounds();
            move_view_to_cursor();
        }
#endregion

#region delete/backspace
        /// <summary>
        /// Delete character at cursor or entire selection
        /// </summary>
        void delete() {
            if (history_index > -1)
                add_undo_state();
            // character changed, reset undo setup
            undo_added = false;
            undo_dt = DateTime.Now;

            // text selected, delete selected text
            if (selection) {
                // create a new undo state
                add_undo_state();

                // remove the selected text
                current_input = current_input.Remove(selection_start, selection_end - selection_start);

                // fix up the cursor
                if (selected_right)
                    cursor_position -= selection_end - selection_start;

                force_cursor_in_bounds();

                // clear selection
                deselect();

                add_undo_state();
                // no text selected, delete character at cursor
            } else if (cursor_position < cursor_max)
                current_input = current_input.Remove(cursor_position, 1);

            if (history_index > -1) {
                add_undo_state();
                history_index = -1;

            } else {
                add_undo_state();
            }

            move_view_to_cursor();
        }

        /// <summary>
        /// Remove character before cursor or entire selection
        /// </summary>
        void backspace() {
            if (history_index > -1)
                add_undo_state();
            // this all works more or less the same as delete() but going the other way
            undo_added = false;
            undo_dt = DateTime.Now;

            if (selection) {
                add_undo_state();

                current_input = current_input.Remove(selection_start, selection_end - selection_start);

                if (selected_right)
                    cursor_position -= selection_end - selection_start;

                force_cursor_in_bounds();

                deselect();

                add_undo_state();

            } else if (cursor_position > cursor_min) {
                current_input = current_input.Remove(cursor_position - 1, 1);

                cursor_position--;
            }

            if (history_index > -1) {
                add_undo_state();
                history_index = -1;

            } else {
                add_undo_state();
            }

            move_view_to_cursor();
        }

        /// <summary>
        /// Deletes the word in front of the cursor
        /// </summary>
        void delete_word() {
            // deselect
            deselect();

            // move from the end of the input backwards until hitting the cursor
            int end = current_input.Length - 1;
            for (int i = current_input.Length - 1; i > cursor_position; i--) {
                if (current_input[i] == ' '
                || current_input[i] == '('
                || current_input[i] == ')'
                || current_input[i] == '{'
                || current_input[i] == '}'
                || current_input[i] == '['
                || current_input[i] == ']'
                || current_input[i] == '.'
                || current_input[i] == ','
                || current_input[i] == ';'
                || current_input[i] == ':'
                || current_input[i] == '\''
                || current_input[i] == '"'
                || current_input[i] == '-'
                || current_input[i] == '+'
                || current_input[i] == '='
                || current_input[i] == '?'
                ) {
                    end = i;
                }
            }

            // do the actual deleting part
            if (cursor_position == end)
                delete();
            else
                current_input = current_input.Remove(cursor_position, end - cursor_position + 1);

            move_view_to_cursor();
        }

        /// <summary>
        /// Removes the word behind the cursor
        /// </summary>
        void backspace_word() {
            // works the same as delete_word() but the other way
            int start = 0;
            int end = cursor_position;
            for (int i = 0; i < cursor_position - 1; i++) {
                if (current_input[i] == ' '
                || current_input[i] == '('
                || current_input[i] == ')'
                || current_input[i] == '{'
                || current_input[i] == '}'
                || current_input[i] == '['
                || current_input[i] == ']'
                || current_input[i] == '.'
                || current_input[i] == ','
                || current_input[i] == ';'
                || current_input[i] == ':'
                || current_input[i] == '\''
                || current_input[i] == '"'
                || current_input[i] == '-'
                || current_input[i] == '+'
                || current_input[i] == '='
                || current_input[i] == '?'
                ) {
                    start = i + 1;
                }
            }

            if (start == end)
                backspace();
            else
                current_input = current_input.Remove(start, end - start);
            move_cursor_left(true, end - start);

            move_view_to_cursor();
        }
#endregion

#region text insertion
        /// <summary>
        /// Inserts an entire string into the input buffer
        /// </summary>
        /// <param name="keys">The string to insert</param>
        void insert_string(string keys) {
            if (history_index > -1)
                add_undo_state();

            if (selection)
                delete();

            force_cursor_in_bounds();

            current_input = current_input.Insert(cursor_position, keys);
            cursor_position += keys.Length;

            if (history_index > -1) {
                add_undo_state();
                history_index = -1;

            } else {
                add_undo_state();
            }
            move_view_to_cursor();
        }

        /// <summary>
        /// Inserts a single character into the input buffer
        /// </summary>
        /// <param name="key">The character to insert</param>
        void insert_char(char key) {
            if (history_index > -1)
                add_undo_state();

            if (selection)
                delete();

            force_cursor_in_bounds();

            current_input = current_input.Insert(cursor_position, key.ToString());
            cursor_position++;

            if (history_index > -1) {
                add_undo_state();
                history_index = -1;

            } else {
                if (undo_index > 0) {
                    for (int i = 0; i < undo_index; i++) {
                        undo_states.RemoveAt(0);
                    }
                }

                undo_index = 0;
                undo_added = false;
                undo_dt = DateTime.Now;
            }
            move_view_to_cursor();

        }

        /// <summary>
        /// Inserts a single character into the input buffer
        /// </summary>
        /// <param name="key">The character to insert</param>
        /// <param name="shift_key">The character to insert if shift is held</param>
        void insert_char(char key, char shift_key) {
            if (history_index > -1)
                add_undo_state();

            if (selection)
                delete();

            force_cursor_in_bounds();

            current_input = current_input.Insert(cursor_position, shift ? shift_key.ToString() : key.ToString());
            cursor_position++;

            if (history_index > -1) {
                add_undo_state();
                history_index = -1;

            } else {
                if (undo_index > 0) {
                    for (int i = 0; i < undo_index; i++) {
                        undo_states.RemoveAt(0);
                    }
                }

                undo_index = 0;
                undo_added = false;
                undo_dt = DateTime.Now;
            }

            move_view_to_cursor();
        }

        /// <summary>
        /// Evaluates the current line
        /// adds current state to history, resets everything, and runs the input as a script
        /// </summary>
        public void line_eval() {
            if (string.IsNullOrEmpty(current_input) || string.IsNullOrWhiteSpace(current_input)) return;

            if (history_index != 0)
                history.Insert(0, current_input);

            _previous_input = current_input;
            current_input = "";
            deselect();
            cursor_position = 0;
            history_index = -1;
            stored_input = "";
            undo_states.Clear();
            undo_added = true;
            undo_index = 0;
            undo_states.Add(("", 0, -1, -1));

            move_view_to_cursor();

            if (run_input)
                ConsoleInputRunner.run_console_input(this);
        }
#endregion

#region update
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check_keys">Disables all key entry</param>
        public void update(bool check_keys, Vector2i parent_top_left) {
            // setup
            var ks_k = State.engine_binds.Keyboard.pressed_keys;
            var pks_k = State.engine_binds.Keyboard.pressed_keys_previous;

            // mouse

            // mouse over the text input
            if (Math2D.AABB_test(MouseWatcher.Position.X, MouseWatcher.Position.Y, parent_top_left.X + X, parent_top_left.Y + Y, width, height)) {
                mouse_over_index = ((MouseWatcher.Position.X - parent_top_left.X - X)) / font_width;
                mouse_over_index += view_left;

            } else {
                mouse_over_index = -1;
            }

            // mouse over input, but past end of the string
            if (mouse_over_index >= current_input.Length)
                mouse_over_index = current_input.Length;

            // mouse just clicked, also over the string
            if (mouse.just_pressed(MouseWatcher.MouseButtons.Left) && mouse_over_index > -1) {
                // basically, if shift is held, and nothing is selected,
                // we pretend that the starting point was the cursor pos,
                // which immediately means everything between there and the mouse pos is selected
                // but if shift is held and there is already a selection
                // we pretend that the starting point was the already selected starting point

                if (shift) {
                    if (!selection)
                        mouse_clicked_on_index = cursor_position;
                    else
                        mouse_clicked_on_index = select_A;
                } else
                    mouse_clicked_on_index = mouse_over_index;

                // mouse click just released
            } else if (mouse.just_released(MouseWatcher.MouseButtons.Left)) {
                // if there's no selection, that means the mouse is still on the same character it started on
                // so we just move the cursor there
                if (!selection) {
                    if (mouse_over_index > -1) {
                        cursor_position = mouse_over_index;
                        mouse_clicked_on_index = -1;
                    }
                }
            }

            // left click is down
            if (mouse.is_pressed(MouseWatcher.MouseButtons.Left)) {
                // if the mouse is currently clicking the form and also over a character
                if (mouse_clicked_on && mouse_over_index > -1) {
                    // basically just set the selected region to the region between where the
                    // click started and where the mouse is now
                    select_A = mouse_clicked_on_index;
                    select_B = mouse_over_index;

                    cursor_position = mouse_over_index;
                    move_view_to_cursor();
                }
            }

            // keyboard

            // if key checking is enabled
            if (check_keys) {
                //reset any keys released this frame
                foreach (Keys pk in pks_k) {
                    if (State.engine_binds.Keyboard.just_released(pk))
                        reset_key_state(pk);
                }

                // if not checking keys, just reset them all
            } else {
                foreach ((Keys key, bool just_pressed, DateTime pressed_time, double time_since_press, double repeat_timer) item in key_states) {
                    if (key_state_exists(item.key))
                        reset_key_state(item.key);
                }
            }

            // iterate over all the pressed keys
            foreach (Keys k in ks_k) {
                // check if k is one of the keys in key_states
                // this is just essentially fancy if (is_pressed(k)) with timers
                if (key_state_exists(k)) {
                    // key_state values default to just_pressed being on,
                    // so doing this here turns that off a frame after they're pressed
                    key_states[key_state_index(k)].just_pressed = false;

                    // pull frame delta here in ms
                    double frame_delta = (DateTime.Now - key_states[key_state_index(k)].pressed_time).TotalMilliseconds - key_states[key_state_index(k)].time_since_press;

                    // update how long key.time_since_press has been pressed
                    key_states[key_state_index(k)].time_since_press = (DateTime.Now - key_states[key_state_index(k)].pressed_time).TotalMilliseconds;

                    // key hold interval for input repeating
                    if (key_states[key_state_index(k)].time_since_press > key_time_before_repeat) {
                        // key is now held, so we need to start caring about the key repeat timer instead
                        key_states[key_state_index(k)].repeat_timer += frame_delta;

                        if (key_states[key_state_index(k)].repeat_timer > key_repeat_time) {
                            // the timer is over key_repeat_time
                            // loop over it to both repeat the key and subtract key_repeat_time from repeat_timer
                            // then stop after key_repeat_time is lower than repeat_timer
                            while (key_states[key_state_index(k)].repeat_timer > key_repeat_time) {
                                read_key(k);
                                key_states[key_state_index(k)].repeat_timer -= key_repeat_time;
                            }
                        }
                    }
                }

                // this is where we set keys to be pressed
                // if check keys is turned on, the key was only just pressed, there is an available key_state slot
                // then we set the first available slot up with a fresh key state
                if (check_keys && State.engine_binds.Keyboard.just_pressed(k)) {
                    if (first_available_key_state() != -1)
                        key_states[first_available_key_state()] = (k, true, DateTime.Now, 0, key_repeat_time);
                }
            }

            // go through all the key states, if they're not F24 then read the key and do input
            foreach ((Keys key, bool just_pressed, DateTime pressed_time, double time_since_press, double repeat_timer) item in key_states) {
                if (item.key == Keys.F24) continue;

                if (item.just_pressed)
                    read_key(item.key);
            }

            // undo stuff
            // update undo timer            
            undo_time = (DateTime.Now - undo_dt).TotalMilliseconds;

            // if it's past the timer length, then add an undo state
            if (undo_time >= undo_timer_length && !undo_added)
                add_undo_state();

            // lock undo timer at undo_timer_length for no particular reason
            if (undo_added)
                undo_time = undo_timer_length;

            // set up state for previous frame comparisons

            pks_k = State.engine_binds.Keyboard.pressed_keys_previous;
        }
#endregion

#region drawing
        const int shadow_dist = 2;
        public void draw() {
            if (history_index > -1 && history_display_dir == display_direction.Up) 
                IMGUI.list_display_reverse_highlight(X, Y + 1, width + 2, color_bg, color_fg, color_accent, info_history_array_list_highlight(5));            

            // background
            Draw2D.fill_rect(X + shadow_dist, Y + shadow_dist, width + 2, height, Color.FromNonPremultiplied(0, 0, 0, 128));
            Draw2D.fill_rect(X, Y, width + 2, height, color_bg);
            Draw2D.rect(X, Y, width + 2, height, color_accent, 1);

            if (selection) {

                int a = 0;
                int ad = 0;

                int b = 0;
                int bd = 0;

                if (selected_right) {
                    if (selection_start - view_left < 0) {
                        a = 0;
                        ad = Math.Abs(selection_start - view_left);
                    } else a = selection_start - view_left;


                    // selection background
                    Draw2D.fill_rect(
                        1 + X + (font_width * a), 1 + Y, (font_width * (selection_end - selection_start - ad)), font_height - 1,
                        color_selection);
                } else {

                    var l = (selection_end - view_right);
                    if (selection_end >= view_right) {

                        Draw2D.fill_rect(
                            1 + X + (font_width * (selection_start - view_left)), 1 + Y, (font_width * ((selection_end - selection_start) - l)) - 1, font_height - 1,
                            color_selection);

                    } else {
                        Draw2D.fill_rect(
                            1 + X + (font_width * (selection_start - view_left)), 1 + Y, (font_width * (selection_end - selection_start)), font_height - 1,
                            color_selection);
                    }
                }

                // selection-bookend cursor
                // selected_right is true if the selection end is to the right of the selection start
                if (selected_right)
                    Draw2D.fill_rect(1 + X + cursor_view_relative - 1, 1 + Y, 1, font_height, color_cursor);
                else
                    Draw2D.fill_rect(1 + X + cursor_view_relative - 1, 1 + Y, 1, font_height, color_cursor);


            } else {
                // normal cursor
                if (block_cursor)
                    Draw2D.fill_rect(1 + X + cursor_view_relative, 1 + Y, font_width, font_height, color_cursor);
                else
                    Draw2D.fill_rect(1 + X + cursor_view_relative-1, Y, 1, font_height+1, color_cursor);
            }

            // mouseover box
            if (mouse_over_index > -1) {
                if (selection)
                    Draw2D.fill_rect(X + ((mouse_over_index - view_left) * font_width) - 1, Y - 1, 1, font_height + 2, color_cursor);
                else
                    Draw2D.rect(X + ((mouse_over_index - view_left) * font_width) - 1, Y - 1, font_width + 3, font_height + 2, color_cursor, 1);
            }

            State.spritebatch.DrawString(Resources.GetFont("profont"), current_input_cropped(width), Vector2.One + new Vector2(X, Y) + Vector2.One, color_fg_shadow);
            State.spritebatch.DrawString(Resources.GetFont("profont"), current_input_cropped(width), Vector2.One + new Vector2(X, Y), color_fg);

            if (history_index > -1 && history_display_dir == display_direction.Down) 
                IMGUI.list_display_highlight(X, Y + height, width, color_bg, color_fg, color_accent, info_history_array_list_highlight(5));            
        }
#endregion

#region keyboard
        public bool key_state_exists(Keys key) {
            if (key == Keys.F24) return false;
            for (int i = 0; i < max_simultaneous_keys; i++) {
                if (key_states[i].key == key) return true;
            }
            return false;
        }
        public int key_state_index(Keys key) {
            for (int i = 0; i < max_simultaneous_keys; i++) {
                if (key_states[i].key == key) return i;
            }
            return -1;
        }
        public int first_available_key_state() {
            for (int i = 0; i < max_simultaneous_keys; i++) {
                if (key_states[i].key == Keys.F24) return i;
            }
            return -1;

        }
        public void reset_key_state(Keys key) {
            for (int i = 0; i < key_states.Length; i++) {
                (Keys key, bool just_pressed, DateTime pressed_time, double time_since_press, double repeat_timer) item = key_states[i];
                if (item.key == key) {
                    key_states[i] = (Keys.F24, false, DateTime.Now, 0, 0);
                }
            }
        }
        public void read_key(Keys key) {
            switch (key) {
                case Keys.Enter: line_eval(); break;

                case Keys.Escape: escape?.Invoke(); break;

                case Keys.PageUp: page_up?.Invoke(); break;
                case Keys.PageDown: page_down?.Invoke(); break;

                case Keys.End: set_cursor_pos(cursor_max, false); break;
                case Keys.Home: set_cursor_pos(cursor_min, false); break;

                case Keys.Up:
                    if (shift) scroll_up?.Invoke();
                    if (history_display_dir == display_direction.Up) { history_up(); } else { history_down(); }
                    break;
                case Keys.Down:
                    if (shift) scroll_down?.Invoke();
                    else {
                        if (history_display_dir == display_direction.Up) { history_down(); } else { history_up(); }
                    }
                    break;
                case Keys.Left: if (ctrl) move_cursor_left_one_word(false); else move_cursor_left(false); break;
                case Keys.Right: if (ctrl) move_cursor_right_one_word(false); else move_cursor_right(false); break;

                case Keys.Delete: if (ctrl) delete_word(); else delete(); break;
                case Keys.Back: if (ctrl) backspace_word(); else backspace(); break;

                case Keys.Insert:
                    if (shift) {
                        if (undo_states[0].entry != current_input || undo_states[0].sel_A != select_A || undo_states[0].sel_B != select_B)
                            undo_states.Insert(0, (current_input, cursor_position, select_A, select_B));

                        string cb = ClipboardService.GetText();
                        if (string.IsNullOrEmpty(cb))
                            break;
                        
                        insert_string(cb.Replace("\n", ""));

                        if (undo_states[0].entry != current_input || undo_states[0].sel_A != select_A || undo_states[0].sel_B != select_B) {
                            undo_states.Insert(0, (current_input, cursor_position, select_A, select_B));
                            undo_added = true;
                        }
                    }
                    
                    break;

                case Keys.Space: insert_char(' '); break;
                case Keys.Tab: break;

                case Keys.A:
                    if (ctrl) {
                        select_A = 0;
                        select_B = cursor_max;
                    } else insert_char('a', 'A'); break;

                case Keys.B: insert_char('b', 'B'); break;

                case Keys.C:
                    if (ctrl) {
                        if (selection)
                            ClipboardService.SetText(current_input.Substring(selection_start, selection_end - selection_start));//System.Windows.Forms.Clipboard.SetText(current_input.Substring(selection_start, selection_end - selection_start));
                    } else insert_char('c', 'C');
                    break;

                case Keys.D: insert_char('d', 'D'); break;
                case Keys.E: insert_char('e', 'E'); break;
                case Keys.F: insert_char('f', 'F'); break;
                case Keys.G: insert_char('g', 'G'); break;
                case Keys.H: insert_char('h', 'H'); break;
                case Keys.I: insert_char('i', 'I'); break;
                case Keys.J: insert_char('j', 'J'); break;
                case Keys.K: insert_char('k', 'K'); break;
                case Keys.L: insert_char('l', 'L'); break;
                case Keys.M: insert_char('m', 'M'); break;
                case Keys.N: insert_char('n', 'N'); break;
                case Keys.O: insert_char('o', 'O'); break;
                case Keys.P: insert_char('p', 'P'); break;
                case Keys.Q: insert_char('q', 'Q'); break;
                case Keys.R: insert_char('r', 'R'); break;
                case Keys.S: insert_char('s', 'S'); break;
                case Keys.T: insert_char('t', 'T'); break;
                case Keys.U: insert_char('u', 'U'); break;

                case Keys.V:
                    if (ctrl) {
                        if (undo_states[0].entry != current_input || undo_states[0].sel_A != select_A || undo_states[0].sel_B != select_B)
                            undo_states.Insert(0, (current_input, cursor_position, select_A, select_B));

                        string cb = ClipboardService.GetText();
                        if (string.IsNullOrEmpty(cb))
                            break;

                        insert_string(cb.Replace("\n", ""));

                        if (undo_states[0].entry != current_input || undo_states[0].sel_A != select_A || undo_states[0].sel_B != select_B) {
                            undo_states.Insert(0, (current_input, cursor_position, select_A, select_B));
                            undo_added = true;
                        }
                    } else insert_char('v', 'V'); break;

                case Keys.W: insert_char('w', 'W'); break;
                case Keys.X:
                    if (ctrl) {
                        if (selection) {
                            if (undo_states[0].entry != current_input || undo_states[0].sel_A != select_A || undo_states[0].sel_B != select_B)
                                undo_states.Insert(0, (current_input, cursor_position, select_A, select_B));

                            ClipboardService.SetText(current_input.Substring(selection_start, selection_end - selection_start));

                            current_input = current_input.Remove(selection_start, selection_end - selection_start);

                            if (selected_right)
                                cursor_position -= selection_end - selection_start;

                            force_cursor_in_bounds();

                            deselect();

                            if (undo_states[0].entry != current_input || undo_states[0].sel_A != select_A || undo_states[0].sel_B != select_B) {
                                undo_states.Insert(0, (current_input, cursor_position, select_A, select_B));
                                undo_added = true;
                            }
                        }
                    } else insert_char('x', 'X'); break;

                case Keys.Y:
                    if (ctrl) redo();
                    else insert_char('y', 'Y');
                    break;

                case Keys.Z:
                    if (ctrl) {
                        if (!shift) undo();
                        else redo();
                    } else insert_char('z', 'Z');
                    break;

                case Keys.OemBackslash: insert_char('\\'); break;
                //case Keys.OemTilde: break;

                case Keys.OemMinus: insert_char('-', '_'); break;
                case Keys.OemPlus: insert_char('=', '+'); break;

                case Keys.OemOpenBrackets: insert_char('[', '{'); break;
                case Keys.OemCloseBrackets: insert_char(']', '}'); break;
                case Keys.OemPipe: insert_char('\\', '|'); break;

                case Keys.OemSemicolon: insert_char(';', ':'); break;
                case Keys.OemQuotes: insert_char('\'', '"'); break;

                case Keys.OemComma: insert_char(',', '<'); break;
                case Keys.OemPeriod: insert_char('.', '>'); break;
                case Keys.OemQuestion: insert_char('/', '?'); break;

                case Keys.Multiply: insert_char('*'); break;
                case Keys.Add: insert_char('+'); break;
                case Keys.Separator: insert_char('-'); break;
                case Keys.Subtract: insert_char('-'); break;
                case Keys.Decimal: insert_char('.'); break;
                case Keys.Divide: insert_char('/'); break;

                case Keys.D0: insert_char('0', ')'); break;
                case Keys.NumPad0: insert_char('0'); break;
                case Keys.D1: insert_char('1', '!'); break;
                case Keys.NumPad1: insert_char('1'); break;
                case Keys.D2: insert_char('2', '@'); break;
                case Keys.NumPad2: insert_char('2'); break;
                case Keys.D3: insert_char('3', '#'); break;
                case Keys.NumPad3: insert_char('3'); break;
                case Keys.D4: insert_char('4', '$'); break;
                case Keys.NumPad4: insert_char('4'); break;
                case Keys.D5: insert_char('5', '%'); break;
                case Keys.NumPad5: insert_char('5'); break;
                case Keys.D6: insert_char('6', '^'); break;
                case Keys.NumPad6: insert_char('6'); break;
                case Keys.D7: insert_char('7', '&'); break;
                case Keys.NumPad7: insert_char('7'); break;
                case Keys.D8: insert_char('8', '*'); break;
                case Keys.NumPad8: insert_char('8'); break;
                case Keys.D9: insert_char('9', '('); break;
                case Keys.NumPad9: insert_char('9'); break;

                case Keys.F1: break;
                case Keys.F2: break;
                case Keys.F3: break;
                case Keys.F4: break;
                case Keys.F5: break;
                case Keys.F6: break;
                case Keys.F7: break;
                case Keys.F8: break;
                case Keys.F9: break;
                case Keys.F10: break;
                case Keys.F11: break;
                case Keys.F12: break;
                default: break;
            }

            split_input = current_input.Split(' ');
        }
#endregion

#region info
        public string info_history() {
            var sb = new StringBuilder();

            sb.AppendLine($"[history index] {history_index}");
            sb.AppendLine($"[stored input] {stored_input}");
            sb.AppendLine($"[stored cursor position] {stored_cursor_pos}");
            sb.AppendLine("[history states] -[");

            for (int i = history_index + 2; i >= history_index - 2; i--) {
                if (i >= 0) {
                    string h = "";
                    if (i < history.Count)
                        h = history[i];

                    sb.AppendLine($"{(i == history_index ? "-->" : "-  ")}{h}");

                } else {
                    sb.AppendLine((i == history_index ? "-->" : "-  "));
                }
            }
            sb.AppendLine("]-");
            sb.AppendLine();

            return sb.ToString();
        }

        string[] info_history_array_list(int show) {
            string[] har = null;

            if (history.Count < show) {
                har = new string[history.Count];
                for (int i = 0; i < history.Count; i++) {
                    har[i] = $"{(i == history_index ? "> " : "  ")}{history[i]}";
                }
            } else {
                har = new string[show];

                if (history_index < 2) {
                    for (int i = 0; i < show; i++) {
                        har[i] = $"{(i == history_index ? "> " : "  ")}{history[i]}";

                    }
                } else if (history_index >= history.Count - 2) {
                    int c = 0;
                    for (int i = history.Count - show; i < history.Count; i++) {
                        har[c] = $"{(i == history_index ? "> " : "  ")}{history[i]}";
                        c++;
                    }
                } else if (history_index >= 2 && history_index < history.Count - 2) {
                    int c = 0;
                    for (int i = history_index - 2; i <= history_index + 2; i++) {
                        har[c] = $"{(i == history_index ? "> " : "  ")}{history[i]}";
                        c++;
                    }
                }
            }

            return har;
        }

        (string s, bool hl)[] info_history_array_list_highlight(int show) {
            (string s, bool hl)[] har = null;

            if (history.Count < show) {
                har = new (string s, bool hl)[history.Count];
                for (int i = 0; i < history.Count; i++) {
                    har[i].s = $"{(i == history_index ? "> " : "  ")}{history[i]}";
                    har[i].hl = (i == history_index);
                }
            } else {
                har = new (string s, bool hl)[show];

                if (history_index < 2) {
                    for (int i = 0; i < show; i++) {
                        har[i].s = $"{(i == history_index ? "> " : "  ")}{history[i]}";
                        har[i].hl = (i == history_index);

                    }
                } else if (history_index >= history.Count - 2) {
                    int c = 0;
                    for (int i = history.Count - show; i < history.Count; i++) {
                        har[c].s = $"{(i == history_index ? "> " : "  ")}{history[i]}";
                        har[c].hl = (i == history_index);
                        c++;
                    }
                } else if (history_index >= 2 && history_index < history.Count - 2) {
                    int c = 0;
                    for (int i = history_index - 2; i <= history_index + 2; i++) {
                        har[c].s = $"{(i == history_index ? "> " : "  ")}{history[i]}";
                        har[c].hl = (i == history_index);
                        c++;
                    }
                }
            }

            return har;
        }

        public string info_undo() {
            var sb = new StringBuilder();

            sb.AppendLine($"[undo timer] {undo_time} ");
            sb.AppendLine($"[undo added] {undo_added}");
            sb.AppendLine($"[undo index] {undo_index}");
            sb.AppendLine();

            sb.AppendLine("[undo states] -[");

            for (int i = undo_index - 2; i <= undo_index + 2; i++) {
                if (i >= 0) {
                    string h = "";
                    if (i < undo_states.Count) {
                        h = undo_states[i].entry;

                        sb.AppendLine($"{(i == undo_index ? "-->" : "-- ")}{h}");
                        sb.AppendLine($"{(i == undo_index ? "  |" : "   ")}{new string(' ', (undo_states[i].cursor_pos > 0 ? undo_states[i].cursor_pos : 0))}^");
                    } else {
                        sb.AppendLine((i == undo_index ? "-->" : "-- "));
                        sb.AppendLine((i == undo_index ? "  |" : "   "));
                    }

                } else {
                    sb.AppendLine((i == undo_index ? "-->" : "-- "));
                    sb.AppendLine((i == undo_index ? "  |" : "   "));
                }
            }

            sb.AppendLine("]-");
            sb.AppendLine();

            return sb.ToString();
        }

        public string info_code() {
            var sb = new StringBuilder();
            sb.AppendLine("## code ##");
 
            sb.Append(ConsoleInputRunner.code_current(this));

            sb.AppendLine();
            return sb.ToString();
        }

        public string info() {
            var sb = new StringBuilder();

            sb.AppendLine($"[mouse over index] {mouse_over_index}");
            sb.AppendLine();

            sb.Append(info_history());
            sb.Append(info_undo());

            sb.AppendLine("## keys ##");
            foreach ((Keys key, bool just_pressed, DateTime pressed_time, double time_since_press, double repeat_timer) item in key_states) {
                if (key_state_exists(item.key))
                    sb.AppendLine($"{item.key}:{item.time_since_press}");
            }
            sb.Append("\n");

            return sb.ToString();
        }
#endregion
    }
}
