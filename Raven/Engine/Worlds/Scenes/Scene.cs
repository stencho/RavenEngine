#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Microsoft.Xna.Framework;
using Raven.Graphics;
using Raven.Graphics.InterpolatedTypes;

namespace Raven.Engine.Worlds;

[GuidManaged]
public partial class Scene : IWorld {
    public static partial class Manager {
        public static Guid ActiveSceneGuid;
        public static Scene? ActiveScene => scenes[ActiveSceneGuid];
    }

    public bool currently_stabilizing { get; set; }
    public Clock.UpdateThread update_thread { get; set; }
    
    ConcurrentDictionary<Guid, Entity> entities = new();
    
    ConcurrentQueue<Entity> spawn_list = new();
    ConcurrentQueue<Guid> kill_list = new ConcurrentQueue<Guid>();
    
    Lock entity_lock = new Lock();
    
    SceneOctree octree;

    public Scene() {
        Manager.Add(this);
        update_thread = new Clock.UpdateThread("Update", Update);
    }
    
    ~Scene() {
        Manager.Remove(GUID);
    }
    
    public void Spawn(Entity entity) {
        var se = entity as Entity;
        spawn_list.Enqueue(se);
    }

    

    public void Update() {
        foreach (var ent in entities.Values) {
            ent.Update();
        }
    }

    public void StabilizePositions() {
        currently_stabilizing = true;
        
        IAutoInterpolate.Manager.UpdateInternalLoop(Clock.update_thread_goal_time.TotalMilliseconds);
                 
        foreach (var ent in entities.Values) {
            ent.StabilizeChunkPosition();
        }

        foreach (var g in kill_list) { entities.Remove(g, out _); }
        //foreach (var ent in spawn_list) entities.TryAdd(ent.GUID, ent);
        
        currently_stabilizing = true;
    }
    
    public void Render(Camera camera, GBuffer gbuffer) {
        
    }
}