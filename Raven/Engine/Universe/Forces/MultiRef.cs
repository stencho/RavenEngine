using Raven.Engine;
using Raven.Engine.Universes;

namespace RavenRPG.Engine.Universe.Forces;

public unsafe class MultiSceneEntityChunkRef {
    public Chunk* chunk;
    public IScene* scene;
    public Entity* entity;
}

public unsafe class MultiCameraChunkRef {
    public Camera* camera;
    public Chunk* chunk;
}

public unsafe class CameraEntityViewListReference {
    public Camera* camera;
    public Entity* entity;
}