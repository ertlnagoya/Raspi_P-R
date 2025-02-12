using System;
using System.Collections.Generic;
using LineTrace;
using NATS.Client;
using UnityEngine;
using System.IO;

namespace Mission
{
    public class Arbitrator : MonoBehaviour
    {
        private Nats nats;
        private int agentsNumber;
        private Dictionary<int, Robot> robots = new Dictionary<int, Robot>();
        private List<int> priorityList = new List<int>();  // 按优先级存储机器人ID
        private Dictionary<int, int[]> robotPaths = new Dictionary<int, int[]>();
        private string Configuration = "unity_case.xml";

        private void Awake()
        {
            nats = new Nats();
            nats.Subscribe();
            nats.OnDemandReceived += HandleDemand;  // 监听 Nats 传递的请求
        }

        private void HandleDemand(Demand demand, string subject)
        {
            Debug.Log($"[Arbitrator] Processing {subject.ToUpper()} Request: Id={demand.Id}, Src={demand.Src}, Dst={demand.Dst}, Goal={demand.Goal}, Re={demand.Re}");           

            if (subject == "goal")
            {
                HandleGoalRequest(demand);
                nats.SendPath(robotPaths);
            }
            else if (subject == "ret")
            {
                HandleRetRequest(demand);
            }
            else
            {
                Debug.LogError("[Arbitrator] Unknown request type.");
                return;
            }

            // 发送路径数据
            
        }

        private void HandleGoalRequest(Demand demand)
        {
            
            Debug.Log($"[Arbitrator] Generating path for Goal Request: Id={demand.Id}");
            RegisterRobot(demand.Id, demand.Src, demand.Dst, demand.Goal);
            PathPlaning();
        }

        private void HandleRetRequest(Demand demand)
        {
            Debug.Log($"[Arbitrator] Generating path for Ret Request: Id={demand.Id}");
            UpdateRobot(demand.Id, demand.Src, demand.Dst);            
        }

        void PathPlaning()
        {
            string fileName = Path.Combine("Examples", Configuration);
            MissionSearch mission = new MissionSearch(fileName);
            if (!mission.GetMap())
            {
                Debug.Log("Incorrect map! PathPlaning halted!");
            }
            else
            {

                if (!mission.GetConfig())
                    Debug.Log("Incorrect configurations! Program halted!");
                else
                {

                    if (!mission.CreateLog())
                        Debug.Log("Log channel has not been created! Program halted!");
                    else
                    {
                        Debug.Log("Start searching");
                    }

                    mission.CreateAlgorithm();

                    int tasksCount = mission.getSingleExecution() ? 1 : mission.getTasksCount();

                    for (int i = 0; i < tasksCount; i++)
                    {
                        string agentsFile = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        if (!mission.creatUnityTask())
                            Debug.Log("Agent set has not been created! Program halted!");
                        else if (mission.checkAgentsCorrectness(agentsFile))
                        {
                            Debug.Log("Starting search for agents file " + agentsFile);
                            mission.startSearch(agentsFile);
                            robotPaths = mission.getResults();
                        }
                    }
                    Debug.Log("All searches are finished!");
                }
            }
        }

        public void RegisterRobot(int id, int src, int dst, int goal)
        {
            if (!robots.ContainsKey(id))
            {
                robots[id] = new Robot(id, src, dst, goal);
                priorityList.Add(id);  // 将机器人ID加入优先级列表
                Debug.Log($"Robot {id} registered at point {src} to {dst}");
            }
            else
            {
                robots[id].UpdateInfo(src, dst, goal);
                UpdateRobotPriority(id);
                Debug.Log($"Robot {id} updated: new src={src}, new dst={dst}, new goal={goal}");
            }
        }

        private void UpdateRobotPriority(int id)
        {
            if (priorityList.Contains(id))
            {
                priorityList.Remove(id);  // 先删除旧位置
                priorityList.Add(id);     // 再插入队列末尾
            }
        }

        public void UpdateRobot(int id, int src, int dst)
        {
            if (!robots.ContainsKey(id))
            {
                Debug.Log($"Robot {id} not registered for update");
            }
            else
            {
                robots[id].UpdateInfo(src, dst);
                Debug.Log($"Robot {id} updated: new src={src}, new dst={dst}");
            }
        }

        void OnApplicationQuit()
        {

        }
    }
}


