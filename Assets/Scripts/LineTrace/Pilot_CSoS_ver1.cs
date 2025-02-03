using LineTrace.Handlers;
using System.Collections.Generic;
using RasPiMouse;
using UnityEngine;
using Random = UnityEngine.Random;
using System.ComponentModel;

namespace LineTrace
{
    /// <summary>
    /// The Pilot class is responsible for controlling the mouse to reach a target location.
    /// </summary>
    public class Pilot_CSoS_ver1 : MonoBehaviour
    {
        /// <summary>
        /// Identifier for the pilot.
        /// </summary>
        public int id;

        /// <summary>
        /// Current position of the pilot.
        /// </summary>
        public int src;

        /// <summary>
        /// Destination of the pilot.
        /// </summary>
        public int dst;

        /// <summary>
        /// Next destination of the pilot.
        /// </summary
        private int next;

        /// <summary>
        /// Number of retries allowed.
        /// </summary>
        public int re;

        /// <summary>
        /// Goal position to reach.
        /// </summary>
        public int goal;

        /// <summary>
        /// Current level of the pilot.
        /// </summary>
        public int level;

        /// <summary>
        /// Mouse control object.
        /// </summary>
        private Mouse mouse;

        /// <summary>
        /// Handler object for managing mouse movements.
        /// </summary>
        private Handler handler;

        /// <summary>
        /// Communication object for sending data.
        /// </summary>
        private Nats nats;

        private RouteSearch routeSearch; // CSoS

        /// <summary>
        /// The last intersection that was crossed.
        /// </summary>
        private Cross preCross = null;

        /// <summary>
        /// Radius of the intersection area.
        /// </summary>
        private float radius = 0f;

        /// <summary>
        /// Released intersection flag.
        /// </summary>
        private bool reted = true;

        /// <summary>
        /// Timer for retrying.
        /// </summary>
        private float reTime;

        private string[] RMColor = { "Red", "Blue", "Green", "Yellow", "Purple", "Orange", "Pink", "Brown", "Gray", "White" };

        /// <summary>
        /// Initializes the components.
        /// </summary>
        private void Awake()
        {
            mouse = GetComponent<Mouse>();
            handler = new Handler(mouse, RMColor[id]);
            nats = new Nats();
            routeSearch = new RouteSearch();
            // エッジデータを作成
            List<RouteSearch.Path> edges = new List<RouteSearch.Path>
            {
                new RouteSearch.Path { Cross1 = 0, Cross2 = 1, Length = 1 },
                new RouteSearch.Path { Cross1 = 1, Cross2 = 2, Length = 1 },
                new RouteSearch.Path { Cross1 = 2, Cross2 = 3, Length = 2 },
                new RouteSearch.Path { Cross1 = 3, Cross2 = 4, Length = 2.1f },
                new RouteSearch.Path { Cross1 = 4, Cross2 = 5, Length = 1.3f },
                new RouteSearch.Path { Cross1 = 5, Cross2 = 6, Length = 2 },
                new RouteSearch.Path { Cross1 = 6, Cross2 = 7, Length = 1.2f },
                new RouteSearch.Path { Cross1 = 7, Cross2 = 0, Length = 1 },
                new RouteSearch.Path { Cross1 = 7, Cross2 = 8, Length = 2.5f },
                new RouteSearch.Path { Cross1 = 0, Cross2 = 8, Length = 1 },
                new RouteSearch.Path { Cross1 = 1, Cross2 = 8, Length = 1 },
                new RouteSearch.Path { Cross1 = 8, Cross2 = 9, Length = 3 },
                new RouteSearch.Path { Cross1 = 6, Cross2 = 9, Length = 1 },
                new RouteSearch.Path { Cross1 = 5, Cross2 = 9, Length = 0.8f },
                new RouteSearch.Path { Cross1 = 4, Cross2 = 10, Length = 1.4f },
                new RouteSearch.Path { Cross1 = 3, Cross2 = 10, Length = 0.8f },
                new RouteSearch.Path { Cross1 = 2, Cross2 = 10, Length = 1.7f },
            };
            routeSearch.Init(11, edges);
            goal = RandomGoal();
            nats.Send_CSoS("init", new Demand_CSoS { Id = id, Src = src, Dst = dst ,});
        }

        /// <summary>
        /// Called every frame to control mouse actions.
        /// </summary>
        private void Update()
        {
            if (re != 0)
            {
                mouse.Stop();

                reTime += Time.deltaTime;
                if (reTime > 1f)
                {
                    re--;
                    reTime = 0f;
                    next = routeSearch.DirectionDijkstra(dst, goal);
                    if( next >= 0){
                        int isPermit = nats.Send_CSoS("next", new Demand_CSoS { Id = id, Src = src, Dst = dst, Next = next, Goal = goal, Re = re == 0 });
                        if (isPermit == 1)
                        {
                            SetNext(next);
                            re = 0;
                        }
                    }
                    else{
                        re = -next; // next is negative value
                    }
                }
            }
            else
            {
                handler.Handle();
                if (!reted && preCross && (preCross.transform.position - transform.position).magnitude > 0.375f)
                {
                    reted = true;
                    nats.Send_CSoS("ret", new Demand_CSoS { Id = id, Src = src, Dst = dst });
                }
            }
        }
        
        /// <summary>
        /// Processes actions when the mouse enters an intersection.
        /// </summary>
        /// <param name="other">Collider object representing the intersection.</param>
        private void OnTriggerEnter(Collider other)
        {
            var cross = other.GetComponent<Cross>();
            if (!cross || preCross == cross) return;
            preCross = cross;
            radius = other.GetComponent<CapsuleCollider>().radius;

            if (!reted)
            {
                nats.Send_CSoS("ret", new Demand_CSoS { Id = id, Src = src, Dst = dst, });
                reted = true;
            }

            if (cross.number != dst)
            {
                // print($"failed : {src} -> {dst} != {cross.number}");
                print("[failed]" + " RM: " + RMColor[id] + $" {src} -> {dst} != {cross.number}");
                dst = cross.number;
            }

            if (cross.number == goal)
            {
                level++;
                goal = RandomGoal();
                print("[Goal]" + " RM: " + RMColor[id] + " Goal count: " + level + " time[s]: " + Time.time);
            }
            
            next = routeSearch.DirectionDijkstra(dst, goal);
            if( next >= 0){
                int isPermit = nats.Send_CSoS("next", new Demand_CSoS { Id = id, Src = src, Dst = dst, Next = next, Goal = goal, Re = false });
                if (isPermit == 1)
                {
                    SetNext(next);
                    re = 0;
                }
            }
            else{
                re = -next; // next is negative value
            }
        }

        /// <summary>
        /// Sets the next intersection to move towards.
        /// </summary>
        /// <param name="next">The number of the next intersection.</param>
        private void SetNext(int next)
        {
            if (src == next)
            {
                handler.SetBack();
                nats.Send_CSoS("ret", new Demand_CSoS { Id = id, Src = src, Dst = dst });
            }
            else
            {
                reted = false;
                handler.SetCross(preCross.transform.position, preCross.GetDir(next));
                print("[Route Set]" + " RM: " + RMColor[id] + " Src: " + src + " Dst: " + dst + " Next: " + next + " Goal: " + goal + " time[s]: " + Time.time);
            }

            src = preCross.number;
            dst = next;
            if (src == dst)
            {
                print($"{src} == {dst}");
            }
        }

        /// <summary>
        /// Generates a random goal that is not the current destination.
        /// </summary>
        /// <returns>The number of the next goal.</returns>
        private int RandomGoal()
        {
            var rv = Random.Range(0, 10);
            if (rv >= dst) rv++;
            return rv;
        }
    }
}
