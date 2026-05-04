using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSScripting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Console;
using Raven.Engine;
using Raven.Engine.Collision;
using Raven.Engine.Collision.Shapes2D;
using Raven.Graphics.Drawing2D;
using Raven.Graphics.Drawing3D;
using Raven.Graphics.Effects;
using Raven.UI;
using static Raven.Engine.State;
using static Raven.Graphics.Drawing2D.Draw2D;

namespace Raven.Graphics.Geometry2D;

enum pixel_state {
    rising_edge,
    falling_edge,
    on_line,
    off_line
}

public enum sdf_pattern {
    NONE = 0,
    DITHER = 1,
    POLKADOT = 2,
    STRIPE = 3,
    GLOW = 4
}

public enum sdf_region {
    outer_border, 
    inner_border, 
    inner
}

public enum render_anchor {
    top_left,
    top_right,
    center,
    bottom_left,
    bottom_right,
    first_point
}

public class SDFShape {
    // GEOMETRY
    public Vector2i[] points;
    public readonly List<(Vector2i A, Vector2i B)> lines = new();
    
    public Vector2i render_position = Vector2i.Zero;
    public render_anchor render_anchor = render_anchor.top_left;
    
    public AABB bounds;
    public AABB bordered_bounds;

    private const int shape_border_size = 32;
    
    public Vector2i shape_top_left_within_border => bordered_bounds.center - bounds.center;
    
    // RENDERING
    public Texture2D shape_texture;
    
    public Color inner_color = Color.Transparent;
    
    public Color inner_color_secondary = Color.Transparent;
    public Color outer_color_secondary = Color.Transparent;
    
    public Color inner_border_color = Color.Transparent;
    public Color outer_border_color = Color.Transparent;
    public Color inner_border_color_secondary = Color.Transparent;
    public Color outer_border_color_secondary = Color.Transparent;
    
    public float opacity = 1f;
    
    // PATTERNS
    public float inner_border_width = 0;
    public float outer_border_width = 0;
    
    public sdf_pattern inner_pattern = sdf_pattern.NONE;
    public sdf_pattern inner_border_pattern = sdf_pattern.NONE;
    public sdf_pattern outer_border_pattern = sdf_pattern.NONE;
    
    public int inner_dither_resolution = 1;
    public int inner_border_dither_resolution = 1;
    public int outer_border_dither_resolution = 1;
    
    public float inner_polkadot_size = 8f;
    public float inner_border_polkadot_size = 8f;
    public float outer_border_polkadot_size = 8f;
    
    public float stripe_angle_degrees = 0f;
    public float stripe_width = 10f;
    
    /// <summary>
    /// Points should be focused around origin and moved as one through the offset variable.
    /// </summary>
    /// <param name="points"></param>
    public SDFShape(params Vector2i[] points) {
        this.points = points;
        bounds = AABB.build_around_points(points);
        //bounds.expand(4);
        bordered_bounds = bounds.create_expanded(shape_border_size);

        for (var index = 0; index < points.Length; index++) {
            points[index] -= bounds.top_left;
        }

        bounds.bottom_right -= bounds.top_left;
        bounds.top_left = Vector2i.Zero;

        for (int i = 1; i < points.Length; i++) 
            lines.Add((points[i - 1], points[i]));
        lines.Add((points[points.Length - 1], points[0]));
        
        shape_texture = new Texture2D(graphics_device, bounds.size.X, bounds.size.Y, false, SurfaceFormat.Color);
        
        build_white_fill();
    }
    
    
    void build_white_fill() {
        Color[] color_array = new Color[bounds.size.X * bounds.size.Y];
        
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
        
        for (int y = 0; y < bounds.size.Y; y++) {
            state = pixel_state.off_line;
            fill_pixel = false;
            lines_crossed_since_last_rising_edge = 0;
            unique_crossed_lines.Clear();
            
            for (int x = 0; x < bounds.size.X; x++) {
                lines_crossed_this_pixel = 0;
                absolute = (y * bounds.size.X) + x;
                color_array[absolute] = Color.Transparent;
                position = new Vector2(x, y);
                
                foreach (var line in lines) {
                    if ((Vector2.Distance(
                            Math2D.point_of_minimum_norm(line.A.ToVector2(), line.B.ToVector2(),
                                position), position)) < .5f) {
                        
                        lines_crossed_this_pixel++;
                        
                        if (!unique_crossed_lines.Contains(line)) {
                            if ((line.B - line.A).Y != 0) 
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
        
        shape_texture.SetData(color_array);
    }
}

public partial class DrawShapesToSurface : ManagedEffect {
    /*private ConcurrentDictionary<string, SDFShape> shapes = new ConcurrentDictionary<string, SDFShape>();

    public SDFShape? get(string name) {
        if (shapes.TryGetValue(name, out var shape)) {
            return shape;
        }

        return null;
    }
    
    public void add_shape(string name, params Vector2i[] points) {
        shapes.TryAdd(name, new SDFShape(points));
    }
    public void draw_shape(string name) {
        draw_shape(get(name));
    }
    public void draw_all_shapes() {
        //set_param("resolution", surface_resolution);
        
    }
    */
    
    internal bool draw_debug = true;

    private Vector2[] shader_points = new Vector2[32];
    
    public Func<Vector2i> surface_resolution_method;
    
    private Vector2i _surface_res;
    public Vector2i surface_resolution {
        get {
            return _surface_res;
        }
        set {
            _surface_res = value;
            set_param("resolution", surface_resolution);
        }
    }

    public void draw_shape(SDFShape shape) {
        set_param("resolution", surface_resolution);
        set_param("fill_texture", shape.shape_texture);
        set_param("fill_resolution", shape.bounds.size);
        set_param("fill_position", shape.shape_top_left_within_border);
        set_param("bordered_resolution", shape.bordered_bounds.size);

        for (var index = 0; index < shape.points.Length; index++) {
            shader_points[index] = shape.points[index].ToVector2();
        }

        set_param("points", shader_points);
        set_param("point_count", shape.points.Length);
        
        configure_region_colors(sdf_region.inner, shape.inner_color, shape.inner_color_secondary);
        configure_region_colors(sdf_region.inner_border, shape.inner_border_color, shape.inner_border_color_secondary);
        configure_region_colors(sdf_region.outer_border, shape.outer_border_color, shape.outer_border_color_secondary);

        configure_inner_border_width(shape.inner_border_width);
        configure_outer_border_width(shape.outer_border_width);
        
        switch (shape.inner_pattern) {
            case sdf_pattern.NONE:     remove_patterns(sdf_region.inner); break;
            case sdf_pattern.DITHER:   pattern_dither(sdf_region.inner, shape.inner_dither_resolution); break;
            case sdf_pattern.POLKADOT:
                break;
            case sdf_pattern.STRIPE:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        switch (shape.inner_border_pattern) {
            case sdf_pattern.NONE:     remove_patterns(sdf_region.inner_border); break;
            case sdf_pattern.DITHER:   pattern_dither(sdf_region.inner_border, shape.inner_border_dither_resolution); break;
            case sdf_pattern.POLKADOT:
                break;
            case sdf_pattern.STRIPE:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        switch (shape.outer_border_pattern) {
            case sdf_pattern.NONE:     remove_patterns(sdf_region.outer_border); break;
            case sdf_pattern.DITHER:   pattern_dither(sdf_region.outer_border, shape.outer_border_dither_resolution); break;
            case sdf_pattern.POLKADOT: 
                break;
            case sdf_pattern.STRIPE:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Vector2i offset = Vector2i.Zero;

        switch (shape.render_anchor) {
            case render_anchor.top_left:break;
            case render_anchor.top_right:
                offset += Vector2i.Right * shape.bounds.size.X;
                break;
            case render_anchor.center:
                offset += shape.bounds.size / 2;
                break;
            case render_anchor.bottom_left:
                offset += Vector2i.Down * shape.bounds.size.Y;
                break;
            case render_anchor.bottom_right:
                offset += Vector2i.Right * shape.bounds.size.X;
                offset += Vector2i.Down * shape.bounds.size.Y;
                break;
            case render_anchor.first_point:
                offset += shape.points[0];
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        draw(shape.render_position - shape.shape_top_left_within_border - offset + (Vector2i.Left * 1), shape.bordered_bounds.size);
    }

    
    public void draw_shape_single_color(SDFShape shape, Vector2i draw_offset, Color color_a, Color color_b, int outer_border_width, sdf_pattern pattern, int dither_res) {
        set_param("resolution", surface_resolution);
        set_param("fill_texture", shape.shape_texture);
        set_param("fill_resolution", shape.bounds.size);
        set_param("fill_position", shape.shape_top_left_within_border);
        set_param("bordered_resolution", shape.bordered_bounds.size);

        for (var index = 0; index < shape.points.Length; index++) {
            shader_points[index] = shape.points[index].ToVector2();
        }

        set_param("points", shader_points);
        set_param("point_count", shape.points.Length);
        
        configure_region_colors(sdf_region.inner, color_a, color_b);
        configure_region_colors(sdf_region.outer_border, color_a, color_b);
        configure_region_colors(sdf_region.inner_border, Color.Transparent);

        configure_inner_border_width(0); 
        configure_outer_border_width(outer_border_width);
        
        switch (pattern) {
            case sdf_pattern.NONE:     
                remove_patterns(sdf_region.inner); 
                remove_patterns(sdf_region.outer_border); 
                break;
            case sdf_pattern.DITHER:   
                pattern_dither(sdf_region.inner, dither_res); 
                pattern_dither(sdf_region.outer_border, dither_res); 
                break;
            case sdf_pattern.POLKADOT:
                break;
            case sdf_pattern.STRIPE:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        remove_patterns(sdf_region.inner_border); 
        
        Vector2i offset = Vector2i.Zero;
        switch (shape.render_anchor) {
            case render_anchor.top_left:
                break;
            case render_anchor.top_right:
                offset += Vector2i.Right * shape.bounds.size.X;
                break;
            case render_anchor.center:
                offset += shape.bounds.size / 2;
                break;
            case render_anchor.bottom_left:
                offset += Vector2i.Down * shape.bounds.size.Y;
                break;
            case render_anchor.bottom_right:
                offset += Vector2i.Right * shape.bounds.size.X;
                offset += Vector2i.Down * shape.bounds.size.Y;
                break;
            case render_anchor.first_point:
                offset += shape.points[0];
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        draw(shape.render_position - shape.shape_top_left_within_border - offset + (Vector2i.Left * 1) + draw_offset, shape.bordered_bounds.size);
    }
    
    void configure_region_colors(sdf_region region, Color color) {
        set_param($"{region.ToString()}_color", color);
        set_param($"{region.ToString()}_color_secondary", color);
    }
    void configure_region_colors(sdf_region region, Color color, Color color_secondary) {
        set_param($"{region.ToString()}_color", color);
        set_param($"{region.ToString()}_color_secondary", color_secondary);
    }

    void remove_patterns(sdf_region region) {
        set_param($"{region.ToString()}_pattern", (int)sdf_pattern.NONE);
    }
        
    void pattern_dither(sdf_region region, int dither_resolution) {
        set_param($"{region.ToString()}_pattern", (int)sdf_pattern.DITHER);
        set_param($"{region.ToString()}_dither_res", dither_resolution);
    }
        
    void configure_outer_border_width(float pixel_width) => set_param("outer_border_width", pixel_width);
    void configure_inner_border_width(float pixel_width) => set_param("inner_border_width", pixel_width);
    
    internal override void update() {
        if (surface_resolution_method != null) {
            var surface_resolution_prev = surface_resolution;
            surface_resolution = surface_resolution_method.Invoke();
            
            if (surface_resolution != surface_resolution_prev)
                set_param("resolution", surface_resolution);
        }
    }
    
    public DrawShapesToSurface(Vector2i surface_resolution) : base(Resources.GetShaderInstance("sdf_shape")) {
        set_param("resolution", surface_resolution);
        Manager.register_for_update(this);
    }
    public DrawShapesToSurface(Func<Vector2i> surface_resolution_method) : base(Resources.GetShaderInstance("sdf_shape")) {
        this.surface_resolution_method = surface_resolution_method;
        surface_resolution = this.surface_resolution_method.Invoke();
        set_param("resolution", surface_resolution);
    }
}


