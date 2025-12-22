using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using ProjectRaven.Caching;
using ProjectRaven.Engine.Controls;
using static ProjectRaven.Engine.State;

namespace ProjectRaven.Engine;

public class Universe {
    public Clock.UpdateThread update_thread = new Clock.UpdateThread();
    
    ConcurrentCache<Chunk> active_chunks = new ConcurrentCache<Chunk>();
    VirtualChunkOctree  octree = new  VirtualChunkOctree();
    
    public Universe() {
        update_thread.external_update = Update;
        active_chunks.prune_rule = active_chunk => !active_chunk.is_near_a_camera;
    }

    public void SpawnEntity(Entity entity) {
        
    }
    public void DespawnEntity(Entity entity) {
        
    }
    
    void Update() {
    }
    
}

public class VirtualChunkOctree {
    private UInt64 max_chunks_per_axis => UInt64.MaxValue;

    public void FindAncCacheAllChunksAround(Vector2ui64 chunk, Vector3i search_distance) {
        search_distance.X = Math.Abs(search_distance.X);
        search_distance.Y = Math.Abs(search_distance.Y);
        search_distance.Z = Math.Abs(search_distance.Z);
    }
}



/*
public class Map {
    private QuadTree quadtree;
    public List<Entity> entities_in_range = new();
    
    public Map() {
        quadtree = new QuadTree();
    }

    public void Update() {
        
    }
}

public class ChunkPosition {
    //TODO actually implement this system and make it work within quadtree
    public Vector2i64 index;
    public Vector3 offset;
}

public class QuadTree {
    public class Chunk(Vector2i64 index, Vector2i64 position, Vector2i64 size) {
        private Vector2i64 _xy_index = index;
        private Vector2i64 _position = position;
        private Vector2i64 _size = size;
        
        public Vector2i64 Index =>  _xy_index;
        public Vector2i64 Position => _position;
        public Vector2i64 Size => _size;

        public List<Entity> Entities { get; set; } = new List<Entity>();
    }
    
    private Chunk[,] _chunks;
    public Chunk[,] Chunks => _chunks;

    public QuadTree() {
        
    }
}

*/
