using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ProjectRaven.Engine;

namespace ProjectRaven.Graphics.Drawing2D.Effects {
    internal class Draw3D : ManagedEffect {
        Matrix _world = Matrix.Identity;
        Matrix _view = Matrix.Identity;
        Matrix _projection = Matrix.Identity;

        Matrix world {
            get { return _world; }
            set { _world = value; set_param("world", value); }
        }
        Matrix view {
            get { return _view; }
            set { _view = value; set_param("view", value); }
        }
        Matrix projection {
            get { return _projection; }
            set { _projection = value; set_param("projection", value); }
        }

        Action update_action;

        internal override void update() { 
            if (update_action != null) update_action();
            base.update();
        }

        public Draw3D(ContentManager content) {
            load_shader_file(content, "effects/draw_3d");
            default_matrices();

            Manager.register_for_update(this);
        }

        void default_matrices() {
            set_param("world", Matrix.Identity);
            set_param("view", Matrix.Identity);
            set_param("projection", Matrix.Identity);
        }
    }


    public class ShadedQuad : ManagedEffect {
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

        public ShadedQuad(ContentManager content, string effect_name, Vector2i position, Vector2i size) : base() {
            _position = position;
            _size = size;

            load_shader_file(content, $"effects/{effect_name}");
            Manager.register_for_update(this);
        }

        public void draw_plane(Vector2i parent_resolution) {

            Vector2 pr = _position.ToVector2() / parent_resolution.ToVector2();
            Vector2 sr = _size.ToVector2() / parent_resolution.ToVector2();

            _world = 
                Matrix.CreateTranslation(1f, -1f, 0) 
                * Matrix.CreateScale(sr.X, sr.Y, 0) 
                * Matrix.CreateTranslation(-1f, 1f, 0) 
                * Matrix.CreateTranslation(pr.X, -pr.Y, 0);

            State.graphics_device.SetVertexBuffer(State.quad_vb);
            State.graphics_device.Indices = State.quad_ib;
            base.draw_buffers_basic_effect_first_pass(_world, Matrix.Identity, Matrix.Identity);
        }
    }

    public class ShadedQuadWVP : ManagedEffect {
    
        Matrix _world = Matrix.Identity;
        Matrix _view = Matrix.Identity;
        Matrix _projection = Matrix.Identity;

        Action update_action;

        Texture2D _texture;
        Texture2D texture { 
            get {
                return _texture;
            } set {
                _texture = value;
                set_param("tx", value);
            }
        }

        public Matrix world {
            get { return _world; }
            set { _world = value; set_param("world", value); }
        }
        public Matrix view {
            get { return _view; }
            set { _view = value; set_param("view", value); }
        }
        public Matrix projection {
            get { return _projection; }
            set { _projection = value; set_param("projection", value); }
        }

        void default_params() {
        }

        internal override void update() {
            if (update_action != null) update_action();
            base.update();
        }

        public ShadedQuadWVP(ContentManager content, string effect_name) : base() {
            if (State.quad_vb == null) {
                State.quad_vb = new VertexBuffer(State.graphics_device, VertexPositionTexture.VertexDeclaration, 4, BufferUsage.None);

                State.quad_vb.SetData(State.vb_data);
            }
            if (State.quad_ib == null) { 
                State.quad_ib = new IndexBuffer(State.graphics_device, IndexElementSize.ThirtyTwoBits, 6, BufferUsage.None);
                State.quad_ib.SetData(State.ib_data);
            }

            load_shader_file(content, $"effects/{effect_name}");
            default_params();

            Manager.register_for_update(this);
        }

        public void draw_plane() {
            base.draw_buffers_basic_effect_first_pass(State.quad_vb, State.quad_ib, _world, _view, _projection);
        }
    }


}
