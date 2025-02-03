using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.Xml.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

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
                Console.WriteLine($"Warning: not enough agents in {agentsFile} agents file. This file will be ignored");
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
                    Console.WriteLine($"Warning: start or goal position of agent {agent.getId()} in {agentsFile} agents file is incorrect. This file will be ignored");
                    return false;
                }
            }

            for (int i = 0; i < agentSet.getAgentCount(); ++i)
            {
                for (int j = i + 1; j < agentSet.getAgentCount(); ++j)
                {
                    if (agentSet.getAgent(i).getStartPosition().Equals(agentSet.getAgent(j).getStartPosition()))
                    {
                        Console.WriteLine($"Warning: start positions of agents {i} and {j} in {agentsFile} are in the same cell. This file will be ignored");
                        return false;
                    }
                    else if (agentSet.getAgent(i).getGoalPosition().Equals(agentSet.getAgent(j).getGoalPosition()))
                    {
                        Console.WriteLine($"Warning: goal positions of agents {i} and {j} in {agentsFile} are in the same cell. This file will be ignored");
                        return false;
                    }
                }
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
            Console.WriteLine("AGENT number is " + maxAgents);
            //Console.WriteLine("agentsStep is " + config.agentsStep);

            for (int i = minAgents; i <= maxAgents; i += config.agentsStep)
            {
                AgentSet curAgentSet = new AgentSet();
                for (int j = 0; j < i; ++j)
                {
                    Agent agent = agentSet.getAgent(j);
                    curAgentSet.addAgent(agent.getCur_i(), agent.getCur_j(), agent.getGoal_i(), agent.getGoal_j());
                }

                multiagentSearch.clear();

                Stopwatch chronoTimer = Stopwatch.StartNew();
                var clockStart = DateTime.Now;
                
                sr = multiagentSearch.startSearch(map, config, curAgentSet);

                chronoTimer.Stop();
                Console.WriteLine($"Chrono time: {chronoTimer.ElapsedMilliseconds} ms");

                var clockEnd = DateTime.Now;
                Console.WriteLine($"Clock time: {(clockEnd - clockStart).TotalMilliseconds} ms");

                if (!sr.pathfound)
                {
                    Console.WriteLine($"Failed to find solution for {i} agents");
                    if (config.singleExecution)
                    {
                        Console.WriteLine("Log will not be created");
                    }
                    break;
                }

                agentsPaths = sr.agentsPaths;

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

                if (config.singleExecution)
                {

                    saveAgentsPathsToLog(agentsFile, sr.time[^1], sr.makespan[^1], sr.flowtime[^1],
                     0, 0,
                     0, 0,
                     0, 0);
                }

                if (!checkCorrectness())
                {
                    Console.WriteLine("Search returned incorrect results!");
                    break;
                }

                Console.WriteLine($"Found solution for {i} agents. Time: {sr.time[^1]}, flowtime: {sr.flowtime[^1]}, makespan: {sr.makespan[^1]}");
            }

            testingResults.Add(res);
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
                    Console.WriteLine("Incorrect result: agent path starts in wrong position!");
                    return false;
                }

                if (!agentsPaths[j][agentsPaths[j].Count - 1].Equals(agentSet.getAgent(j).getGoalPosition()))
                {
                    Console.WriteLine("Incorrect result: agent path ends in wrong position!");
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
                        Console.WriteLine("Incorrect result: agent path goes through obstacle!");
                        return false;
                    }

                    if (i > 0 &&
                        Math.Abs(agentsPaths[j][i].i - agentsPaths[j][i - 1].i) +
                        Math.Abs(agentsPaths[j][i].j - agentsPaths[j][i - 1].j) > 1)
                    {
                        Console.WriteLine("Incorrect result: consecutive nodes in agent path are not adjacent!");
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

    }
}


