using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RavenRPG.Engine;
using RavenRPG.Graphics.Drawing2D.Effects;

namespace RavenRPG.Graphics.Drawing2D;

public static class Draw2D {
    public class GradientLineGenerator {
        public float min, max, value;
        public Color start_color;

        public Texture2D debug_band;

        public void build_debug_band_texture(int width = 256) {
            debug_band = new Texture2D(State.graphics_device, width, 1);
            var data = new Color[width];
            
            for (var i = 0; i < data.Length; i++) {
                float a = i; float b = data.Length;
                float ab = a / b;

                data[i] = get_color_at(ab);
            }

            debug_band.SetData(data);
        }

        public struct CLerpPck {
            public Color color; public float position;
            public CLerpPck(Color color, float position) {
                this.color = color;
                this.position = position;
            }
        }

        List<CLerpPck> Lerps = new List<CLerpPck>();

        public Color get_color_at(float position) {
            float position_within_lerp = 0.0f;
            float position_end = 1f;
            float lerp_length = 1f;
            float norm_pos_in_lerp = 0.0f;

            var first_lerp = Lerps[0];
            if (position <= first_lerp.position) {
                lerp_length = first_lerp.position;

                norm_pos_in_lerp = position / lerp_length;
                return ColorInterpolate(start_color, first_lerp.color, norm_pos_in_lerp);
            }
            

            for (int i = 0; i < Lerps.Count-1; i++) {
                CLerpPck lerp = Lerps[i];
                CLerpPck next_lerp = Lerps[i+1];

                if (position >= lerp.position && position < next_lerp.position) {
                    position_within_lerp = (position - lerp.position);
                    
                    position_end = next_lerp.position;
                    lerp_length = next_lerp.position - lerp.position;

                    norm_pos_in_lerp = position_within_lerp / lerp_length;

                    return ColorInterpolate(lerp.color, next_lerp.color, norm_pos_in_lerp);
                }
            }

            return start_color;
        }

        public Color current_color {
            get {
                foreach (CLerpPck lerp in Lerps) {

                }

                return Color.White;
            }
        }
        
        public GradientLineGenerator(Color start_color) {
            min = 0.0f;
            max = 0.0f;
            value = 0.0f;

            this.start_color = start_color;
        }

        public void add_lerp(Color color, float position) {
            if (position > max) max = position;
            
            var tmp = new CLerpPck(color, position);

            Lerps.Add(tmp);
        }
    }
    
    public static Color ColorRandomFromString(string s) {
        int seed = 0;
        foreach (byte b in Encoding.ASCII.GetBytes(s)) {
            seed += b;
        }
        RNG.change_seed(seed);
        var c = Color.FromNonPremultiplied(RNG.rng_byte(), RNG.rng_byte(), RNG.rng_byte(), 255);
        RNG.change_seed();
        return c;

    }

    /// <summary>
    /// Interpolates between two colors
    /// </summary>
    /// <param name="colorA">The first color</param>
    /// <param name="colorB">The second color</param>
    /// <param name="bAmount">The amount to interpolate; 0.0 for 100% color A, 1.0 for color B</param>
    /// <returns>The resulting Color</returns>
    public static Color ColorInterpolate(Color colorA, Color colorB, float bAmount) {
        var aAmount = 1.0f - bAmount;
        var r = (int)(colorA.R * aAmount + colorB.R * bAmount);
        var g = (int)(colorA.G * aAmount + colorB.G * bAmount);
        var b = (int)(colorA.B * aAmount + colorB.B * bAmount);

        return Color.FromNonPremultiplied(r, g, b, 255);
    }

    /// <summary>
    /// Generates a muted version of the input color
    /// </summary>
    /// <param name="input">the color to mute</param>
    /// <param name="amount">the amount to mute by</param>
    /// <returns></returns>
    public static Color MuteColor(Color input, float amount) {
        return Color.FromNonPremultiplied(
            int.Clamp((int)(input.R * (1.0f - amount)), 0, 255),
            int.Clamp((int)(input.G * (1.0f - amount)), 0, 255),
            int.Clamp((int)(input.B * (1.0f - amount)), 0, 255),
            input.A);
    }
    
    public static SpriteBatch sb => State.sprite_batch;

    private static bool _sb_drawing = false;
    public static bool sb_drawing {
        get { return _sb_drawing; }
        set { _sb_drawing = value; }
    }

    public static Texture2D OnePXWhite;
    
    public static SpriteFont fnt_profont;
    
    static Effects.Dither dither_effect;

    public static void load() {
        //create a 1x1 white texture
        OnePXWhite = new Texture2D(State.graphics_device, 1, 1); OnePXWhite.SetData([Color.White]);
        
        
        Resources.AddTexture("OnePXWhite", new Texture2D(State.graphics_device, 1, 1));
        Resources.GetTextureContent("OnePXWhite").Texture.SetData([Color.White]);
        
        
        Resources.AddTexture("OnePXBlack", new Texture2D(State.graphics_device, 1, 1));
        Resources.GetTextureContent("OnePXBlack").Texture.SetData([Color.Black]);

        
        Resources.AddTexture("OnePXGrey", new Texture2D(State.graphics_device, 1, 1));
        Resources.GetTextureContent("OnePXGrey").Texture.SetData([Color.Gray]);
        
        
        Resources.AddTexture("Missing", new Texture2D(State.graphics_device, 2, 2));
        Resources.GetTextureContent("Missing").Texture.SetData([
            Color.Magenta, Color.Black,
            Color.Black, Color.Magenta
        ]);
        
        
        Resources.AddTexture("checker", new Texture2D(State.graphics_device, 2,2));
        Resources.GetTextureContent("checker").Texture.SetData([
            Color.White, Color.Black,
            Color.Black, Color.White
        ]);
        
        
        Resources.AddTexture("center_glow", new Texture2D(State.graphics_device, 1, 256));
        var glowData = new Color[256];
        for (var i = 0; i < glowData.Length; i++) {
            var p = i / (glowData.Length / 2f);
            if (p > 1) p = 1f - (p - 1);

            glowData[i] = Color.FromNonPremultiplied(255, 255, 255, (int)(p * 155));
        }
        Resources.GetTextureContent("center_glow").Texture.SetData(glowData);
        
        
        Resources.AddTexture("radial_glow", new Texture2D(State.graphics_device, 256, 256));
        glowData = new Color[256 * 256];

        for (var i = 0; i < 255; i++) {

            float px, py;

            for (var x = 0; x < 255; x++) {

                px = x / 255f;
                py = i / 255f;

                float t = 0.5f - Vector2.Distance(Vector2.One * 0.5f, new Vector2(px, py));
                t *= 0.8f;

                int o = (int)((t*6) * 255);
                glowData[(i * 256) + x] = Color.FromNonPremultiplied(o,o,o, 255);
            }
        }

        Resources.GetTextureContent("radial_glow").Texture.SetData(glowData);
        
        
        Resources.AddTexture("sdf_square", new Texture2D(State.graphics_device, 256, 256));
        glowData = new Color[256 * 256];

        for (var i = 0; i < 256; i++) {

            float px, py;

            for (var x = 0; x < 256; x++) {

                px = x / 255f;
                py = i / 255f;

                float t = Vector2.Distance(Vector2.One * 0.5f, new Vector2(px, py)) ;

                int o = (int)((t) * 255);
                glowData[(i * 256) + x] = Color.FromNonPremultiplied(o, o, o, 255);
            }
        }
        Resources.GetTextureContent("sdf_square").Texture.SetData(glowData);
        
        
        Resources.AddTexture("gradient_vertical", new Texture2D(State.graphics_device, 1, 256));
        glowData = new Color[1 * 256];
        for (var i = 0; i < 255; i++) {
            glowData[i] = Color.FromNonPremultiplied(255, 255, 255, i);                
        }
        Resources.GetTextureContent("gradient_vertical").Texture.SetData(glowData);

        
        Resources.AddTexture("skybox_gradient", new Texture2D(State.graphics_device, 512, 512));
        glowData = new Color[512 * 512];
        for (var y = 0; y < 512; y++) {
            for (var x = 0; x < 512; x++) {
                float px = 1.0f - x / 512f; //x pos
                float pxs = x / (512f / 2f); //x pos wave func, 0 to 1 to 0 at left/middle/right
                if (pxs > 1) pxs = 1f - (pxs - 1);
                float py = y / 512f; //y pos

                int v = 128; //set entire image to 50%
                v = (int)(v * (1 - (1 - (py / 2)))); //~50% black, 50% gradient fade from top to bottom
                v += (y - (int)((512f / (MathHelper.Pi)) * (Math.Sin(px * MathHelper.Pi)))) / 4; //the actual curve
                v = (int)(v - (((MathHelper.Clamp(1 - pxs, 0f, 1f)) * v) / 4f)); //reduce the brightness of the left and right edges slightly

                if (v < 0) v = 0;

                glowData[(y * 512) + x] = Color.FromNonPremultiplied(255, 255, 255,
                    v);
            }
        }
        Resources.GetTextureContent("skybox_gradient").Texture.SetData(glowData);

        
        //create an SDF of a circle
        int sdf_circle_res = 1024;
        Color[] sdf_data = new Color[sdf_circle_res * sdf_circle_res];

        Resources.AddTexture("sdf_circle", new Texture2D(State.graphics_device, sdf_circle_res, sdf_circle_res));
        
        for (var i = 0; i < sdf_circle_res; i++) {

            float px, py;

            for (var x = 0; x < sdf_circle_res; x++) {

                px = x / (float)sdf_circle_res;
                py = i / (float)sdf_circle_res;

                float t = Vector2.Distance(Vector2.One * 0.5f, new Vector2(px, py))*2;

                int o = (int)((t) * 255);
                sdf_data[(i * sdf_circle_res) + x] = Color.FromNonPremultiplied(255 - o, 255 - o, 255 - o, 255);
            }
        }
        
        Resources.GetTextureContent("sdf_circle").Texture.SetData(sdf_data);
        
        fnt_profont = Resources.GetFont("profont");
        
        SDF.load();
    }

    public static void begin() {
        if (!sb_drawing) {
            //sb.Begin();
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, null, null, null, null);
            sb_drawing = true;
        }
    }

    public static void begin(BlendState blend_state) {
        if (!sb_drawing) {
            sb.Begin(SpriteSortMode.Immediate, blend_state, SamplerState.PointWrap, null, null, null, null);
            sb_drawing = true;
        }
    }
    public static void begin(BlendState blend_state, SamplerState sampler_state) {
        if (!sb_drawing) {
            sb.Begin(SpriteSortMode.Immediate, blend_state, sampler_state, null, null, null, null);
            sb_drawing = true;
        }
    }

    public static void begin(Effect effect) {
        if (!sb_drawing) {
            //sb.Begin();
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, null, null, effect, null);
            sb_drawing = true;
        }
    }
    public static void begin(Effect effect, BlendState blend_state) {
        if (!sb_drawing) {
            //sb.Begin();
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, null, null, effect, null);
            sb_drawing = true;
        }
    }
    public static void begin(Effect effect, BlendState blend_state, SamplerState sampler_state) {
        if (!sb_drawing) {
            //sb.Begin();
            sb.Begin(SpriteSortMode.Immediate, blend_state, sampler_state, null, null, effect, null);
            sb_drawing = true;
        }
    }
    public static void begin(SpriteSortMode sprite_sort_mode, Effect effect, BlendState blend_state, SamplerState sampler_state, DepthStencilState  depth_state) {
        if (!sb_drawing) {
            //sb.Begin();
            sb.Begin(sprite_sort_mode, blend_state, sampler_state, depth_state, null, effect, null);
            sb_drawing = true;
        }
    }
    public static void begin(SpriteSortMode sprite_sort_mode, BlendState blend_state, SamplerState sampler_state, DepthStencilState  depth_state) {
        if (!sb_drawing) {
            //sb.Begin();
            sb.Begin(sprite_sort_mode, blend_state, sampler_state, depth_state, null,null, null);
            sb_drawing = true;
        }
    }

    public static void end() {
        if (sb_drawing) {
            sb.End();
            sb_drawing = false;
        }
    }

    public static void pixel(Vector2 position, Color color) {
        begin();
        sb.Draw(OnePXWhite, position, color);
    }
    public static void pixel(Vector2i position, Color color) {
        begin();
        sb.Draw(OnePXWhite, position.ToVector2(), color);
    }

    public static void line(Vector2 A, Vector2 B, Color color, float thickness) {
        begin();

        var tan = B - A;
        var rot = (float)Math.Atan2(tan.Y, tan.X);

        var middlePoint = new Vector2(0, 0.5f);
        var scale = new Vector2(tan.Length(), thickness);

        sb.Draw(OnePXWhite, A, null, color, rot, middlePoint, scale, SpriteEffects.None, 0f);
    }


    public static void line(Vector2i A, Vector2i B, Color color, float thickness) {
        begin();

        var tan = B - A;
        var rot = (float)Math.Atan2(tan.Y, tan.X);

        var middlePoint = new Vector2(0, 0.5f);
        var scale = new Vector2(tan.Length(), thickness);

        sb.Draw(OnePXWhite, A.ToVector2(), null, color, rot, middlePoint, scale, SpriteEffects.None, 0f);
    }
    public static void line_rounded_ends(Vector2 A, Vector2 B, Color color, float thickness) {
        line(A, B, color, thickness);

        fill_circle(A, thickness + 1f, color);
        fill_circle(B, thickness + 1f, color);
    }
    public static void line_rounded_ends(Vector2i A, Vector2i B, Color color, float thickness) {
        line(A, B, color, thickness);

        fill_circle(A.ToVector2(), thickness / 2f, color);
        fill_circle(B.ToVector2(), thickness / 2f, color);
    }

    public static void cross(Vector2 center, float size, Color color) {
        center.Round();
        line(center - (Vector2.UnitX * (size + 1)), center + (Vector2.UnitX * size), color, 1f);
        line(center - (Vector2.UnitY * (size + 1)), center + (Vector2.UnitY * size), color, 1f);
    }

    public static void cross(Vector2i center, int size, Color color) {
        line(center - (Vector2.UnitX * (size + 1)), center + (Vector2.UnitX * size), color, 1f);
        line(center - (Vector2.UnitY * (size + 1)), center + (Vector2.UnitY * size), color, 1f);
    }
    public static void poly(Color color, float thickness, bool close_polygon, params Vector2[] points) {
        if (points.Length < 2) return;
        begin();

        for (var i = 0; i < points.Length - 1; i++) {
            var p = points[i];
            var pP = points[i + 1];
            line(p, pP, color, thickness);
        }

        if (points.Length > 2 && close_polygon)
            line(points.First(), points.Last(), color, thickness);

        //for (var i = 0; i < points.Length; i++) {
            //fill_circle(points[i], thickness, color);
        //}
    }
    public static void poly(Color color, float thickness, bool close_polygon, params Vector2i[] points) {
        if (points.Length < 2) return;
        begin();

        for (var i = 0; i < points.Length - 1; i++) {
            var p = points[i];
            var pP = points[i + 1];
            line(p, pP, color, thickness);
        }

        if (points.Length > 2 && close_polygon)
            line(points.First(), points.Last(), color, thickness);

        //for (var i = 0; i < points.Length; i++) {
            //fill_circle(points[i], thickness, color);
        //}
    }


    public static void tri(Vector2 A, Vector2 B, Vector2 C, Color color, float thickness) {
        begin();
        poly(color, thickness, true, A, B, C);
    }
    public static void tri(Vector2i A, Vector2i B, Vector2i C, Color color, float thickness) {
        begin();
        poly(color, thickness, true, A.ToVector2(), B.ToVector2(), C.ToVector2());
    }

    public static void point(Vector2 point, Vector2 size, Color color) {
        fill_rect(point - new Vector2(size.X/2, size.Y/2), point + new Vector2(size.X, size.Y), color);
    }
    public static void point(Vector2i point, Vector2i size, Color color) {
        fill_rect(point - new Vector2i(size.X/2, size.Y/2), point + new Vector2i(size.X, size.Y), color);
    }
    
    public static void rect(Vector2 min, float size_x, float size_y, Color color, float thickness) {
        rect(min, min + new Vector2(size_x, size_y), color, thickness);
    }
    public static void rect(Vector2i min, float size_x, float size_y, Color color, float thickness) {
        rect(min.ToVector2(), min.ToVector2() + new Vector2(size_x, size_y), color, thickness);
    }
    public static void rect(Vector2i min, Vector2i max, Color color, float thickness) {
        rect(min.ToVector2(), max.ToVector2(), color, thickness);
    }
    public static void rect(Vector2 min, Vector2 max, Color color, float thickness) {
        min.Floor(); max.Ceiling();
        begin();

        var w = Vector2.UnitX * (max.X - min.X);
        var h = Vector2.UnitY * (max.Y - min.Y);

        var half_line_x = Vector2.UnitX * (thickness/2f);

        //draw sides
        //top
        line(min - half_line_x,
            (min + half_line_x) + w,
            color, thickness);
        //bottom
        line((min - half_line_x) + h,
            (min + half_line_x) + h + w,
            color, thickness);
        //left
        line(min, min + h, color, thickness);
        //right
        line(min + w, min + h + w, color, thickness);
    }

    public static void fill_rect(Vector2 min, float size_x, float size_y, Color color) {
        fill_rect(min, min + new Vector2(size_x, size_y), color);
    }
    public static void fill_rect(int min_x, int min_y, int size_x, int size_y, Color color) {
        fill_rect(new Vector2i(min_x, min_y), new Vector2i(min_x + size_x, min_y + size_y), color);
    }
    public static void fill_rect(Vector2i min, float size_x, float size_y, Color color) {
        fill_rect(min.ToVector2(), min.ToVector2() + new Vector2(size_x, size_y), color);
    }


    public static void fill_rect(Vector2 min, Vector2 max, Color color) {
        min.Floor(); max.Ceiling();
        begin();
        sb.Draw(OnePXWhite, min, null, color, 0f, Vector2.Zero, max - min, SpriteEffects.None, 0f);
    }

    public static void fill_rect(Vector2i min, Vector2i max, Color color) {
        begin();
        sb.Draw(OnePXWhite, min.ToVector2(), null, color, 0f, Vector2.Zero, (max - min).ToVector2(), SpriteEffects.None, 0f);
    }

    public static void fill_rect_dither(Vector2i min, Vector2i max, Color color_a, Color color_b) {
        if (dither_effect == null) dither_effect = new Dither(State.content_manager);

        dither_effect.configure_shader(min.ToVector2(), max.ToVector2(), color_a, color_b);
        dither_effect.begin_spritebatch(sb);
        sb.Draw(OnePXWhite, min.ToVector2(), null, Color.White, 0f, Vector2.Zero, (max - min).ToVector2(), SpriteEffects.None, 0f);
        end();
    }


    public static void fill_rect_outline(Vector2 min, Vector2 max, Color color, Color outline, float outline_thickness) {
        min.Floor(); max.Ceiling();
        fill_rect(min, max, color);
        rect(min, max, outline, outline_thickness);
    }
    public static void fill_rect_outline(Vector2i min, Vector2i max, Color color, Color outline, float outline_thickness) {
        fill_rect(min, max, color);
        rect(min, max, outline, outline_thickness);
    }


    public static void circle(Vector2 P, float radius, float thickness, Color color) {
        SDF.draw_circle(P, radius, thickness, color);
    }
    public static void circle(Vector2i P, float radius, float thickness, Color color) {
        SDF.draw_circle(P.ToVector2(), radius, thickness, color);
    }
    public static void fill_circle(Vector2 P, float radius, Color color) {
        SDF.fill_circle(P, radius, color);
    }
    public static void fill_circle(Vector2i P, float radius, Color color) {
        SDF.fill_circle(P.ToVector2(), radius, color);
    }

    public static void image(Texture2D image, Vector2 position, Vector2 size) {
        begin();
        sb.Draw(image, new Rectangle(position.ToPoint(), size.ToPoint()), Color.White);
    }
    public static void image(RenderTarget2D image, Vector2 position, Vector2 size) {
        begin();
        sb.Draw(image, new Rectangle(position.ToPoint(), size.ToPoint()), Color.White);
    }

    public static void image(Texture2D image, Vector2 position, Vector2 size, SpriteEffects flip_mode) {
        begin();
        sb.Draw(image, new Rectangle(position.ToPoint(), size.ToPoint()), null, Color.White, 0f, Vector2.Zero, flip_mode, 1f);
    }
    public static void image(RenderTarget2D image, Vector2 position, Vector2 size, SpriteEffects flip_mode) {
        begin();
        sb.Draw(image, new Rectangle(position.ToPoint(), size.ToPoint()), null, Color.White, 0f, Vector2.Zero, flip_mode, 1f);
    }

    public static void image(Texture2D image, Vector2 position, Point size) {
        begin();
        sb.Draw(image, new Rectangle(position.ToPoint(), size), Color.White);
    }
    public static void image(RenderTarget2D image, Vector2 position, Point size) {
        begin();
        sb.Draw(image, new Rectangle(position.ToPoint(), size), Color.White);
    }
    public static void image(Texture2D image, Point position, Point size) {
        begin();
        sb.Draw(image, new Rectangle(position, size), Color.White);
    }
    public static void image(RenderTarget2D image, Point position, Point size) {
        begin();
        sb.Draw(image, new Rectangle(position, size), Color.White);
    }
    public static void image(Texture2D image, Vector2i position, Vector2i size) {
        begin();
        sb.Draw(image, new Rectangle(position.ToPoint(), size.ToPoint()), Color.White);
    }
    public static void image(RenderTarget2D image, Vector2i position, Vector2i size) {
        begin();
        sb.Draw(image, new Rectangle(position.ToPoint(), size.ToPoint()), Color.White);
    }
    public static void image(RenderTarget2D image, Vector2i position, Vector2i size, Vector2i crop_position, Vector2i crop_size) {
        begin();
        sb.Draw(image, 
            new Rectangle(position.ToPoint(), size.ToPoint()), 
            new Rectangle(crop_position.X, crop_position.Y, crop_size.X, crop_size.Y), 
            Color.White);
    }
    public static void image(Texture2D image, Vector2i position, Vector2i size, Vector2i crop_position, Vector2i crop_size) {
        begin();
        sb.Draw(image,
            new Rectangle(position.ToPoint(), size.ToPoint()),
            new Rectangle(crop_position.X, crop_position.Y, crop_size.X, crop_size.Y),
            Color.White);
    }
    public static void image(RenderTarget2D image, Vector2i position, Vector2i size, Vector2i crop_position, Vector2i crop_size, Color tint) {
        begin();
        sb.Draw(image,
            new Rectangle(position.ToPoint(), size.ToPoint()),
            new Rectangle(crop_position.X, crop_position.Y, crop_size.X, crop_size.Y),
            tint);
    }
    public static void image(Texture2D image, Vector2i position, Vector2i size, Vector2i crop_position, Vector2i crop_size, Color tint) {
        begin();
        sb.Draw(image,
            new Rectangle(position.ToPoint(), size.ToPoint()),
            new Rectangle(crop_position.X, crop_position.Y, crop_size.X, crop_size.Y),
            tint);
    }

    public static void image(Texture2D image, Vector2 position, Vector2 size, Color tint) {
        begin();
        sb.Draw(image, new Rectangle(position.ToPoint(), size.ToPoint()), tint);
    }
    public static void image(RenderTarget2D image, Vector2 position, Vector2 size, Color tint) {
        begin();
        sb.Draw(image, new Rectangle(position.ToPoint(), size.ToPoint()), tint);
    }
    public static void image(Texture2D image, Vector2 position, Vector2 size, float rotation) {
        begin();
        sb.Draw(image, new Rectangle((position + (image.Bounds.Size.ToVector2() / 2f)).ToPoint(), size.ToPoint()), null, Color.White, MathHelper.ToRadians(rotation), image.Bounds.Size.ToVector2() / 2f, SpriteEffects.None, 0f);
    }
    public static void image(RenderTarget2D image, Vector2 position, Vector2 size, float rotation) {
        begin();
        sb.Draw(image, new Rectangle((position + (image.Bounds.Size.ToVector2() / 2f)).ToPoint(), size.ToPoint()), null, Color.White, MathHelper.ToRadians(rotation), image.Bounds.Size.ToVector2() / 2f, SpriteEffects.None, 0f);
    }
    public static void image(Texture2D image, Vector2 position, Vector2 size, Color tint, float rotation) {
        begin();
        sb.Draw(image, new Rectangle((position + (image.Bounds.Size.ToVector2() / 2f)).ToPoint(), size.ToPoint()), null, tint, MathHelper.ToRadians(rotation), image.Bounds.Size.ToVector2() / 2f, SpriteEffects.None, 0f);
    }
    public static void image(RenderTarget2D image, Vector2 position, Vector2 size, Color tint, float rotation) {
        begin();
        sb.Draw(image, new Rectangle((position + (image.Bounds.Size.ToVector2() / 2f)).ToPoint(), size.ToPoint()), null, tint, MathHelper.ToRadians(rotation), image.Bounds.Size.ToVector2() / 2f, SpriteEffects.None, 0f);
    }


    public static void image(Texture2D image, Vector2 position, Vector2 size, int source_rect_x, int source_rect_y, int source_rect_w, int source_rect_h) {
        begin();
        sb.Draw(image, new Rectangle(position.ToPoint(), size.ToPoint()),
            new Rectangle(source_rect_x, source_rect_y, source_rect_w, source_rect_h),
            Color.White);
    }
    
    public static void image(string texture_name, Vector2i position, Vector2i size) {
        begin();
        sb.Draw(Resources.GetTexture(texture_name), new Rectangle(position.ToPoint(), size.ToPoint()), Color.White);
    }
    
    public static void image(string texture_name, Vector2i position, Vector2i size, int source_rect_x, int source_rect_y, int source_rect_w, int source_rect_h) {
        begin();
        sb.Draw(Resources.GetTexture(texture_name), new Rectangle(position.ToPoint(), size.ToPoint()),
            new Rectangle(source_rect_x, source_rect_y, source_rect_w, source_rect_h),
            Color.White);
    }

    public static void image(Texture2D image, Vector2 position, Vector2 size, Vector2 fractional_source_rect_pos, Vector2 fractional_source_rect_size) {
        begin();
        sb.Draw(image, new Rectangle(position.ToPoint(), size.ToPoint()),
            new Rectangle(
                (int)(image.Width * fractional_source_rect_pos.X), (int)(image.Height * fractional_source_rect_pos.Y),
                (int)(image.Width * fractional_source_rect_size.X), (int)(image.Height * fractional_source_rect_size.Y)),
            Color.White);
    }

    public static void text(string text, Vector2i position, Color color) {
        Draw2D.text(text, position.ToVector2(), color);
    }

    public static void text_centered(string text, Vector2i position, Color color) {
        string line = string.Empty;
        Vector2i size = Vector2i.Zero;
        Vector2 pos = position.ToVector2();

        int max_w = 0;

        using (StringReader sr = new StringReader(text)) {
            while (sr.Peek() > -1) {
                line = sr.ReadLine();
                size = measure_string_profont_int(line);
                if (size.X > max_w) max_w = size.X;
            }
        }

        line = string.Empty;
        size = Vector2i.Zero;
        pos = position.ToVector2();

        //Drawing.fill_circle(pos, 1f, Color.Red);

        using (StringReader sr = new StringReader(text)) {      
            while (sr.Peek() > -1) {
                line = sr.ReadLine();
                size = measure_string_profont_int(line);
                
                pos.X = (position.X) - (size.X / 2f) + 1;

                //Drawing.fill_circle(pos, 1f, Color.Green);

                Draw2D.text(line, pos, color);
                pos.Y += size.Y;
            }
        }
    }


    public static void text(string text, Vector2 position, Color color) {
        begin();
        position.Ceiling(); //this prevents half-pixel positioning which helps keep text crisp and artifact-free
        //font_manager_profont.draw_string(text, position.ToVector2i(), color);
        sb.DrawString(fnt_profont, text, position, color);
    }
    public static void text_vertical(string text, Vector2 position, Color color) {
        begin();
        position.Ceiling(); //this prevents half-pixel positioning which helps keep text crisp and artifact-free
        sb.DrawString(fnt_profont, text, position, color, MathHelper.ToRadians(90f), Vector2.Zero, 1f, SpriteEffects.None, 1f);
    }

    public static void text_shadow(string text, Vector2i position) {
        Draw2D.text(text, position + Vector2i.One, Color.Black);
        Draw2D.text(text, position, Color.White);
    }
    public static void text_shadow(string text, Vector2i position, Color color_fg) {
        Draw2D.text(text, position + Vector2i.One, Color.Black);
        Draw2D.text(text, position, color_fg);
    }
    public static void text_shadow(string text, Vector2i position, Color color_fg, Color color_bg) {
        Draw2D.text(text, position + Vector2i.One, color_bg);
        Draw2D.text(text, position, color_fg);
    }
    public static void text_shadow(string text, Vector2i position, Color color_fg, Color color_bg, Vector2i shadow_offset) {
        Draw2D.text(text, position + shadow_offset, color_bg);
        Draw2D.text(text, position, color_fg);
    }
    public static void text_shadow(string text, Vector2i position, Color color_fg, Color color_bg, params Vector2i[] shadow_offsets) {
        foreach (var offset in shadow_offsets) 
            Draw2D.text(text, position + offset, color_bg);

        Draw2D.text(text, position, color_fg);
    }

    public static Vector2 measure_string_profont(string text) {
        return fnt_profont.MeasureString(text);
        //return Vector2.One;
        //return font_manager_profont.measure_string(text).ToVector2();
    }
    public static Point measure_string_profont_pt(string text) {
        return fnt_profont.MeasureString(text).ToPoint();
        //return new Point(1, 1);
        //return font_manager_profont.measure_string(text).ToPoint();
    }
    public static Vector2i measure_string_profont_int(string text) {
        return fnt_profont.MeasureString(text).ToVector2i();
        //return Vector2i.One;
        //return font_manager_profont.measure_string(text);
    }
}
