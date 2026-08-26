using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CSScripting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Engine.Collision.Shapes3D;
using Raven.Graphics.Effects;

namespace Raven.Graphics.Drawing3D;

public static class NewGenDraw3D {
    internal struct LineDrawPacket3D {
        public Color color = Color.White;
        public Matrix world = Matrix.Identity;
        public bool close_loop = true;
        public bool ignore_depth = false;
        public Vector3[] points;

        public LineDrawPacket3D() { }
    }

    private static List<LineDrawPacket3D> line_draw_queue = new();

    public static void Line(Color color, Matrix world, bool close_loop, params Vector3[] points) {
        line_draw_queue.Add(new LineDrawPacket3D {
            color = color, world = world,
            close_loop = close_loop,
            points = points
        });
    }

    public static void Triangle(Color color, Matrix world, Vector3 A, Vector3 B, Vector3 C) {
        line_draw_queue.Add(new LineDrawPacket3D {
            color = color, world = world,
            points = [A,B,C]
        });
    } 
    public static void Quad(Color color, Matrix world, Vector3 A, Vector3 B, Vector3 C, Vector3 D) {
        line_draw_queue.Add(new LineDrawPacket3D {
            color = color, world = world,
            points = [A,B,C]
        });
    } 
    public static void QuadTriangles(Color color, Matrix world, bool alternate_indexing, Vector3 A, Vector3 B, Vector3 C, Vector3 D) {
        if (!alternate_indexing) {
            line_draw_queue.Add(new LineDrawPacket3D {
                color = color, world = world,
                points = [A, B, C]
            });
            line_draw_queue.Add(new LineDrawPacket3D {
                color = color, world = world,
                points = [A, C, D]
            });
        } else {
            line_draw_queue.Add(new LineDrawPacket3D {
                color = color, world = world,
                points = [A,B,D]
            });
            line_draw_queue.Add(new LineDrawPacket3D {
                color = color, world = world,
                points = [B,C,D]
            });
        }
    } 
    
    public static void Cube(Color color, Matrix world, Vector3 A, Vector3 B, Vector3 C, Vector3 D, Vector3 E, Vector3 F, Vector3 G, Vector3 H ) {
        line_draw_queue.Add(new LineDrawPacket3D {
            color = color, world = world,
            points = [A,B,C,D]
        });
        line_draw_queue.Add(new LineDrawPacket3D {
            color = color, world = world,
            points = [E,F,G,H]
        });
        line_draw_queue.Add(new LineDrawPacket3D {
            color = Color.Blue, world = world,
            close_loop = false,
            points = [A,E]
        });
        line_draw_queue.Add(new LineDrawPacket3D {
            color = Color.Purple, world = world,
            close_loop = false,
            points = [B,F]
        });
        line_draw_queue.Add(new LineDrawPacket3D {
            color = Color.Red, world = world,
            close_loop = false,
            points = [C,G]
        });
        line_draw_queue.Add(new LineDrawPacket3D {
            color = Color.Green, world = world,
            close_loop = false,
            points = [D,H]
        });
    }

    public static void Cone(Cone cone, Color color, Matrix world) {
        line_draw_queue.Add(new LineDrawPacket3D {
            color = color, world = world,close_loop = true,
            points = cone.points[1..]
        });
        for (int i = 1; i < cone.points.Length; i++) {
            line_draw_queue.Add(new LineDrawPacket3D {
                color = color, world = world,
                close_loop = false,
                points = [cone.points[0], cone.points[i]]
            });
        }
        
    }

    public static void render(Camera camera) {
        Renderer.forward.line_render_setup(camera);

        //line_draw_queue = (Queue<LineDrawPacket3D>)line_draw_queue.OrderBy(item => item.points.shortest_distance_to_camera(item.world, camera.position));
        line_draw_queue.ForEach(item => {
            Renderer.forward.render_lines_step(camera, item.color, item.world, item.close_loop, false, item.points);
        });
    }

    public static void clear_draw_list() => line_draw_queue.Clear();
}