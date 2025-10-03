using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.Xml.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.VisualScripting;
using System.Xml;

namespace Mission
{
    public class MissionSearch
    {
        private Map map;
        private AgentSet agentSet;
        private Config config;
        private string mapFile;
        private int searchType;
        private ILogger logger;
        private MultiagentSearchInterface? multiagentSearch = null;
        private MultiagentSearchResult? sr;
        private List<List<Node>>? agentsPaths; // Replacing std::vector<std::vector<Node>> with List<List<Node>>

        private List<TestingResults>? testingResults = new List<TestingResults>();

        // 带参数的构造函数
        public MissionSearch(string mapFile)
        {
            this.mapFile = mapFile;
            this.map = new Map();
            this.agentSet = new AgentSet();
            this.config = new Config();
            this.logger = new XmlLogger("defaultLogLevel");
            this.multiagentSearch = null;

            //logger = null;
            //multiagentSearch = null;
        }

        // 获取地图
        public bool GetMap()
        {
            
            return map.GetMap(mapFile);
        }

        public void ShowMap()
        {
            map.ShowMap();
        }
        // 读取代理信息
        public bool GetAgents(string agentsFile)
        {
            agentSet.Clear();
            return agentSet.ReadAgents(agentsFile);
        }

        // 获取配置
        public bool GetConfig()
        {
            return config.GetConfig(mapFile);
        }


        // 创建日志
        public bool CreateLog()
        {
            logger = new XmlLogger(config.LogParams[Constants.CN_LP_LEVEL]);
            return logger.GetLog(mapFile, config.LogParams);
        }

        public void CreateAlgorithm()
        {
            if (config.searchType == Constants.CN_ST_PR)
            {
                multiagentSearch = new PushAndRotate(new Astar(false));
            }
        }

        public int getTasksCount()
        {
            return config.tasksCount;
        }

        public int getFirstTask()
        {
            return config.firstTask;
        }

        public string getAgentsFile()
        {
            return config.agentsFile;
        }

        public bool getSingleExecution()
        {
            return config.singleExecution;
        }

        public bool getSaveAggregatedResults()
        {
            return config.saveAggregatedResults;
        }

        public bool checkAgentsCorrectness(string agentsFile)
        {
            if (config.maxAgents != -1 && agentSet.getAgentCount() < config.maxAgents)
            {
                UnityEngine.Debug.Log($"Warning: not enough agents in {agentsFile} agents file. This file will be ignored");
                return false;
            }

            for (int i = 0; i < agentSet.getAgentCount(); ++i)
            {
                Agent agent = agentSet.getAgent(i);
                Node start = agent.getStartPosition();
                Node goal = agent.getGoalPosition();

                if (!map.CellOnGrid(start.i, start.j) || !map.CellOnGrid(goal.i, goal.j) ||
                    map.CellIsObstacle(start.i, start.j) || map.CellIsObstacle(goal.i, goal.j))
                {
                    UnityEngine.Debug.Log($"Warning: start or goal position of agent {agent.getId()}: ({start.i},{start.j}) in {agentsFile} agents file is incorrect. This file will be ignored");
                    return false;
                }
            }

            for (int i = 0; i < agentSet.getAgentCount(); ++i)
            {
                for (int j = i + 1; j < agentSet.getAgentCount(); ++j)
                {
                    if (agentSet.getAgent(i).getStartPosition().Equals(agentSet.getAgent(j).getStartPosition()))
                    {
                        UnityEngine.Debug.Log($"Warning: start positions of agents {i} and {j} in {agentsFile} are in the same cell. This file will be ignored");
                        return false;
                    }
                    else if (agentSet.getAgent(i).getGoalPosition().Equals(agentSet.getAgent(j).getGoalPosition()))
                    {
                        //UnityEngine.Debug.Log($"Warning: goal positions of agents {i} and {j} in {agentsFile} are in the same cell.");
                       // return true;
                    }
                }
            }

            HashSet<Node> goalList = new HashSet<Node>();
            HashSet<Node> startList = new HashSet<Node>();
            for (int i = 0; i < agentSet.getAgentCount(); ++i)
            {
                while (goalList.Contains(agentSet.getAgent(i).getGoalPosition()))
                {
                    Node conflicPosition = agentSet.getAgent(i).getGoalPosition();
                    agentSet.getAgent(i).setGoalPosition(map.RandomConnectingCell(conflicPosition, goalList));
                    UnityEngine.Debug.Log($"Warning: goal positions of agents {i} adjusted from ({conflicPosition.i * map.getMapHeight() + conflicPosition.j} to {agentSet.getAgent(i).getGoalPosition().i * map.getMapHeight() + agentSet.getAgent(i).getGoalPosition().j}!");
                }
                goalList.Add(agentSet.getAgent(i).getGoalPosition());
            }
            

            return true;
        }

        public void startSearch(string agentsFile)
        {
            int minAgents = config.singleExecution ? config.maxAgents : config.minAgents;
            int maxAgents = config.maxAgents == -1 ? agentSet.getAgentCount() : config.maxAgents;
            //minAgents = 1;

            TestingResults res = new TestingResults();
            //Console.WriteLine("minAgents is " + minAgents);
            //UnityEngine.Debug.Log("AGENT number is " + maxAgents);
            //Console.WriteLine("agentsStep is " + config.agentsStep);

            for (int i = minAgents; i <= maxAgents; i += config.agentsStep)
            {
                AgentSet curAgentSet = new AgentSet();
                for (int j = 0; j < i; ++j)
                {
                    Agent agent = agentSet.getAgent(j);
                    curAgentSet.addAgent(agent.getCur_i(), agent.getCur_j(), agent.getGoal_i(), agent.getGoal_j());
                    //UnityEngine.Debug.Log($"search for agent {agent.getId()} from {agent.getStart_i() * map.getMapHeight() + agent.getStart_j()} to {agent.getGoal_i() * map.getMapHeight() + agent.getGoal_j()}");
                }

                multiagentSearch.clear();

                Stopwatch chronoTimer = Stopwatch.StartNew();
                var clockStart = DateTime.Now;
                
                
                sr = multiagentSearch.startSearch(map, config, curAgentSet);
                //UnityEngine.Debug.Log($"search result is {sr.pathfound}");
                chronoTimer.Stop();
                //UnityEngine.Debug.Log($"[P&R] Chrono time: {chronoTimer.ElapsedMilliseconds} ms");

                var clockEnd = DateTime.Now;
                //UnityEngine.Debug.Log($"[P&R] Clock time: {(clockEnd - clockStart).TotalMilliseconds} ms");

                if (!sr.pathfound)
                {
                    UnityEngine.Debug.LogError($"Failed to find solution for {i} agents");
                    if (config.singleExecution)
                    {
                        UnityEngine.Debug.LogError("Log will not be created");
                    }
                    break;
                }

                agentsPaths = sr.agentsPaths;
                /*
                res.data[Constants.CNS_TAG_ATTR_MAKESPAN][i] = sr.makespan;
                res.data[Constants.CNS_TAG_ATTR_FLOWTIME][i] = sr.flowtime;
                res.data[Constants.CNS_TAG_ATTR_TIME][i] = sr.time;
                res.data[Constants.CNS_TAG_ATTR_HLE][i] = sr.HLExpansions;
                res.data[Constants.CNS_TAG_ATTR_HLN][i] = sr.HLNodes;

                res.data[Constants.CNS_TAG_FOCAL_W][i] = sr.focalW;
                res.data[Constants.CNS_TAG_ATTR_TN][i] = sr.totalNodes;
                res.finalTotalNodes[i] = sr.finalTotalNodes;
                res.finalHLNodes[i] = (int)sr.finalHLNodes;
                res.finalHLNodesStart[i] = (int)sr.finalHLNodesStart;
                res.finalHLExpansions[i] = (int)sr.finalHLExpansions;
                res.finalHLExpansionsStart[i] = (int)sr.finalHLExpansionsStart;
                //UnityEngine.Debug.Log($"search result is {agentsFile}");
                /*
                if (config.singleExecution)
                {

                    try
                    {
                        saveAgentsPathsToLog(agentsFile, sr.time[^1], sr.makespan[^1], sr.flowtime[^1],
                                              0, 0,
                                              0, 0,
                                              0, 0);
                    }
                    catch (Exception ex)
                    {
                        // 捕获异常后可以在这里处理，比如记录日志或输出错误信息
                        //UnityEngine.Debug.Log("调用 saveAgentsPathsToLog 时发生异常: " + ex.Message);
                        // 如果需要，还可以将异常写入日志文件或执行其他恢复操作
                    }
                }
                */
                if (!checkCorrectness())
                {
                    UnityEngine.Debug.LogError("Search returned incorrect results!");
                    break;
                }

               // UnityEngine.Debug.Log($"[P&R] Found solution for {i} agents. Time: {sr.time[^1]}, flowtime: {sr.flowtime[^1]}, makespan: {sr.makespan[^1]}");
            }

            //testingResults.Add(res);
        }

        public void saveAgentsPathsToLog(
            string agentsFile,
            double time,
            double makespan,
            double flowtime,
            int HLExpansions,
            int HLNodes,
            int HLExpansionsStart,
            int HLNodesStart,
            double LLExpansions,
            double LLNodes)
        {
            logger.WriteToLogAgentsPaths(
                agentSet,
                agentsPaths,
                agentsFile,
                time,
                makespan,
                flowtime,
                HLExpansions,
                HLNodes,
                HLExpansionsStart,
                HLNodesStart,
                LLExpansions,
                LLNodes
            );

            logger.SaveLog();
        }



        public bool checkCorrectness()
        {
            int agentCount = agentsPaths.Count;
            int solutionSize = 0;

            for (int j = 0; j < agentCount; ++j)
            {
                solutionSize = Math.Max(solutionSize, agentsPaths[j].Count);
            }

            List<IEnumerator<Node>> starts = new List<IEnumerator<Node>>();
            List<IEnumerator<Node>> ends = new List<IEnumerator<Node>>();

            for (int j = 0; j < agentCount; ++j)
            {
                if (!agentsPaths[j][0].Equals(agentSet.getAgent(j).getStartPosition()))
                {
                    UnityEngine.Debug.LogError("[P&R] Incorrect result: agent path starts in wrong position!");
                    return false;
                }

                if (!agentsPaths[j][agentsPaths[j].Count - 1].Equals(agentSet.getAgent(j).getGoalPosition()))
                {
                    UnityEngine.Debug.Log($"[P&R] Incorrect result: agent path ends in wrong position! {agentsPaths[j][agentsPaths[j].Count - 1]} != {agentSet.getAgent(j).getGoalPosition()}");
                    return false;
                }

                starts.Add(agentsPaths[j].GetEnumerator());
                ends.Add(agentsPaths[j].GetEnumerator());
            }

            for (int i = 0; i < solutionSize; ++i)
            {
                for (int j = 0; j < agentCount; ++j)
                {
                    if (i >= agentsPaths[j].Count)
                    {
                        continue;
                    }

                    if (map.CellIsObstacle(agentsPaths[j][i].i, agentsPaths[j][i].j))
                    {
                        UnityEngine.Debug.Log("[P&R] Incorrect result: agent path goes through obstacle!");
                        return false;
                    }

                    if (i > 0 &&
                        Math.Abs(agentsPaths[j][i].i - agentsPaths[j][i - 1].i) +
                        Math.Abs(agentsPaths[j][i].j - agentsPaths[j][i - 1].j) > 1)
                    {
                        UnityEngine.Debug.LogError("[P&R] Incorrect result: consecutive nodes in agent path are not adjacent!");
                        return false;
                    }
                }
            }

            /*
            ConflictSet conflictSet = ConflictBasedSearch.findConflict<List<Node>.Enumerator>(starts, ends);

            if (!conflictSet.empty())
            {
                Conflict conflict = conflictSet.getBestConflict();
                if (conflict.edgeConflict)
                {
                    Console.WriteLine("Incorrect result: two agents swap positions!");
                }
                else
                {
                    Console.WriteLine("Incorrect result: two agents occupy the same node!");
                }
                return false;
            }
            */
            return true;
        }



        public bool creatTask()
        {
            agentSet.Clear();
            return agentSet.CreatAgents(map, config);
        }

        public bool creatUnityTask(Dictionary<int, Robot> robots, List<int> priorityList)
        {
            
            if (robots == null || priorityList == null)
            {
                UnityEngine.Debug.LogError("[P&R] Error: Robots dictionary or priority list is null.");
                return false;
            }

            if (robots.Count == 0)
            {
                UnityEngine.Debug.LogError("[P&R] Error: Robots dictionary is empty.");
                return false;
            }

            if (priorityList.Count == 0)
            {
                UnityEngine.Debug.LogError("[P&R] Error: Priority list is empty.");
                return false;
            }
            
            foreach (int robotId in robots.Keys)
            {
                if (!priorityList.Contains(robotId))
                {
                    UnityEngine.Debug.LogError($"[P&R] Error: Robot ID {robotId} is not in the priority list.");
                    return false;
                }
            }

            agentSet.Clear();
            foreach (int robotId in priorityList)
            {
                int start_i = 0, start_j = 0, goal_i = 0, goal_j = 0;
                if (robots.TryGetValue(robotId, out Robot robot))
                {
                    ParsePosition(robot.Dst, out start_i, out start_j);
                    ParsePosition(robot.Goal, out goal_i, out goal_j);
                    agentSet.addAgent(start_i, start_j, goal_i, goal_j);
                }
            }
            //Console.WriteLine("Priority List: " + string.Join(", ", priorityList));
            return true;

        }
        public Dictionary<int, List<int>> getResults(List<int> priorityList)
        {
            Dictionary<int, List<int>> paths = new Dictionary<int, List<int>>();
            for (int i = 0; i < agentsPaths.Count; ++i)
            {
                int positionStart = 0;
                int positionEnd = 0;

                Agent agent = agentSet.getAgent(i);
                var robotID = priorityList[i];
                ParseNode(agent.getStart_i(), agent.getStart_j(), out positionStart);
                ParseNode(agent.getGoal_i(), agent.getGoal_j(), out positionEnd);
                AddPositionToPath(robotID, positionStart, paths);

                for (int j = 0; j < agentsPaths[i].Count - 1; ++j)
                {
                    int positionMiddle = 0;
                    Node curNode = agentsPaths[i][j];
                    Node nextNode = agentsPaths[i][j + 1];
                    ParseNode(nextNode.i, nextNode.j, out positionMiddle);
                    AddPositionToPath(robotID, positionMiddle, paths);
                    //sectionElement.SetAttribute(Constants.CNS_TAG_ATTR_STARTX, curNode.j.ToString());
                    //sectionElement.SetAttribute(Constants.CNS_TAG_ATTR_STARTY, curNode.i.ToString());
                }
            }
            return paths;
        }

        private void ParsePosition(int position, out int i, out int j)
        {
            j = position % map.getMapHeight();  // 计算 b
            i = position / map.getMapHeight();  // 计算 a
        }

        private void ParseNode(int i, int j, out int position)
        {
            position = i * map.getMapHeight() + j; // Caution! different with ParsePosition
        }

        private void AddPositionToPath(int robotID, int position, Dictionary<int, List<int>> paths)
        {
            if (!paths.ContainsKey(robotID))
            {
                // 如果 robotID 还没有路径，创建一个新的 List
                paths[robotID] = new List<int>();
            }

            // 添加新位置到路径中
            paths[robotID].Add(position);
        }

    }
}


