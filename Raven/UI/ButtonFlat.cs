using Microsoft.Xna.Framework;
using Raven.Engine;
using Raven.Graphics.Drawing2D;
using Raven.Graphics.InterpolatedTypes;
using Raven.UI.Forms;

namespace Raven.UI;

public class ButtonFlat : UIButton {
    private Lerper mouse_over_fade = new Lerper(0f, 1f, 200);

    public ButtonFlat(string text, string font = "profont") : base((int)Draw2D.measure_string(font, text).X, (int)Draw2D.measure_string(font, text).Y, text, font) {
        
    }

    public new void draw() {
        Draw2D.fill_rect(top_left, bottom_right, UIColors.Foreground);
        Draw2D.text(text, top_left + (size / 2f) - (measure_string(text) / 2f), UIColors.Background);
        Draw2D.rect(top_left, bottom_right, UIColors.Background, 1f);
    }
}