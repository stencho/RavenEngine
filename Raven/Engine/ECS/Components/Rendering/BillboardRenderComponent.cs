using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Engine.Collision;

namespace Raven.Graphics.Drawing3D;
//make into an entity??? it should probably be an entity.
//I should maybe follow my own engine design patterns

[HashSetManaged]
public partial class Billboard : Component {
    public static partial class Manager {
        public static void draw_all_billboards() {
            foreach (Billboard b in billboards) {
                b.draw_target();
            }
        }    
    }

    public override ComponentFlags flags => ComponentFlags.Render;
    
    public RenderTarget2D render_target;
    
    public Action<RenderTarget2D>? draw_to_render_target;

    public Billboard() {
        
    }
    
    public Billboard(Matrix orientation, Vector2i texture_resolution, bool always_face_camera, Action<RenderTarget2D> draw_to_render_target) {
        render_target = RenderTargetEx.create(texture_resolution);
    }
    
    internal void draw_target() {
        render_target.use();
        render_target.clear();
        render_target.draw_to(render_target => draw_to_render_target?.Invoke(render_target));
    }
    
    public override void RenderZPrepass(Camera camera) {}
    public override void RenderDeferred(Camera camera) {}
    public override void RenderForward(Camera camera) {}

    public override Shape3D GetShape() {
        throw new NotImplementedException();
    }

    public override collision_result? TestGJKAgainstShape(Shape3D test_shape, Matrix world) {
        throw new NotImplementedException();
    }

    public override BoundingBox? GetBounds() {
        throw new NotImplementedException();
    }
}