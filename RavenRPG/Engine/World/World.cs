using System.Collections.Generic;

namespace RavenRPG.Engine.World;

public class World {
    
}

public class Map {
    private QuadTree quadtree;

    public Map() {
        quadtree = new QuadTree();
    }
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