using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Engine.Collision;
using Raven.Engine.Collision.Shapes3D;
using Raven.Engine.Geometry3D;
using Raven.Graphics.Effects;

namespace Raven.Engine.Geometry3D;

public class GeneratedCube : GeneratedGeometry {
    public Cube shape => _collision as Cube;
    
    private Shape3D _collision;
    public override Shape3D collision => _collision;
    
    private RenderPackage _render_info = new();
    public override RenderPackage render_info => _render_info;

    public GeneratedCube() {
        generate();
    }

    /*
    public Texture2D top_texture;
    public Texture2D bottom_texture;
    public Texture2D left_texture;
    public Texture2D right_texture;
    public Texture2D front_texture;
    public Texture2D back_texture;

    public void set_all_textures(Texture2D texture) {
        top_texture = texture;
        bottom_texture = texture;
        left_texture = texture;
        right_texture = texture;
        front_texture = texture;
        back_texture = texture;
    }
    */
    
    public void generate() {
        _collision = new Cube();
        
        render_info.build_vertex_buffer(
            //front
            new VertexPositionColorTexture(shape.A, Color.White, Vector2.Zero), //0
            new VertexPositionColorTexture(shape.B, Color.White, Vector2.UnitX),//1
            new VertexPositionColorTexture(shape.C, Color.White, Vector2.One),//2
            new VertexPositionColorTexture(shape.D, Color.White, Vector2.UnitY),//3
            
            //back
            new VertexPositionColorTexture(shape.E, Color.White, Vector2.Zero),//4
            new VertexPositionColorTexture(shape.F, Color.White, Vector2.UnitX),//5
            new VertexPositionColorTexture(shape.G, Color.White, Vector2.One),//6
            new VertexPositionColorTexture(shape.H, Color.White, Vector2.UnitY),//7
            
            //top
            new VertexPositionColorTexture(shape.A, Color.White, Vector2.Zero),//8
            new VertexPositionColorTexture(shape.B, Color.White, Vector2.UnitX),//9
            new VertexPositionColorTexture(shape.F, Color.White, Vector2.One),//10
            new VertexPositionColorTexture(shape.E, Color.White, Vector2.UnitY),//11
            
            //bottom
            new VertexPositionColorTexture(shape.C, Color.White, Vector2.Zero),//12
            new VertexPositionColorTexture(shape.D, Color.White, Vector2.UnitX),//13
            new VertexPositionColorTexture(shape.H, Color.White, Vector2.One),//14
            new VertexPositionColorTexture(shape.G, Color.White, Vector2.UnitY),//15
            
            //right
            new VertexPositionColorTexture(shape.B, Color.White, Vector2.Zero),//16
            new VertexPositionColorTexture(shape.F, Color.White, Vector2.UnitX),//17
            new VertexPositionColorTexture(shape.G, Color.White, Vector2.One),//18
            new VertexPositionColorTexture(shape.C, Color.White, Vector2.UnitY),//19
            
            //left
            new VertexPositionColorTexture(shape.A, Color.White, Vector2.Zero),//20
            new VertexPositionColorTexture(shape.E, Color.White, Vector2.UnitX),//21
            new VertexPositionColorTexture(shape.H, Color.White, Vector2.One),//22
            new VertexPositionColorTexture(shape.D, Color.White, Vector2.UnitY)//23
        );

        render_info.build_index_buffer(
            1,0,2,  3,2,0, //front
            4,5,6, 6,7,4, //back
            8,9,10, 10,11,8, //top
            12,13,14, 14,15,12, //bottom
            17,16,18, 19,18,16, //right
            20,21,22, 22,23,20 //left
        );

    }
}

public static partial class Geometry {
    public static partial class Collision {
        public static Cube cube = new();
    }

    public static partial class Generation {
        //public static (VertexBuffer V, IndexBuffer I) Cube() {
            
       // }
    }
}