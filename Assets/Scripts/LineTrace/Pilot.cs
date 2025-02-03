using LineTrace.Handlers;
using RasPiMouse;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LineTrace
{
    public class Pilot : MonoBehaviour
    {
        public int id;
        public int src;
        public int dst;
        public int re;

        public int goal;
        public int level;

        private Mouse mouse;
        private Handler handler;
        private Nats nats;

        private Cross preCross = null;
        private float radius = 0f;
        private bool reted = true;

        private float reTime;

        private string[] RMColor = { "Red", "Blue", "Green", "Yellow", "Purple", "Orange", "Pink", "Brown", "Gray", "White" };
        
        private void Awake()
        {
            mouse = GetComponent<Mouse>();
            handler = new Handler(mouse, RMColor[id]);
            nats = new Nats();

            goal = RandomGoal();
            nats.Send("init", new Demand {Id = id, Src = src, Dst = dst});
        }

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
                    var next = nats.Send("next", new Demand {Id = id, Src = src, Dst = dst, Goal = goal, Re = re == 0});

                    if (next >= 0)
                    {
                        SetNext(next);
                        re = 0;
                    }
                }
            }
            else
            {
                handler.Handle();
                if (!reted && preCross && (preCross.transform.position - transform.position).magnitude > 0.375f)
                {
                    reted = true;
                    nats.Send("ret", new Demand {Id = id, Src = src, Dst = dst});
                }
            }
        }


        private void OnTriggerEnter(Collider other)
        {
            var cross = other.GetComponent<Cross>();
            if (!cross || preCross == cross) return;
            preCross = cross;
            radius = other.GetComponent<CapsuleCollider>().radius;

            if (!reted)
            {
                nats.Send("ret", new Demand {Id = id, Src = src, Dst = dst});
                reted = true;
            }

            if (cross.number != dst)
            {
                print($"failed : {src} -> {dst} != {cross.number}");
                dst = cross.number;
            }

            if (cross.number == goal)
            {
                level++;
                goal = RandomGoal();
                print("[Goal]" + " RM: " + RMColor[id] + " Goal count: " + level + " time[s]: " + Time.time);
            }

            var next = nats.Send("next", new Demand {Id = id, Src = src, Dst = dst, Goal = goal, Re = false});

            if (next >= 0)
            {
                SetNext(next);
                re = 0;
            }
            else
            {
                re = -next;
            }
        }

        private void SetNext(int next)
        {
            if (src == next)
            {
                handler.SetBack();
                nats.Send("ret", new Demand {Id = id, Src = src, Dst = dst});
            }
            else
            {
                reted = false;
                handler.SetCross(preCross.transform.position, preCross.GetDir(next));
            }

            src = preCross.number;
            dst = next;
            if (src == dst)
            {
                print($"{src} == {dst}");
            }
        }

//        private void OnTriggerExit(Collider other)
//        {
//            var cross = other.GetComponent<Cross>();
//            if (!cross || preCross != cross || reted) return;
//
//            nats.Send("ret", new Demand {Id = id, Src = src, Dst = dst});
//            reted = true;
//        }

        private int RandomGoal()
        {
            var rv = Random.Range(0, 10);
            if (rv >= dst) rv++;
            return rv;
        }
    }
}