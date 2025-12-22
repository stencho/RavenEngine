using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Xna.Framework;
using ProjectRaven.Engine;
using ProjectRaven.Engine.Collision;

namespace RavenRPG.Engine.Collision.Solver {


    internal class NarrowPhaseSolver {
        public volatile ConcurrentQueue<narrow_queue_obj> queue = new ConcurrentQueue<narrow_queue_obj>();

        Thread solver_thread = null;

        BroadPhaseSolver parent_solver;

        public NarrowPhaseSolver(ref BroadPhaseSolver parent_solver) {
            this.parent_solver = parent_solver;

            this.solver_thread = new Thread(thread_loop);
            this.solver_thread.Start();
        }

        public bool working = false;

        void thread_loop() {

            while (State.running) {

                if (queue.Count > 0) {
                    working = true;
                    narrow_queue_obj nqo;
                    if (!queue.TryDequeue(out nqo)) continue;
                    if (nqo.A == 0) continue;

                    //World.internal_frame_probe.set("solving nqo " + nqo.A.ToString());

                    Shape3D shape_a = State.world.current_map.game_objects[nqo.A].collision.movebox;

                    if (State.world.current_map.game_objects[nqo.A].wants_movement.contains_nan()) {
                        State.world.current_map.game_objects[nqo.A].wants_movement = Vector3.Zero;
                    }

                    iterate:
                    if (!State.running) return;

                    bool hit = false;

                    foreach (int target in nqo.B) {
                        if (!State.running) return;

                        Shape3D shape_b = State.world.current_map.game_objects[target].collision.movebox;
                        collision_result 
                            result = GJK.gjk_intersects(
                                   shape_a, shape_b,
                                   State.world.current_map.game_objects[nqo.A].world,
                                   State.world.current_map.game_objects[target].world,
                                   State.world.current_map.game_objects[nqo.A].wants_movement,
                                   State.world.current_map.game_objects[target].wants_movement);


                        if (result.intersects) {
                            hit = true;

                            if (Vector3.Dot(State.world.current_map.game_objects[nqo.A].wants_movement, result.penetration_scalar) < Math3D.epsilon) {
                                State.world.current_map.game_objects[nqo.A].wants_movement += result.penetration_scalar;

                            } else {
                                result = GJK.swept_gjk_intersects_with_halving(
                                   shape_a, shape_b,
                                   State.world.current_map.game_objects[nqo.A].world,
                                   State.world.current_map.game_objects[target].world,
                                   State.world.current_map.game_objects[nqo.A].wants_movement,
                                   State.world.current_map.game_objects[target].wants_movement);

                                State.world.current_map.game_objects[nqo.A].wants_movement = result.sweep_end + result.penetration_scalar + result.sweep_slide;
                            }

                            
                            if (State.world.current_map.game_objects[nqo.A].wants_movement.contains_nan() ||
                                State.world.current_map.game_objects[nqo.A].wants_movement.Length() < Math3D.epsilon) {
                                State.world.current_map.game_objects[nqo.A].wants_movement = Vector3.Zero;
                                break;
                            }
                        } 
                    }
                    
                    State.world.current_map.game_objects[nqo.A].collision.solve.solver_iterations++;

                    State.world.current_map.game_objects[nqo.A].post_solve();

                    if (hit && State.world.current_map.game_objects[nqo.A].collision.solve.solver_iterations < 4)
                        goto iterate;

                    State.world.current_map.game_objects[nqo.A].position += State.world.current_map.game_objects[nqo.A].wants_movement;
                    State.world.current_map.game_objects[nqo.A].wants_movement = Vector3.Zero;

                    State.world.current_map.game_objects[nqo.A].post_solve();


                } else { working = false; }
            }
        }

    }
}
