using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using RavenRPG.Graphics;
using RavenRPG.Graphics.Drawing2D;
using RavenRPG.Graphics.Drawing3D;

namespace RavenRPG.Engine.Collision {

    public class Octree {
        public class Node {
            BoundingBox _bounds;
            
            Vector3 _size;
            Vector3 _center_pos;

            void bounding_box(Vector3 min, Vector3 max) {
                _bounds = new BoundingBox(min, max);
                _size = max - min;
                _center_pos = min + (_size / 2);
            }

            public BoundingBox bounds { get => _bounds; set { bounding_box(value.Min, value.Max); } }
            public Vector3 size => _size;
            public Vector3 center => _center_pos;

            public List<int> contains_objects;

            int _path;
            int _parent;
            int _depth;

            public int path => _path;
            public int depth => _depth;

            public bool subdivided = false;

            public Node(int path, int parent, int depth) {
                this._path = path;
                this._parent = parent;
                this._depth = depth;
            }
        }

        Vector3 _min, _max, _size;
        BoundingBox _bounds;
        float _width, _height, _depth;

        int _subdivisions; public float subdivisions => _subdivisions;


        Dictionary<int, Node> nodes = new Dictionary<int, Node>();

        public Node get_node(int node_path) => nodes[node_path];

        public Octree(Vector3 min, Vector3 max, int subdivisions) {
            if (subdivisions < 1 || subdivisions > 7) throw new Exception();

            _min = min;
            _max = max;

            _size = max - min;

            _bounds = new BoundingBox(min, max);

            _width = _size.X;
            _height = _size.Y;
            _depth = _size.Z;

            _subdivisions = subdivisions;

            subdivide_all();

        }


        public void update_leaves_within_radius(float radius, Vector3 center) {
            List<int> leaf_ids;

        }


        public int walk_test_node_corner = 0;        
        public int walk_test_node = 0;

        public Node[] get_all_nodes_at_path(int path) {
            if (path == 0 || !nodes[path].subdivided) return null;
            
            var l = new Node[8];
            int depth = get_path_depth(path) + 1;
            int c = 0;

            for (int z = 0; z < 2; z++) {
                for (int y = 0; y < 2; y++) {
                    for (int x = 0; x < 2; x++) {
                        int current_path = path | (1 << (4 * (depth)));
                        if (x > 0) enable_bit(ref current_path, (4 * (depth)) + 1);
                        if (y > 0) enable_bit(ref current_path, (4 * (depth)) + 2);
                        if (z > 0) enable_bit(ref current_path, (4 * (depth)) + 3);
                        l[c] = nodes[current_path];
                        c++;
                    }
                }
            }
            return l;
        }

        public List<int> leaf_nodes_in_ray(Ray ray) {
            var l = get_all_nodes_at_path(0 | (1 << 0));
            List<int> hits = new List<int>();

            foreach(var node in l) {
                if (node.subdivided) {
                    if (node.bounds.Intersects(ray) != null) {
                        raycast_recurse(ref hits, ray, node.path);
                    }
                } else {
                    if (node.bounds.Intersects(ray) != null) {
                        hits.Add(node.path);
                    }
                }
            }


            return hits;
        }
        void raycast_recurse(ref List<int> hits, Ray ray, int path) {
            var l = get_all_nodes_at_path(path);

            foreach (var node in l) {
                if (node.subdivided) {
                    if (node.bounds.Intersects(ray) != null) {
                        raycast_recurse(ref hits, ray, node.path);
                    }
                } else {
                    if (node.bounds.Intersects(ray) != null) {
                        hits.Add(node.path);
                    }
                }
            }
        }


        public List<int> leaf_nodes_in_boundingbox(BoundingBox bb) {
            var l = get_all_nodes_at_path(0 | (1 << 0));
            List<int> hits = new List<int>();

            foreach(var node in l) {
                if (node.subdivided) {
                    if (node.bounds.Intersects(bb)) {
                        bb_recurse(ref hits, bb, node.path);
                    }
                } else {
                    if (node.bounds.Intersects(bb)) {
                        hits.Add(node.path);
                    }
                }
            }

            return hits;
        }
        void bb_recurse(ref List<int> hits, BoundingBox bb, int path) {
            var l = get_all_nodes_at_path(path);

            foreach (var node in l) {
                if (node.subdivided) {
                    if (node.bounds.Intersects(bb)) {
                        bb_recurse(ref hits, bb, node.path);
                    }
                } else {
                    if (node.bounds.Intersects(bb)) {
                        hits.Add(node.path);
                    }
                }
            }
        }

        //TODO REPLACE THIS WITH DIVISION ONCE LEAF_NODE_SIZE EXISTS
        public int leaf_node_at_point(Vector3 point) {
            int start_node = 0b1000000000000000;
            int hit_node = 0;
            for (int z = 0; z < 2; z++) {
                for (int y = 0; y < 2; y++) {
                    for (int x = 0; x < 2; x++) {
                        int current_path = start_node;
                        if (x > 0) enable_bit(ref current_path, 1);
                        if (y > 0) enable_bit(ref current_path, 2);
                        if (z > 0) enable_bit(ref current_path, 3);
                        if (nodes[current_path].bounds.Contains(point) != ContainmentType.Disjoint) {
                            lnap_recurse(ref hit_node, 1, current_path, point);
                        }
                        if (hit_node != 0) return hit_node;
                    }
                }
            }
            return 0;
        }
        void lnap_recurse(ref int hit, int depth, int path, Vector3 point) {

            if (path == 0) return;
            if (!nodes[path].subdivided) {
                if (nodes[path].bounds.Contains(point) != ContainmentType.Disjoint) {
                    hit = path;
                }
                return;
            }

            for (int z = 0; z < 2; z++) {
                for (int y = 0; y < 2; y++) {
                    for (int x = 0; x < 2; x++) {
                        int current_path = path | (1 << (4 * (depth)));
                        if (x > 0) enable_bit(ref current_path, (4 * (depth)) + 1);
                        if (y > 0) enable_bit(ref current_path, (4 * (depth)) + 2);
                        if (z > 0) enable_bit(ref current_path, (4 * (depth)) + 3);

                        if (nodes[path].bounds.Contains(point) != ContainmentType.Disjoint) {
                            lnap_recurse(ref hit, depth + 1, current_path, point);
                        }
                    }
                }
            }
        }

        public List<int> leaf_nodes_in_boundingbox_bitwise_walk(BoundingBox bb) {
            int leaf_min = leaf_node_at_point(bb.Min);
            int leaf_max = leaf_node_at_point(bb.Max);

            int walk_id = leaf_min;
            //Debug.WriteLine($"{leaf_min} -> {leaf_max}");

            List<int> leaves = new List<int>();

            // HOW TO MAKE THIS TOMORROW OR WHATEVER

            // need to walk the ID from leaf_min to leaf_max,
            // basically the same way as the for x/y/z stuff above, 
            // but instead of 0-1, add X/Y/Z values to the Node class,
            // representing the X/Y/Z position in terms of nodes/axis
            // at a depth of 2 it would be 8 * 8 nodes, 4 nodes per axis,
            // so 32,32,32 would be just after the center point 

            // knowing this, we can walk the tree on each axis from min to max
            // and stop once we hit the difference between node[leaf_min].node_xyz and node[leaf_max].node_xyz 
            
            //lnibbbw_recurse();
            
            
            return leaves;

        }
        void lnibbbw_recurse() {

        }

        internal void draw_all_layers(int path) {
            int working_path = path;
            int depth = get_path_depth(path);
            int idepth = depth;

            while (depth > 0) {
                Draw3D.cube(nodes[working_path].bounds, Color.ForestGreen);
                move_up_one_level(ref working_path);
                depth--;
            }
        }

        public void draw_nodes() {
            //Draw3D.cube(Vector3.Zero, Vector3.One, Color.Red, Matrix.Identity);
            Draw3D.cube(_bounds, Color.MonoGameOrange);
            return;

            var ray_nodes = leaf_nodes_in_ray(new Ray(State.camera.position, State.camera.direction * State.camera.far_clip));


            //Draw3D.cube(nodes[ray_nodes[0]].bounds, Color.Red);
            foreach (int n in ray_nodes) {
                //draw_all_layers(n);
                Draw3D.cube(nodes[n].bounds, Color.MonoGameOrange);
            }



            var nn = get_all_nodes_at_path(walk_test_node);

            if (nn == null) {
                Draw3D.cube(nodes[walk_test_node].bounds, Color.ForestGreen);
            } else {

                foreach (Node nn_node in nn) {
                    Draw3D.cube(nn_node.bounds, Color.HotPink);
                }

                draw_all_layers(walk_test_node);


                Draw3D.cube(nodes[walk_test_node].bounds, Color.ForestGreen);
            }


        }

        static Vector2 tl = Vector2.One * 30f;

        public void draw_info_2D() {
            var l = get_all_nodes_at_path(walk_test_node);
            StringBuilder sb = new StringBuilder();

            int c = 0;
            if (l != null) {
                foreach (Node n in l) {
                    sb.Append($"n{binary_string_short(n.path)}");
                    c++;
                    if (c == 4) sb.Append(",\n");
                    else if (c != 8) sb.Append(", ");
                }
            }

            Draw2D.text($"{node_count_total} : {node_count_smallest}\n" +
                        $"{binary_string(step_left(walk_test_node))}<-{binary_string(walk_test_node)}->{binary_string(step_right(walk_test_node))}\n" +
                        $"{sb.ToString()}",
                Vector2.One * 7, Color.HotPink);

            /*
            Draw2D.text("pf",
                $"{}",
                Vector2.One * 7, Color.HotPink);
            */
        }






        void subdivide_all() {
            BoundingBox parent_bounds = new BoundingBox(_min, _max);

            for (int z = 0; z < 2; z++) {
                for (int y = 0; y < 2; y++) {
                    for (int x = 0; x < 2; x++) {
                        int tmp_id = 0;

                        //set the first section to, say, 1101
                        //VVVV
                        //1101 0000 0000 0000 0000 0000 0000
                        //first bit of each new section always set to 1

                        tmp_id |= (1 << 0);
                        if (x > 0) tmp_id |= (1 << 1);
                        if (y > 0) tmp_id |= (1 << 2);
                        if (z > 0) tmp_id |= (1 << 3);

                        Vector3 s = new Vector3(x,y,z);

                        BoundingBox pb = new BoundingBox(
                            (_min + ((_size / 2) * s)),
                            (_min + ((_size / 2) * s)) + (_size/2f));

                        nodes.Add(tmp_id, new Node(tmp_id, 0, 0));
                        nodes[tmp_id].bounds = pb;
                        subdivide(tmp_id, 1, pb);

                        //     VVVV
                        //1101 0000 0000 0000 0000 0000 0000
                    }
                }
            }
        }
        internal void subdivide(int id, int current_depth, BoundingBox parent_bounds) {
            int tmp_id = id | (1 << (4 * (current_depth)));

            if (current_depth >= subdivisions) {
                if (walk_test_node == 0) walk_test_node = id;
                walk_test_node_corner = walk_test_node;
                nodes[id].contains_objects = new List<int>();
                return;
            }

            nodes[id].subdivided = true;

            //set first bit of section to 1
            //     V
            //1101 1000 0000 0000 0000 0000 0000 0000

            for (int z = 0; z < 2; z++) {
                for (int y = 0; y < 2; y++) {
                    for (int x = 0; x < 2; x++) {
                        int nid = tmp_id;

                        //      VVV
                        //1101 1000 0000 0000 0000 0000 0000 0000
                        if (x > 0) nid |= (1 << (4 * current_depth) + 1);
                        if (y > 0) nid |= (1 << (4 * current_depth) + 2);
                        if (z > 0) nid |= (1 << (4 * current_depth) + 3);

                        Vector3 s = new Vector3(x,y,z);

                        BoundingBox pb = new BoundingBox(
                            (parent_bounds.Min + (((parent_bounds.Max - parent_bounds.Min) / 2) * s)),
                            (parent_bounds.Min + (((parent_bounds.Max - parent_bounds.Min) / 2) * s)) + ((parent_bounds.Max - parent_bounds.Min) /2f));

                        nodes.Add(nid, new Node(nid, id, current_depth));
                        nodes[nid].bounds = pb;
                        subdivide(nid, current_depth + 1, pb);
                        //move on to next section
                        //          VVVV
                        //1101 1010 0000 0000 0000 0000 0000 0000
                    }
                }
            }
        }

        public bool isbitset(int path, int bit) => ((path & (1 << bit)) != 0);

        public string binary_string(int path) {
            StringBuilder sb = new StringBuilder();
            for (int s = 0; s < 32; s += 1) {
                if (isbitset(path, s)) sb.Append("1");
                else sb.Append("0");
            }
            return sb.ToString();
        }

        /// <summary>
        /// simply cuts trailing 0s
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string binary_string_short(int path) {
            StringBuilder sb = new StringBuilder();
            for (int s = 0; s < 32; s += 1) {
                
                if (s % 4 == 0) { if (!isbitset(path, s)) break; }
                if (isbitset(path, s)) sb.Append("1");
                else sb.Append("0");
            }
            return sb.ToString();
        }

        public void enable_bit(ref int path, int bit) => path |= (1 << bit);
        public void disable_bit(ref int path, int bit) => path &= ~(1 << bit);

        public bool x_at_depth(int path, int depth) => ((path & (1 << (depth * 4) + 1)) != 0);
        public bool y_at_depth(int path, int depth) => ((path & (1 << (depth * 4) + 2)) != 0);
        public bool z_at_depth(int path, int depth) => ((path & (1 << (depth * 4) + 3)) != 0);

        public int node_count_smallest => _subdivisions * 8;
        public int node_count_total => nodes.Count;
        public int node_count(int depth) => (int)Math.Pow(8, depth);

        public int get_path_depth(int path) {
            int depth = 0;
            for (int s = 0; s < 32; s += 4) {
                if (isbitset(path, s)) depth++;
                else break;
            }
            depth -= 1;

            if (depth < 0) depth = 0;
            return depth;
        }

        public int move_up_one_level(int path) {
            int p = path;
            int d = get_path_depth(path);

            disable_bit(ref p, (d*4));
            disable_bit(ref p, (d*4)+1);
            disable_bit(ref p, (d*4)+2);
            disable_bit(ref p, (d*4)+3);

            return p;
        }

        public void move_up_one_level(ref int path) {
            path = move_up_one_level(path);            
        }

        #region step in direction
        public int step_left(int path) {
            int tmp_path = path;
            int flipped = 0;

            //get the depth of the path
            int depth = get_path_depth(path);
            int idepth = depth;

            while (depth > 0) {
                //can move left in this branch, set x at this depth to 0 then exit
                if (x_at_depth(path, depth)) {
                    disable_bit(ref tmp_path, (depth * 4) + 1);
                    break;

                    //can't move left in this branch, set x at this depth to 1, then move up
                } else {
                    enable_bit(ref tmp_path, (depth * 4) + 1);
                    flipped++; depth--;
                }
            }

            if (tmp_path == 0 || idepth == flipped) return path;
            return tmp_path;
        }

        public int step_right(int path) {
            int tmp_path = path;
            int flipped = 0;

            int depth = 0;
            depth = get_path_depth(path);
            int idepth  = depth;

            while (depth > 0) {
                //can move right in this branch, set x at this depth to 1 then exit
                if (!x_at_depth(path, depth)) {
                    enable_bit(ref tmp_path, (depth * 4) + 1);
                    break;

                    //can't move right in this branch, set x at this depth to 0, then move up
                } else {
                    disable_bit(ref tmp_path, (depth * 4) + 1);
                    flipped++; depth--;
                }
            }

            if (tmp_path == 0 || idepth == flipped) return path;
            return tmp_path;
        }

        public int step_forward(int path) {
            int tmp_path = path;
            int flipped = 0;

            int depth = 0;
            depth = get_path_depth(path);
            int idepth  = depth;

            while (depth > 0) {
                if (!z_at_depth(path, depth)) {
                    enable_bit(ref tmp_path, (depth * 4) + 3);
                    break;

                } else {
                    disable_bit(ref tmp_path, (depth * 4) + 3);
                    flipped++; depth--;
                }
            }

            if (tmp_path == 0 || idepth == flipped) return path;
            return tmp_path;
        }

        public int step_backward(int path) {
            int tmp_path = path;
            int flipped = 0;

            int depth = 0;
            depth = get_path_depth(path);
            int idepth  = depth;

            while (depth > 0) {
                if (z_at_depth(path, depth)) {
                    disable_bit(ref tmp_path, (depth * 4) + 3);
                    break;

                } else {
                    enable_bit(ref tmp_path, (depth * 4) + 3);
                    flipped++; depth--;
                }
            }

            if (tmp_path == 0 || idepth == flipped) return path;
            return tmp_path;
        }

        public int step_up(int path) {
            int tmp_path = path;
            int flipped = 0;

            int depth = 0;
            depth = get_path_depth(path);
            int idepth  = depth;

            while (depth > 0) {
                if (!y_at_depth(path, depth)) {
                    enable_bit(ref tmp_path, (depth * 4) + 2);
                    break;

                } else {
                    disable_bit(ref tmp_path, (depth * 4) + 2);
                    flipped++; depth--;
                }
            }

            if (tmp_path == 0 || idepth == flipped) return path;
            return tmp_path;
        }

        public int step_down(int path) {
            int tmp_path = path;
            int flipped = 0;

            int depth = 0;
            depth = get_path_depth(path);
            int idepth  = depth;

            while (depth > 0) {
                if (y_at_depth(path, depth)) {
                    disable_bit(ref tmp_path, (depth * 4) + 2);
                    break;

                } else {
                    enable_bit(ref tmp_path, (depth * 4) + 2);
                    flipped++; depth--;
                }
            }

            if (tmp_path == 0 || idepth == flipped) return path;
            return tmp_path;
        }
        #endregion

        #region step in direction, in place
        public void step_left(ref int path) => path = step_left(path);
        public void step_right(ref int path) => path = step_right(path);

        public void step_forward(ref int path) => path = step_forward(path);
        public void step_backward(ref int path) => path = step_backward(path);

        public void step_up(ref int path) => path = step_up(path);
        public void step_down(ref int path) => path = step_down(path);
        #endregion

        #region step in direction, report OOB
        public int step_left(int path, out bool at_edge) {
            int np = step_left(path);
            at_edge = (np == path);
            return np;
        }
        public int step_right(int path, out bool at_edge) {
            int np = step_right(path);
            at_edge = (np == path);
            return np;
        }

        public int step_forward(int path, out bool at_edge) {
            int np = step_forward(path);
            at_edge = (np == path);
            return np;
        }
        public int step_backward(int path, out bool at_edge) {
            int np = step_backward(path);
            at_edge = (np == path);
            return np;
        }

        public int step_up(int path, out bool at_edge) {
            int np = step_up(path);
            at_edge = (np == path);
            return np;
        }
        public int step_down(int path, out bool at_edge) {
            int np = step_down(path);
            at_edge = (np == path);
            return np;
        }

        #endregion

    }
}
