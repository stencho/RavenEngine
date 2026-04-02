using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Engine.Collision;
using Raven.Engine.Collision.Shapes2D;
using Raven.Engine.Controls;
using Raven.Graphics.Drawing2D;

namespace Raven.UI;

public enum FormAnchor {
    TopLeft, TopCenter, TopRight,
    CenterLeft, Center, CenterRight,
    BottomLeft, BottomCenter, BottomRight
}

public interface IUIForm {
    string name { get; }
    string text { get; }

    ui_layer_state layer_state { get; }
    FormAnchor anchor { get; set; } 
        
    Vector2i position { get; set; }
    Vector2i absolute_position { get; }
    Vector2i size { get; set; }

    Vector2i top_left { get; }
    Vector2i bottom_right { get; }
    
    Vector2i client_size { get; }
    Vector2i client_top_left { get; }
    Vector2i client_bottom_right { get; }

    bool use_internal_rendering { get; }
    RenderTarget2D client_area { get; }

    bool mouse_over { get; } 
    bool has_focus { get; set; }
    bool top_of_mouse_stack { get; set; }
    bool visible { get; }

    List<IUIForm> subforms { get; set; }
    Dictionary<string, Collision2D.Shape2D> collision { get; }

    IUIForm parent_form { get; set; }

    List<string> mouse_interactions { get; }

    bool test_mouse();

    void update();
    void update_collision();
    void render_internal();
    void draw();
    void recurse_all_subforms(Action<IUIForm> run_on_all_subforms);
    string list_subforms();
    int get_form_depth();
}