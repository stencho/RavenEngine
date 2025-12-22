using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace RavenRPG.Engine.Collision.Solver {
    internal class BroadPhaseSolver {
        public volatile ConcurrentQueue<int> queue = new ConcurrentQueue<int>();

        Thread solver_thread = null;

        NarrowPhaseSolver output_solver;

        public BroadPhaseSolver() {
            solver_thread = new Thread(thread_loop);
            solver_thread.Start();
        }

        public void set_output_solver(ref NarrowPhaseSolver output_solver) {
            this.output_solver = output_solver;
        }
        public bool working = false;
        void thread_loop() {
            while (State.running) {
                if (queue.Count > 0 && output_solver != null) {
                    working = true;

                    int obj = 0;
                    if (!queue.TryDequeue(out obj)) continue;
                    if (obj == 0) continue;


                    var current_obj = State.world.current_map.game_objects[obj];

                    if (current_obj.collision == null) continue;

                    var bb = current_obj.bounding_box();
                    return;
                    //var octree_hits = State.world.current_map.octree.objects_in_intersecting_nodes(bb);

                    narrow_queue_obj nqo;
                    List<int> bb_hits = new List<int>();


                    //foreach (var target in octree_hits) {
                    //foreach(var target in State.world.current_map.game_objects.Keys) { 
                        //if (State.world.current_map.game_objects[target].collision == null || target == obj) continue;
                        //var tbb =State.world.current_map.game_objects[target].bounding_box();
                        //if (tbb.Intersects(bb)) {
                             //bb_hits.Add(target);
                        //}
                    //}
                    /*
                    if (bb_hits.Count > 0) {
                        World.internal_frame_probe.set("solving bb " + obj.ToString());
                        nqo = new narrow_queue_obj(obj, bb_hits);
                        output_solver.working = true;
                        output_solver.queue.Enqueue(nqo);
                        output_solver.working = true;
                    } else {
                        State.world.current_map.game_objects[obj].position += State.world.current_map.game_objects[obj].wants_movement;
                        State.world.current_map.game_objects[obj].post_solve();
                        State.world.current_map.game_objects[obj].wants_movement = Vector3.Zero;
                    }
                    */
                } else if (output_solver == null) {
                    queue.Clear();
                }
                working = false;
            }
        }
    }
}
