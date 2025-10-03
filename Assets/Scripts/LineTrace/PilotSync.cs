#define SelectGoal 

using LineTrace.Handlers;
using RasPiMouse;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LineTrace
{
    public class PilotSync : MonoBehaviour
    {
        public int id;
        public int src;
        public int dst;
        public bool re = false;

        public int goal;
        public int level;

        private Mouse mouse;
        private Handler handler;
        private Nats nats;

        private Cross preCross = null;
        private float radius = 0f;
        private bool reted = true;
        private bool isSetCross = true;

        private float reTime;
        public float capsuleRadius = 0.03f;
        private bool firstGoal = true;
        bool collision = false;

        private string[] RMColor = { "Red", "Blue", "Green", "Yellow", "Purple", "Orange", "Pink", "Brown", "Gray", "White" };
        private List<int> targetPointIndices = new List<int>();  // 存储目标点序号
        private int currentTargetIndex = 0;  // 当前目标点索引


        private void Awake()
        {
            mouse = GetComponent<Mouse>();
            handler = new Handler(mouse, RMColor[id]);
            nats = new Nats();
            nats.SubscribeSync(ChangeNext, id);
#if SelectGoal
            goal = SelectGoal(); 
#else
            goal = RandomGoal(-1);
#endif
        }
        private void Start()
        {

            //Debug.Log($"[start] robot {id} start from {src} to {dst}");

            nats.Send("goal", new Demand { Id = id, Src = src, Dst = dst, Goal = goal, Re = false });

        }

        private void ChangeNext(int newNext)
        {
            //Debug.Log($"robot {id} Reseived Next:{newNext}.");            
            SetNext(newNext);
        }
        private void Update()
        {
            if (re)  // re means retry 针对错误回复
            {
                mouse.Stop();
                dst = src;
                re = false;
                Debug.Log("ERROR next");
                nats.Send("goal", new Demand { Id = id, Src = src, Dst = dst, Goal = goal, Re = true });                
                return;
            }
            if (src == dst)
            {
                mouse.Stop();
                return;
            }
            if (!isSetCross)
            {
                //Debug.Log($"robot {id} getdir()" );
                try
                {
                    handler.SetCross(preCross.transform.position, preCross.GetDir(dst));
                }
                catch (System.Exception e)
                {
                    Debug.Log("Error in getdir()" + e.Message);
                    re = true;
                    return;
                }
                isSetCross = true;
            }
            //Debug.Log($"robot {id} is");
            if (collision)
            {
                nats.Send("goal", new Demand { Id = id, Src = dst, Dst = src, Goal = goal, Re = true});
                collision = true;
            }

            collision = handler.Handle();           
        }


        private void OnTriggerEnter(Collider other)
        {
            var cross = other.GetComponent<Cross>();
            if (!cross || preCross == cross) return;
            preCross = cross;
            radius = other.GetComponent<CapsuleCollider>().radius;

            //Debug.Log($"[Collider] robot {id} start from {src} to {dst} and cross is {cross.name}");
            /*
            if (firstGoal)
            {
                firstGoal = false;
                goal = RandomGoal(-1);
                nats.Send("goal", new Demand { Id = id, Src = src, Dst = dst, Goal = goal, Re = false });
                return;
            }
            */
            var preSrc = src;
            src = cross.number;
            if (cross.number != dst)
            {
                Debug.Log($"failed : robot {id} from {preSrc} to {dst} != cross {cross.number}");
                dst = cross.number;
                nats.Send("goal", new Demand { Id = id, Src = src, Dst = dst, Goal = goal, Re = true });
            }
            
            if (cross.number == goal)
            {
                level++;
#if SelectGoal
                goal = SelectGoal();
#else
                goal = RandomGoal(-1);
#endif
                nats.Send("goal", new Demand { Id = id, Src = src, Dst = dst, Goal = goal, Re = false });
                print("[Goal]" + " RM: " + RMColor[id] + " Goal count: " + level + " time[s]: " + Time.time);
            }   
        }

        private void SetNext(int next)
        {
            if (next == src)
            {
                reted = false;
                mouse.Stop();
            }
            else
            {
                reted = false;         
                dst = next;
                isSetCross = false;
            }

            //Debug.Log($"[setNext] robot {id} start from {src} to {dst}");
        }

        private int RandomGoal(int preGoal)
        {            
            int[] points = {119, 149, 140, 64, 10, 5, 40, 79, 96, 44, 61};
            int newPoint;

            do
            {
                newPoint = points[Random.Range(0, points.Length)];
            }
            while (newPoint == preGoal);  // 确保新点不同于上次的点           
            return newPoint;
        }

        private int[] goalList = { 44, 96, 64, 140, 5, 61, 79, 96, 119, 10, 40, 149};
        private int SelectGoal()
        {
            int goal = goalList[(level + id) % goalList.Length];
            return goal;
        }
    }
}