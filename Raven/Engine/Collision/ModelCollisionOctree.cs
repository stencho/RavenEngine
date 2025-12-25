using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Raven.Graphics;
using Raven.Graphics.Drawing3D;

namespace Raven.Engine.Collision
{
    public class MCOctree
    {
        public Node[,,] nodes;

        public BoundingBox bounds;

        public MCOctree(Vector3 min, Vector3 max)
        {
            bounds = new BoundingBox(min, max);

            nodes = new Node[2, 2, 2];

            for (int z = 0; z < 2; z++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        nodes[x, y, z] = new Node(null, min, max, x, y, z);
                        nodes[x, y, z].color = RNG.random_opaque_color();
                    }
                }
            }

        }

        public void subdivide_all(int depth)
        {
            foreach (Node node in nodes)
            {
                if (!node.subdivided) node.subdivide();
                if (depth > 0) subdivide_all(node.nodes, depth, 1);
            }
        }

        internal void subdivide_all(Node[,,] nodes, int depth, int current_depth = 0)
        {
            foreach (Node node in nodes)
            {
                if (!node.subdivided) node.subdivide();
                if (depth > current_depth) subdivide_all(node.nodes, depth, current_depth + 1);
            }
        }

        public void draw(Camera camera, Matrix world)
        {
            Draw3D.cube(camera, bounds, world, Color.Purple);
            foreach (Node node in nodes)
            {
                node.draw(camera, world);
            }
        }

        public void add_value(int value, BoundingBox bb)
        {
            foreach (Node node in nodes)
            {
                if (bb.Intersects(node.bounds))
                {
                    node.values.Add(value);

                    if (node.subdivided)
                    {
                        add_value(node.nodes, value, bb);
                    }
                }
            }
        }

        void add_value(Node[,,] nodes, int value, BoundingBox bb)
        {
            foreach (Node node in nodes)
            {
                if (bb.Intersects(node.bounds))
                {
                    node.values.Add(value);

                    if (node.subdivided)
                    {
                        add_value(node.nodes, value, bb);
                    }
                }
            }
        }

        public HashSet<int> get_all_values(BoundingBox bb)
        {
            HashSet<int> values = new HashSet<int>();

            foreach (Node node in nodes)
            {
                if (bb.Intersects(node.bounds))
                {
                    if (node.subdivided)
                    {
                        recurse(node.nodes, bb, ref values);
                    }
                    else
                    {
                        foreach (int value in node.values)
                        {
                            values.Add(value);
                        }
                    }
                }
            }

            return values;
        }

        void recurse(Node[,,] nodes, BoundingBox bb, ref HashSet<int> values)
        {
            foreach (Node node in nodes)
            {
                if (bb.Intersects(node.bounds))
                {
                    if (!node.subdivided)
                    {
                        foreach (int value in node.values)
                        {
                            values.Add(value);
                        }
                    }
                    else
                    {
                        recurse(node.nodes, bb, ref values);
                    }
                }
            }
        }

    }

    public class Node
    {
        Node parent;

        public Node[,,] nodes;

        public bool subdivided = false;
        int x, y, z;

        public List<int> values = new List<int>();

        public BoundingBox bounds;

        public Color color;

        public Node(Node parent, Vector3 parent_min, Vector3 parent_max, int x, int y, int z)
        {
            this.parent = parent;
            this.x = x; this.y = y; this.z = z;

            var min = parent_min;
            var max = parent_max;
            var half = min + (max - min) / 2;

            if (x == 0)
                max.X = half.X;
            else if (x == 1)
                min.X = half.X;

            if (y == 0)
                max.Y = half.Y;
            else if (y == 1)
                min.Y = half.Y;

            if (z == 0)
                max.Z = half.Z;
            else if (z == 1)
                min.Z = half.Z;

            bounds = new BoundingBox(min, max);

        }

        public void draw(Camera camera, Matrix world)
        {
            if (values.Count > 0)
            {
                Draw3D.cube(camera, bounds, world, color);
            }

            if (subdivided)
            {
                foreach (Node node in nodes)
                {
                    //node.draw(world);
                }
            }

        }

        public void subdivide()
        {
            if (subdivided) return;

            subdivided = true;
            nodes = new Node[2, 2, 2];

            for (int z = 0; z < 2; z++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        nodes[x, y, z] = new Node(this, bounds.Min, bounds.Max, x, y, z);
                        nodes[x, y, z].color = RNG.similar_color(color, 0.3f);
                    }
                }
            }
        }
    }
}
