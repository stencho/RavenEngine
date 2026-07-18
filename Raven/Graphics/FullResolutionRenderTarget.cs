using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;

namespace Raven.Graphics.Drawing2D;

public class FullResolutionRenderTarget {
    public RenderTarget2D rt2D;
    
    public FullResolutionRenderTarget() {
        create();
        State.resolution_changed += create;
    }

    void create() {
        rt2D = null;
        rt2D = RenderTargetEx.create(State.resolution.X, State.resolution.Y);
    }
}