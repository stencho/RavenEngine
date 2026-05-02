using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Raven.Graphics.Drawing2D;
using Raven.Engine;

namespace Raven.Graphics.Effects {
    public class DrawModelDeferred : ManagedEffect {
        private Matrix _world;

        private Camera camera => Camera.current_render_camera;

        public DrawModelDeferred() : base(Resources.GetShader("draw_deferred")) {
            
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

            load_shader_file(content, effect_name);
            default_params();

            Manager.register_for_update(this);
        }

        public void draw_plane() {
            base.draw_buffers_basic_effect_first_pass(State.quad_vb, State.quad_ib, _world, _view, _projection);
        }
    }


}
