using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Raven.Graphics.Effects {
    public class Dither : ManagedEffect {
        public void configure_shader(Vector2 top_left, Vector2 bottom_right, Color color_a, Color color_b, int pattern_size = 4, bool clip_b = false) {
            set_param("clip_b", clip_b);

            set_param("pattern_size", pattern_size);

            set_param("color_a", color_a);
            set_param("color_b", color_b);

            set_param("top_left", top_left);
            set_param("bottom_right", bottom_right);
        }

        public Dither(ContentManager content) : base(content, "Shaders/dither") {}
    }
}
