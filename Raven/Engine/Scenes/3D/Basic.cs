using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Raven.Engine.Components;
using Raven.Graphics;
using Raven.Graphics.Drawing3D;

namespace Raven.Engine.Scene3D;

public class BasicScene : Scene {
    public SceneType scene_type => SceneType.Basic;
 
    internal ConcurrentDictionary<Guid, Entity> entities = new();
    
    ConcurrentQueue<Entity> spawn_list = new();
    ConcurrentQueue<Guid> kill_list = new ConcurrentQueue<Guid>();
    
    public string scene_info() {
        string s = $"[SCENE]\n[entities]";

        foreach (Entity e in entities.Values) {
            s += $"     > [{e.name}] -> (pos) {e.position.XYZ.ToXString()} (ipos) {e.position.position_interpolated.ToXString()}\n";
            s += e.Components.ListAllComponents(8);
        }
        
        return s + "\n\n";
    }

    
    public override void Save() {        
    }

    public override void Load() {
    }

    public override void Spawn(Entity entity) {
        entity.parent_scene = this;
        spawn_list.Enqueue(entity);
    }

    public override void Spawn(Entity entity, Vector3 position) {
        entity.parent_scene = this;
        entity.SetPosition(position);
        spawn_list.Enqueue(entity);
    }

    public override void Kill(Entity entity) {
        kill_list.Enqueue(entity.GUID);
    }

    public override void Update() {
        foreach (var ent in entities.Values) {
            ent.Update();
        }
        foreach (var ent in entities.Values) {
            ent.position.FinalizeMove();
        }
    }

    public override void UpdatePhysics() {
        throw new NotImplementedException();
    }

    public override void PostPhysics() {
        throw new NotImplementedException();
    }

    public override void UpdateGraphics() {
        foreach (var ent in entities.Values) {
            ent.UpdateInterpolatedPosition();
            ent.UpdateGraphics();
        }
        
        GBufferCamera.Manager.UpdateLinkedChunkPositions();
        Camera.Manager.UpdateAllCameras();
    }

    public override void Stabilize() {
        foreach (var ent in entities.Values) {
            ent.StabilizePosition();
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

    public override void BuildVisibilityLists(Camera camera) {
        foreach (var e in entities) {
            var key = e.Key; var ent = e.Value;
            render_list_deferred.Add(new EntityVisibility(ent, camera));
        }
    }
}