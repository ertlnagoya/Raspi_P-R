using System;
using System.Diagnostics;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq;



namespace Mission
{
    class PushAndRotate : MultiagentSearchInterface
    {
        public PushAndRotate(ISearch search)
        {
            this.search = search;
        }

        ~PushAndRotate()
        {
            if (search != null)
            {
                search = null;
            }
        }



        public bool clearNode(Map map, AgentSet agentSet, Node nodeToClear,
                      HashSet<Node> occupiedNodes)
        {
            Func<Node, Node, Map, AgentSet, bool> isGoal = (start, cur, mapInstance, agentSetInstance) =>
            {
                return !agentSetInstance.isOccupied(cur.i, cur.j);
            }; //Is node (i,j) occupied?
            //Console.WriteLine(agentSet.occupiedNodes);
            
            Dijkstra dijkstraSearch = new Dijkstra(); // Generic template for searching the map
            SearchResult searchResult = dijkstraSearch.startSearch(map, agentSet, nodeToClear.i, nodeToClear.j,
                                                                   0, 0, isGoal, true, true,
                                                                   0, -1, -1, occupiedNodes, null, false, null, DateTime.UtcNow, -1);
            
            if (!searchResult.pathfound)
            {
                return false;
            }
            
            // Assuming searchResult.lppath is a List<Node> //先找找路径 再move
            var path = searchResult.lppath.ToList();
            var reversedPath = path.AsEnumerable().Reverse().Skip(1);
            Node? previous = null;

            foreach (var current in reversedPath)
            {
                if (previous == null) // 初始化前一个节点
                {
                    previous = path.Last();
                }               

                if (agentSet.isOccupied(current.i, current.j))
                {
                    
                    Node from = current;
                    Node to = previous; // 直接使用前一个节点
                    agentSet.moveAgent(ref from, ref to, agentsMoves);
                }

                previous = current; // 更新前一个节点
            }
            //Debugger.Break();

            return true;
        }

        public ISearch? search { get; set; } = new ISearch();
        public List<AgentMove> agentsMoves { get; set; } = new List<AgentMove>();
        public MultiagentSearchResult result { get; set; } = new MultiagentSearchResult();

        public bool push(Map map, AgentSet agentSet,  Node from,  Node to, HashSet<Node> occupiedNodes)
        {
            
            if (occupiedNodes.Contains(to))  //
            {
                if (false)
                {
                    Console.WriteLine("pass here");
                    foreach (var node in occupiedNodes)
                    {
                        Console.WriteLine($"Node ocupied: {node.i}, Node j: {node.j}");
                    }
                    Console.WriteLine($"from is: {from.i}, Node j: {from.j}");
                    Console.WriteLine($"to is: {to.i}, Node j: {to.j}");
                    Debugger.Break();
                }

                return false;
            }
            if (agentSet.isOccupied(to.i, to.j)) //当前目标被占据
            {
                bool inserted = false;

                if (!occupiedNodes.Contains(from)) //加一个锁，在clear的时候不让你进from的点
                {
                    occupiedNodes.Add(from);
                    inserted = true;                   
                }
                //Console.WriteLine("push from " + from.i + "," + from.j + " to " + to.i + "," + to.j);
                bool canClear = clearNode(map, agentSet, to, occupiedNodes);
                
                if (inserted)
                {
                    occupiedNodes.Remove(from);
                }

                if (!canClear)
                {
                    return false;
                }
            }
            
            agentSet.moveAgent(ref from, ref to, agentsMoves);
            return true;
        }

        public bool multipush(Map map, AgentSet agentSet, Node first, Node second, ref Node to, List<Node> path)
        {
            // 如果路径的大小大于 1 且路径的第二个节点是 second，则交换 first 和 second，并移除路径的第一个节点
            //Console.WriteLine($"first ({first.i},{first.j}), second ({second.i},{second.j}), target ({to.i},{to.j}), flag {(path.Count > 1 && path[1].Equals(second))}");
            if (path.Count > 1 && path[1].Equals(second))
            {
                // 交换 first 和 second
                var temp = first;
                first = second;
                second = temp;

                // 移除路径的第一个节点
                path.RemoveAt(0);
            }
            //Console.WriteLine($"first ({first.i},{first.j}), second ({second.i},{second.j}), target ({to.i},{to.j})");
            Node prevNode = second;
            //Debugger.Break();
            // 遍历路径中的节点，直到倒数第二个节点
            for (int i = 0; i < path.Count - 1; i++)
            {
                Node curNode = path[i];
                Node nextNode = path[i + 1];

                // 创建 occupiedNodes 集合，包含 prevNode 和 curNode
                HashSet<Node> occupiedNodes = new HashSet<Node> { prevNode, curNode };

                // 检查 nextNode 是否被占用
                if (agentSet.isOccupied(nextNode.i, nextNode.j))
                {
                    //Console.WriteLine("pass here");
                    // 如果 nextNode 被占用，尝试清除它
                    if (!clearNode(map, agentSet, nextNode, occupiedNodes))
                    {
                        return false;
                    }
                }

                // 移动代理
                agentSet.moveAgent(ref curNode, ref nextNode, agentsMoves);
                agentSet.moveAgent(ref prevNode, ref curNode, agentsMoves);
                //Console.WriteLine($"move from ({curNode.i},{curNode.j}), to ({nextNode.i},{nextNode.j})");
                //Console.WriteLine($"move from ({prevNode.i},{prevNode.j}), to ({curNode.i},{curNode.j})");
                // 更新 prevNode
                
                prevNode = curNode;
            }
            return true;
        }


        public bool clear(Map map, AgentSet agentSet, ref Node first, ref Node second)
        {
            var successors = search.findSuccessors(first, map);
            HashSet<Node> unoccupied = new HashSet<Node>();

            foreach (var node in successors)
            {
                if (!agentSet.isOccupied(node.i, node.j))
                {
                    unoccupied.Add(node);
                }
            }

            if (unoccupied.Count >= 2)
            {
                return true;
            }

            HashSet<Node> forbidden = new HashSet<Node> { first, second };
            forbidden.UnionWith(unoccupied);

            foreach (var node in successors)
            {
                if (!unoccupied.Contains(node) && !node.Equals(second) && clearNode(map, agentSet, node, forbidden))
                {
                    if (unoccupied.Count >= 1)
                    {
                        return true;
                    }
                    unoccupied.Add(node);
                    forbidden.Add(node);
                }
            }

            if (unoccupied.Count == 0)
            {
                return false;
            }

            Node freeNeigh = unoccupied.First();
            foreach (var node in successors)
            {
                if (!node.Equals(second) && !node.Equals(freeNeigh))
                {
                    int curSize = agentsMoves.Count;
                    var newAgentSet = agentSet.Clone();// Assuming AgentSet has a Clone method
                    if (clearNode(map, newAgentSet, node, new HashSet<Node> { first, second }))
                    {
                        if (clearNode(map, newAgentSet, freeNeigh, new HashSet<Node> { first, second, node }))
                        {
                            agentSet = newAgentSet;
                            return true;
                        }
                    }
                    agentsMoves.RemoveRange(curSize, agentsMoves.Count - curSize);
                    break;
                }
            }

            foreach (var node in successors)
            {
                if (!node.Equals(second) && !node.Equals(freeNeigh))
                {
                    int curSize = agentsMoves.Count;
                    var newAgentSet = agentSet.Clone();
                    newAgentSet.moveAgent(ref first, ref freeNeigh, agentsMoves);
                    newAgentSet.moveAgent(ref second, ref first, agentsMoves);

                    if (clearNode(map, newAgentSet, node, new HashSet<Node> { first, second }))
                    {
                        if (clearNode(map, newAgentSet, second, new HashSet<Node> { first, second, node }))
                        {
                            agentSet = newAgentSet;
                            return true;
                        }
                    }
                    agentsMoves.RemoveRange(curSize, agentsMoves.Count - curSize);
                    break;
                }
            }

            int secondAgentId = agentSet.getAgentId(second.i, second.j);

            if (!clearNode(map, agentSet, second, new HashSet<Node> { first }))
            {
                return false;
            }

            agentSet.moveAgent(ref first, ref second, agentsMoves);
            Node secondPosition = agentSet.getAgent(secondAgentId).getCurPosition();

            if (!clearNode(map, agentSet, freeNeigh, new HashSet<Node> { first, second, secondPosition }))
            {
                return false;
            }

            for (int i = 0; i < successors.Count; i++)
            {
                var node = successors[i];
                if (!node.Equals(second) && !node.Equals(freeNeigh))
                {
                    agentSet.moveAgent(ref node, ref first, agentsMoves);
                    agentSet.moveAgent(ref first, ref freeNeigh, agentsMoves);
                    agentSet.moveAgent(ref second, ref first, agentsMoves);
                    agentSet.moveAgent(ref secondPosition, ref second, agentsMoves);

                    return clearNode(map, agentSet, freeNeigh, new HashSet<Node> { first, second, node });
                }
            }
            return false;
        }

        public void exchange(Map map, AgentSet agentSet, ref Node first, ref Node second)
        {
            // 调用 search.findSuccessors 获取相邻节点
            List<Node> successors = search.findSuccessors(first, map);
            List<Node> freeNeigh = new List<Node>();

            // 找到未被占用的相邻节点
            foreach (var node in successors)
            {
                if (!agentSet.isOccupied(node.i, node.j))
                {
                    freeNeigh.Add(node);
                }
            }

            var freeNeigh0 = freeNeigh[0];
            var freeNeigh1 = freeNeigh[1];
            // 执行交换逻辑
            agentSet.moveAgent(ref first, ref freeNeigh0, agentsMoves);   
            agentSet.moveAgent(ref second, ref first, agentsMoves);
            agentSet.moveAgent(ref first, ref freeNeigh1, agentsMoves);
            agentSet.moveAgent(ref freeNeigh0, ref first, agentsMoves);
            agentSet.moveAgent(ref first, ref second, agentsMoves);
            agentSet.moveAgent(ref freeNeigh1, ref first, agentsMoves);
            freeNeigh[0] = freeNeigh0;
            freeNeigh[1] = freeNeigh1;
        }

        public void reverse(int begSize, int endSize, int firstAgentId, int secondAgentId, AgentSet agentSet)
        {
            for (int i = endSize - 1; i >= begSize; --i)
            {
                // 获取当前移动记录
                AgentMove pos = agentsMoves[i].Copy();

                // 交换 agentId
                if (pos.id == firstAgentId)
                {
                    pos.id = secondAgentId;
                }
                else if (pos.id == secondAgentId)
                {
                    pos.id = firstAgentId;
                }

                // 获取当前 agent 的位置
                Node from = agentSet.getAgent(pos.id).getCurPosition();

                // 计算目标位置
                Node to = new Node(from.i - pos.di, from.j - pos.dj);

                // 执行移动
                agentSet.moveAgent(ref from, ref to, agentsMoves);
                
            }
            
            
        }

        public bool swap(Map map, ref AgentSet agentSet, ref Node first, ref Node second)
        {
            //Console.WriteLine($"First Node is ({first.i},{first.j})  and Second node is ({second.i},{second.j})");
            int firstAgentId = agentSet.getAgentId(first.i, first.j);
            int secondAgentId = agentSet.getAgentId(second.i, second.j);
            
            Func<Node, Node, Map, AgentSet, bool> isGoal = (start, cur, mapInstance, agentSetInstance) =>
            {
                return mapInstance.getCellDegree(cur.i, cur.j) >= 3;
            };

            var dijkstraSearch = new Dijkstra();
            var searchResult = dijkstraSearch.startSearch(map, agentSet, first.i, first.j, 0, 0, isGoal);

            
            while (searchResult.pathfound)
            {
                int begSize = agentsMoves.Count;
                var newAgentSet = agentSet.Clone(); // 假设 AgentSet 提供 Clone 方法
                var path = searchResult.lppath.ToList(); // 将路径转换为 List<Node>
                Node exchangeNode = path.Last();
                //Console.WriteLine($"exchangeNode Node is ({exchangeNode.i},{exchangeNode.j})");
                if (multipush(map, newAgentSet, first, second, ref exchangeNode, path))
                {
                    int exchangeAgentId = newAgentSet.getAgentId(exchangeNode.i, exchangeNode.j);
                    int neighAgentId = (exchangeAgentId == firstAgentId) ? secondAgentId : firstAgentId;
                    Node neigh = newAgentSet.getAgent(neighAgentId).getCurPosition();
                    //Console.WriteLine($"neigh Node is ({neigh.i},{neigh.j})");
                    if (clear(map, newAgentSet, ref exchangeNode, ref neigh))
                    {
                        //Debugger.Break();
                        agentSet = newAgentSet.Clone();
                        int endSize = agentsMoves.Count;

                        exchange(map, agentSet, ref exchangeNode, ref neigh);
                        //Debugger.Break();
                        reverse(begSize, endSize, firstAgentId, secondAgentId, agentSet);
                        
                        return true;
                    }
                }

                searchResult = dijkstraSearch.startSearch(map, agentSet, first.i, first.j, 0, 0, isGoal, false);
            }
            
            return false;
        }


        public bool rotate(Map map, AgentSet agentSet, List<Node> qPath, int cycleBeg)
        {
            int size = qPath.Count - cycleBeg;
            
            // 尝试找到未占用的节点进行旋转
            for (int i = cycleBeg; i < qPath.Count; ++i)
            {
                if (!agentSet.isOccupied(qPath[i].i, qPath[i].j))
                {
                    for (int j = 0; j < size - 1; ++j)
                    {
                        int from = cycleBeg + (i - cycleBeg - j - 1 + size) % size;
                        int to = cycleBeg + (i - cycleBeg - j + size) % size;
                        if (agentSet.isOccupied(qPath[from].i, qPath[from].j))
                        {
                            var qpathFrom = qPath[from];
                            var qpathTo = qPath[to];
                            agentSet.moveAgent(ref qpathFrom, ref qpathTo, agentsMoves);
                            qPath[from] = qpathFrom;
                            qPath[to] = qpathTo;
                        }
                    }
                    return true;
                }
            }

            // 初始化 cycleNodes 集合
            HashSet<Node> cycleNodes = new HashSet<Node>(qPath.Skip(cycleBeg));

            for (int i = cycleBeg; i < qPath.Count; ++i)
            {
                cycleNodes.Remove(qPath[i]);
                int firstAgentId = agentSet.getAgentId(qPath[i].i, qPath[i].j);
                int begSize = agentsMoves.Count;

                // 尝试清理节点
                if (clearNode(map, agentSet, qPath[i], cycleNodes))
                {
                    int endSize = agentsMoves.Count;
                    int secondAgentIndex = cycleBeg + (i - cycleBeg - 1 + size) % size;
                    int secondAgentId = agentSet.getAgentId(qPath[secondAgentIndex].i, qPath[secondAgentIndex].j);

                    // 移动代理
                    var qpathSecond = qPath[secondAgentIndex];
                    var qpathi = qPath[i];
                    agentSet.moveAgent(ref qpathSecond, ref qpathi, agentsMoves);
                    qPath[secondAgentIndex] = qpathSecond;
                    qPath[i] = qpathi;
                    Node curPosition = agentSet.getAgent(firstAgentId).getCurPosition();

                    // 调用 swap 交换位置
                    var qpathCurPosition = qPath[secondAgentIndex];
                    qpathi = qPath[i];
                    swap(map, ref agentSet, ref qpathi, ref qpathCurPosition);
                    curPosition = qpathCurPosition;
                    qPath[i] = qpathi;


                    // 完成剩余的旋转逻辑
                    for (int j = 0; j < size - 1; ++j)
                    {
                        int from = cycleBeg + (i - cycleBeg - j - 2 + size) % size;
                        int to = cycleBeg + (i - cycleBeg - j - 1 + size) % size;
                        if (agentSet.isOccupied(qPath[from].i, qPath[from].j))
                        {
                            var qpathFrom2 = qPath[from];
                            var qpathTo2 = qPath[to];
                            agentSet.moveAgent(ref qpathFrom2, ref qpathTo2, agentsMoves);
                            qPath[from] = qpathFrom2;
                            qPath[to] = qpathTo2;
                        }
                    }

                    // 反转操作
                    reverse(begSize, endSize, firstAgentId, secondAgentId, agentSet);

                    return true;
                }

                // 恢复 cycleNodes
                cycleNodes.Add(qPath[i]);
            }

            return false;
        }

        public void getPaths(AgentSet agentSet)
        {
            // 初始化 agentsPaths 列表大小
            agentsPaths = new List<List<Node>>(new List<Node>[agentSet.getAgentCount()]);

            // 初始化 agentPositions
            List<Node> agentPositions = new List<Node>();

            // 获取每个代理的起始位置并添加到路径中
            for (int i = 0; i < agentSet.getAgentCount(); ++i)
            {
                Node startPosition = agentSet.getAgent(i).getStartPosition();
                agentPositions.Add(startPosition);

                // 初始化每个代理的路径
                if (agentsPaths[i] == null)
                {
                    agentsPaths[i] = new List<Node>();
                }

                agentsPaths[i].Add(startPosition);
            }

            // 遍历代理的移动记录，更新路径
            for (int i = 0; i < agentsMoves.Count; ++i)
            {
                // 更新代理位置
                agentPositions[agentsMoves[i].id] = new Node
                {
                    i = agentPositions[agentsMoves[i].id].i + agentsMoves[i].di,
                    j = agentPositions[agentsMoves[i].id].j + agentsMoves[i].dj
                };

                // 添加当前代理的位置到路径
                for (int j = 0; j < agentSet.getAgentCount(); ++j)
                {
                    agentsPaths[j].Add(agentPositions[j]);
                }
            }
        }

        public void getParallelPaths(AgentSet agentSet, Config config)
        {
            int agentCount = agentSet.getAgentCount();
            List<List<Node>> agentsPositions = new List<List<Node>>(agentCount);
            List<int> agentInd = new List<int>(new int[agentCount]);
            Dictionary<Node, List<int>> nodesOccupations = new Dictionary<Node, List<int>>();
            Dictionary<Node, int> nodeInd = new Dictionary<Node, int>();            
            agentsPaths = new List<List<Node>>(agentCount);
            
            for (int i = 0; i < agentCount; ++i)
            {
                Node startPosition = agentSet.getAgent(i).getStartPosition(); 
                agentsPositions.Add(new List<Node> { startPosition });
                agentsPaths.Add(new List<Node> { startPosition });

                if (!nodesOccupations.ContainsKey(startPosition))
                {
                    nodesOccupations[startPosition] = new List<int>();  //把每个点的初始点位放入occupied
                    nodeInd[startPosition] = 0; // ？Nodeindex
                }
                nodesOccupations[startPosition].Add(i); //代理i占用startposition这个node
            }



            foreach (var move in agentsMoves) 
            {
                Node cur = agentsPositions[move.id][^1].DeepCopy();
                
                cur.i += move.di;
                cur.j += move.dj;
                /*
                Console.WriteLine("Node " + agentsPositions[move.id][^1].i.ToString() + " , " + agentsPositions[move.id][^1].j.ToString());
                Console.WriteLine("Node " + cur.i.ToString() + " , " + cur.j.ToString());
                Debugger.Break();
                */
                if (!nodesOccupations.ContainsKey(cur))  //移动后如果不被占据
                {
                    nodesOccupations[cur] = new List<int>(); //添加进占据
                    nodeInd[cur] = 0;
                }


                if (nodesOccupations[cur].Count > 0 && nodesOccupations[cur][^1] == move.id) //如果重复记录
                {
                    while (!(agentsPositions[move.id][^1] == cur)) //移动前 不等于 移动后
                    {
                        if (agentsPositions[move.id][^1] == cur) Debugger.Break();
                        Node curBack = agentsPositions[move.id][^1];  // curBack是移动前Node
                        int lastInd = nodesOccupations[curBack].LastIndexOf(move.id); //agent 最后一次在该Node占据列表出现的索引
                        if (lastInd >= 0)
                        {
                            nodesOccupations[curBack].RemoveAt(lastInd); //删除该索引元素
                        }
                        agentsPositions[move.id].RemoveAt(agentsPositions[move.id].Count - 1); //删除路径上一次位置
                    }
                }
                else
                {
                    agentsPositions[move.id].Add(cur); //记录这次移动的路径
                    nodesOccupations[cur].Add(move.id);  //当前点被占据列表加入agent i
                }
                //Console.WriteLine("next move ");
            }

            List<bool> finished = new List<bool>(new bool[agentCount]);
            while (true)
            {
                List<bool> hasMoved = new List<bool>(new bool[agentCount]); //每轮所有id是否完成
                
                for (int i = 0; i < agentCount; ++i) 
                {
                    if (hasMoved[i] || finished[i]) continue; //跳过已经移动（hasMoved[i] == true）或完成路径规划（finished[i] == true）的代理。

                    if (agentsPositions[i].Count == 1) //如果只剩一个
                    {
                        agentsPaths[i].Add(agentsPositions[i][0]);
                        finished[i] = true;
                        continue;
                    }

                    List<int> path = new List<int>(); //当前轮次中代理的移动路径，用于记录涉及冲突的代理。
                    int curAgent = i;
                    bool canMove = true;
                    while (true)
                    {
                        path.Add(curAgent);
                        Node nextNode = agentsPositions[curAgent][agentInd[curAgent] + 1].DeepCopy(); //获取当前代理的下一个目标节点 nextNode 
                                                                                           //agentInd[i] 表示代理 i 当前处理的节点在 agentsPositions[i] 中的索引位置。初始值为 0，表示代理从路径的起点开始。

                        
                        int lastInd = nodeInd[nextNode];   

                        if (nodesOccupations[nextNode][lastInd] == curAgent)  //如果目标节点的当前占用代理是 curAgent 本身，则不需要调整路径，直接跳出循环。
                        {
                            
                            break;
                        }
                        else if (nodesOccupations[nextNode][lastInd + 1] == curAgent) //如果目标节点的下一个占用才归cur，则。
                        {
                            int nextAgent = nodesOccupations[nextNode][lastInd]; //获取当前占用的agent
                            if (finished[nextAgent] || hasMoved[nextAgent] || nextAgent < curAgent ||
                                agentsPositions[nextAgent][agentInd[nextAgent]] != nextNode)  //如果该目标已经完成，本轮完成，错误 则本轮不移动该agent
                            {
                                canMove = false;
                                break;
                            }
                            curAgent = nextAgent; // 先操作nextAgent
                            if (curAgent == i) break;
                        }
                        else
                        {
                            canMove = false;
                            break;
                        }
                    }

                    if (canMove)
                    {
                        foreach (int agentId in path) //移动path里的agent
                        {
                            hasMoved[agentId] = true; //本轮移动了
                            nodeInd[agentsPositions[agentId][agentInd[agentId]]]++; //agentsPositions[agentId][agentInd[agentId]] 该agent操作到自己路径哪个位置了，找到这个位置的占用序列并加1
                            agentInd[agentId]++; //agent 处理下一个位置
                            agentsPaths[agentId].Add(agentsPositions[agentId][agentInd[agentId]]);//添加该路径
                            if (agentInd[agentId] == agentsPositions[agentId].Count - 1)  //是否完成所有路径
                            {
                                finished[agentId] = true;
                            }
                        }
                        //Debugger.Break();
                    }
                    else
                    {
                        agentsPaths[i].Add(agentsPositions[i][agentInd[i]]); //原地踏步
                    }
                }
                
                if (!hasMoved.Any(x => x)) {
                    
                    break; 
                
                } //是否全部完成
            }
            /*
            if (config.parallelizePaths2)
            {
                ConstraintsSet constraints = new ConstraintsSet();
                for (int i = 0; i < agentCount; ++i)
                {
                    constraints.addAgentPath(agentsPaths[i], i);
                }

                for (int i = 0; i < agentCount; ++i)
                {
                    constraints.RemoveAgentPath(agentsPaths[i], i);
                    List<List<bool>> dp = Enumerable.Repeat(new List<bool>(), agentsPaths[i].Count).ToList();
                    List<List<bool>> move = Enumerable.Repeat(new List<bool>(), agentsPaths[i].Count).ToList();
                    dp[0].Add(true);
                    move[0].Add(false);

                    int last = agentsPositions[i].Count - 2;
                    for (int time = 1; time < agentsPaths[i].Count; ++time)
                    {
                        for (int j = 0; j <= time && j < agentsPositions[i].Count; ++j)
                        {
                            dp[time].Add(false);
                            move[time].Add(false);

                            Node curPos = agentsPositions[i][j];
                            if (!constraints.HasNodeConstraint(curPos.i, curPos.j, time, i) &&
                                (j < agentsPositions[i].Count - 1 ||
                                 !constraints.HasFutureConstraint(curPos.i, curPos.j, time, i)))
                            {
                                if (j < time && dp[time - 1][j])
                                {
                                    dp[time][j] = true;
                                }
                                else if (j > 0)
                                {
                                    Node prevPos = agentsPositions[i][j - 1];
                                    if (dp[time - 1][j - 1] &&
                                        !constraints.HasEdgeConstraint(curPos.i, curPos.j, time, i, prevPos.i, prevPos.j))
                                    {
                                        dp[time][j] = true;
                                        move[time][j] = true;
                                    }
                                }
                            }
                        }

                        if (dp[time].Count >= agentsPositions[i].Count && !dp[time][agentsPositions[i].Count - 1])
                        {
                            last = time;
                        }
                    }

                    agentsPaths[i].Clear();
                    int posInd = agentsPositions[i].Count - 1;
                    for (int j = last + 1; j >= 0; --j)
                    {
                        agentsPaths[i].Add(agentsPositions[i][posInd]);
                        if (move[j][posInd])
                        {
                            posInd--;
                        }
                    }
                    agentsPaths[i].Reverse();
                    constraints.addAgentPath(agentsPaths[i], i);
                }
            }*/
        }

        public bool solve(Map map, Config config, AgentSet agentSet, DateTime begin)
        {
            var comparator = new Comparison<int>((id1, id2) =>
            {
                int subgraph1 = agentSet.getAgent(id1).getSubgraph();
                int subgraph2 = agentSet.getAgent(id2).getSubgraph();

                if (subgraph1 != subgraph2)
                {
                    if (subgraph1 == -1 || agentSet.hasPriority(subgraph2, subgraph1))
                        return 1;
                    else if (subgraph2 == -1 || agentSet.hasPriority(subgraph1, subgraph2))
                        return -1;
                }
                return id1.CompareTo(id2);
            });

            
            var notFinished = new SortedSet<int>(Comparer<int>.Create(comparator));

            HashSet<int> finished = new HashSet<int>();
            HashSet<Node> qPathNodes = new HashSet<Node>();
            HashSet<Node> finishedPositions = new HashSet<Node>();
            List<Node> qPath = new List<Node>();

            for (int i = 0; i < agentSet.getAgentCount(); ++i)
            {
                notFinished.Add(i);
            }

            bool isPolygon = true;
            for (int i = 0; i < map.getMapHeight(); ++i)
            {
                for (int j = 0; j < map.getMapWidth(); ++j)
                {
                    if (!map.CellIsObstacle(i, j) && map.getCellDegree(i, j) != 2)
                    {
                        isPolygon = false;
                        break;
                    }
                }
                if (!isPolygon)
                    break;
            }

            int curAgentId = -1;
            Agent curAgent = null;

            while (notFinished.Any())
            {
                //Console.WriteLine(notFinished.Count);
                DateTime now = DateTime.UtcNow;
                if ((now - begin).TotalMilliseconds > config.maxTime)
                    return false;

                if (curAgentId == -1)
                {
                    curAgent = agentSet.getAgent(notFinished.First());
                }
                else
                {
                    curAgent = agentSet.getAgent(curAgentId);
                }
                notFinished.Remove(curAgent.getId());
                //Console.WriteLine($"Current agent is {curAgent.getId()} at ({curAgent.getCur_i()},{curAgent.getCur_j()})");
                SearchResult searchResult = search.startSearch(
                    map,
                    agentSet,
                    curAgent.getCur_i(),
                    curAgent.getCur_j(),
                    curAgent.getGoal_i(),
                    curAgent.getGoal_j(),
                    null,
                    true,
                    true,
                    0,
                    -1,
                    -1,
                    isPolygon ? finishedPositions : new HashSet<Node>()
                );

                if (!searchResult.pathfound)
                    return false;

                var path = searchResult.lppath.ToList();
                qPath.Add(path.First());
                qPathNodes.Add(path.First()); //

                for (int it = 0; it < path.Count - 1; ++it)
                {
                    var pathIt = path[it];
                    var pathNext = path[it + 1];
                    
                    if (qPathNodes.Contains(path[it + 1]))
                    {
                        
                        int cycleBeg = qPath.FindLastIndex(x => x.Equals(path[it + 1]));
                        rotate(map, agentSet, qPath, cycleBeg);

                        bool toErase = false;
                        while (qPath.Count > cycleBeg)
                        {
                            Node lastNode = qPath[^1];
                            if (agentSet.isOccupied(lastNode.i, lastNode.j) &&
                                finished.Contains(agentSet.getAgentId(lastNode.i, lastNode.j)))
                            {
                                if (!toErase)
                                {
                                    finishedPositions.Add(lastNode);
                                    toErase = true;
                                }
                            }
                            else
                            {
                                if (toErase)
                                {
                                    finishedPositions.Remove(lastNode);
                                    toErase = false;
                                }
                            }
                            qPathNodes.Remove(lastNode);
                            qPath.RemoveAt(qPath.Count - 1);
                        }
                    }
                    else if (!push(map, agentSet,  pathIt,  pathNext, finishedPositions))
                    {
                        
                        if (!swap(map, ref agentSet, ref pathIt, ref pathNext))
                        {
                            path[it] = pathIt;
                            path[it + 1] = pathNext;
                            return false;
                        }
                        
                        if (finished.Contains(agentSet.getAgentId(path[it].i, path[it].j)))
                        {
                            finishedPositions.Remove(path[it + 1]);
                            finishedPositions.Add(path[it]);
                        }
                        
                        //Console.WriteLine($"Swap finished");
                        
                    }
                    qPath.Add(path[it + 1]);
                    qPathNodes.Add(path[it + 1]);
                }
                
                finished.Add(curAgent.getId());
                finishedPositions.Add(curAgent.getGoalPosition());
                
                curAgentId = -1;
                while (qPath.Any())
                {
                    Node lastNode = qPath[^1];
                    if (agentSet.isOccupied(lastNode.i, lastNode.j))
                    {
                        Agent tempAgent = agentSet.getAgent(agentSet.getAgentId(lastNode.i, lastNode.j));
                        Node goal = new Node(tempAgent.getGoal_i(), tempAgent.getGoal_j());

                        if (!notFinished.Contains(tempAgent.getId()) && !lastNode.Equals(goal))
                        {
                            if (!agentSet.isOccupied(goal.i, goal.j))
                            {                       
                                agentSet.moveAgent(ref lastNode, ref goal, agentsMoves);
                                finishedPositions.Remove(lastNode);
                                finishedPositions.Add(goal);
                            }
                            else
                            {
                                curAgentId = agentSet.getAgentId(goal.i, goal.j);
                                break;
                            }
                        }
                    }
                    qPathNodes.Remove(lastNode);
                    qPath.RemoveAt(qPath.Count - 1);
                }
            }
            
            return true;
        }

        public void getComponent(
                                AgentSet agentSet,
                                ref KeyValuePair<Node, Node> startEdge,
                                List<KeyValuePair<Node, Node>> edgeStack,
                                List<HashSet<Node>> components)
        {
            HashSet<Node> component = new HashSet<Node>();
            KeyValuePair<Node, Node> curEdge;

            do
            {
                curEdge = edgeStack[^1]; // 取最后一个元素
                component.Add(curEdge.Key);
                component.Add(curEdge.Value);
                edgeStack.RemoveAt(edgeStack.Count - 1); // 移除最后一个元素
            } while (!curEdge.Equals(startEdge));

            if (component.Count <= 2)
            {
                return;
            }

            foreach (var node in component)
            {
                agentSet.setNodeSubgraph(node.i, node.j, components.Count);
            }

            components.Add(component);
        }

        public void combineNodeSubgraphs(
                                        AgentSet agentSet,
                                        List<HashSet<Node>> components,
                                        Node subgraphNode,
                                        int subgraphNum)
        {
            List<int> subgraphs = agentSet.getSubgraphs(subgraphNode.i, subgraphNode.j);

            for (int j = 0; j < subgraphs.Count; ++j)
            {
                if (subgraphs[j] != subgraphNum)
                {
                    foreach (var node in components[subgraphs[j]])
                    {
                        agentSet.removeSubgraphs(node.i, node.j);
                        agentSet.setNodeSubgraph(node.i, node.j, subgraphNum);
                    }
                    components[subgraphs[j]].Clear();
                }
            }
        }

        public void getSubgraphs(Map map, AgentSet agentSet)
    {
        HashSet<Node> close = new HashSet<Node>();
        List<HashSet<Node>> components = new List<HashSet<Node>>();
        HashSet<Node> joinNodes = new HashSet<Node>();
        int connectedComponentNum = 0;

        for (int i = 0; i < map.getMapHeight(); ++i)
        {
            for (int j = 0; j < map.getMapWidth(); ++j)
            {
                if (!map.CellIsObstacle(i, j))
                {
                    Node curNode = new Node(i, j);
                    if (!close.Contains(curNode))
                    {
                        int oldSize = close.Count;
                        List<KeyValuePair<Node, Node>> edgeStack = new List<KeyValuePair<Node, Node>>();
                        Dictionary<Node, int> inDict = new Dictionary<Node, int>();
                        Dictionary<Node, int> upDict = new Dictionary<Node, int>();
                        Stack<(Node cur, int lastInd, int depth)> stack = new Stack<(Node, int, int)>();
                        stack.Push((curNode, -1, 0));

                        while (stack.Any())
                        {
                            var (cur, lastInd, depth) = stack.Pop();
                            List<Node> successors = search.findSuccessors(cur, map).ToList();
                            var it = successors.GetEnumerator();

                            for (int index = 0; index < lastInd; ++index)
                            {
                                it.MoveNext();
                            }

                            if (lastInd == -1)
                            {
                                close.Add(cur);
                                agentSet.setConnectedComponent(cur.i, cur.j, connectedComponentNum);
                                inDict[cur] = depth;
                                upDict[cur] = depth;
                            }
                            else
                            {
                                it.MoveNext();
                                if ((depth != 0 && upDict[it.Current] >= inDict[cur]) || depth == 0)
                                {
                                    var curEdge = new KeyValuePair<Node, Node>(cur, it.Current);
                                    getComponent(agentSet, ref curEdge, edgeStack, components);
                                    if (depth != 0)
                                    {
                                        joinNodes.Add(cur);
                                    }
                                }
                                upDict[cur] = Math.Min(upDict[it.Current], upDict[cur]);
                            }

                            while (it.MoveNext())
                            {
                                ++lastInd;
                                if (close.Contains(it.Current))
                                {
                                    upDict[cur] = Math.Min(inDict[it.Current], upDict[cur]);
                                }
                                else
                                {
                                    var curEdge = new KeyValuePair<Node, Node>(cur, it.Current);
                                    edgeStack.Add(curEdge);
                                    stack.Push((cur, lastInd, depth));
                                    stack.Push((it.Current, -1, depth + 1));
                                    break;
                                }
                            }
                        }

                        agentSet.addComponentSize(close.Count - oldSize);
                        ++connectedComponentNum;
                    }
                }
            }
        }

        for (int i = 0; i < map.getMapHeight(); ++i)
        {
            for (int j = 0; j < map.getMapWidth(); ++j)
            {
                if (!map.CellIsObstacle(i, j) && map.getCellDegree(i, j) >= 3 && !agentSet.getSubgraphs(i, j).Any())
                {
                    agentSet.setNodeSubgraph(i, j, components.Count);
                    components.Add(new HashSet<Node> { new Node(i, j) });
                    joinNodes.Add(new Node(i, j));
                }
            }
        }

        int m = map.getEmptyCellCount() - agentSet.getAgentCount();
        Func<Node, Node, Map, AgentSet, bool> isGoal = (start, cur, map, agentSet) =>
        {
            List<int> startSubgraphs = agentSet.getSubgraphs(start.i, start.j);
            List<int> curSubgraphs = agentSet.getSubgraphs(cur.i, cur.j);
            return curSubgraphs.Count > 1 || (curSubgraphs.Count == 1 && curSubgraphs[0] != startSubgraphs[0]);
        };

        List<int> order = Enumerable.Range(0, components.Count).ToList();
        order.Sort((a, b) => components[b].Count - components[a].Count);

        foreach (int i in order)
        {
            foreach (var start in components[i])
            {
                if (joinNodes.Contains(start))
                {
                    // var nodeStart = start;
                    combineNodeSubgraphs(agentSet, components, start, i);
                    // start = nodeStart;
                    var dijkstraSearch = new Dijkstra();
                    var searchResult = dijkstraSearch.startSearch(
                        map,
                        agentSet,
                        start.i,
                        start.j,
                        0,
                        0,
                        isGoal,
                        true,
                        true,
                        0,
                        -1,
                        m - 2,
                        components[i]
                    );

                    while (searchResult.pathfound)
                    {
                        var path = searchResult.lppath.ToList();
                        foreach (var it in path.Skip(1).Take(path.Count - 2))
                        {
                            if (!agentSet.getSubgraphs(it.i, it.j).Any())
                            {
                                agentSet.setNodeSubgraph(it.i, it.j, i);
                            }
                        }

                        combineNodeSubgraphs(agentSet, components,path.Last(), i);
                        searchResult = dijkstraSearch.startSearch(
                            map,
                            agentSet,
                            start.i,
                            start.j,
                            0,
                            0,
                            isGoal,
                            false,
                            true, 0, -1, m - 2
                        );
                    }
                }
            }
        }
    }

        public int getReachableNodesCount(
                                        Map map,
                                        AgentSet agentSet,
                                        Node start,
                                        Func<Node, Node, Map, AgentSet, bool> condition,
                                        HashSet<Node> occupiedNodes)
    {
        int res = 0;
        var dijkstraSearch = new Dijkstra();

        SearchResult searchResult = dijkstraSearch.startSearch(
            map,
            agentSet,
            start.i,
            start.j,
            0,
            0,
            condition,
            true,
            false,
            0,
            -1,
            -1,
            occupiedNodes
        );

        while (searchResult.pathfound)
        {
            ++res;
            searchResult = dijkstraSearch.startSearch(
                map,
                agentSet,
                start.i,
                start.j,
                0,
                0,
                condition,
                false,
                false,
                0,
                -1,
                -1,
                occupiedNodes
            );
        }

        return res;
    }

        public void assignToSubgraphs(Map map, AgentSet agentSet)
    {
        Func<Node, Node, Map, AgentSet, bool> isUnoccupied = (start, cur, map, agentSet) =>
        {
            return !agentSet.isOccupied(cur.i, cur.j);
        };

        List<int> agentsInConnectedComponents = new List<int>(new int[agentSet.getConnectedComponentsCount()]);
        for (int i = 0; i < agentSet.getAgentCount(); ++i)
        {
            Agent agent = agentSet.getAgent(i);
            ++agentsInConnectedComponents[agentSet.getConnectedComponent(agent.getCur_i(), agent.getCur_j())];
        }

        int m = map.getEmptyCellCount() - agentSet.getAgentCount();
        for (int i = 0; i < map.getMapHeight(); ++i)
        {
            for (int j = 0; j < map.getMapWidth(); ++j)
            {
                if (map.CellIsObstacle(i, j))
                {
                    continue;
                }

                Node pos = new Node { i = i, j = j };
                var subgraphs = agentSet.getSubgraphs(pos.i, pos.j);
                if (!subgraphs.Any())
                {
                    continue;
                }

                int subgraph = subgraphs[0];
                var successors = search.findSuccessors(pos, map);
                int totalCount = agentSet.getComponentSize(pos.i, pos.j) -
                                    agentsInConnectedComponents[agentSet.getConnectedComponent(pos.i, pos.j)];
                int throughPos = 0;
                bool hasSuccessorsInOtherSubgraph = false;

                foreach (var neigh in successors)
                {
                    var neighSubgraphs = agentSet.getSubgraphs(neigh.i, neigh.j);
                    if (!neighSubgraphs.Any() || neighSubgraphs[0] != subgraph)
                    {
                        hasSuccessorsInOtherSubgraph = true;
                        int throughNeigh = getReachableNodesCount(map, agentSet, neigh, isUnoccupied, new HashSet<Node> { pos });
                        int m1 = totalCount - throughNeigh;

                        if (m1 >= 1 && m1 < m && agentSet.isOccupied(i, j))
                        {
                            agentSet.setAgentSubgraph(agentSet.getAgentId(pos.i, pos.j), subgraph);
                        }

                        Func<Node, Node, Map, AgentSet, bool> isGoal = (start, cur, map, agentSet) =>
                        {
                            return map.getCellDegree(cur.i, cur.j) == 1 || agentSet.getSubgraphs(cur.i, cur.j).Any();
                        };

                        var dijkstraSearch = new Dijkstra();
                        var searchResult = dijkstraSearch.startSearch(
                            map,
                            agentSet,
                            neigh.i,
                            neigh.j,
                            0,
                            0,
                            isGoal,
                            true,
                            true,
                            0,
                            -1,
                            -1,
                            new HashSet<Node> { pos }
                        );

                        var path = searchResult.lppath.ToList();
                        int agentCount = 0;

                        foreach (var node in path)
                        {
                            if (agentSet.isOccupied(node.i, node.j))
                            {
                                if (agentCount >= m1 - 1)
                                {
                                    break;
                                }

                                agentSet.setAgentSubgraph(agentSet.getAgentId(node.i, node.j), subgraph);
                                ++agentCount;
                            }
                        }
                        throughPos += throughNeigh;
                    }
                }

                if (agentSet.isOccupied(i, j) && (!hasSuccessorsInOtherSubgraph || totalCount - throughPos >= 1))
                {
                    agentSet.setAgentSubgraph(agentSet.getAgentId(pos.i, pos.j), subgraph);
                }
            }
        }
    }

        public void getPriorities(Map map, AgentSet agentSet)
    {
        Dictionary<Node, int> goalPositions = new Dictionary<Node, int>();
        for (int i = 0; i < agentSet.getAgentCount(); ++i)
        {
            goalPositions[agentSet.getAgent(i).getGoalPosition()] = i;
        }

        Func<Node, Node, Map, AgentSet, bool> isGoal = (start, cur, map, agentSet) =>
        {
            return agentSet.getSubgraphs(cur.i, cur.j).Any();
        };

        for (int i = 0; i < map.getMapHeight(); ++i)
        {
            for (int j = 0; j < map.getMapWidth(); ++j)
            {
                if (map.CellIsObstacle(i, j))
                {
                    continue;
                }

                var subgraphs = agentSet.getSubgraphs(i, j);
                if (!subgraphs.Any())
                {
                    continue;
                }

                int subgraph = subgraphs[0];
                var successors = search.findSuccessors(new Node { i = i, j = j }, map);

                foreach (var neigh in successors)
                {
                    var neighSubgraphs = agentSet.getSubgraphs(neigh.i, neigh.j);
                    if (!neighSubgraphs.Any() || neighSubgraphs[0] != subgraph)
                    {
                        var dijkstraSearch = new Dijkstra();
                        var searchResult = dijkstraSearch.startSearch(
                            map,
                            agentSet,
                            neigh.i,
                            neigh.j,
                            0,
                            0,
                            isGoal,
                            true,
                            true,
                            0,
                            -1,
                            -1,
                            new HashSet<Node> { new Node { i = i, j = j } }
                        );

                        if (!searchResult.pathfound)
                        {
                            continue;
                        }

                        var path = searchResult.lppath.ToList();
                        path.Insert(0, new Node { i = i, j = j }); // 等价于 C++ 的 `push_front`

                        foreach (var node in path)
                        {
                            if (!goalPositions.TryGetValue(node, out int agentId))
                            {
                                break;
                            }

                            var agent = agentSet.getAgent(agentId);
                            int agentSubgraph = agent.getSubgraph();

                            if (agentSubgraph != -1)
                            {
                                if (agentSubgraph != subgraph)
                                {
                                    agentSet.setPriority(subgraph, agentSubgraph);
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }
    }



        public override MultiagentSearchResult startSearch(Map map, Config config, AgentSet agentSet, DateTime? globalBegin, int globalTimeLimit)
    {
            //Console.WriteLine(agentSet.getAgentCount());

            DateTime begin = DateTime.Now;
            getSubgraphs(map, agentSet);

            AgentSet goalAgentSet = agentSet.Clone(); // 假设 AgentSet 实现了 Clone 方法
            for (int i = 0; i < agentSet.getAgentCount(); ++i)
            {
                Node goal = agentSet.getAgent(i).getGoalPosition();
                goalAgentSet.setAgentPosition(i, goal);
            }

            assignToSubgraphs(map, agentSet);
            assignToSubgraphs(map, goalAgentSet);

            for (int i = 0; i < agentSet.getAgentCount(); ++i)
            {
                if (agentSet.getAgent(i).getSubgraph() != goalAgentSet.getAgent(i).getSubgraph())
                {
                    result.pathfound = false;
                    return result;
                }
            }

            getPriorities(map, agentSet);

            result.pathfound = solve(map, config, agentSet, begin);
            //Console.WriteLine("path found finished" + result.pathfound);
            if (result.pathfound)
            {
                //if (config.parallelizePaths1)
                if (true)
                {
                    getParallelPaths(agentSet, config);
                }
                else
                {
                    getPaths(agentSet);
                }
            }
            
            DateTime end = DateTime.Now;
            int elapsedMilliseconds = (int)(end - begin).TotalMilliseconds;

            if (elapsedMilliseconds > config.maxTime)
            {
                result.pathfound = false;
            }

            if (result.pathfound)
            {                
                result.agentsMoves = agentsMoves;
                result.agentsPaths = agentsPaths;
                result.time = new List<double> { elapsedMilliseconds / 1000.0 };

                var costs = result.getCosts();
                result.makespan = new List<double> { (double)costs.makespan };
                result.flowtime = new List<double> { (double)costs.timeflow };
        }

            return result;
        }


          
        }

    
}

