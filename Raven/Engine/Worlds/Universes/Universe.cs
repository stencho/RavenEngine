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
using Raven.Graphics.InterpolatedTypes;
using static Raven.Engine.State;

namespace Raven.Engine;

public class Universe {
    
    public ChunkCache chunks = new();
    
    internal ControlBinds binds => update_thread.binds;
    internal Input input => binds.input;
    
    public string universe_info {
        get {
            string output = "[Universe]\n  [Loaded Chunks]\n";

            foreach (Vector3ui128 chunk_id in chunks.Cache.Keys) {
                var c = chunks.Cache[chunk_id].item;
                output += $"    [{c.position.ToXString()}]\n";
                output += $"      [{c.Entities.Count} Entit{(c.Entities.Count == 1 ? "y" : "ies")}]\n";
                
                foreach (Entity e in c.Entities.Values) {
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
        
        Vector2 entity_offset = new Vector2(entity.position.offset.X, entity.position.offset.Z);
        
        var entity_X = entity.position.index.X; 
        var entity_Z = entity.position.index.Z;
        
        for (int z = - ((chunks_per_axis - 1) / 2); z <= ((chunks_per_axis) / 2); z++) {
            for (int x = - ((chunks_per_axis - 1) / 2); x <= ((chunks_per_axis) / 2); x++) {
                Vector3ui128 chunk_ind = center_ind + (Vector3ui128.UnitX * x) + (Vector3ui128.UnitZ * z);
                var c_top_left = position + (chunk_size * new Vector2i(x + ((chunks_per_axis - 1) / 2), z + ((chunks_per_axis - 1) / 2)));
                var c_bottom_right = c_top_left + chunk_size;
                var g = (entity_X == chunk_ind.X) && (entity_Z == chunk_ind.Z) ? 64 : 0;
                if (chunks.Test(chunk_ind)) {
                    if (chunks.Cache[chunk_ind].item.Entities.Count > 0) {
                        Draw2D.fill_rect_dither(
                            c_top_left, c_bottom_right,
                            Color.FromNonPremultiplied(255, g, 255, 127), Color.Transparent);
                    }
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
            
            chunks.Cache[chunk_pos].item.SpawnEntity(entity);
            
        } else {
            //chunk not cached
            chunks.Store(chunk_pos, new Chunk(this, chunk_pos));
            entity.SetPosition(new ChunkPosition(chunks.Request(chunk_pos), offset));
            entity.Initialize();
            entity.parent_universe = this;
            
            chunks.Cache[chunk_pos].item.SpawnEntity(entity);
        }
        
        entity.Initialized();
    }
    
    
    public void StartUpdating() {
        update_thread.Start();
    }

    public int chunks_updated = 0;

    internal void universe_task_callback() {
        Interlocked.Increment(ref chunks_updated);
    }

    public bool currently_stabilizing { get; set; } = false;
    public bool CurrentlyStabilizing => currently_stabilizing;
    
    public static string VisibilityString = "";
    private List<Vector3ui128> UpdateChunks = new();
    public Clock.UpdateThread update_thread { get; set; }
    
    public void Render(Camera camera, GBuffer gbuffer) {
        RenderUniverse(camera, gbuffer);
    }

    void Update() {
        VisibilityString = "";
        UpdateChunks.Clear();
        
        foreach (var c in Camera.AllCameras) {
            if (c.Value.current_camera_chunk == null) continue;
            
            var camera_chunk = c.Value.current_camera_chunk.index;
            UpdateChunks.Add(camera_chunk);
            
            foreach (var chunk in chunks.Cache.Values) {
                var dir = chunk.item.position - camera_chunk;
                var len = Vector3ui128.Distance(camera_chunk, chunk.item.position);
                
                if (len > 3) continue;
                
                //VisibilityString += $"{(len == 0 ? ">" : "")} {chunk.item.position.ToXString()} {dir.ToXString()} {len.ToString()}\n";
                UpdateChunks.Add(chunk.item.position);
            }
        }
        
        /*
        Threads.StartTaskBatch(
            chunks.Cache,
            c => c.Value.item.Update(),
            c => "ChunkUpdateBatch::" + c.Value.item.position.ToXString()
            );*/
        /*
        Threads.StartTaskBatch(
            UpdateChunks,
            c => chunks.Cache[c].item.Update(),
            c => "ChunkUpdateBatch::" + chunks.Cache[c].item.position.ToXString()
        );
        */
        foreach (var c in UpdateChunks) {
            //var dir = c.item.position - camera_chunk;
            //var len = dir.Length();
            
            //Threads.Request(chunk.chunk_update_packet);
            chunks.Cache[c].item.Update();
        }
        
        //while (State.CurrentlyRendering) ;
        foreach (var chunk in UpdateChunks) {
            lock (chunks.Cache[chunk].item.Entities) {
                foreach (var e in chunks.Cache[chunk].item.entity_moves) {
                    e.MakeChange();
                }
            }
        }
        
        currently_stabilizing = true;
        IAutoInterpolate.Manager.UpdateInternalLoop(Clock.update_thread_goal_time.TotalMilliseconds);
        
        foreach (var chunk in UpdateChunks) {
            
            lock (chunks.Cache[chunk].item.Entities) {
                
                foreach (var ent in chunks.Cache[chunk].item.Entities.Values) {
                    ent.StabilizeChunkPosition();
                }
            }
        }
        currently_stabilizing = false;
        //while (chunks_updated < chunks.Cache.Values.Count) ;
    }

    public void UpdateGraphics() {
        foreach (var c in chunks.Cache.Values) {
            var chunk = c.item;
            chunk.UpdateGraphics();
        }
        GBufferCamera.Manager.UpdateLinkedChunkPositions();
    }

    public void RenderUniverse(Camera camera, GBuffer gbuffer) {
        foreach (var c in Renderer.VisibleChunks) {
            var chunk = chunks.Cache[c.index].item;
            //check if camera can see chunk
            //if it can, find parts of octree that it can see
            //find objects in those octree parts and add their
            //relative offsets to a visibility list
            //or for now
            lock (chunk.Entities) {
                foreach (var e in chunk.Entities.Values) {
                    if (e.Components.HasComponentOfType<RenderModelStatic>(out var rm)) {
                        rm.DrawBasic(camera, gbuffer, c.offset);
                    }
                }
            }
        }
    }
    
    public void DebugDraw(Camera camera) {
        foreach (var chunk in chunks.Cache.Values) {
            var c = chunk.item;
        }
    }

    public void StabilizePositions() {}
    
    public List<EntityVisibilityInfo> BuildVisibilityList(Camera camera) {
        List<EntityVisibilityInfo> visible = new();
        
        foreach (var chunk in chunks.Cache.Values) {
            foreach (var ent in chunk.item.Entities.Values) {
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
    public ChunkCache() : base("ChunkCache", 
            chunk => !chunk.is_near_a_camera() && chunk.is_empty) {
        //empty chunks need to be pruned from the cache but also should not do this while
        //near a camera/player, as quickly moving between chunks could introduce stutters as they
        //are loaded and unloaded, hence the prune_rule
        
        
    }
}