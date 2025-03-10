using System;
using System.Collections.Generic;
using LineTrace;
using NATS.Client;
using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Linq;

namespace Mission
{
    public class ArbitratorSync : MonoBehaviour
    {
        private Nats nats;
        private int agentsNumber;
        private Dictionary<int, Robot> robots = new Dictionary<int, Robot>();
        private List<int> priorityList = new List<int>();  // 按优先级存储机器人ID
        private Dictionary<int, List<int>> robotPaths = new Dictionary<int, List<int>>();
        private string Configuration = "unity_case.xml";
        private int pathIndex = 0;

        private void Awake()
        {
            nats = new Nats();
            nats.SubscribeSync();
            nats.OnDemandReceived += HandleDemand;  // 监听 Nats 传递的请求
            Debug.Log("[Arbitrator] start.");
        }

        void Start()
        {
        }

        void Update()
        {
            GetAllRobots();
            CheckAndSendNext();
        }

        public void CheckAndSendNext()
        {
            // 确保所有机器人都满足 Src == Dst
            bool allReached = robots.Values.All(robot => robot.Src == robot.Dst);

            if (!allReached)
                return;
            if (robotPaths.Count == 0)
            {
                Debug.Log("Error: robotPaths is empty!");
                PathPlaning();
                pathIndex = 0;   // 归零 pathIndex
                pathIndex++;     // 规划后立刻进入下一步
                SendNext();
                return;
            }
            foreach (var robot in robots.Values)
            {
                if (!robotPaths.ContainsKey(robot.Id))
                {
                    Debug.LogError($"Error: No path found for robot {robot.Id}. Replanning...");                    
                    return;
                }
            }          
            if (!CheckNextExist())
            {
                PathPlaning();
                pathIndex = 0;   // 归零 pathIndex
                pathIndex++;     // 规划后立刻进入下一步
                SendNext();
                return;
            }
            bool allMatched = robots.Values.All(robot => robot.Src == robotPaths[robot.Id][pathIndex]);
            if (!allMatched)
            {
                Debug.Log("Error: Some robots are not at the expected path index. Replanning...");
                PathPlaning();  // 重新规划路径
                pathIndex = 0;   // 归零 pathIndex
                pathIndex++;     // 规划后立刻进入下一步
                SendNext();
                return;
            }
            else
            {
                pathIndex++;
                SendNext();
                return;               
            }                     
        }
        private void SendNext()
        {
            Dictionary<int, int> nextPositions = new Dictionary<int, int>();
            foreach (var kvp in robotPaths)
            {
                string pathStr = string.Join(", ", kvp.Value); // 将 List<int> 转换为字符串
                Debug.Log($"Robot ID: {kvp.Key}, Path: [{pathStr}]");
            }

            foreach (var robot in robots.Values)
            {
                nextPositions[robot.Id] = robotPaths[robot.Id][pathIndex];
            }
            nats.SendNext(nextPositions);
        }

        private bool CheckNextExist()
        {
            foreach (var robot in robots.Values)
            {
                List<int> path = robotPaths[robot.Id];

                if (pathIndex + 1 >= path.Count)  // 确保能访问 path[pathIndex + 1]
                {
                    Debug.Log($"Error: Path index {pathIndex} out of bounds for robot {robot.Id}. Replanning...");
                    return false;
                }
            }
            return true;
        }

        private void HandleDemand(Demand demand, string subject)
        {
            Debug.Log($"[Arbitrator] Processing {subject.ToUpper()} Request: Id={demand.Id}, Src={demand.Src}, Dst={demand.Dst}, Goal={demand.Goal}, Re={demand.Re}");           

            if (subject == "goal")
            {
                HandleGoalRequest(demand);                
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
            
            //Debug.Log($"[Arbitrator] Generating path for Goal Request: Id={demand.Id}");
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
                return;
            }
            else
            {
                //Debug.Log("Get map!");
                if (!mission.GetConfig())
                {
                    Debug.LogError("Incorrect configurations! Program halted!");
                    return;
                }
                else
                {
                    if (!mission.CreateLog())
                    {
                        Debug.LogError("Log channel has not been created! Program halted!");
                        return;
                    }
                    mission.CreateAlgorithm();
                    int tasksCount = mission.getSingleExecution() ? 1 : mission.getTasksCount();
                    //Debug.Log("Start 2");
                    for (int i = 0; i < tasksCount; i++)
                    {
                        string agentsFile = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


                        if (!mission.creatUnityTask(robots, priorityList))
                        {
                            Debug.LogError("Agent set has not been created! Program halted!");
                            return;
                        }
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
            pathIndex = 0;
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
                    //UpdateRobotPriority(pilot.id);
                    //Debug.Log($"Robot {pilot.id} updated: new src={pilot.src}, new dst={pilot.dst}, new goal={pilot.goal}");
                }
            }
            if (true)
            {
                // Remove the left robot here!
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
                if (!re)
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


