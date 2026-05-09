using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Raven.Engine;
using Raven.Graphics.Drawing2D;

namespace Raven.UI.Forms.Layout;

public partial class LayoutStripManager : IUIForm {
    private UIWindow parent;
    
    public Vector2i client_size => size;
    public Vector2i client_top_left => top_left;
    public Vector2i client_bottom_right => bottom_right;
    
    List<LayoutStrip> strips = new();
    
    private int total_height() {
        var current_height = 0;
        foreach (var strip in strips) current_height += strip.height;
        return current_height;
    }

    public LayoutStripManager(UIWindow parent) {
        this.parent = parent;
        size = parent.client_size;
        setup(position.X, position.Y, size.X, size.Y);
        disable_focusing();
    }

    public void add_strip(params IUIForm[] strip_forms) {
        var strip = new LayoutStrip(this, strip_forms);
        strips.Add(strip);
        foreach (var form in strip.subforms) {
            add_subform(form);    
        }
    }
    
    public void update() {
        update_collision();
        size = parent.client_size;

        int c = 4;
        foreach (var strip in strips) {
            strip.top = c + strip.vertical_gap_top;
            c += strip.height + strip.vertical_gap_top + strip.vertical_gap_bottom;
        }

        foreach (var strip in strips) {
            strip.arrange();
            foreach (var form in strip.subforms) {
                form.update();
            }
        }
    }

    public void draw() {
        render_all_subform_internals();
        
        var current_height = 0;
        foreach (var strip in strips) {
            Vector2i size = new Vector2i(this.size.X, strip.height);
            
            foreach (var form in strip.subforms) {
                form.draw();
            }
            
            current_height += strip.height;
        }
        
    }

    public void parent_size_changed(Vector2i new_size) { 
        size = new_size;
        reconfigure_client_area();
    }

    public void render_internal() {}
}


public class LayoutStrip {
    public List<IUIForm> subforms = new();

    public int height => get_tallest() + vertical_gap_top + vertical_gap_bottom;
    public int top = 0;
    
    public int horizontal_form_gap = 2;
    public int horizontal_end_gaps = 4;
    
    public int vertical_gap_top = 1;
    public int vertical_gap_bottom = 0;
    
    public bool automatic_subform_width { get; set; } = true;
    
    private LayoutStripManager parent;
    public LayoutStrip(LayoutStripManager parent, params IUIForm[] subforms) {
        this.parent = parent;
        this.subforms.AddRange(subforms);
    }

    public void arrange() {
        int current_X = horizontal_end_gaps;
        int total_horizontal_gap = (horizontal_form_gap * subforms.Count) + (horizontal_end_gaps);
        int total_width = parent.size.X - (horizontal_end_gaps * 2);

        for (var index = 0; index < subforms.Count; index++) {
            
            if (automatic_subform_width) {
                subforms[index].size = new Vector2i((parent.size.X - total_horizontal_gap) / subforms.Count, subforms[index].size.Y);
                subforms[index].position = new Vector2i(current_X, top + (height / 2) - (subforms[index].size.Y / 2));

                if (index == subforms.Count - 1) {
                    if (subforms[index].bottom_right.X != parent.size.X - horizontal_end_gaps) {
                        var diff = (parent.size.X - horizontal_end_gaps) - subforms[index].bottom_right.X;
                        subforms[index].size += Vector2i.Right * diff;
                    }
                }
            } else {
            }

            current_X += subforms[index].size.X + horizontal_form_gap;
        }
    }
    
    public int get_tallest() {
        int h = 0;
        foreach (var form in subforms) {
            if (form.size.Y > h) {
                h = form.size.Y;
            }
        }
        return h;
    }
}