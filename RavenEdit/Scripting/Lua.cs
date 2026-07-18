using MoonSharp.Interpreter;

namespace RavenEdit.Scripting;

public static class Lua {
    public static void Run() {
        
    }
}

//[MoonSharpUserData]
public static class LuaExposed {
    public static class EditorWindow {
        public static void move() {}
        public static void resize() {}
    }
    
    public static class UIWindowManager {
        public static void show_window() {}
        public static void hide_window() {}
        public static void move_window() {}
        public static void resize_window() {}
    }
}