using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using static RavenRPG.Engine.Math2D;

namespace RavenRPG.Engine {
    public enum gvar_data_type {
        BOOL,
        INT,
        FLOAT,
        VECTOR2I,
        VECTOR2,
        VECTOR3,
        STRING,
        UNKNOWN
    }

    public static class command_parser {
        public static void parse_string(string input) {
            input = input.Replace("=", " = ");
            input = input.Replace("[", " [ ");
            input = input.Replace("]", " ] ");
            input = input.Replace("(", " ( ");
            input = input.Replace(")", " ) ");

            string[] split = input.Split(' ');
        }
    }


    public class gvar {
        public string name;
        
        public gvar_data_type data_type;
        
        public object data;

        public Action changed;

        public bool save = false;

        public gvar(string name, gvar_data_type data_type, object data, bool save) {
            this.name = name; this.data_type = data_type; this.data = data; this.save = save;
        }

        public string to_string() {
            switch (data_type) {
                case gvar_data_type.BOOL:
                    return ((bool)data).ToString().ToLower();

                case gvar_data_type.INT:
                    return ((int)data).ToString();

                case gvar_data_type.FLOAT:
                    return ((float)data).ToString();

                case gvar_data_type.VECTOR2I:
                    return ((Vector2i)data).ToXString();

                case gvar_data_type.VECTOR2:
                    return ((Vector2)data).simple_vector2_string_full_acc();

                case gvar_data_type.VECTOR3:
                    return ((Vector3)data).simple_vector3_string_full_acc();

                case gvar_data_type.STRING:
                    return (string)data;

                default:
                    return "";
            }
        }
    }

    public static class gvars {
        static Dictionary<string, gvar> _gvars = new Dictionary<string, gvar>();

        public static void add_gvar(string name, gvar_data_type data_type, object data, bool save) {
            _gvars.Add(name, new gvar(name, data_type, data, save));            
        }
        public static void remove_gvar(string name) {
            _gvars.Remove(name); 
        }
        
        public static bool exists(string name) {
            return _gvars.ContainsKey(name);
        }

        public static void toggle_saving(string name, bool save) {
            if (exists(name)) {
                _gvars[name].save = save;
            }
        } 

        #region CONTAINS
        public static bool contains_bool(string name) => (_gvars.ContainsKey(name) && _gvars[name].data_type == gvar_data_type.BOOL);
        public static bool contains_int(string name) => (_gvars.ContainsKey(name) && _gvars[name].data_type == gvar_data_type.INT);
        public static bool contains_float(string name) => (_gvars.ContainsKey(name) && _gvars[name].data_type == gvar_data_type.FLOAT);
        public static bool contains_string(string name) => (_gvars.ContainsKey(name) && _gvars[name].data_type == gvar_data_type.STRING);
        public static bool contains_xy(string name) => (_gvars.ContainsKey(name) && _gvars[name].data_type == gvar_data_type.VECTOR2I);
        public static bool contains_v2(string name) => (_gvars.ContainsKey(name) && _gvars[name].data_type == gvar_data_type.VECTOR2);
        public static bool contains_v3(string name) => (_gvars.ContainsKey(name) && _gvars[name].data_type == gvar_data_type.VECTOR3);
        #endregion


        public static void add_change_action(string name, Action action) {
            _gvars[name].changed = action;
        }

        #region GET/SET
        public static void set(string name, bool value) {
            if (!contains_bool(name)) throw new Exception("no such bool with name \"" + name + "\"");                
            _gvars[name].data = value;
            _gvars[name].changed?.Invoke();
        }
        public static void set(string name, int value) {
            if (!contains_int(name)) throw new Exception("no such int with name \"" + name + "\"");
            _gvars[name].data = value;
            _gvars[name].changed?.Invoke();
        }
        public static void set(string name, float value) {
            if (!contains_float(name)) throw new Exception("no such float with name \"" + name + "\"");
            _gvars[name].data = value;
            _gvars[name].changed?.Invoke();
        }
        public static void set(string name, Vector2i value) {
            if (!contains_xy(name)) throw new Exception("no such Vector2i with name \"" + name + "\"");
            _gvars[name].data = value;
            _gvars[name].changed?.Invoke();
        }
        public static void set(string name, Vector2 value) {
            if (!contains_v2(name)) throw new Exception("no such vector2 with name \"" + name + "\"");
            _gvars[name].data = value;
            _gvars[name].changed?.Invoke();
        }
        public static void set(string name, Vector3 value) {
            if (!contains_v3(name)) throw new Exception("no such vector3 with name \"" + name + "\"");
            _gvars[name].data = value;
            _gvars[name].changed?.Invoke();
        }
        public static void set(string name, string value) {
            if (!contains_string(name)) throw new Exception("no such string with name \"" + name + "\"");
            _gvars[name].data = value;
            _gvars[name].changed?.Invoke();
        }                     

        public static void get(string name, out bool result ) {
            if (!contains_bool(name)) throw new Exception("no such bool with name \"" + name + "\"");
            result = (bool)_gvars[name].data;            
        }
        public static void get(string name, out int result) {
            if (!contains_int(name)) throw new Exception("no such int with name \"" + name + "\"");
            result = (int)_gvars[name].data;
        }
        public static void get(string name, out float result) {
            if (!contains_float(name)) throw new Exception("no such float with name \"" + name + "\"");
            result = (float)_gvars[name].data;
        }
        public static void get(string name, out Vector2i result) {
            if (!contains_xy(name)) throw new Exception("no such Vector2i with name \"" + name + "\"");
            result = (Vector2i)_gvars[name].data;           
        }
        public static void get(string name, out Vector2 result) {
            if (!contains_v2(name)) throw new Exception("no such vector2 with name \"" + name + "\"");
            result = (Vector2)_gvars[name].data;
        }
        public static void get(string name, out Vector3 result) {
            if (!contains_v3(name)) throw new Exception("no such vector3 with name \"" + name + "\"");
            result = (Vector3)_gvars[name].data;
        }
        public static void get(string name, out string result) {
            if (!contains_string(name)) throw new Exception("no such string with name \"" + name + "\"");
            result = (string)_gvars[name].data;
        }

        public static bool get_bool(string name) {
            if (!contains_bool(name)) throw new Exception("no such bool with name \"" + name + "\"");
            if (_gvars[name].data_type == gvar_data_type.BOOL) {
                return (bool)_gvars[name].data;
            } else {
                throw new Exception("get_bool is not suitable for use on " + _gvars[name].data_type.ToString());
            }
        }
        public static int get_int(string name) {
            if (!contains_int(name)) throw new Exception("no such int with name \"" + name + "\"");
            if (_gvars[name].data_type == gvar_data_type.INT || _gvars[name].data_type == gvar_data_type.FLOAT) {
                return (int)_gvars[name].data;
            } else {
                throw new Exception("get_int is not suitable for use on " + _gvars[name].data_type.ToString());
            }
        }
        public static float get_float(string name) {
            if (!contains_float(name)) throw new Exception("no such float with name \"" + name + "\"");
            if (_gvars[name].data_type == gvar_data_type.FLOAT || _gvars[name].data_type == gvar_data_type.INT) {
                return (float)_gvars[name].data;
            } else {
                throw new Exception("get_float is not suitable for use on " + _gvars[name].data_type.ToString());
            }
        }
        public static Vector2i get_Vector2i(string name) {
            if (!contains_xy(name)) return Vector2i.Zero; // throw new Exception("no such Vector2i with name \"" + name + "\"");
            if (_gvars[name].data_type == gvar_data_type.VECTOR2I) {
                return (Vector2i)_gvars[name].data;
            } else {
                throw new Exception("get_Vector2i is not suitable for use on " + _gvars[name].data_type.ToString());
            }
        }
        public static Vector2 get_vector2(string name) {
            if (!contains_v2(name)) throw new Exception("no such vector2 with name \"" + name + "\"");
            if (_gvars[name].data_type == gvar_data_type.VECTOR2) {
                return (Vector2)_gvars[name].data;
            } else {
                throw new Exception("get_vector2 is not suitable for use on " + _gvars[name].data_type.ToString());
            }
        }
        public static Vector3 get_vector3(string name) {
            if (!contains_v3(name)) throw new Exception("no such vector3 with name \"" + name + "\"");
            if (_gvars[name].data_type == gvar_data_type.VECTOR3) {
                return (Vector3)_gvars[name].data;
            } else {
                throw new Exception("get_vector3 is not suitable for use on " + _gvars[name].data_type.ToString());
            }
        }
        
        public static string get_string(string name) {
            string s = "";
            gvar_data_type gdt = _gvars[name].data_type;

            switch (gdt) {
                case gvar_data_type.BOOL:
                    s = ((bool)_gvars[name].data).ToString().ToLower();
                    break;
                case gvar_data_type.INT:
                    s = string.Format("{0}", (int)_gvars[name].data);
                    break;
                case gvar_data_type.FLOAT:
                    s = ((float)_gvars[name].data).ToString();

                    if (!s.Contains('.')) {
                        s += ".0";
                    }

                    break;
                case gvar_data_type.STRING:
                    s = (string)_gvars[name].data;
                    break;
                case gvar_data_type.VECTOR2I:
                    s = Vector2i.simple_string_brackets((Vector2i)_gvars[name].data);
                    break;
                case gvar_data_type.VECTOR2:
                    s = ((Vector2)_gvars[name].data).simple_vector2_string_brackets();
                    break;
                case gvar_data_type.VECTOR3:
                    s =  ((Vector3)_gvars[name].data).simple_vector3_string_brackets();
                    break;
            }
            return s;
        }
        public static string get_string(string name, int decimal_places) {
            string s = "";
            gvar_data_type gdt = _gvars[name].data_type;

            switch (gdt) {
                case gvar_data_type.BOOL:
                    s = ((bool)_gvars[name].data).ToString().ToLower();
                    break;
                case gvar_data_type.INT:
                    s = string.Format("{0}", (int)_gvars[name].data);
                    break;
                case gvar_data_type.FLOAT:
                    s = string.Format("{0:F" + decimal_places + "}", (float)_gvars[name].data);
                    break;
                case gvar_data_type.STRING:
                    s = (string)_gvars[name].data;
                    break;
                case gvar_data_type.VECTOR2I:
                    s = Vector2i.simple_string_brackets((Vector2i)_gvars[name].data);
                    break;
                case gvar_data_type.VECTOR2:
                    s = ((Vector2)_gvars[name].data).simple_vector2_string_brackets();
                    break;
                case gvar_data_type.VECTOR3:
                    s = ((Vector3)_gvars[name].data).simple_vector3_string_brackets();
                    break;
            }
            return s;
        }

        #endregion

        public static gvar_data_type detect_type_from_string(string input) {
            var l = input.ToLower();
            if (l == "true" || l == "false" || l == "yes" || l == "no" || l == "y" || l == "n") {
                return gvar_data_type.BOOL;
            }

            if (input.StartsWith("\"") && input.EndsWith("\"")) {
                return gvar_data_type.STRING;
            }

            if (input.EndsWith("f") && float.TryParse(input, out _)) {
                return gvar_data_type.FLOAT;
            }
            if (int.TryParse(input, out _)) {
                return gvar_data_type.INT;
            }

            //if (input.StartsWith("[") && input.EndsWith("]")) {
                if (Vector2i.TryParse(input, out _)) {
                    return gvar_data_type.VECTOR2I;
                }
                if (Vector2TryParse(input, out _)) {
                    return gvar_data_type.VECTOR2;
                }
                if (Vector3TryParse(input, out _)) {
                    return gvar_data_type.VECTOR3;
                }
            //}
            return gvar_data_type.UNKNOWN;
        }

        public static string list_all() {
            var s = "";
            int c = 0;
            foreach (string gvar_key in _gvars.Keys) {
                var gvar = _gvars[gvar_key];

                s += string.Format("{1} :: [{0}] {2}{3}", gvar.data_type.ToString(), gvar.name, get_string(gvar_key), c < _gvars.Count - 1 ? "\n" : "");
                c++;
            }

            return s;
        }

        #region FIFO

        public static string get_string_for_disk(string name) {
            string s = "";
            gvar_data_type gdt = _gvars[name].data_type;

            switch (gdt) {
                case gvar_data_type.BOOL:
                    s = ((bool)_gvars[name].data).ToString().ToLower();
                    break;
                case gvar_data_type.INT:
                    s = string.Format("{0}", (int)_gvars[name].data);
                    break;
                case gvar_data_type.FLOAT:
                    s = ((float)_gvars[name].data).ToString();

                    if (!s.Contains('.')) {
                        s += ".0";
                    }

                    break;
                case gvar_data_type.STRING:
                    s = "\"" + (string)_gvars[name].data + "\"";
                    break;
                case gvar_data_type.VECTOR2I:
                    s = ((Vector2i)_gvars[name].data).ToXString();
                    break;
                case gvar_data_type.VECTOR2:
                    s = ((Vector2)_gvars[name].data).simple_vector2_string_brackets();
                    break;
                case gvar_data_type.VECTOR3:
                    s = ((Vector3)_gvars[name].data).simple_vector3_string_brackets();
                    break;
            }
            return s;
        }
        public static gvar_data_type detect_type_from_string_on_disk(string input) {
            var l = input.ToLower();
            if (l == "true" || l == "false") {
                return gvar_data_type.BOOL;
            }

            if (input.StartsWith("\"") && input.EndsWith("\"")) {
                return gvar_data_type.STRING;
            }

            if (int.TryParse(input, out _)) {
                return gvar_data_type.INT;
            }

            if (float.TryParse(input, out _)) {
                return gvar_data_type.FLOAT;
            }

            if (Vector2i.TryParse(input, out _)) {
                return gvar_data_type.VECTOR2I;
            }

            if (input.StartsWith("[") && input.EndsWith("]")) {
                if (Vector2TryParse(input, out _)) {
                    return gvar_data_type.VECTOR2;
                }
                if (Vector3TryParse(input, out _)) {
                    return gvar_data_type.VECTOR3;
                }
            }
            return gvar_data_type.UNKNOWN;
        }

        public static bool saved(string name) {
            if (exists(name)) {
                return _gvars[name].save;
            }
            return false;
        }

        public static void write_gvars_to_disk() {
            StringBuilder sb = new StringBuilder();

            foreach (string gvar_key in _gvars.Keys) {                
                var gvar = _gvars[gvar_key];
                if (gvar.save) 
                    sb.AppendLine($"{gvar.name} = {get_string_for_disk(gvar_key)}");
            }

            using (FileStream fs = new FileStream("gvars", FileMode.Create)) {
                fs.Write(Encoding.UTF8.GetBytes(sb.ToString()), 0, sb.Length);
            }
            
            Debug.WriteLine("Wrote GVars to disk");
        }
        
        public static void string_set(string name, gvar_data_type type, string data) {
            if (!exists(name)) throw new Exception($"key \"{name}\" does not exist");
            else if (_gvars[name].data_type != type) throw new Exception($"key \"{name}\" is not type {type.ToString()}");
            
            switch (type) {
                case gvar_data_type.BOOL:
                    if (data == "true")
                        _gvars[name].data = true;
                    else
                        _gvars[name].data = false;
                    break;
                case gvar_data_type.INT:
                    var i = 0;
                    int.TryParse(data, out i);
                    _gvars[name].data = i;
                    break;
                case gvar_data_type.FLOAT:
                    var f = 0.0f;
                    float.TryParse(data, out f);
                    _gvars[name].data = f;                        
                    break;
                case gvar_data_type.VECTOR2I:
                    var xy = Vector2i.Zero;
                    Vector2i.TryParse(data, out xy);
                    _gvars[name].data = xy;
                    break;
                case gvar_data_type.VECTOR2:
                    var v2 = Vector2.Zero;
                    Vector2TryParse(data, out v2);
                    _gvars[name].data = v2;
                    break;
                case gvar_data_type.VECTOR3:
                    var v3 = Vector3.Zero;
                    Vector3TryParse(data, out v3);
                    _gvars[name].data = v3;
                    break;
                case gvar_data_type.STRING:
                    _gvars[name].data = data.TrimStart('\"').TrimEnd('\"');
                    break;
                case gvar_data_type.UNKNOWN:
                    break;
            }

            _gvars[name].changed?.Invoke();
        }
        public static bool read_gvars_from_disk() {
            try {
                using (FileStream filestream = new FileStream("gvars", FileMode.Open)) {
                    byte[] buffer = new byte[filestream.Length];
                    filestream.Read(buffer, 0, (int)filestream.Length);

                    StringReader string_reader = new StringReader(Encoding.UTF8.GetString(buffer));

                    while (string_reader.Peek() > -1) {
                        string line = string_reader.ReadLine();
                        string[] split = line.Split('=');
                        for (int i = 0; i < split.Length; i++) 
                            split[i] = split[i].Trim();

                        // Log.log($"{split[0]} :: {detect_type_from_string_on_disk(split[1])} :: {split[1]}");       

                        if (gvars.exists(split[0])) {
                            if (gvars.saved(split[0])) { 
                                string_set(split[0], detect_type_from_string_on_disk(split[1]), split[1]);
                            }
                        }
                    }

                    return true;
                }
            } catch (FileNotFoundException) { return false; }

        }

        #endregion

    }

}