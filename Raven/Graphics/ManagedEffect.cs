using System;
using System.Collections.Generic;
using System.Linq;
using CSScripting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Graphics.Drawing2D;
using Raven.Graphics.Drawing3D;
using Raven.Graphics.Effects;

namespace Raven.Graphics.Effects;
[GuidManaged]
public partial class ManagedEffect {
    public static partial class Manager {
        public static void do_updates() {
            foreach (var effect in managedeffects.Values) {
                effect.update();
            }
        }

        public static void set_param_on_all_effects_of_same_type<E, P>(E managed_effect, string param, P data) {
            if (!(managed_effect is ManagedEffect)) return;
            foreach (var me in managedeffects.Values) {
                if (me.GetType() == typeof(E)) {
                    me.set_param(param, data);
                }
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
        Manager.Add(this);
        
        post_init();
    }
    public ManagedEffect(Effect effect) {
        _effect = effect;
        build_basic_effect();
        Manager.Add(this);
        
        post_init();
    }
    public ManagedEffect(ContentManager content, string effect_name) {
        load_shader_file(content, effect_name);
        build_basic_effect();
        Manager.Add(this);
        
        post_init();
    }

    ~ManagedEffect() {
        Manager.Remove(GUID);
    }
    
    internal virtual void post_init() {}
    
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

    public virtual void draw(Vector2i position, Vector2i size) {
        Draw2D.sb.Draw(Draw2D.OnePXWhite, new Rectangle(position.ToPoint(), size.ToPoint()), Color.Transparent);
    }
    public virtual void draw_texture(Texture2D texture, Vector2i position, Vector2i size) {
        Draw2D.sb.Draw(texture, new Rectangle(position.ToPoint(), size.ToPoint()), Color.White);
    }
    public virtual void draw_texture(Texture2D texture, Vector2i position, Vector2i size, Vector2i crop_position, Vector2i crop_size) {
        Draw2D.sb.Draw(texture, 
            new Rectangle(position.ToPoint(), size.ToPoint()),
            new Rectangle(crop_position.X, crop_position.Y, crop_size.X, crop_size.Y),
            Color.White);
    }

    public virtual void end_spritebatch() {
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

    private int current_vb_vert_count = 0;
    public virtual void set_vertex_buffer(VertexBuffer vertex_buffer, IndexBuffer index_buffer) {
        State.graphics_device.SetVertexBuffer(vertex_buffer);
        State.graphics_device.Indices = index_buffer;
        current_vb_vert_count = index_buffer.IndexCount / 3;
    }
    
    public virtual void render_vertex_buffer() {
        State.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, current_vb_vert_count);
    }
}

public abstract class ManagedEffectPipelineReplacement : ManagedEffect {
    private string shader_name;
    public ManagedEffectPipelineReplacement(string shader_name) : base (Resources.GetShaderInstance(shader_name)){
        var s = Resources.GetShader(shader_name);
        if (s.Techniques.Where(t => t.Name.ToLower() != "forward").Any() ||
            s.Techniques.Where(t => t.Name.ToLower() != "deferred").Any()) {
            throw new Exception("Pipeline replacement shaders must contain at least two techniques named \"forward\" and \"deferred\"");
        }

        this.shader_name = shader_name;
    }

    public abstract void batch_render_setup();

    public abstract void deferred_batch_setup();
    public abstract void forward_batch_setup();

    public abstract void setup_deferred_render_step();
    public abstract void setup_forward_render_step();

}