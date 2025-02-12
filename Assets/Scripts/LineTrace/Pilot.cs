using LineTrace.Handlers;
using RasPiMouse;
using System;
using System.Collections.Generic;
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
        private bool isMoving = false;

        private float reTime;

        private string[] RMColor = { "Red", "Blue", "Green", "Yellow", "Purple", "Orange", "Pink", "Brown", "Gray", "White" };
        private List<int> targetPointIndices = new List<int>();  // 存储目标点序号
        private int currentTargetIndex = 0;  // 当前目标点索引


        private void Awake()
        {
            mouse = GetComponent<Mouse>();
            handler = new Handler(mouse, RMColor[id]);
            nats = new Nats();
            nats.Subscribe(ChangeTargetPoints, id);

            goal = RandomGoal(-1);
            nats.Send("goal", new Demand {Id = id, Src = src, Dst = dst, Goal = goal});
        }

        private void ChangeTargetPoints(int[] newTargetIndices)
        {
            targetPointIndices.Clear();
            targetPointIndices.AddRange(newTargetIndices);
            currentTargetIndex = 0;
            isMoving = true;

            Debug.Log($"New path received: {string.Join(", ", targetPointIndices)}");

            if (targetPointIndices.Count == 0)
            {
                Debug.LogWarning("No target points available.");
                nats.Send("goal", new Demand { Id = id, Src = src, Dst = dst, Goal = goal });
                return;
            }
            if (dst != targetPointIndices[0])  // 起点
            {
                Debug.Log("Last point is NOT the end point.");
                return;
            }
            if (goal != targetPointIndices[targetPointIndices.Count - 1])  // endPointIndex 是终点索引
            {
                Debug.Log("Last point is NOT the end point.");
                return;
            }


        }

        private void Update()
        {
            if (re != 0)  // re means retry
            {
                mouse.Stop();

                reTime += Time.deltaTime;
                if (reTime > 1f)
                {
                    re--;
                    reTime = 0f;
                    nats.Send("goal", new Demand {Id = id, Src = src, Dst = dst, Goal = goal, Re = re == 0});
                }
            }
            else
            {
                handler.Handle();
                if (!reted && preCross && (preCross.transform.position - transform.position).magnitude > 0.05f)
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
                goal = RandomGoal(goal);
                nats.Send("goal", new Demand { Id = id, Src = src, Dst = dst, Goal = goal, Re = false });
                print("[Goal]" + " RM: " + RMColor[id] + " Goal count: " + level + " time[s]: " + Time.time);
            }

            if (currentTargetIndex + 1 < targetPointIndices.Count && dst == targetPointIndices[currentTargetIndex]) // How if next is not connect to des?
            {
                currentTargetIndex++;
                var next = targetPointIndices[currentTargetIndex];
                SetNext(next);
            }
            else
            {
                Debug.Log("Error in the plan ---> repaln from cuerrent position");
                handler.SetBack();
                nats.Send("goal", new Demand { Id = id, Src = dst, Dst = src, Goal = goal, Re = true });
            }    
        }

        private void SetNext(int next)
        {
            if (src == next)
            {
                handler.SetBack();
                nats.Send("ret", new Demand {Id = id, Src = dst, Dst = src});
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

        int RandomGoal(int preGoal)
        {            
            int[] points = { 259, 281, 251, 151, 12, 22, 104, 204, 211, 74, 130 };
            int newPoint;

            do
            {
                newPoint = points[Random.Range(0, points.Length)];
            }
            while (newPoint == preGoal);  // 确保新点不同于上次的点           
            return newPoint;
        }
    }
}