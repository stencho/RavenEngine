using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven;
using Raven.Engine;
using Raven.Graphics.Drawing2D;
using Raven.UI;

namespace RavenEdit;

public class Canvas {
    public FullResolutionRenderTarget render_target = new FullResolutionRenderTarget();
    
    public Canvas() {}

    public void Update() {
        
    }
    
    public void Draw() {
        State.graphics_device.SetRenderTarget(render_target.rt2D);
        State.graphics_device.Clear(Color.Transparent);
        
        Draw2D.fill_rect_dither(Vector2i.Zero, State.resolution, 
                UIColors.MiddleGrey.multiply_color(0.9f),
                UIColors.MiddleGrey,
                16
            );
    }
}