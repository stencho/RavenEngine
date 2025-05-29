using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RavenRPG.Engine.Collision.Solver {
    public struct narrow_queue_obj {
        public int A;
        public List<int> B;

        public narrow_queue_obj(int a, List<int> b) {
            A = a;
            B = b;
        }
    }

    public class contact_point {
        public int id;
        public Vector3 contact;
        public Vector3 normal;
        public int frames;
        public bool dead = false;

        public contact_point(int id, Vector3 contact, Vector3 normal) {
            this.id = id;
            this.contact = contact;
            this.normal = normal;
            this.frames = 2;
            
        }
    }

    public class solve_result {
        public List<(int id, collision_result result)> collision_steps = new List<(int id, collision_result result)>();

        public bool solved = false;
        public int solver_iterations = 0;

        public void reset() {
            collision_steps.Clear();
            solved = false; solver_iterations = 0;
        }
    }

    public class CollisionSolver {
        BroadPhaseSolver broad;
        NarrowPhaseSolver narrow;

        public CollisionSolver() {
            broad = new BroadPhaseSolver();
            narrow = new NarrowPhaseSolver(ref broad);

            broad.set_output_solver(ref narrow);
        }

        public bool solving = false;

        public void solve() {
            World.internal_frame_probe.set("solve start");
            while (State.drawing) { if (!State.running) return; }
            solving = true;
            foreach (var obj in State.world.current_map.game_objects.Keys) {
                if (State.world.current_map.game_objects[obj].dynamic) {
                    if (State.world.current_map.game_objects[obj].collision != null) {
                        State.world.current_map.game_objects[obj].collision.solve.reset();
                        //State.world.current_map.game_objects[obj].collision.contact_points.Clear();
                        broad.queue.Enqueue(obj);
                    } else {
                        State.world.current_map.game_objects[obj].position += State.world.current_map.game_objects[obj].wants_movement;
                        State.world.current_map.game_objects[obj].wants_movement = Vector3.Zero;
                        State.world.current_map.game_objects[obj].post_solve();
                    }
                }
            }

            while ((broad.queue.Count > 0 || narrow.queue.Count > 0 || broad.working || narrow.working) && State.running) { }

            solving = false;
            World.internal_frame_probe.set("solve end");
        }

    }
}
