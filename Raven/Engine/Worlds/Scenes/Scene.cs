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

[GuidManaged]
public partial class Scene {
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
            
            foreach (Scene scene in scenes.Values) {
                if (scene.always_update && scene.GUID != active_scene_id) {
                    scene.Update();
                }
            }
            
            while (State.using_scene == (int)State.SceneUseState.RENDER) { Thread.SpinWait(1); }
            
            Interlocked.Exchange(ref State.using_scene, (int)State.SceneUseState.STABILIZING);
            if (ActiveScene != null) ActiveScene?.Stabilize();
            
            foreach (Scene scene in scenes.Values) {
                if (scene.always_update && scene.GUID != active_scene_id) {
                    scene.Stabilize();
                }
            }
            
            IAutoInterpolate.Manager.UpdateInternalLoop(Manager.update_thread.fixed_timestep ? Manager.update_thread.goal_time : Manager.update_thread.delta_ms);
            Interlocked.Exchange(ref State.using_scene, (int)State.SceneUseState.NONE);
        }

        public static void UpdateGraphics() {
            foreach (Scene scene in scenes.Values) {
                if (scene.always_update && scene.GUID != active_scene_id) {
                    scene.UpdateGraphics();
                }
            }
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
    
    internal ConcurrentDictionary<Guid, Entity> entities = new();
    
    ConcurrentQueue<Entity> spawn_list = new();
    ConcurrentQueue<Guid> kill_list = new ConcurrentQueue<Guid>();
    
    //Lock entity_lock = new Lock();
    
    SceneOctree octree;

    public string scene_info() {
        string s = $"[SCENE]\n[entities]";

        foreach (Entity e in entities.Values) {
            s += $"     > [{e.name}] -> (pos) {e.position.XYZ.ToXString()} (ipos) {e.position.position_interpolated.ToXString()}\n";
            s += e.Components.ListAllComponents(8);
        }
        
        return s + "\n\n";
    }
    
    public Scene() {
        Manager.Add(this);
    }
    
    ~Scene() {
        Manager.Remove(GUID);
    }
    
    public void Spawn(Entity entity) {
        entity.parent_scene = this;
        spawn_list.Enqueue(entity);
    }
    public void Spawn(Entity entity, Vector3 position) {
        entity.parent_scene = this;
        entity.SetPosition(position);
        spawn_list.Enqueue(entity);
    }

    public void Kill(Entity entity) {
        kill_list.Enqueue(entity.GUID);
    }
    
    public void Update() {
        foreach (var ent in entities.Values) {
            ent.Update();
        }
        foreach (var ent in entities.Values) {
            ent.position.FinalizeMove();
        }
    }
    
    public void UpdateGraphics() {
        foreach (var ent in entities.Values) {
            ent.UpdateInterpolatedPosition();
            ent.UpdateGraphics();
        }
        GBufferCamera.Manager.UpdateLinkedChunkPositions();
    }
    
    public void Stabilize() {
        foreach (var ent in entities.Values) {
            ent.StabilizeChunkPosition();
        }

        foreach (var g in kill_list) { entities.Remove(g, out _); }

        foreach (var ent in spawn_list) {
            entities.TryAdd(ent.GUID, ent);
            ent.Initialize();
            ent.Initialized();
        }
        
        spawn_list.Clear();
        kill_list.Clear();
    }
    
    public void Render(Camera camera, GBuffer gbuffer) {
        Draw3D.batch_draw_setup(camera,gbuffer);
        foreach (var e in entities.Values) {
            if (e.Components.HasComponentOfType<RenderModelStatic>(out var rm)) {
                rm.DrawBasic(camera, gbuffer);
            }
        }
    }
}