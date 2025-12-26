using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Raven.Engine.Controls;
using Raven.Caching;
using Raven.Engine.Components;
using Raven.Graphics;
using Raven.Graphics.Drawing2D;
using Raven.Graphics.Drawing3D;
using static Raven.Engine.State;

namespace Raven.Engine;

public class Universe {
    ChunkCache chunks = new();
    Clock.UpdateThread update_thread;
    internal ControlBinds binds => update_thread.binds;
    internal Input input => binds.input;
    public string universe_info {
        get {
            string output = "[Universe]\n  [Loaded Chunks]\n";

            foreach (Vector3ui128 chunk_id in chunks.Cache.Keys) {
                var c = chunks.Cache[chunk_id].item;
                output += $"    [{c.position.ToXString()}]\n";
                output += $"      [{c.Entities.Count} Entit{(c.Entities.Count == 1 ? "y" : "ies")}]\n";
                foreach (Entity e in c.Entities) {
                    output += $"     > [{e.name}]\n";
                    output += $"        [chunk ID] {e.position.index.ToXString()} [chunk offset] {e.position.offset.ToXString()}\n";
                    output += e.Components.ListAllComponents(8);
                }
                output += "\n";
            }
            
            return output;
        }
    }

    public void DrawChunkMapAroundEntity(Entity entity, Vector2i position, Vector2i size, int chunks_per_axis = 4) {
        if (chunks_per_axis < 3) chunks_per_axis = 3;
        if (chunks_per_axis % 2 == 0) chunks_per_axis++;
        
        Draw2D.fill_rect(position, position + size, Color.FromNonPremultiplied(30, 30, 30, 76));

        Vector2 chunk_size = size / (float)chunks_per_axis;

        var center_ind = entity.position.index;

        for (int z = - ((chunks_per_axis - 1) / 2); z <= ((chunks_per_axis) / 2); z++) {
            for (int x = - ((chunks_per_axis - 1) / 2); x <= ((chunks_per_axis) / 2); x++) {
                Vector3ui128 chunk_ind = center_ind + (Vector3ui128.UnitX * x) + (Vector3ui128.UnitZ * z);
                var c_top_left = position + (chunk_size * new Vector2i(x + ((chunks_per_axis - 1) / 2), z + ((chunks_per_axis - 1) / 2)));
                var c_bottom_right = c_top_left + chunk_size;
                
                if (chunks.Test(chunk_ind)) {
                    Draw2D.fill_rect_dither(
                         c_top_left, c_bottom_right,
                        Color.FromNonPremultiplied(255, 0, 255, 127), Color.Transparent);
                } else {
                    Draw2D.rect(c_top_left, c_bottom_right, Color.FromNonPremultiplied(255, 0, 255, 127), 1f);
                }
            }
        }


    }
    
    public Universe() {
        update_thread = new Clock.UpdateThread("Universe", Update);
    }

    public void SpawnEntity(Entity entity, Vector3ui128 chunk_pos, Vector3 offset) {
        if (chunks.Test(chunk_pos)) {
            //chunk already cached
            entity.SetPosition(new ChunkPosition(chunks.Request(chunk_pos), offset));
            entity.Initialize();
            entity.parent_universe = this;
            entity.parent_chunk = chunks.Cache[chunk_pos].item;
            
            chunks.Cache[chunk_pos].item.SpawnEntity(entity);
            
        } else {
            //chunk not cached
            chunks.Store(chunk_pos, new Chunk(this, chunk_pos));
            entity.SetPosition(new ChunkPosition(chunks.Request(chunk_pos), offset));
            entity.Initialize();
            entity.parent_universe = this;
            entity.parent_chunk = chunks.Cache[chunk_pos].item;
            
            chunks.Cache[chunk_pos].item.SpawnEntity(entity);
        }
        
        entity.Initialized();
    }
    
    public void DespawnEntity(Entity entity) {
        
    }
    
    public void StartUpdating() {
        update_thread.Start();
    }

    public int chunks_updated = 0;

    internal void universe_task_callback() {
        Interlocked.Increment(ref chunks_updated);
    }
    
    void Update() {
        
        foreach (var c in chunks.Cache.Values) {
            var chunk = c.item;
            Threads.Request(chunk.chunk_update_packet);
            //chunk.Update();
        }

        while (chunks_updated < chunks.Cache.Values.Count) ;
    }

    public void UpdateGraphics() {
        foreach (var c in chunks.Cache.Values) {
            var chunk = c.item;
            chunk.UpdateGraphics();
        }
        GBufferCamera.Manager.UpdateLinkedChunkPositions();
    }

    public void DebugDraw(Camera camera) {
        foreach (var chunk in chunks.Cache.Values) {
            var c = chunk.item;
            
        }
    }

    public void StabilizeChunkPositions() {
        //need to wait for update to finish here
        while (update_thread.CurrentlyUpdating) {}

        lock (chunks.Cache) {
            foreach (var chunk in chunks.Cache.Values) {
                lock (chunk.item.Entities) {
                    foreach (var ent in chunk.item.Entities) {
                        ent.StabilizeChunkPosition();
                    }
                }
            }
        }
        
    }
    
    public List<EntityVisibilityInfo> BuildVisibilityList(Camera camera) {
        List<EntityVisibilityInfo> visible = new();
        
        foreach (var chunk in chunks.Cache.Values) {
            foreach (var ent in chunk.item.Entities) {
                var v = new EntityVisibilityInfo() {
                    camera = camera,
                    entity = ent,
                    //camera_chunk_position =
                };
                v.FindOffsets();
                visible.Add(v);
            }
        }
        
        return visible;
    } 
}

public class ChunkCache : ConcurrentCache<Vector3ui128, Chunk> {
    public ChunkCache() {
        this.name = "UniverseChunkCache";
        
        //empty chunks need to be pruned from the cache but also should not do this while
        //near a camera/player, as quickly moving between chunks could introduce stutters as they
        //are loaded and unloaded, hence the prune_rule
        this.prune_rule = chunk => !chunk.is_near_a_camera() && chunk.is_empty;
    }
}

/*
public class Map {
    private QuadTree quadtree;
    public List<Entity> entities_in_range = new();
    
    public Map() {
        quadtree = new QuadTree();
    }

    public void Update() {
        
    }
}

public class ChunkPosition {
    //TODO actually implement this system and make it work within quadtree
    public Vector2i64 index;
    public Vector3 offset;
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

*/
