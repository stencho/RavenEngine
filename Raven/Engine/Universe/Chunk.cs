using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using RavenRPG.Engine.Universe;
using RavenRPG.Engine.Universe.SpacialPartitioning;

namespace Raven.Engine;
public class ChunkPosition {
    public Chunk chunk;
    
    public Vector3ui128 index => chunk.position;

    public Vector3 offset = Vector3.One * (Chunk.base_chunk_size_per_direction * 0.5f);
        
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
    public ChunkPosition(Chunk chunk, Vector3 offset) { this.chunk = chunk; this.offset = offset; }

    public static bool EnableInterpolation = true;
    
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
    
    public bool will_be_out_of_current_chunk() {
        if (MathF.Abs(offset.X + final_position.X) > Chunk.base_chunk_size_per_direction) return true;
        if (MathF.Abs(offset.Y + final_position.Y) > Chunk.base_chunk_size_per_direction) return true;
        if (MathF.Abs(offset.Z + final_position.Z) > Chunk.base_chunk_size_per_direction) return true;
        return false;
    }

    public Vector3ui128 find_chunk_move_direction() {
        Vector3ui128 chunk_move = new Vector3ui128(
            offset.X + final_position.X / Chunk.base_chunk_size_per_direction,
            offset.Y + final_position.Y / Chunk.base_chunk_size_per_direction,
            offset.Z + final_position.Z / Chunk.base_chunk_size_per_direction
        );
        
        return chunk_move;
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
    public const float base_chunk_size_per_direction = 500f;
    
    public bool is_near_a_camera() {
        //Camera.CameraTracker.
        return true;
    }
    
    public bool is_empty => Entities.Count == 0;
    
    private Universe parent;
    
    private Vector3ui128 _pos = Vector3ui128.Zero;  
    private Vector3ui128 TopLeft = Vector3ui128.Zero;
    
    public Vector3ui128 position => _pos;
    
    public Vector3 size => base_chunk_size_per_direction * Vector3.One * 2;

    private Octree<Entity> octree;

    public List<Entity> Entities { get; set; } = new();

    public Threads.ThreadRequestPacket chunk_update_packet;
    
    public Chunk(Universe parent, Vector3ui128 pos) {
        this.parent = parent;
        _pos = pos;
        chunk_update_packet = new Threads.ThreadRequestPacket(
            Update, parent.universe_task_callback
            );
    }

    public void SpawnEntity(Entity entity) {
        Entities.Add(entity);
    }

    public int entities_updated = 0;

    internal void task_callback() => Interlocked.Increment(ref entities_updated);
    private TimeSpan update_thread_spawner_wait_delay = new TimeSpan(100);
    
    public void Update() {
        
        /*
        Threads.StartTaskBatch(
            Entities, 
            ent => ent.Update(),
            ent => "EntityUpdateBatch::" + ent.name
        );
        */
        
        foreach (var e in Entities) {
            e.Update();
        }

        //movement/collision solver needs to happen at this point
        
        //this should be separated and threaded
        foreach (var e in Entities) {
            e.position.FinalizeMove();
        }
        
        parent.universe_task_callback();
    }
    
    public void UpdateGraphics() {
        foreach (var e in Entities) {
            e.UpdateInterpolatedPosition();
            e.UpdateGraphics();
        }
    }
}