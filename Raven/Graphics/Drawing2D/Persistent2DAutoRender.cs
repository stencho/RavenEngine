using System;
using Raven.Engine;
namespace Raven.Graphics.Drawing2D;

[HashSetManaged]
public partial class AutoRender2D {
    public static partial class Manager {
        public static void RenderAll() {
            foreach (var ar2d in autorender2ds) {
                ar2d.draw_2d?.Invoke();
            }
        }
    }

    public Func<AutoRender2D> draw_2d;
    
    public AutoRender2D(Func<AutoRender2D> draw_2d) {
        this.draw_2d = draw_2d;
        Manager.Add(this);
    }

    ~AutoRender2D() {
        Manager.Remove(this);
    }
}