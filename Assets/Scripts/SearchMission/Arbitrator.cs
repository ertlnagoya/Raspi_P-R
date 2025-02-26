using System;
using System.Collections.Generic;
using LineTrace;
using NATS.Client;
using UnityEngine;
using System.IO;
using System.Security.Cryptography;

namespace Mission
{
    public class Arbitrator : MonoBehaviour
    {
        private Nats nats;
        private int agentsNumber;
        private Dictionary<int, Robot> robots = new Dictionary<int, Robot>();
        private List<int> priorityList = new List<int>();  // 按优先级存储机器人ID
        private Dictionary<int, List<int>> robotPaths = new Dictionary<int, List<int>>();
        private string Configuration = "unity_case.xml";

        private void Awake()
        {
            nats = new Nats();
            nats.Subscribe();
            nats.OnDemandReceived += HandleDemand;  // 监听 Nats 传递的请求
            Debug.Log("[Arbitrator] start.");
        }

        void Start()
        {
        }

        void Update()
        {
            GetAllRobots();
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
                //HandleRetRequest(demand);
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
            RegisterRobot(demand.Id, demand.Src, demand.Dst, demand.Goal, demand.Re);
            PathPlaning();
        }

        private void HandleRetRequest(Demand demand)
        {
            Debug.Log($"[Arbitrator] Generating path for Ret Request: Id={demand.Id}");
            UpdateRobot(demand.Id, demand.Src, demand.Dst);            
        }

        private void PathPlaning()
        {
            Debug.Log("[P&R] Start planning");
            
            string fileName = Path.Combine(Application.dataPath, "Scripts", "SearchMission", "Examples", Configuration);
            MissionSearch mission = new MissionSearch(fileName);
            //Debug.Log(fileName);
            if (!mission.GetMap())
            {
                Debug.LogError("Incorrect map! PathPlaning halted!");
            }
            else
            {
                //Debug.Log("Get map!");
                if (!mission.GetConfig())
                    Debug.LogError("Incorrect configurations! Program halted!");
                else
                {
                    Debug.Log("Get config!");
                    if (!mission.CreateLog())
                        Debug.LogError("Log channel has not been created! Program halted!");
                    else
                    {
                        //Debug.Log("Start searching");
                    }

                    mission.CreateAlgorithm();
                    int tasksCount = mission.getSingleExecution() ? 1 : mission.getTasksCount();
                    //Debug.Log("Start 2");
                    for (int i = 0; i < tasksCount; i++)
                    {
                        string agentsFile = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        
                                              
                        if (!mission.creatUnityTask(robots, priorityList))
                            Debug.LogError("Agent set has not been created! Program halted!");
                        else if (mission.checkAgentsCorrectness(agentsFile))
                        {
                            //Debug.Log("Starting search for agents file " + agentsFile);
                            mission.startSearch(agentsFile);
                            robotPaths = mission.getResults(priorityList);
                        }
                    }
                    Debug.Log("[P&R] All searches are finished!");
                }
            }
        }

        private void GetAllRobots()
        {
            PilotSync[] allPilot = FindObjectsOfType<PilotSync>(true);
            foreach (PilotSync pilot in allPilot)
            {
                if (!robots.ContainsKey(pilot.id))
                {
                    robots[pilot.id] = new Robot(pilot.id, pilot.src, pilot.dst, pilot.goal);
                    priorityList.Add(pilot.id);  // 将机器人ID加入优先级列表
                    //Debug.Log($"Robot {pilot.id} registered at point {pilot.src} to {pilot.dst}");
                }
                else
                {
                    robots[pilot.id].UpdateInfo(pilot.src, pilot.dst, pilot.goal);
                    UpdateRobotPriority(pilot.id);
                    //Debug.Log($"Robot {pilot.id} updated: new src={pilot.src}, new dst={pilot.dst}, new goal={pilot.goal}");
                }
            }
        }

        public void RegisterRobot(int id, int src, int dst, int goal, bool re)
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
                if (re)
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


