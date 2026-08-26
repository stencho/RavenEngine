using Raven.Engine;
using Raven.Graphics.Effects;

namespace RavenEdit;

public class FontDraw : ManagedEffect {
    public FontDraw() : base(Resources.GetShader("r_font")) {
    }
}