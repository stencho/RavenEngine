using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Raven.Console;

namespace Raven.Engine;
public class ChunkPosition {
    public Chunk chunk;
    
    public Vector3ui128 index => chunk.position;

    public Vector3 offset = Vector3.Zero;
        
    public Vector3 offset_stable = Vector3.Zero;
    public Vector3 offset_stable_previous = Vector3.Zero;
    public Vector3 offset_interpolated = Vector3.Zero;
    
    private double current_time = 0.0;
    public double InterpolationCurrentTime => current_time;

    private double length;
    public double InterpolationLength => length;

    public float InterpolationPosition => (float)(current_time / length);
    
    public Vector3 wants_movement = Vector3.Zero;
    List<Vector3> finalized_movement_path = new List<Vector3>();
    Vector3 final_position => finalized_movement_path.Last();
    Vector3 previous_final_position;
    
    public ChunkPosition(Chunk chunk) { this.chunk = chunk; }
    public ChunkPosition(Chunk chunk, Vector3 offset) { this.chunk = chunk; SetOffset(offset); }

    public ChunkPosition(Universe u, Vector3ui128 index, Vector3 offset) {
        if (u.chunks.Test(index)) {
            //chunk in cache
            this.chunk = u.chunks.Request(index);
        } else {
            u.chunks.Store(index, new Chunk(u, index));
            this.chunk = u.chunks.Request(index);
        }
        SetOffset(offset);
    }

    public void SetOffset(Vector3 offset) {
        this.offset = offset;
        this.offset_stable = offset;
        this.offset_stable_previous = offset;
        this.offset_interpolated = offset;
    }
    
    public static bool EnableInterpolation = true;
    
    public static float MeasureAbsoluteDistance(Vector3ui128 chunk_a, Vector3 offset_a, Vector3ui128 chunk_b, Vector3 offset_b, out Vector3 AB) {
        var chunk_dist = Vector3ui128.DistanceF(chunk_a, chunk_b);
        var direction = Vector3ui128.NormalizedDirection(chunk_a, chunk_b);
        var A = offset_a;
        var B = (chunk_dist * direction) + offset_b;
        AB = B - A;
        return AB.Length();
    }
    
    public void interpolate(double step_milliseconds) {
        if (!EnableInterpolation) {
            offset_interpolated = offset_stable;
            return;
        }
        
        current_time += step_milliseconds;
        if (current_time > length) 
            current_time = length;

        offset_interpolated = Vector3.Lerp(offset_stable_previous, offset_stable, InterpolationPosition);
    }
    
    public void stabilize(double frame_time) {
        offset_stable_previous = offset_stable;
        offset_stable = offset;
        
        current_time = 0.0;
        length = frame_time;
    }
    

    public Action chunk_changed;
    public Action movement_finalized;

    public enum MoveStyle {
        MoveAndSlide,
        MoveUntilCollision,
        Direct
    }
    
    MoveStyle current_move_style = MoveStyle.Direct;
    
    public void MoveAndSlide(Vector3 movement) {
        wants_movement = movement * (float)Clock.update_thread_delta;
        current_move_style = MoveStyle.MoveAndSlide;
    }

    public void MoveUntilCollision(Vector3 movement) {
        wants_movement = movement;
        current_move_style = MoveStyle.MoveUntilCollision;
    }

    public void MoveDirectly(Vector3 movement) {
        wants_movement = movement;
        current_move_style = MoveStyle.Direct;
    }

    internal void FinalizeMove() {
        if (wants_movement != Vector3.Zero) {
            switch (current_move_style) {
                case MoveStyle.MoveAndSlide:
                    move_and_slide();
                    break;
                case MoveStyle.MoveUntilCollision:
                    move_until_collision();
                    break;
                case MoveStyle.Direct:
                    move_directly();
                    break;
            }
        }
        
        movement_finalized?.Invoke();
    }
    
    void move_and_slide() {
        offset += wants_movement;
        wants_movement = Vector3.Zero;
    }

    void move_until_collision() {
        offset += wants_movement;
        wants_movement = Vector3.Zero;
    }
    
    void move_directly() {
        offset += wants_movement;
        wants_movement = Vector3.Zero;
    }
    
    //public void MoveDirectlyToAbsoolute() {}
}

public class LinkedChunkPosition {
    public Vector3ui128 chunk_parent => parent.index;
    public Vector3ui128 chunk_child => child.index;

    public Vector3 child_offset_from_parent = Vector3.Zero;

    public ChunkPosition parent;
    ChunkPosition _child;

    public LinkedChunkPosition() { }

    public ChunkPosition child => _child;

    public Vector3 parent_to_child => child_offset_from_parent;
    public Vector3 child_to_parent => -child_offset_from_parent;
    
    public void Update() {
        _child = parent;
        _child.offset += child_offset_from_parent;
        //TODO check if child is now in new chunk
    }
}

public class Chunk {
    public const int base_chunk_size_per_direction = 55;

    static double WrapSymmetric(double x, double range)
    {
        double width = range * 2.0;
        return x - width * Math.Floor((x + range) / width);
    }
    static float WrapSymmetric(float x, float range)
    {
        float width = range * 2f;
        return x - width * MathF.Floor((x + range) / width);
    }
    
    public static bool check_if_entity_has_switched_chunks(Universe universe, Entity entity, out Vector3ui128 new_chunk, out Vector3 new_offset) {
        var cp = entity.position;
        int x, y, z;
        var offset = entity.position.offset;
        
        new_offset = offset;
        bool changedX = false, changedY = false, changedZ = false;
        
        if (cp.offset.X >= base_chunk_size_per_direction) {
            if (cp.index.X < UInt128.MaxValue) {
                x = (int)(cp.offset.X / base_chunk_size_per_direction);
                new_offset.X = -base_chunk_size_per_direction + (offset.X % base_chunk_size_per_direction);
            } else {
                x = 0;
                new_offset.X = base_chunk_size_per_direction - 0.5f;
            }
            changedX = true;
        } else if (cp.offset.X < -base_chunk_size_per_direction) {
            if (cp.index.X > 0) {
                x = -(int)(Math.Abs(cp.offset.X) / base_chunk_size_per_direction);
                new_offset.X = base_chunk_size_per_direction - (Math.Abs(offset.X) % base_chunk_size_per_direction);
            } else {
                x = 0;
                new_offset.X = -base_chunk_size_per_direction + 0.5f;
            }
            changedX = true;
        } else x = 0;
        
        if (cp.offset.Y >= base_chunk_size_per_direction) {
            if (cp.index.X < UInt128.MaxValue) {
                y = (int)(cp.offset.Y / base_chunk_size_per_direction);
                new_offset.Y = -base_chunk_size_per_direction + (offset.Y % base_chunk_size_per_direction);
            } else {
                y = 0;
                new_offset.Y = base_chunk_size_per_direction- 0.5f;
            }
            changedY = true;
        } else if (cp.offset.Y < -base_chunk_size_per_direction) {
            if (cp.index.Y > 0) {
                y = -(int)(Math.Abs(offset.Y) / base_chunk_size_per_direction);
                new_offset.Y = base_chunk_size_per_direction - (Math.Abs(offset.Y) % base_chunk_size_per_direction);
            } else {
                y = 0;
                new_offset.Y = -base_chunk_size_per_direction + 0.5f;
            }
            changedY = true;
        } else y = 0;
        
        if (cp.offset.Z >= base_chunk_size_per_direction) {
            if (cp.index.X < UInt128.MaxValue) {
                z = (int)(cp.offset.Z / base_chunk_size_per_direction);
                new_offset.Z = -base_chunk_size_per_direction + (offset.Z % base_chunk_size_per_direction);
            } else {
                z = 0;
                new_offset.Z = base_chunk_size_per_direction- 0.5f;
            }
            changedZ = true;
        } else if (cp.offset.Z < -base_chunk_size_per_direction) {
            if (cp.index.Z > 0) {
                z = -(int)(Math.Abs(cp.offset.Z) / base_chunk_size_per_direction);
                new_offset.Z = base_chunk_size_per_direction - (Math.Abs(offset.Z) % base_chunk_size_per_direction);
            } else {
                z = 0;
                new_offset.Z = -base_chunk_size_per_direction + 0.5f;
            }
            changedZ = true;
        } else z = 0;
        
        new_chunk = new Vector3ui128(cp.index.X.Plus(x), cp.index.Y.Plus(y), cp.index.Z.Plus(z));

        bool changed_any = changedX || changedY || changedZ;

        if (entity.Components.HasComponent("GBufferCamera") && changed_any) {
            Log.log($"{cp.index.ToXString()} -> {new_chunk.ToXString()} || {offset.ToXString()} -> {new_offset.ToXString()} ");
        }
        
        return changed_any;
    }

    
    public bool is_near_a_camera() {
        //Camera.Manager.Test()
        return true;
    }
    
    public bool is_empty => Entities.Count == 0;
    
    private Universe parent;
    
    private Vector3ui128 _pos = Vector3ui128.Zero;  
    private Vector3ui128 TopLeft = Vector3ui128.Zero;
    
    public Vector3ui128 position => _pos;
    
    public Vector3 size => base_chunk_size_per_direction * Vector3.One * 2;

    public ConcurrentDictionary<string, Entity> Entities { get; set; } = new();

    public Threads.ThreadRequestPacket chunk_update_packet;
    
    public Chunk(Universe parent, Vector3ui128 pos) {
        this.parent = parent;
        _pos = pos;
        chunk_update_packet = new Threads.ThreadRequestPacket(
            Update, parent.universe_task_callback
            );
    }

    public void SpawnEntity(Entity entity) {
        Entities.TryAdd(entity.name, entity);
    }

    public int entities_updated = 0;

    internal void task_callback() => Interlocked.Increment(ref entities_updated);
    private TimeSpan update_thread_spawner_wait_delay = new TimeSpan(100);

    public class EntityChunkChangeInfo(Universe universe, Entity universeEntity, Vector3ui128 old_pos, Vector3ui128 new_pos, Vector3 new_offset) {
        public Vector3ui128 NewPos => new_pos;
        public Vector3ui128 OldPos => old_pos;
        public Entity UniverseEntity => universeEntity;

        public void MakeChange() {
            //universe.chunks.Cache[OldPos].item.Entities.TryRemove(entity); 
            universeEntity.SetPosition(new ChunkPosition(universe, new_pos, new_offset));
            
        }
    }

    public void AddEntityToChunk(Universe universe, Vector3ui128 chunk_id, Entity entity) {
        universe.chunks.Cache[chunk_id].item.Entities.TryAdd(entity.name, entity); 
        
    }
    
    internal List<EntityChunkChangeInfo> entity_moves = new();
    
    public void Update() {
        
        /*
        Threads.StartTaskBatch(
            Entities, 
            ent => ent.Update(),
            ent => "EntityUpdateBatch::" + ent.name
        );
        */
        
        foreach (var e in Entities.Values) {
            e.Update();
        }

        //movement/collision solver needs to happen at this point
        var new_index = Vector3ui128.Zero;
        var old_index = Vector3ui128.Zero;
        var new_offset = Vector3.Zero;
        
        entity_moves.Clear();
        
        //this should be separated and threaded
        foreach (var e in Entities.Values) {
            e.position.FinalizeMove();
            
            old_index = e.position.index;
            
            if (check_if_entity_has_switched_chunks(parent, e, out new_index, out new_offset)) {
                //this should be wrapped properly cos as of right now, moving more than one chunk at once will fuck up your offset
                entity_moves.Add(new EntityChunkChangeInfo(parent, e, old_index, new_index, new_offset));
            }
        }
        
        parent.universe_task_callback();
    }
    
    public void UpdateGraphics() {
        foreach (var e in Entities.Values) {
            e.UpdateInterpolatedPosition();
            e.UpdateGraphics();
        }
    }
}