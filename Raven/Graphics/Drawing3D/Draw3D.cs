using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Graphics.Skybox;

namespace Raven.Graphics.Drawing3D;

    public class Draw3D {
        static VertexPositionColor[] verts = new VertexPositionColor[2];
        public static Effect line_effect;
        public static BasicEffect basic_effect;
        public static Texture2D onePXWhite;
        public static Texture2D testing_gradient;

        public static Vector3 find_any_line_perpendicular(Vector3 A, Vector3 B) {
            Vector3 AB = B - A;
            Vector3 dir = Vector3.Normalize(B - A);

            var cross = Vector3.Cross(dir, Vector3.Cross(dir, new Vector3(dir.X, dir.Z, -dir.Y)));
            if (cross.contains_nan())
                cross = Vector3.Cross(dir, Vector3.Cross(dir, new Vector3(-dir.Z, dir.Y, dir.X)));
            if (cross.contains_nan())
                cross = Vector3.Cross(dir, Vector3.Cross(dir, new Vector3(dir.Y, -dir.X, dir.Z)));

            return Vector3.Normalize(cross);
        }

        public static void line(Camera camera, Vector3 A, Vector3 B, Color color) {
            
            line_effect = Resources.GetShader("fill_gbuffer");
            //ContentLoader.resources["diffuse"].value_fx. = color.ToVector3();

            line_effect.Parameters["World"].SetValue(Matrix.Identity);
            line_effect.Parameters["View"].SetValue(camera.view);
            line_effect.Parameters["Projection"].SetValue(camera.projection);
            line_effect.Parameters["DiffuseMap"].SetValue(onePXWhite);
            line_effect.Parameters["tint"].SetValue(color.ToVector3());
            //line_effect.Parameters["FarClip"].SetValue(2000f);
            //line_effect.Parameters["opacity"].SetValue(-1f);

            verts[0] = new VertexPositionColor(A, color);
            verts[1] = new VertexPositionColor(B, color);

            var bs = State.graphics_device.BlendState;
            //line_effect.DiffuseColor = color.ToVector3();

            State.graphics_device.BlendState = BlendState.Opaque;
            State.graphics_device.DepthStencilState = DepthStencilState.DepthRead;

            for (int i = 0; i < line_effect.CurrentTechnique.Passes.Count; i++) {
                line_effect.CurrentTechnique.Passes[i].Apply();
                State.graphics_device.DrawUserPrimitives(PrimitiveType.LineList, verts, 0, 1);
            }

            line_effect.Parameters["tint"].SetValue(Color.White.ToVector3());

            State.graphics_device.DepthStencilState = DepthStencilState.Default;
            State.graphics_device.BlendState = bs;
        }
        
        public static void lines(Camera camera, Color color, params Vector3[] points) {
            //if (color.A == 255) Renderer.deferred.render_lines_step(camera, color, Matrix.Identity, points);
            //else Renderer.forward.render_lines_step(camera, color, Matrix.Identity, points);
        }

        public static void swept_capsule(Camera camera, float radius, Vector3 AA, Vector3 AB, Vector3 BA, Vector3 BB, Color color) {
            Vector3 AAAB = Vector3.Normalize(AB - AA);
            Vector3 BABB = Vector3.Normalize(BB - BA);

            capsule(camera, AA, AB, radius, color);
            capsule(camera, BA, BB, radius, color);

            lines(camera, color,
                AA - (AAAB * radius), AB + (AAAB * radius),
                BA - (BABB * radius), BB + (BABB * radius),
                AA - (AAAB * radius)
            );

            lines(camera, color,
                AA - (AAAB * radius),
                AB + (AAAB * radius),
                BA - (BABB * radius),
                BB + (BABB * radius),
                AA - (AAAB * radius)
            );

            Vector3 C = Vector3.Normalize(Vector3.Cross(AAAB, (AA - BA)));

            Vector3 ABH = ((AA + AB) / 2f);
            Vector3 BBH = ((BA + BB) / 2f);

            line(camera, ABH - (C * radius), BBH - (C * radius), color);
            line(camera, ABH + (C * radius), BBH + (C * radius), color);

            lines(camera, color,
                AA - (C * radius),
                AB - (C * radius),
                BA - (C * radius),
                BB - (C * radius),
                AA - (C * radius)
            );

            lines(camera, color,
                AA + (C * radius),
                AB + (C * radius),
                BA + (C * radius),
                BB + (C * radius),
                AA + (C * radius)
            );

        }

        public static void xyz_cross(Camera camera, Vector3 P, float line_distance, Color color) {
            line(camera, P - (Vector3.UnitX * (line_distance / 2)), P + (Vector3.UnitX * (line_distance / 2)), color);
            line(camera, P - (Vector3.UnitY * (line_distance / 2)), P + (Vector3.UnitY * (line_distance / 2)), color);
            line(camera, P - (Vector3.UnitZ * (line_distance / 2)), P + (Vector3.UnitZ * (line_distance / 2)), color);
        }
        public static void gizmo(Camera camera, Vector3 P, Matrix world, float line_distance) {
            var dir = Vector3.Normalize(world.Right);
            line(camera, P - dir * line_distance, P + dir * line_distance, Color.Red);
            dir = Vector3.Normalize(world.Up);
            line(camera, P - dir * line_distance, P + dir * line_distance, Color.Green);
            dir = Vector3.Normalize(world.Backward);
            line(camera, P - dir * line_distance, P + dir * line_distance, Color.Blue);
        }

        public static void circle(Camera camera, Vector3 p, float radius, Vector3 normal, int subdivs, Color color) {
            if (subdivs < 6) return;
            Vector3[] verts = new Vector3[subdivs];

            normal = Vector3.Normalize(normal);

            var cross = Vector3.Normalize(Vector3.Cross(normal, Vector3.Cross(normal, new Vector3(normal.X, normal.Z, -normal.Y))));
            if (float.IsNaN(cross.X) || float.IsNaN(cross.Y) || float.IsNaN(cross.Z)) {
                cross = Vector3.Normalize(Vector3.Cross(normal, Vector3.Cross(normal, new Vector3(-normal.Z, normal.Y, normal.X))));
            }
            if (float.IsNaN(cross.X) || float.IsNaN(cross.Y) || float.IsNaN(cross.Z)) {
                cross = Vector3.Normalize(Vector3.Cross(normal, Vector3.Cross(normal, new Vector3(normal.Y, -normal.X, normal.Z))));
            }

            for (int i = 0; i < subdivs; i++) {
                verts[i] = p + (Vector3.Transform(cross, 
                    Matrix.CreateFromAxisAngle(normal, 
                        MathHelper.ToRadians(((float)i / (subdivs - 1)) * 360f))) * (radius));
            }

            lines(camera, color, verts);
        }
        
        public static void sphere(Camera camera, Vector3 P, float radius, Color color) {
            circle(camera, P, radius, Vector3.Up, 32, color);
            circle(camera, P, radius, Vector3.Right, 32, color);
            circle(camera, P, radius, Vector3.Forward, 32, color);
        }

        public static void sprite_line(Camera camera, GBuffer buffer, Vector3 a, Vector3 b, float line_width, Color color) {
            var pomn = CollisionHelper.line_closest_point(a, b, camera.position);

            var t = b-a;
            var scale = new Vector3(line_width, t.Length(), 1);

            var p = Vector3.Normalize(t);
            var p2 = Vector3.Normalize(pomn - camera.position);
            var c = Vector3.Normalize(Vector3.Cross(p, Vector3.Cross(p, p2)));

            Matrix billboard = Matrix.CreateConstrainedBillboard(a + (t / 2),
                (a + (t / 2)) - c, Vector3.Normalize(t), c, null);
            
            fill_quad(camera, buffer, Matrix.CreateScale(scale) * billboard,
                (Vector3.Up * 0.5f) + (Vector3.Left * 0.5f),
                (Vector3.Up * 0.5f) + (Vector3.Right * 0.5f),
                (Vector3.Down * 0.5f) + (Vector3.Right * 0.5f),
                (Vector3.Down * 0.5f) + (Vector3.Left * 0.5f), 
                color);
        }

        public static void capsule(Camera camera, Vector3 A, Vector3 B, float radius, Color color) {
            //line_effect.Parameters["World"].SetValue(Matrix.Identity);

            Vector3 AB = B - A;
            Vector3 normal = Vector3.Normalize(B - A);
            Vector3 origin = (A + B) / 2f;

            var cross = find_any_line_perpendicular(A, B);
            var criss = Vector3.Normalize(Vector3.Cross(normal, cross));

            line(camera, A - (normal * radius), B + (normal * radius), color);

            circle(camera, origin, radius, AB, 19, color);
            circle(camera, A, radius, AB, 19, color);
            circle(camera, B, radius, AB, 19, color);

            circle(camera, A, radius, cross, 19, color);
            circle(camera, B, radius, cross, 19, color);

            line(camera, A + (cross * radius), B + (cross * radius), color);
            line(camera, A - (cross * radius), B - (cross * radius), color);

            circle(camera, A, radius, criss, 19, color);
            circle(camera, B, radius, criss, 19, color);

            line(camera, A + (criss * radius), B + (criss * radius), color);
            line(camera, A - (criss * radius), B - (criss * radius), color);
        }

        public static void cylinder(Camera camera, Vector3 A, Vector3 B, float radius, Color color) {
            Vector3 AB = B - A;
            Vector3 normal = Vector3.Normalize(B - A);
            Vector3 origin = (A + B) / 2f;

            var cross = find_any_line_perpendicular(A, B);
            var criss = Vector3.Normalize(Vector3.Cross(normal, cross));

            line(camera, A, B, color);

            circle(camera, origin, radius, AB, 19, color);
            circle(camera, A, radius, AB, 19, color);
            circle(camera, B, radius, AB, 19, color);

            line(camera, A + (cross * radius), B + (cross * radius), color);
            line(camera, A - (cross * radius), B - (cross * radius), color);
            line(camera, A + (criss * radius), B + (criss * radius), color);
            line(camera, A - (criss * radius), B - (criss * radius), color);
        }

        public static void cube(Camera camera, Vector3 center, Vector3 size, Color color, Matrix world) {
            cube(camera, 
                Vector3.Transform(center + ((size.X) * Vector3.Right) + ((size.Y) * Vector3.Up) + ((size.Z) * Vector3.Forward), world),     //A
                Vector3.Transform(center + ((size.X) * Vector3.Left) + ((size.Y) * Vector3.Up) + ((size.Z) * Vector3.Forward), world),     //B
                Vector3.Transform(center + ((size.X) * Vector3.Right) + ((size.Y) * Vector3.Down) + ((size.Z) * Vector3.Forward), world),     //D
                Vector3.Transform(center + ((size.X) * Vector3.Left) + ((size.Y) * Vector3.Down) + ((size.Z) * Vector3.Forward), world),     //C

                Vector3.Transform(center + ((size.X) * Vector3.Right) + ((size.Y) * Vector3.Up) + ((size.Z) * Vector3.Backward), world),     //E
                Vector3.Transform(center + ((size.X) * Vector3.Left) + ((size.Y) * Vector3.Up) + ((size.Z) * Vector3.Backward), world),     //F
                Vector3.Transform(center + ((size.X) * Vector3.Right) + ((size.Y) * Vector3.Down) + ((size.Z) * Vector3.Backward), world),     //H
                Vector3.Transform(center + ((size.X) * Vector3.Left) + ((size.Y) * Vector3.Down) + ((size.Z) * Vector3.Backward), world),     //G

            color);
        }

        public static void square(Camera camera, Vector3 A, Vector3 B, Vector3 C, Vector3 D, Color color) {
            lines(camera, color, A, B, C, D, A);
        }

        public static void cube(Camera camera, Vector3 A, Vector3 B, Vector3 C, Vector3 D, Vector3 E, Vector3 F, Vector3 G, Vector3 H, Color color) {
            //top
            square(camera, A, E, F, B, color);
            //right
            square(camera, A, C, G, E, color);
            //left
            square(camera, F, H, D, B, color);
            //bottom
            square(camera, D, H, G, C, color);
        }

        public static void cube(Camera camera, BoundingBox bb, Color color) {
            cube(camera,(bb.Min + bb.Max) / 2, (bb.Max - bb.Min) / 2, color, Matrix.Identity);
        }
        public static void cube(Camera camera, BoundingBox bb, Matrix world, Color color) {
            cube(camera,(bb.Min + bb.Max) / 2, (bb.Max - bb.Min) / 2, color, world );
        }

        public static void load() {
            if (onePXWhite == null) {
                onePXWhite = new Texture2D(State.graphics_device, 1, 1);
                onePXWhite.SetData<Color>(new Color[1] { Color.White });

                testing_gradient = new Texture2D(State.graphics_device, 256, 256);

                Color[] glowData = new Color[256 * 256];
                for (var i = 0; i < 256; i++) {
                    for (var x = 0; x < 256; x++) {
                        glowData[(i * 256) + x] = Color.FromNonPremultiplied(i, 256 - i, i, 256);
                    }
                }
                testing_gradient.SetData(glowData);
                text_effect = new BasicEffect(State.graphics_device);
            }
        }

        public static BasicEffect text_effect;
        public static void text_3D(Camera camera, SpriteBatch sb, string text, string fontname, Vector3 offset, Vector3? normal, float scale, Color color, bool always_visible = false) {
            var t = Encoding.ASCII.GetString(Encoding.UTF8.GetBytes(text));
            Vector2 origin = Resources.GetFont(fontname).MeasureString(t) / 2f;
            text_effect.World = Matrix.CreateScale(scale, -scale, 0) * Matrix.CreateLookAt(Vector3.Zero, camera.view.Forward, Vector3.Up) * Matrix.CreateTranslation(offset);
            text_effect.View = camera.view;
            text_effect.Projection = camera.projection;
            text_effect.DiffuseColor = color.ToVector3();
            text_effect.TextureEnabled = true;
            sb.Begin(0, null, SamplerState.PointWrap, (always_visible ? DepthStencilState.None : DepthStencilState.DepthRead), RasterizerState.CullNone, text_effect);
            sb.DrawString(Resources.GetFont(fontname), t, Vector2.Zero, Color.White, 0, origin, 0.015f, SpriteEffects.None, 1);
            sb.End();
        }

        static string[] q_textures = new string[] { "OnePXWhite" };

        public static void triangle(Camera camera, Vector3 A, Vector3 B, Vector3 C, Color color) {
            lines(camera, color, A, B, C, A);
        }

        
        public static void fill_quad(Camera camera, GBuffer buffer, Matrix world, Vector3 A, Vector3 B, Vector3 C, Vector3 D, Color color, string texture = "OnePXWhite") {
            State.graphics_device.RasterizerState = RasterizerState.CullNone;

            //Renderer.deferred.render_step(camera, Renderer.fullscreen_quad.vertex_buffer, Renderer.fullscreen_quad.index_buffer, Resources.GetTexture(texture), world, color); 
            
            State.graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
            //draw_buffers(gd, q_vertex_buffer, q_index_buffer, world, color, Renderer.camera.view, Renderer.camera.projection);
        }
        

        public static void arrow(Camera camera, Vector3 A, Vector3 B, float chevron_distance_percent, Vector3 color) { arrow(camera, A, B, chevron_distance_percent, Color.FromNonPremultiplied(new Vector4(color, 1.0f))); }

        public static void arrow(Camera camera, Vector3 A, Vector3 B, float chevron_distance_percent, Vector4 color) { arrow(camera, A, B, chevron_distance_percent, Color.FromNonPremultiplied(color)); }

        public static void arrow(Camera camera, Vector3 A, Vector3 B, float chevron_distance_percent, Color color) {
            line(camera, A, B, color);
            Vector3 BA = (A - B) * chevron_distance_percent;
            
            line_effect.Parameters["View"].SetValue(camera.view);
            line_effect.Parameters["Projection"].SetValue(camera.projection);
            
            VertexPositionColor[] verts = new VertexPositionColor[9];

            verts[0] = new VertexPositionColor(A, color);
            verts[1] = new VertexPositionColor(B, color);
            verts[2] = new VertexPositionColor(B + (Vector3.Cross(Vector3.Cross(BA, Vector3.Up), BA) * chevron_distance_percent) + (BA * chevron_distance_percent), color);

            verts[3] = new VertexPositionColor(B, color);
            verts[4] = new VertexPositionColor(B + (Vector3.Cross(Vector3.Cross(BA, Vector3.Down), BA) * chevron_distance_percent) + (BA * chevron_distance_percent), color);

            verts[5] = new VertexPositionColor(B, color);
            verts[6] = new VertexPositionColor(B + (Vector3.Cross(Vector3.Cross(BA, Vector3.Left), BA) * chevron_distance_percent) + (BA * chevron_distance_percent), color);

            verts[7] = new VertexPositionColor(B, color);
            verts[8] = new VertexPositionColor(B + (Vector3.Cross(Vector3.Cross(BA, Vector3.Right), BA) * chevron_distance_percent) + (BA * chevron_distance_percent), color);

            line_effect.Parameters["tint"].SetValue(color.ToVector3());

            for (int i = 0; i < line_effect.CurrentTechnique.Passes.Count; i++) {
                line_effect.CurrentTechnique.Passes[i].Apply();
                State.graphics_device.DrawUserPrimitives(PrimitiveType.LineStrip, verts, 0, 8);
            }

            line_effect.Parameters["tint"].SetValue(Color.White.ToVector3());
        }
    }