using Microsoft.Xna.Framework;
using Raven.Engine;

namespace Raven.Graphics;


public struct EntityVisibilityInfo {
    public Camera camera;
    public ChunkPosition camera_chunk_position => camera.current_camera_chunk;
    
    public Entity entity;
    public ChunkPosition entity_chunk_position => entity.position;

    public Vector3 entity_camera_offset;
    public float entity_camera_distance;

    public EntityVisibilityInfo(Entity entity, Camera camera) {}
    
    public void FindOffsets() {
        
    }
}