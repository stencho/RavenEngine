using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;

namespace Raven.UI.Fonts;

public class SpriteFontCharacterKerning {
    private SpriteFont original_font;
    private Dictionary<char, SpriteFont.Glyph> glyphs;
    private Texture2D texture;
    
    public SpriteFontCharacterKerning(SpriteFont font) {
        this.original_font = font;
        texture = font.Texture;
        glyphs = font.GetGlyphs();
        //
    }
    
    public void draw_string(string text, Vector2i position, Color color) {
        
    }
}