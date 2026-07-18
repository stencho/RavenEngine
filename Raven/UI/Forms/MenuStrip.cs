using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Engine.Collision;
using Raven.Graphics.Drawing2D;

namespace Raven.UI.Forms;

public partial class Menu : IUIForm {
    public Vector2i client_size { get; }
    public Vector2i client_top_left { get; }
    public Vector2i client_bottom_right { get; }
    public void update() {
        throw new NotImplementedException();
    }

    public void render_internal() {
        throw new NotImplementedException();
    }

    public void draw() {
        throw new NotImplementedException();
    }

    public void parent_size_changed(Vector2i new_size) {
        throw new NotImplementedException();
    }
}

public partial class MenuStrip : IUIForm {
    public Vector2i client_size => size;
    public Vector2i client_top_left => Vector2i.Zero;
    public Vector2i client_bottom_right => size;

    public List<ButtonFlat> menu_buttons = new List<ButtonFlat>();
    
    private int height = 20;
        
    public MenuStrip() {
        setup(0,0, State.resolution.X, height);
        reconfigure_client_area();
        disable_focusing();

        State.resolution_changed += () => {
            parent_size_changed(State.resolution);
        };
    }
    
    public void update() {
        update_collision();
        test_mouse();
        update_all_subforms();
    }

    public void render_internal() {
        if (!visible) return; 
        render_all_subform_internals();
        
        State.graphics_device.SetRenderTarget(client_area);
        State.graphics_device.Clear(UIColors.Background);

        Draw2D.fill_rect_dither(Vector2i.Zero, client_size,
            UIColors.Foreground, UIColors.Foreground.multiply_color(0.9f),
            height / 5);

        var x = 3;
        foreach (var button in menu_buttons) {
            button.position = Vector2i.Up + (Vector2i.Right * x);
            button.draw();
            x = button.size.X + 3;
        }
        
        draw_all_subforms();
        
        Draw2D.end();
    }

    public void draw() {
        if (!visible) return;
        Draw2D.image(client_area, position, size);
        Draw2D.line(Vector2i.Down * height, new Vector2i(State.resolution.X, height), UIColors.Background, 1f);
    }

    public void parent_size_changed(Vector2i new_size) {
        size = new Vector2i(new_size.X, height);
        reconfigure_client_area();
    }
}