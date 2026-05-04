using System;
using System.Collections.Generic;
using System.Linq;
using CSScripting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Graphics.Drawing2D;

namespace Raven.Graphics.Effects {

    public partial class ManagedEffect {
        public static class Manager {
            static HashSet<ManagedEffect> registered_effects_update = new HashSet<ManagedEffect>();

            public static void register_for_update(ManagedEffect effect) => registered_effects_update.Add(effect);            
            public static void unregister_for_update(ManagedEffect effect) => registered_effects_update.Remove(effect);

            public static void do_updates() {
                foreach (var effect in registered_effects_update) {
                    effect.update();
                }
            }
        }

        static BasicEffect basic_effect;

        public Effect effect => _effect;
        internal Effect _effect;

        public bool throw_error_on_bad_param { get; set; } = false;

        public Matrix basic_effect_world { get { return basic_effect.World; } set { basic_effect.World = value; } }
        public Matrix basic_effect_view { get { return basic_effect.View; } set { basic_effect.View = value; } }
        public Matrix basic_effect_projection { get { return basic_effect.Projection; } set { basic_effect.Projection = value; } }

        public ManagedEffect() {
            build_basic_effect();
            Manager.register_for_update(this);
        }
        public ManagedEffect(Effect effect) {
            _effect = effect;
            build_basic_effect();
            Manager.register_for_update(this);
        }
        public ManagedEffect(ContentManager content, string effect_name) {
            load_shader_file(content, effect_name);
            build_basic_effect();
            Manager.register_for_update(this);
        }

        ~ManagedEffect() {
            Manager.unregister_for_update(this);
        }
        
        void build_basic_effect() {
            if (basic_effect == null) {
                basic_effect = new BasicEffect(State.graphics_device);

                basic_effect.TextureEnabled = true;
                basic_effect.Texture = Draw2D.OnePXWhite;
            }
        }

        /// <summary>
        /// Used by the Manager class in its update loop
        /// </summary>
        internal virtual void update() { }

        internal void load_shader_file(ContentManager content, string effect_name) {
            _effect = Resources.GetShaderInstance(effect_name);
        }        

        internal bool shader_has_param(string param) {
            if (_effect == null) return false;

            foreach (EffectParameter parameter in _effect.Parameters) 
                if (parameter.Name == param) return true;
            
            return false;
        }

        public void set_param<T>(string param, T value) {
            if (value == null || _effect == null || !shader_has_param(param)) {
                if (throw_error_on_bad_param) throw new Exception("Bad shader param: " + param);
                else return;
            }

            var t = typeof(T); var obj = (object)value;
            if (t == typeof(bool)) _effect.Parameters[param].SetValue((bool)obj);
            else if (t == typeof(int)) _effect.Parameters[param].SetValue((int)obj);
            else if (t == typeof(int[])) _effect.Parameters[param].SetValue((int[])obj);
            else if (t == typeof(float)) _effect.Parameters[param].SetValue((float)obj);
            else if (t == typeof(float[])) _effect.Parameters[param].SetValue((float[])obj);
            else if (t == typeof(Vector2i)) _effect.Parameters[param].SetValue(((Vector2i)obj).ToVector2());
            else if (t == typeof(Vector2)) _effect.Parameters[param].SetValue((Vector2)obj);
            else if (t == typeof(Point)) _effect.Parameters[param].SetValue(((Point)obj).ToVector2());
            else if (t == typeof(Vector3)) _effect.Parameters[param].SetValue((Vector3)obj);
            else if (t == typeof(Vector4)) _effect.Parameters[param].SetValue((Vector4)obj);
            else if (t == typeof(Vector2[])) _effect.Parameters[param].SetValue((Vector2[])obj);
            else if (t == typeof(Vector3[])) _effect.Parameters[param].SetValue((Vector3[])obj);
            else if (t == typeof(Vector4[])) _effect.Parameters[param].SetValue((Vector4[])obj);
            else if (t == typeof(Matrix)) _effect.Parameters[param].SetValue((Matrix)obj);
            else if (t == typeof(Matrix[])) _effect.Parameters[param].SetValue((Matrix[])obj);
            else if (t == typeof(Color)) _effect.Parameters[param].SetValue(((Color)obj).ToVector4());            
            else if (t == typeof(Texture2D)) _effect.Parameters[param].SetValue((Texture2D)obj);
            else if (t == typeof(TextureCube)) _effect.Parameters[param].SetValue((TextureCube)obj);
            else if (t == typeof(RenderTarget2D)) _effect.Parameters[param].SetValue((RenderTarget2D)obj);
            else { throw new Exception("Bad shader object type"); }
        }


        public virtual void begin_spritebatch() {
            if (Draw2D.sb_drawing) Draw2D.sb.End();
            Draw2D.sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, null, null, effect, null);
            Draw2D.sb_drawing = true;
        }
        public virtual void begin_spritebatch(BlendState blend_state) {
            if (Draw2D.sb_drawing) Draw2D.sb.End();
            Draw2D.sb.Begin(SpriteSortMode.Immediate, blend_state, SamplerState.PointWrap, null, null, effect, null);
            Draw2D.sb_drawing = true;
        }
        public virtual void begin_spritebatch(SamplerState sampler_state) {
            if (Draw2D.sb_drawing) Draw2D.sb.End();
            Draw2D.sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, sampler_state, null, null, effect, null);
            Draw2D.sb_drawing = true;
        }
        public virtual void begin_spritebatch(BlendState blend_state, SamplerState sampler_state) {
            if (Draw2D.sb_drawing) Draw2D.sb.End();
            Draw2D.sb.Begin(SpriteSortMode.Immediate, blend_state, sampler_state, null, null, effect, null);
            Draw2D.sb_drawing = true;
        }

        public virtual void begin_spritebatch(SpriteBatch sb) {
            if (Draw2D.sb_drawing) Draw2D.sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, null, null, effect, null);
            Draw2D.sb_drawing = true;
        }
        public virtual void begin_spritebatch(SpriteBatch sb, BlendState blend_state) {
            if (Draw2D.sb_drawing) Draw2D.sb.End();
            sb.Begin(SpriteSortMode.Immediate, blend_state, SamplerState.PointWrap, null, null, effect, null);
            Draw2D.sb_drawing = true;
        }
        public virtual void begin_spritebatch(SpriteBatch sb, SamplerState sampler_state) {
            if (Draw2D.sb_drawing) Draw2D.sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, sampler_state, null, null, effect, null);
            Draw2D.sb_drawing = true;
        }
        public virtual void begin_spritebatch(SpriteBatch sb, BlendState blend_state, SamplerState sampler_state) {
            if (Draw2D.sb_drawing) Draw2D.sb.End();
            sb.Begin(SpriteSortMode.Immediate, blend_state, sampler_state, null, null, effect, null);
            Draw2D.sb_drawing = true;
        }


        public virtual void draw(Vector2i position, Vector2i size) {
            begin_spritebatch();
            Draw2D.sb.Draw(Draw2D.OnePXWhite, new Rectangle(position.ToPoint(), size.ToPoint()), Color.Transparent);
            Draw2D.end();
        }
        public virtual void draw_texture(Texture2D texture, Vector2i position, Vector2i size) {
            begin_spritebatch();
            Draw2D.sb.Draw(texture, new Rectangle(position.ToPoint(), size.ToPoint()), Color.White);
            Draw2D.end();
        }
        public virtual void draw_texture(Texture2D texture, Vector2i position, Vector2i size, Vector2i crop_position, Vector2i crop_size) {
            begin_spritebatch();
            Draw2D.sb.Draw(texture, 
                new Rectangle(position.ToPoint(), size.ToPoint()),
                new Rectangle(crop_position.X, crop_position.Y, crop_size.X, crop_size.Y),
                Color.White);
            Draw2D.end();
        }

        /// <summary>
        /// Apply all passes from the current technique
        /// </summary>
        public void apply_passes() {
            for (int i = 0; i < _effect.CurrentTechnique.Passes.Count; i++) {
                _effect.CurrentTechnique.Passes[i].Apply();
            }
        }
        
        public void change_technique(int technique) {
            if (technique < 0 || technique >= _effect.Techniques.Count) 
                throw new Exception($"Technique index {technique} is out of range.");
            _effect.CurrentTechnique = _effect.Techniques[technique];
        }

        public void change_technique(string technique) {
            // none of the techniques have this name
            if (_effect.Techniques.All(t => technique != t.Name))
                throw new Exception($"Technique \"{technique}\" is not defined in effect \"{effect.Name}\".");
            _effect.CurrentTechnique = _effect.Techniques[technique];
        }

        /// <summary>
        /// <para>Uses the BasicEffect to set up the vertices and paint a base white texture,
        /// then uses the selected effect's pixel shader to draw its texture</para>
        /// <para></para>
        /// <para>Useful for drawing a 3D mesh with an appearance entirely determined by a pixel shader</para>
        /// </summary>
        /// <param name="vb">The vertex buffer of the mesh</param>
        /// <param name="ib">The index buffer of the mesh</param>
        /// <param name="world">World matrix</param>
        /// <param name="view">View matrix</param>
        /// <param name="projection">Projection matrix</param>
        public virtual void draw_buffers_basic_effect_first_pass(VertexBuffer vb, IndexBuffer ib, Matrix world, Matrix view, Matrix projection) {
            if (_effect == null) return;

            basic_effect.World = world;
            basic_effect.View = view;
            basic_effect.Projection = projection;
            basic_effect.Texture = Draw2D.OnePXWhite;

            State.graphics_device.SetVertexBuffer(vb);
            State.graphics_device.Indices = ib;

            apply_passes();

            State.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        }

        /// <summary>
        /// <para>Uses the BasicEffect to set up the vertices and paint a base white texture, 
        /// then uses the selected effect's pixel shader to draw its texture</para>
        /// <para></para>
        /// <para>Useful for drawing a 3D mesh with an appearance entirely determined by a pixel shader</para>
        /// <para>This call assumes you have already set up the vertex and index buffers</para>
        /// </summary>
        /// <param name="world">World matrix</param>
        /// <param name="view">View matrix</param>
        /// <param name="projection">Projection matrix</param>
        public virtual void draw_buffers_basic_effect_first_pass(Matrix world, Matrix view, Matrix projection) {
            if (_effect == null) return;
            
            basic_effect.World = world;
            basic_effect.View = view;
            basic_effect.Projection = projection;

            apply_passes();    

            State.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        }

        /// <summary>
        /// <para>Uses the BasicEffect to set up the vertices and paint a base white texture, 
        /// then uses the selected effect's pixel shader to draw its texture</para>
        /// <para></para>
        /// <para>Useful for drawing a 3D mesh with an appearance entirely determined by a pixel shader</para>
        /// <para>This call assumes you have already set up the vertex and index buffers using GraphicsDevice,
        /// as well as the BasicEffect's WVP using the basic_effect_world/view/projection properties </para>
        /// </summary>
        public virtual void draw_buffers_basic_effect_first_pass() {
            if (_effect == null) return;
            
            apply_passes();   

            State.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        }
    }
}
