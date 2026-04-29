using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;

namespace Raven.Graphics.Effects.Forward;

public class QuadForward : ManagedEffect {
    Matrix _world = Matrix.Identity;

    Vector2i _position;
    Vector2i _size;

    public Vector2i position {
        get { return _position; } 
        set { _position = value; }
    }
    public Vector2i size {
        get { return _size; }
        set { _size = value; }
    }

    public QuadForward(ContentManager content, Vector2i position, Vector2i size) : base() {
        _position = position;
        _size = size;

        load_shader_file(content, "draw_forward");
        Manager.register_for_update(this);
    }

    public void draw_quad(Vector2i parent_resolution, Camera camera) {
        if (_effect == null) return;
        
        Vector2 pr = _position.ToVector2() / parent_resolution.ToVector2();
        Vector2 sr = _size.ToVector2() / parent_resolution.ToVector2();

        _world = 
            Matrix.CreateTranslation(1f, -1f, 0) 
            * Matrix.CreateScale(sr.X, sr.Y, 0) 
            * Matrix.CreateTranslation(-1f, 1f, 0) 
            * Matrix.CreateTranslation(pr.X, -pr.Y, 0);

        State.graphics_device.SetVertexBuffer(State.quad_vb);
        State.graphics_device.Indices = State.quad_ib;
        
        set_param("world", _world);
        set_param("view", camera.view);
        set_param("projection", camera.projection);
        
        apply_passes();

        State.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        
    }
}