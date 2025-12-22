using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Raven.Engine.Universes;

public interface IScene {
    
    bool is_visible(Camera camera);
    void draw();
    void update();
}

public class BasicListScene : IScene {
    public BoundingBox AABB { get; set; }

    public ConcurrentBag<Entity> entities { get; set; } = new();
    public ConcurrentQueue<Entity> visibility_list { get; set; } = new();

    private void build_visibility_list(Camera camera) {
        
    }

    public bool is_visible(Camera camera) {
        return true;
    }

    public void draw() {
        
    }

    public void update() {
        
    }
}