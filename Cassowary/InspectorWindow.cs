using Raven.Console;
using Raven.Engine;
using Raven.UI;

namespace Cassowary.UI;

public class InspectorWindow : UIWindow {
    public InspectorWindow(IUIForm parent_form = null) : base(parent_form) {
        change_text("inspector");
        change_name("inspector");
    }
    public InspectorWindow(Vector2i position, Vector2i size, IUIForm parent_form = null) : base(position, size, parent_form) {
        change_text("inspector");
        change_name("inspector");
    }

    public override void update() {
        base.update();
        
    }
}