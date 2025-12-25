using System.Collections.Generic;
using Raven.Engine.Components;
using Raven.Graphics.Drawing3D;

namespace Raven.Engine;

public partial class TestEntity : Entity {
    public string name { get; set; } = "TestEntity";
    
    public ChunkPosition position { get; set; }
    public ChunkPosition position_stable { get; set; }
    
    public ComponentManager Components { get; set; }
    
    public List<DynamicLight> lights { get; set; }= new();
    
    public Universe parent { get; set; }
    public Threads.ThreadRequestPacket update_packet { get; set; }

    public TestEntity(Universe universe) {
        parent = universe;
        Components = new ComponentManager(this);
        Components.AddComponent(new RenderModelStatic("cube", "smugdean"));
        
        update_packet_init();
    }

    public void Initialized() {
    }

    public void Update() {
    }

    public void AfterCollision() {
    }

    public void UpdateGraphics() {
    }

}