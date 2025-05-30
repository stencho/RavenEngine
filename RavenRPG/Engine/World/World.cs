using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using System.Threading;
using System.Threading.Tasks;
using RavenRPG.Engine.Controls;

namespace RavenRPG.Engine.World;

public class Scene {
    public UpdateThread update_thread = new UpdateThread();
    
    private Map current_map = new Map();
    Dictionary<string, Map> maps = new();

    void Update() {
        current_map.Update();
    }
}

public class Map {
    private QuadTree quadtree;

    public Map() {
        quadtree = new QuadTree();
    }

    public void Update() {
        
    }
}

public class QuadTree {
    public class Chunk(Vector2i64 index, Vector2i64 position, Vector2i64 size) {
        private Vector2i64 _xy_index = index;
        private Vector2i64 _position = position;
        private Vector2i64 _size = size;
        
        public Vector2i64 Index =>  _xy_index;
        public Vector2i64 Position => _position;
        public Vector2i64 Size => _size;

        public List<Entity> Entities { get; set; } = new List<Entity>();
    }
    
    private Chunk[,] _chunks;
    public Chunk[,] Chunks => _chunks;

    public QuadTree() {
        
    }
}

public class UpdateThread {
    private Task update_thread_task;
    
    private static CancellationTokenSource cancellation_token_source = new CancellationTokenSource();
    private static CancellationToken cancellation_token => cancellation_token_source.Token;
    
    public void Start() {
        update_thread_task = Task.Run(update, cancellation_token);        
    }
    
    public static void Stop() {
        cancellation_token_source.Cancel();
    }
    public static void Cancel() => Stop();
    
    private void update() {
        while (!cancellation_token_source.IsCancellationRequested) {
            var start_dt = DateTime.Now;
            
            //world update and such goes here
            Input.Update();
            StaticControlBinds.update();

            if (StaticControlBinds.just_pressed("switch_buffer")) {
                State.draw_debug_buffer += 1;
                if (State.draw_debug_buffer > 3) 
                    State.draw_debug_buffer = -1;
            }
            
            if (StaticControlBinds.just_pressed("toggle_full_info")) {
                State.show_all_debug_info = !State.show_all_debug_info;
            }
            
            while (!cancellation_token_source.IsCancellationRequested) {
                var ts = DateTime.Now - start_dt;
                if (ts.TotalMilliseconds >= Clock.update_thread_goal_time.TotalMilliseconds) break;
                Thread.Sleep(new TimeSpan(5000));
            }
            
            Clock.TickRateUpdate((DateTime.Now.Ticks-start_dt.Ticks) / 10000.0);
            
        }
    }
}