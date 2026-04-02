using System;
using System.Collections.Generic;
using Raven.Engine;
using Raven.Engine.Collision;
using Raven.Graphics.Drawing2D;

namespace Raven.UI.Forms;

public partial class TabbedPanel : IUIForm {
    public int tab_bar_height = 16;
    
    public Vector2i client_top_left => (Vector2i.UnitY * tab_bar_height);
    public Vector2i client_size => size - (Vector2i.UnitY * tab_bar_height);
    public Vector2i client_bottom_right => client_top_left + size;

    public TabbedPanel(Vector2i position, Vector2i size) {
        setup(position.X, position.Y, size.X, size.Y);
        reconfigure_client_area();
    }
    
    public void update() {
        update_collision();
        test_mouse();
        foreach (var subform in subforms) {
            subform.update();
        }
    }

    public void render_internal() {
        foreach (var subform in subforms) {
            subform.render_internal();
            
        }
        Draw2D.fill_rect(Vector2i.Zero, client_size, UIColors.Background);
        foreach (var subform in subforms) {
            subform.draw();
        }
    }

    public void draw() {
        Draw2D.image(client_area, client_top_left, client_size);
    }
}