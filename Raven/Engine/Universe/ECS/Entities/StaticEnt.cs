using System;
using System.Collections.Generic;
using Raven.Graphics.Drawing2D;
using Raven.Graphics.Drawing3D;

namespace Raven.Engine;

public partial class StaticEnt : Entity {
    public string name { get; set; } = "StaticEntity";
    
    public ChunkPosition position { get; set; }
    public ChunkPosition position_stable { get; set; }
    
    public ComponentManager Components { get; set; } 
    
    public Universe parent { get; set; }
    public Threads.ThreadRequestPacket update_packet { get; set; }

    public StaticEnt(Universe universe) {
        Components = new ComponentManager(this);
        parent = universe;
        
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

    //public Action<Entity> Draw2D { get; set; }
}