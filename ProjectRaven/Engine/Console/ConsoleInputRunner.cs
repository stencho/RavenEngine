using System;
using System.Linq;
using System.Reflection;
using CSScripting;
using CSScriptLib;

namespace ProjectRaven.Console {
    public static class ConsoleInputRunner {
        public static string code_current(ConsoleInputHandler cih) { return preamble + cih.current_input + postamble; }
        public static string code_previous(ConsoleInputHandler cih) { return preamble + cih.previous_input + postamble; }

        public static string[] list_all_namespaces_in_assembly(string root) {
            var asm = Assembly.GetExecutingAssembly();
            var children = asm.GetTypes().Where(t => t.Namespace != null && t.Namespace.StartsWith(root+".")).Select(t => t.Namespace!).Distinct().OrderBy(ns => ns);
            return children.ToArray();
        }

        private static string using_list = "";
        internal static void build_using_list() {
            var nslist = ConsoleInputRunner.list_all_namespaces_in_assembly("ProjectRaven");

            foreach (var ns in nslist) {
                using_list += $"using {ns};\n";
            }
        }
        
        public static string preamble = @"
using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

{using_list}

using static ProjectRaven.Engine.State;
using static ProjectRaven.Console.Log;

public class Script { 
    public void Func() {
";
        public static string postamble = @"    
}}
";

        public static void run_console_input(ConsoleInputHandler handler) {
            string input = handler.previous_input;

            input = input.TrimEnd();

            if (!input.EndsWith(";")) {
                input = input + ";";
            }

            var console_script = preamble.Replace("{using_list}", using_list) + input + postamble;

            try {
                dynamic script = CSScript.RoslynEvaluator.LoadCode(console_script);
                
                script.Func();

                

            } catch (Exception ex) {
                string message = ex.Message.Remove(0, ex.Message.IndexOf(":") + 1);
                message = message.Remove(0, message.IndexOf(":") + 1);
                message = message.Remove(0, message.IndexOf(":") + 1);
                message = message.Replace("\n", "");

                Log.log($"[script error] {message}");
            }
        }        
    }
}
