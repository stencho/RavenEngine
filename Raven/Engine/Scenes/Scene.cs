#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Microsoft.Xna.Framework;
using Raven.Engine.Components;
using Raven.Graphics;
using Raven.Graphics.Drawing3D;
using Raven.Graphics.InterpolatedTypes;

namespace Raven.Engine;

public abstract class ScenePositionInfo() {}

[GuidManaged]
public abstract partial class Scene {
    public enum SceneType {
        Basic,
        BSP, TilingBSP,
        ChunkedXZ, ChunkedXYZ,
        
        Basic2D, Room2D, Chunk2D
    }
    
    public SceneType scene_type { get; }
    
    public static partial class Manager {
        private static Guid active_scene_id;
        public static Guid ActiveSceneGuid => active_scene_id;
        public static Scene? ActiveScene => Guid.Empty != ActiveSceneGuid ? scenes[ActiveSceneGuid] : null;
        
        public static Clock.UpdateThread update_thread;
        public static void StartUpdateThread() {
            update_thread = new Clock.UpdateThread("Update", Update);
            update_thread.Start();
        }
        
        public static void SetActiveScene(Guid scene_guid) {
            active_scene_id = scene_guid;
        }
        
        public static void Update() {
            if (ActiveScene != null) ActiveScene?.Update();
            
            GameSystem.Manager.AllUpdate();
            
            foreach (Scene scene in scenes.Values) {
                if (scene.always_update && scene.GUID != active_scene_id) {
                    scene.Update();
                }
            }
            
            while (true) {
                var current = State.using_scene;
                if (current == State.SceneUseState.RENDER) continue;
            
                if (Interlocked.CompareExchange(ref State.using_scene, State.SceneUseState.STABILIZING,current) == current) {
                    break;
                } 
            }
            
            if (ActiveScene != null) ActiveScene?.Stabilize();
            foreach (Scene scene in scenes.Values) {
                if (scene.always_update && scene.GUID != active_scene_id) {
                    scene.Stabilize();
                }
            }
            
            IAutoInterpolate.Manager.UpdateInternalLoop(Manager.update_thread.fixed_timestep ? Manager.update_thread.goal_time : Manager.update_thread.delta_ms);
            
            GameSystem.Manager.AllUpdateEndOfFrame();
            
            Interlocked.Exchange(ref State.using_scene, State.SceneUseState.NONE);
        }

        public static void UpdateGraphics() {
            GameSystem.Manager.AllUpdateGraphics();
            
            foreach (Scene scene in scenes.Values) {
                if ((scene.always_update && scene.GUID != active_scene_id) || scene.GUID == active_scene_id) {
                    scene.UpdateGraphics();
                }
            }
            
            GameSystem.Manager.AllUpdateGraphicsEndOfFrame();
        }
    }

    public static Guid? ActiveSceneGuid => Manager.ActiveSceneGuid;
    public static Scene? ActiveScene => Manager.ActiveScene;
    
    public static void SetActiveScene(Scene scene) {
        Manager.SetActiveScene(scene.GUID);
    }

    public static void SetActiveScene(Guid scene_guid) {
        Manager.SetActiveScene(scene_guid);
    }
    
    public bool always_update { get; set; }
    public string VisibilityString { get; set; }
    
    public Scene() {
        Manager.Add(this);
    }
    
    ~Scene() {
        Manager.Remove(GUID);
    }

    public abstract void Save();
    public abstract void Load();

    public abstract void Spawn(Entity entity);
    public abstract void Spawn(Entity entity, Vector3 position);

    public abstract void Kill(Entity entity);

    public abstract void Update();
    public abstract void UpdatePhysics();
    public abstract void PostPhysics();
    
    public abstract void UpdateGraphics();
    public abstract void Stabilize();

    internal List<EntityVisibility> render_list_deferred = new();
    internal List<EntityVisibility> render_list_forward = new();
    
    internal void ClearVisibilityLists() {
        render_list_deferred.Clear();
        render_list_forward.Clear();
    }
    public abstract void BuildVisibilityLists(Camera camera);
}

public class EntityVisibility(Entity entity_obj, Camera camera_obj) {
    public Guid entity_id => entity_obj.GUID;
    public Entity entity => entity_obj;
    
    public Guid camera_id => camera_obj.GUID;
    public Camera camera => camera_obj;
    
    public float distance => Vector3.Distance(camera_obj.position, entity_obj.position.XYZ);
}

