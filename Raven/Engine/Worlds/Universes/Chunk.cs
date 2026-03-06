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
public class ObjectPosition {
    public Vector3 XYZ = Vector3.Zero;

    public float X {
        get => XYZ.X;
        set => XYZ.X = value;
    } 
    public float Y {
        get => XYZ.Y;
        set => XYZ.Y = value;
    } 
    public float Z {
        get => XYZ.Z;
        set => XYZ.Z = value;
    } 
    
    public Vector3 position_stable = Vector3.Zero;
    public Vector3 position_stable_previous = Vector3.Zero;
    public Vector3 position_interpolated = Vector3.Zero;
    
    private double current_time = 0.0;
    public double InterpolationCurrentTime => current_time;

    private double length;
    public double InterpolationLength => length;

    public float InterpolationPosition => (float)(current_time / length);
    
    public Vector3 wants_movement = Vector3.Zero;
    List<Vector3> finalized_movement_path = new List<Vector3>();
    Vector3 final_position => finalized_movement_path.Last();
    Vector3 previous_final_position;

    public Entity parent { get; private set; }
    
    public ObjectPosition(Entity parent) => this.parent = parent; 

    public ObjectPosition(Entity parent, Vector3 position) {
        this.parent = parent;
        this.Set(position);
    }

    public void Set(Vector3 position) {
        this.XYZ = position;
        this.position_stable = position;
        this.position_stable_previous = position;
        this.position_interpolated = position;
    }
    
    public static bool EnableInterpolation = true;
    
    public void interpolate(double step_milliseconds) {
        if (!EnableInterpolation) {
            position_interpolated = position_stable;
            return;
        }
        
        current_time += step_milliseconds;
        if (current_time > length) 
            current_time = length;

        position_interpolated = Vector3.Lerp(position_stable_previous, position_stable, InterpolationPosition);
    }
    
    public void stabilize(double frame_time) {
        position_stable_previous = position_stable;
        position_stable = XYZ;
        
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
    
    private double DELTA_MS => Scene.Manager.update_thread.delta_ms;
    private double DELTA => Scene.Manager.update_thread.delta_s;
    
    public void MoveAndSlide(Vector3 movement) {
        wants_movement = movement * (float)DELTA;
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
        XYZ += wants_movement;
        wants_movement = Vector3.Zero;
    }

    void move_until_collision() {
        XYZ += wants_movement;
        wants_movement = Vector3.Zero;
    }
    
    void move_directly() {
        XYZ += wants_movement;
        wants_movement = Vector3.Zero;
    }
    
    //public void MoveDirectlyToAbsoolute() {}
}


public class LinkedObjectPosition {
    public Vector3 child_offset_from_parent = Vector3.Zero;
    
    public ObjectPosition parent;
    ObjectPosition _child;
    public ObjectPosition child => _child;

    public LinkedObjectPosition() { }
    
    public Vector3 parent_to_child => child_offset_from_parent;
    public Vector3 child_to_parent => -child_offset_from_parent;
    
    public void Update() {
        _child = parent;
        _child.XYZ += child_offset_from_parent;
    }
}