using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mission
{
    public class AgentSet
    {
        private int width;
        private int height;

        // 使用 Dictionary 代替 std::map
        public Dictionary<(int, int), int> occupiedNodes = new Dictionary<(int, int), int>();
        private Dictionary<(int, int), int> connectivityComponents = new Dictionary<(int, int), int>();

        // 使用 List 代替 std::vector
        private List<int> componentSizes = new List<int>();

        // 使用 Dictionary 代替 std::multimap
        private Dictionary<(int, int), List<int>> subgraphNodes = new Dictionary<(int, int), List<int>>();

        // 使用 HashSet 代替 std::set
        private HashSet<(int, int)> subgraphPriorities = new HashSet<(int, int)>();

        // 使用 List 代替 std::vector
        private List<Agent> agents = new List<Agent>();

        public AgentSet()
        {
        }

        public bool CreatAgents(Map map, Config config)         
        {
            Console.WriteLine($"agent number in CreatAgents() is {config.maxAgents}");
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<(int i, int j)> generatedStart = new List<(int, int)>();
            List<(int i, int j)> generatedGoal = new List<(int, int)>();

            for (int index = 0; index < config.maxAgents; index++)
            {
                if (stopwatch.ElapsedMilliseconds > 100)
                {
                    return false;
                }
               
                //var startCell = map.getRandomAvalibleCell();
                //var goalCell = map.getRandomAvalibleCell();
            
                var startCell = map.getRandomAvalibleCross();
                var goalCell = map.getRandomAvalibleCross();

                
                if (startCell == (0, 0) || generatedStart.Contains(startCell)|| goalCell == (0, 0) || generatedGoal.Contains(goalCell) || startCell == goalCell)
                {
                    //Console.WriteLine($"Creat agent at exist position");
                    index--;
                    continue;
                }
                else
                {
                    int id = index;
                    int start_i = startCell.i;
                    int start_j = startCell.j;
                    int goal_i = goalCell.i;
                    int goal_j = goalCell.j;
                    //Console.WriteLine($"Creat agent {id} from ({start_i} , {start_j}) to ({goal_i} , {goal_j})");
                    addAgent(start_i, start_j, goal_i, goal_j);
                    generatedStart.Add(startCell);
                    generatedGoal.Add(goalCell);
                }
                //Debugger.Break();
            }
            return true;
        }

        public bool ReadAgents(string fileName)
        {
            XmlDocument doc = new XmlDocument();
            fileName = Path.Combine("Examples", fileName);
            //Console.WriteLine("XML file path: " + fileName);

            // 加载XML文件
            try
            {
                doc.Load(fileName);
                //
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening XML file: " + ex.Message);
                return false;
            }

            // 获取根元素
            XmlNode? root = doc.SelectSingleNode(Constants.CNS_TAG_ROOT);
            if (root == null)
            {
                Console.WriteLine("Error! No "+ Constants.CNS_TAG_ROOT + " tag found in XML file!");
                return false;
            }

            // 遍历代理节点
            foreach (XmlNode? node in root.ChildNodes)
            {
                if (node.Attributes != null)
                {
                    int id = int.Parse(node.Attributes["id"].Value);
                    int start_i = int.Parse(node.Attributes["start_i"].Value);
                    int start_j = int.Parse(node.Attributes["start_j"].Value);
                    int goal_i = int.Parse(node.Attributes["goal_i"].Value);
                    int goal_j = int.Parse(node.Attributes["goal_j"].Value);

                    addAgent(start_i, start_j, goal_i, goal_j);
                }
            }

            return true;
        }

        public void Clear()
        {
            occupiedNodes.Clear();
            agents.Clear();
        }

        public AgentSet Clone()
        {
            // 创建新实例
            var newAgentSet = new AgentSet
            {
                width = this.width,
                height = this.height,

                // 深拷贝 occupiedNodes
                occupiedNodes = new Dictionary<(int, int), int>(this.occupiedNodes),

                // 深拷贝 connectivityComponents
                connectivityComponents = new Dictionary<(int, int), int>(this.connectivityComponents),

                // 深拷贝 componentSizes
                componentSizes = new List<int>(this.componentSizes),

                // 深拷贝 subgraphNodes
                subgraphNodes = this.subgraphNodes.ToDictionary(
                    entry => entry.Key,
                    entry => new List<int>(entry.Value)
                ),

                // 深拷贝 subgraphPriorities
                subgraphPriorities = new HashSet<(int, int)>(this.subgraphPriorities),

                // 深拷贝 agents
                agents = this.agents.Select(agent => agent.Clone()).ToList() // 假设 Agent 类提供 Clone 方法
            };

            return newAgentSet;
        }


        public void addAgent(int start_i, int start_j, int goal_i, int goal_j)
        {
            occupiedNodes[(start_i, start_j)] = agents.Count;
            agents.Add(new Agent(start_i, start_j, goal_i, goal_j, agents.Count));
        }



        public int getAgentId(int i, int j)
        {
            return occupiedNodes[(i, j)];
        }

        public bool isOccupied(int i, int j)
        {
            return occupiedNodes.ContainsKey((i, j));
        }

        public void setAgentPosition(int agentId, Node pos)
        {
            agents[agentId].setCurPosition(pos.i, pos.j);
        }

        public void setPriority(int first, int second)
        {
            subgraphPriorities.Add((first, second));
        }

        public void setConnectedComponent(int i, int j, int compNum)
        {
            connectivityComponents[(i, j)] = compNum;
        }

        public void addComponentSize(int compSize)
        {
            componentSizes.Add(compSize);
        }

        public int getAgentCount()
        {
            return agents.Count;
        }

        public Agent getAgent(int i)
        {
            return agents[i];
        }

        public void moveAgent(ref Node from, ref Node to, List<AgentMove> result)
        {
            int id = occupiedNodes[(from.i, from.j)];
            //Console.WriteLine($"Push Node from ({from.i},{from.j}) to ({to.i},{to.j})");
            //Console.WriteLine($"moved id is {id}");
            //Debugger.Break();
            occupiedNodes[(to.i, to.j)] = id;
            occupiedNodes.Remove((from.i, from.j));
            agents[id].setCurPosition(to.i, to.j);
            result.Add(new AgentMove(to.i - from.i, to.j - from.j, id));

        }

        public bool readAgents(string FileName)
        {
            var doc = new XmlDocument();
            try
            {
                doc.Load(FileName);
            }
            catch (Exception)
            {
                Console.WriteLine("Error opening XML file!");
                return false;
            }

            var root = doc.DocumentElement;
            if (root == null || root.Name != Constants.CNS_TAG_ROOT)
            {
                Console.WriteLine($"Error! No '{Constants.CNS_TAG_ROOT}' tag found in XML file!");
                return false;
            }

            foreach (XmlNode node in root.ChildNodes)
            {
                if (node is XmlElement element)
                {
                    int id = int.Parse(element.GetAttribute("id"));
                    int start_i = int.Parse(element.GetAttribute("start_i"));
                    int start_j = int.Parse(element.GetAttribute("start_j"));
                    int goal_i = int.Parse(element.GetAttribute("goal_i"));
                    int goal_j = int.Parse(element.GetAttribute("goal_j"));
                    addAgent(start_i, start_j, goal_i, goal_j);
                }
            }
            return true;
        }

        public void setNodeSubgraph(int i, int j, int subgraphNum)
        {
            if (!subgraphNodes.ContainsKey((i, j)))
                subgraphNodes[(i, j)] = new List<int>();
            subgraphNodes[(i, j)].Add(subgraphNum);
        }

        public void setAgentSubgraph(int agentId, int subgraphNum)
        {
            agents[agentId].setSubgraph(subgraphNum);
        }

        public void removeSubgraphs(int i, int j)
        {
            subgraphNodes.Remove((i, j));
        }

        public List<int> getSubgraphs(int i, int j)
        {
            var res = new List<int>();
            if (subgraphNodes.TryGetValue((i, j), out var values))
            {
                res.AddRange(values);
            }
            return res;
        }

        public bool hasPriority(int first, int second)
        {
            return subgraphPriorities.Contains((first, second));
        }

        public int getConnectedComponentsCount()
        {
            return componentSizes.Count;
        }

        public int getComponentSize(int i, int j)
        {
            return componentSizes[connectivityComponents[(i, j)]];
        }

        public int getConnectedComponent(int i, int j)
        {
            return connectivityComponents[(i, j)];
        }


    }
}

