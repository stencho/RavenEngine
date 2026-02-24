using Raven.Graphics;

namespace Raven.Engine.Worlds;

public interface IWorld {
    public bool currently_stabilizing { get; set; }
    public Clock.UpdateThread update_thread { get; set; }
    
    public void Update();
    public void StabilizePositions();
    public void Render(Camera camera, GBuffer gbuffer);
}