using System.Collections.Generic;
using Raven.Engine;
using Raven.Engine.Collision;

namespace Raven.UI.Forms;

public class TabbedPanel : IUIForm {
    public string name { get; }
    public string text { get; }
    public ui_layer_state layer_state { get; }
    public Vector2i position { get; set; }
    public Vector2i size { get; set; }
    public Vector2i client_top_left { get; }
    public Vector2i client_size { get; }
    public Vector2i client_bottom_right { get; }
    public bool mouse_over { get; }
    public bool has_focus { get; set; }
    public bool top_of_mouse_stack { get; set; }
    public bool visible { get; }
    public List<IUIForm> subforms { get; set; }
    public Dictionary<string, Collision2D.Shape2D> collision { get; }
    public IUIForm parent_form { get; set; }
    public List<string> mouse_interactions { get; }
    public bool test_mouse() {
        return false;
    }

    public void update() {
        
    }

    public void render_internal() {
        
    }

    public void draw() {
        
    }

    public string list_subforms() {
        return "";
    }
}