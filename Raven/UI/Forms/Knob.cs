using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Raven.Engine;
using Raven.Graphics.Drawing2D;
using Raven.Graphics.InterpolatedTypes;

namespace Raven.UI.Forms;

public partial class UIKnob : IUIForm {
    public Vector2i client_size => size;
    public Vector2i client_top_left => top_left;
    public Vector2i client_bottom_right => bottom_right;

    private ConcurrentDictionary<string, float> indents;
    
    public bool allow_values_between_indents = true;

    private float min = 0f; 
    private float value = 0.5f;
    private float max = 1f;

    private float radius => size.X / 2;
    
    public UIKnob(Vector2i position, int size) {
        this.position = position;
        this.size = Vector2i.One * size;
        setup(position.X, position.Y, this.size.X, this.size.Y);
    }
    public UIKnob(Vector2i position, int size, params (string, float)[] indents) {
        this.position = position;
        this.size = Vector2i.One * size;
        
        this.indents = new ConcurrentDictionary<string, float>();
        foreach ((string text, float angle) in indents) {
            while (!this.indents.TryAdd(text, angle)) {}
        }
        setup(position.X, position.Y, this.size.X, this.size.Y);
    }
    
    public void update() {
        update_collision();
        test_mouse();
        update_all_subforms();
    }

    public void render_internal() {
    }

    private int outer_ring_width = 5;
    
    public void draw() {
        //draw outer ring
        Draw2D.fill_circle(position + (size/2), radius + 5, color_foreground);
        
        //fill center knob and then do outer ring of it over in solid
        Draw2D.circle(position + (size/2), radius, 3f, color_background);
        Draw2D.fill_circle_dither(position + (size/2), radius, color_foreground, color_background, 2);
        Draw2D.circle(position + (size/2), radius, 3f, color_foreground);
        //TODO make the dither shader able to take an offset and then move the dithering in a circle when the knob turns
        //TODO make circle shader able to skip parts of the circle 
        
        //draw knob pointer bit and a bit of foreground behind it just to make it pop against the dithering of the knob
        Vector2 angle = -Vector2.UnitY;
        Draw2D.line(position +
                    (size / 2) + (angle * (radius / 3f)),
            position + (size / 2) + (angle * radius),
            color_foreground, 7f);
        Draw2D.line(position + 
                    (size/2) + (angle * (radius - (radius / 2f))), 
                    position + (size/2) + (angle * radius), 
                    color_background, 3f);
        
        //DRAW INDENTS AS DITHERED LINES ON OUTER RING HERE
        
        //outer dark ring
        Draw2D.circle(position + (size/2), radius + 2, 3f, color_background);
        
        Draw2D.text_centered("mouse", text, position + (size/2) + (Vector2i.Down * (radius + 11)), color_foreground);
    }
    
    public void parent_size_changed(Vector2i new_size) { }
}