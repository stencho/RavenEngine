using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Console;
using Raven.Engine;
using Raven.Graphics.Drawing2D;
using Raven.Graphics.Drawing3D;
using Raven.Graphics.Effects;
using Raven.UI;
using static Raven.Engine.State;
using static Raven.Graphics.Drawing2D.Draw2D;

namespace Raven.Graphics.Geometry2D;

public enum sdf_pattern {
    NONE = 0,
    DITHER = 1,
    POLKADOT = 2,
    STRIPE = 3
}

public enum sdf_region {
    outer, 
    outer_border, 
    inner_border, 
    inner
}

public class Polygon2D {
    private List<Vector2i> points = new();
    private List<(Vector2i A, Vector2i B)> lines = new();

    private Vector2i output_texture_resolution { get; set; }

    public Texture2D white_fill;

    public Texture2D signed_distance_positive;
    public Texture2D signed_distance_negative;
    
    public Texture2D signed_distance_from_each_point;

    SDFDualTexture renderer;

    private float per_pixel = 0;

    public float outer_border_width { get => renderer.outer_border_width; set => renderer.outer_border_width = value; }
    public float inner_border_width { get => renderer.inner_border_width; set => renderer.inner_border_width = value; }
    
    public class region_info {
        public sdf_region region;
        
        public Color color = Color.Transparent;
        public Color secondary = Color.Transparent;
        
        public sdf_pattern pattern_type = sdf_pattern.NONE;
        public int dither_resolution = 2;
        
        public region_info(sdf_region region) {
            this.region = region;
        }
        public region_info(sdf_region region, Color color, Color secondary, sdf_pattern pattern_type) {
            this.region = region;
            this.color = color;
            this.secondary = secondary;
            this.pattern_type = pattern_type;
        }

        public static void configure_shader(Dictionary<sdf_region, region_info> regions, ref SDFDualTexture shader) {
            shader.change_inner_border_width(shader.inner_border_width);
            shader.change_outer_border_width(shader.outer_border_width);
            
            foreach (var region in regions.Values) {
                shader.configure_region_colors(region.region, region.color, region.secondary);
                
                switch (region.pattern_type) {
                    case sdf_pattern.NONE:
                        shader.remove_patterns(region.region);
                        break;
                    case sdf_pattern.DITHER:
                        shader.pattern_dither(region.region, region.dither_resolution);
                        break;
                    case sdf_pattern.POLKADOT:
                        break;
                    case sdf_pattern.STRIPE:
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public Dictionary<sdf_region, region_info> regions = new Dictionary<sdf_region, region_info>(4) {
        { sdf_region.inner, new region_info(sdf_region.inner) },
        { sdf_region.inner_border, new region_info(sdf_region.inner_border) },
        { sdf_region.outer, new region_info(sdf_region.outer) },
        { sdf_region.outer_border, new region_info(sdf_region.outer_border) }
    };
    
    public Polygon2D(Vector2i output_texture_resolution, params Vector2i[] points) {
        this.output_texture_resolution = output_texture_resolution;
        
        foreach (Vector2i point in points) this.points.Add(point);
        for (int i = 1; i < points.Length; i++) 
            lines.Add((points[i - 1], points[i]));
        lines.Add((points[points.Length - 1], points[0]));
        
        white_fill = new Texture2D(graphics_device, output_texture_resolution.X, output_texture_resolution.Y, false, SurfaceFormat.Color);
        
        signed_distance_positive = new Texture2D(graphics_device, output_texture_resolution.X, output_texture_resolution.Y, false, SurfaceFormat.Color);
        signed_distance_negative = new Texture2D(graphics_device, output_texture_resolution.X, output_texture_resolution.Y, false, SurfaceFormat.Color);
        signed_distance_from_each_point = new Texture2D(graphics_device, output_texture_resolution.X, output_texture_resolution.Y, false, SurfaceFormat.Color);

        renderer = new();
        
        build_white_fill();
        build_sdfs();
        
        renderer.configure_region_colors(sdf_region.inner, Color.Transparent);
        renderer.configure_region_colors(sdf_region.inner_border, Color.Transparent);
        renderer.configure_region_colors(sdf_region.outer_border, Color.Transparent);
        renderer.configure_region_colors(sdf_region.outer, Color.Transparent);
        
        renderer.change_outer_border_width(0);
        renderer.change_inner_border_width(0);
        
        renderer.configure_sdfs(signed_distance_negative, signed_distance_positive);
    }

    enum pixel_state {
        rising_edge,
        falling_edge,
        on_line,
        off_line
    }

    public void render(Vector2i position) {
        renderer.configure_sdfs(signed_distance_negative, signed_distance_positive);
        renderer.configure_draw_info(position, output_texture_resolution);

        region_info.configure_shader(regions, ref renderer);
        renderer.draw(position, output_texture_resolution);
    }

    public void render(Vector2i position, Vector2i size) {
        renderer.configure_sdfs(signed_distance_negative, signed_distance_positive);
        renderer.configure_draw_info(position, size);

        region_info.configure_shader(regions, ref renderer);
        renderer.draw(position, size);
    }
    
    void build_sdfs() {
        Color[] shape_colors = new Color[output_texture_resolution.X * output_texture_resolution.Y];
        white_fill.GetData(shape_colors);
        
        Color[] sdf_positive = new Color[output_texture_resolution.X * output_texture_resolution.Y];
        Color[] sdf_negative = new Color[output_texture_resolution.X * output_texture_resolution.Y];
        
        Color[] sdf_to_each_point = new Color[output_texture_resolution.X * output_texture_resolution.Y];

        float nearest_distance = float.MaxValue;
        float nearest_distance_shortened = float.MaxValue;
        
        float nearest_distance_to_points = float.MaxValue;
        float nearest_distance_to_points_shortened = float.MaxValue;
        
        int abs_ind = 0;
        for (int y = 0; y < output_texture_resolution.Y; y++) {
            for (int x = 0; x < output_texture_resolution.X; x++) {
                abs_ind = (y * output_texture_resolution.X) + x;
                var position = new Vector2(x, y);
                
                nearest_distance = float.MaxValue;
                nearest_distance_to_points = float.MaxValue;

                foreach (var point in points) {
                    var distance = Vector2.Distance(point.ToVector2(), position);
                    if (distance < nearest_distance_to_points) {
                        nearest_distance_to_points = distance;
                        nearest_distance_to_points_shortened = (nearest_distance_to_points * per_pixel);
                    }
                }

                sdf_to_each_point[abs_ind] = ColorInterpolate(Color.Black, Color.White, nearest_distance_to_points_shortened);;
                
                foreach (var line in lines) {
                    Vector2 pomn = Math2D.point_of_minimum_norm(line.A.ToVector2(), line.B.ToVector2(), position);
                    var distance = Vector2.Distance(pomn, position);
                    if (distance < nearest_distance) {
                        nearest_distance = distance;
                        nearest_distance_shortened = (nearest_distance  * per_pixel);
                    }
                }

                if (shape_colors[abs_ind] == Color.Transparent) {
                    sdf_positive[abs_ind] = ColorInterpolate(Color.Black, Color.White, nearest_distance_shortened);
                    sdf_negative[abs_ind] = Color.Transparent;
                } else if (shape_colors[abs_ind] == Color.White) {
                    sdf_positive[abs_ind] = Color.Transparent;
                    sdf_negative[abs_ind] = ColorInterpolate(Color.Black, Color.White, nearest_distance_shortened);
                }
            }
        }
        
        signed_distance_from_each_point.SetData(sdf_to_each_point);
        
        signed_distance_positive.SetData(sdf_positive);
        signed_distance_negative.SetData(sdf_negative);
    }
    
    void build_white_fill() {
        per_pixel = .1f / MathF.Sqrt(output_texture_resolution.X * output_texture_resolution.Y);
        renderer.change_pixel_size(per_pixel);
        
        Color[] color_array = new Color[output_texture_resolution.X * output_texture_resolution.Y];
        
        int lines_crossed_this_pixel = 0;
        int lines_crossed_since_last_rising_edge = 0;
        int absolute = 0;
        
        List<(Vector2i, Vector2i)> unique_crossed_lines = new();
        
        bool last_pixel_hit_line = false;
        bool this_pixel_hit_line = false;
        bool fill_pixel = false;

        Vector2 position;
        
        pixel_state state = pixel_state.off_line;
        pixel_state previous_state = pixel_state.off_line;
        
        for (int y = 0; y < output_texture_resolution.Y; y++) {
            state = pixel_state.off_line;
            fill_pixel = false;
            lines_crossed_since_last_rising_edge = 0;
            unique_crossed_lines.Clear();
            
            for (int x = 0; x < output_texture_resolution.X; x++) {
                lines_crossed_this_pixel = 0;
                absolute = (y * output_texture_resolution.X) + x;
                color_array[absolute] = Color.Transparent;
                position = new Vector2(x, y);
                
                foreach (var line in lines) {
                    if ((Vector2.Distance(
                            Math2D.point_of_minimum_norm(line.A.ToVector2(), line.B.ToVector2(),
                                position), position)) < .5f) {
                        
                        lines_crossed_this_pixel++;
                        
                        if (!unique_crossed_lines.Contains(line)) {
                            if ((line.B.ToVector2() - line.A.ToVector2()).Y != 0) 
                                lines_crossed_since_last_rising_edge++;
                            //else
                                //fill_pixel = !fill_pixel;
                            unique_crossed_lines.Add(line);
                        }
                    } 
                }

                this_pixel_hit_line = lines_crossed_this_pixel > 0;

                if (this_pixel_hit_line && !last_pixel_hit_line) state = pixel_state.rising_edge;
                else if (!this_pixel_hit_line && last_pixel_hit_line) state = pixel_state.falling_edge;
                else {
                    if (previous_state == pixel_state.rising_edge) state = pixel_state.on_line;
                    if (previous_state == pixel_state.falling_edge) state = pixel_state.off_line;
                }

                if (state == pixel_state.rising_edge||state == pixel_state.falling_edge) {
                    if (state == pixel_state.rising_edge && lines_crossed_this_pixel >= 2) {
                        fill_pixel = false;
                    }

                    for (int i = 0; i < lines_crossed_since_last_rising_edge; i++) 
                        fill_pixel = !fill_pixel;
                    lines_crossed_since_last_rising_edge = 0;
                }
               
                if (fill_pixel 
                    || state == pixel_state.on_line
                    || state == pixel_state.rising_edge) 
                    color_array[absolute] = Color.White;
                else color_array[absolute] = Color.Transparent;
            
                last_pixel_hit_line = this_pixel_hit_line;
                previous_state = state;
            }
        }
        
        white_fill.SetData(color_array);
    }

    public partial class SDFDualTexture : ManagedEffect {
        private float change_per_pixel = 0f;
        
        public float outer_border_width = 0;
        public float inner_border_width = 0;
        
        public void change_pixel_size(float float_per_pixel) {
            change_per_pixel = float_per_pixel;
        }
        
        public void configure_sdfs(Texture2D negative, Texture2D positive) {
            set_param("sdf_pos", positive);
            set_param("sdf_neg", negative);
        }

        public void configure_draw_info(Vector2i position, Vector2i size) {
            set_param("top_left", position);
            set_param("bottom_right", position + size);
        }

        public void configure_region_colors(sdf_region region, Color color) {
            set_param($"{region.ToString()}_color", color);
            set_param($"{region.ToString()}_color_secondary", color);
        }
        public void configure_region_colors(sdf_region region, Color color, Color color_secondary) {
            set_param($"{region.ToString()}_color", color);
            set_param($"{region.ToString()}_color_secondary", color_secondary);
        }

        public void remove_patterns(sdf_region region) {
            set_param($"{region.ToString()}_pattern", (int)sdf_pattern.NONE);
        }
        
        public void pattern_dither(sdf_region region, int dither_resolution) {
            set_param($"{region.ToString()}_pattern", (int)sdf_pattern.DITHER);
            set_param($"{region.ToString()}_dither_res", dither_resolution);
        }
        
        public void change_outer_border_width(float pixel_width) => set_param("outer_border_width", pixel_width * change_per_pixel);
        public void change_inner_border_width(float pixel_width) => set_param("inner_border_width", pixel_width * change_per_pixel);
        
        public SDFDualTexture() : base(Resources.GetShaderInstance("sdf_dual_texture")) { }
    }
}