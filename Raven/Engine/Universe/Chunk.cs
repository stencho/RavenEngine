using System.Collections.Concurrent;
using Microsoft.Xna.Framework;
using RavenRPG.Engine.Universe;

namespace Raven.Engine;

public class ChunkPosition {
    public Vector3ui64 index => chunk.position;
    public Vector3 offset = Vector3.One * (Chunk.base_chunk_size * 0.5f);
    
    public Chunk? chunk;
    
    public ChunkPosition(Chunk chunk) {this.chunk = chunk;}
    public ChunkPosition(Chunk chunk, Vector3 offset) {this.chunk = chunk; this.offset = offset;}

    public Vector3 wants_movement = Vector3.Zero;
    
    public void Move(Vector3 movement) {
        wants_movement = movement;
    }

    public void MoveDirectlyTo() {
        
    }
    public void MoveDirectlyToAbsoolute() {
        
    }
}

public class Chunk {
    public bool is_near_a_camera = false;
    public bool is_empty = false;
    
    public const float base_chunk_size = 5000f;
    
    private Universe parent;
    
    private Vector3ui64 _pos = Vector3ui64.Zero;  
    private Vector3ui64 TopLeft = Vector3ui64.Zero;
    
    public Vector3ui64 position => _pos;
    
    public Vector3 size => base_chunk_size * Vector3.One;

    private Octree<Entity> octree;
    
    public ConcurrentBag<Entity> Entities { get; }

    public Chunk(Universe parent, Vector3ui64 pos) {
        this.parent = parent;
        _pos = pos;
    }
}