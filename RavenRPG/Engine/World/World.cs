using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using System.Threading;
using System.Threading.Tasks;
using RavenRPG.Engine.Controls;
using static RavenRPG.Engine.State;

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
    public List<Entity> entities_in_range = new();
    
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
        if (update_thread_task == null || update_thread_task.Status != TaskStatus.Running) {
            update_thread_task = Task.Run(update, cancellation_token);
        }
    }
    
    public static void Stop() => cancellation_token_source.Cancel();
    
    private void update() {
        while (!cancellation_token_source.IsCancellationRequested) {
            var start_dt = DateTime.Now;
            
            //UPDATE 
            binds.Update();
            
            if (binds.just_pressed("switch_buffer")) {
                State.draw_debug_buffer += 1;
                if (State.draw_debug_buffer > 3) 
                    State.draw_debug_buffer = -1;
            }
            
            if (binds.just_pressed("toggle_full_info")) {
                State.show_all_debug_info = !State.show_all_debug_info;
            }
            
            //SLEEP
            while (!cancellation_token_source.IsCancellationRequested) {
                //time since start of tick
                var time_since_start = DateTime.Now - start_dt;
                
                //if it's been longer than the goal time, break the while and start the next tick
                if (time_since_start.TotalMilliseconds >= Clock.update_thread_goal_time.TotalMilliseconds) break;
                
                //sleep until much closer to end of frame
                var sleep_to_tick_end = Clock.update_thread_goal_time.Ticks -
                                 (DateTime.Now - start_dt).Ticks;
                Thread.Sleep(new TimeSpan(sleep_to_tick_end));
            }
            
            Clock.TickRateUpdate((DateTime.Now.Ticks-start_dt.Ticks) / 10000.0);
        }
    }
}