using Raven.Engine;

namespace Raven.UI.Forms;

public partial class Label : IUIForm {
    public Vector2i client_size => size;
    public Vector2i client_top_left => top_left;
    public Vector2i client_bottom_right => top_right;
    
    
    
    public void update() {
        throw new System.NotImplementedException();
    }

    public void render_internal() {
        throw new System.NotImplementedException();
    }

    public void draw() {
        throw new System.NotImplementedException();
    }

    public void parent_size_changed(Vector2i new_size) {
        throw new System.NotImplementedException();
    }
}