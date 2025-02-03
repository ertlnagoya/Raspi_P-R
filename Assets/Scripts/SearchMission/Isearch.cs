using System.Diagnostics;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mission
{
    public class ISearch
    {

        public ISearch(bool withTime = false)
        {
            hweight = 1;
            breakingties = Constants.CN_SP_BT_GMAX;
            this.withTime = withTime;
        }

        // Public properties and methods
        public SearchResult StartSearch(Map map, AgentSet agentSet,
                                        int start_i, int start_j, int goal_i = 0, int goal_j = 0,
                                        Func<Node, Node, Map, AgentSet, bool>? isGoal = null,
                                        bool freshStart = true, bool returnPath = true,
                                        int startTime = 0, int goalTime = -1, int maxTime = -1,
                                        HashSet<Node>? occupiedNodes = null,
                                        ConstraintsSet? constraints = null,
                                        bool withCAT = false, ConflictAvoidanceTable? CAT = null,
                                        DateTime? globalBegin = null, int globalTimeLimit = -1)
        {
            return null; // Placeholder
        }

        public virtual List<Node> findSuccessors(Node curNode, Map map,
                                                 int goal_i = 0, int goal_j = 0, int agentId = -1,
                                                 HashSet<Node>? occupiedNodes = null,
                                                 ConstraintsSet? constraints = null,
                                                 bool withCAT = false, ConflictAvoidanceTable? CAT = null)
        {
            if (occupiedNodes == null) occupiedNodes = new HashSet<Node>();
            if (constraints == null) constraints = new ConstraintsSet();
            var successors = new List<Node>();
            for (int di = -1; di <= 1; ++di)
            {
                for (int dj = -1; dj <= 1; ++dj)
                {
                    int newi = curNode.i + di, newj = curNode.j + dj;
                    //Console.WriteLine("withtime is " + withTime);
                    if ((di == 0 || dj == 0) &&
                        (CanStay() || di != 0 || dj != 0) &&
                        map.CellOnGrid(newi, newj) &&
                        map.CellIsTraversable(newi, newj, occupiedNodes))
                    {
                        double newh = computeHFromCellToCell(newi, newj, goal_i, goal_j);
                        Node neigh = new Node(newi, newj, null, curNode.g + 1, newh);
                        //neigh.conflictsCount = CAT.getAgentsCount(neigh, curNode);
                        createSuccessorsFromNode(curNode,ref neigh, ref successors, agentId, constraints, CAT,
                                                 neigh.i == goal_i && neigh.j == goal_j);
                    }
                }
            }
            return successors;
        }

        public virtual void clearLists()
        {
            open.Clear();
            close.Clear();
        }

        public virtual void addStartNode(Node node, Map map, ConflictAvoidanceTable CAT)
        {
            open.Insert(map, node, withTime);
        }

        public virtual bool checkOpenEmpty()
        {
            return open.IsEmpty();
        }

        public virtual Node getCur(Map map)
        {
            Node cur = open.GetFront();
            return cur;
        }

        public void removeCur(Node cur, Map map)
        {
            open.Erase(map, cur, withTime);
        }

        public bool updateFocal(Node neigh, Map map)
        {
            return false;
        }

        public double getMinFocalF()
        {
            return Constants.CN_INFINITY;
        }

        public void setEndTime(ref Node node, int start_i, int start_j, int startTime, int agentId, ConstraintsSet constraints)
        {
            return;
        }

        public bool checkGoal(Node cur, int goalTime, int agentId, ConstraintsSet constraints)
        {
            return goalTime == -1 || cur.g == goalTime;
        }

        public virtual double computeHFromCellToCell(int start_i, int start_j, int fin_i, int fin_j)
        {
            //Debugger.Break();
            return 0;
        }

        public void makePrimaryPath(ref Node curNode, int endTime)
        {
            if (withTime && endTime != -1)
            {
                int startTime = curNode.g;
                for (curNode.g = endTime - 1; curNode.g > startTime; --curNode.g)
                {
                    lppath.Insert(0, curNode); // Equivalent to push_front in C++
                }
            }
            lppath.Insert(0, curNode);
            if (curNode.parent != null)
            {
                makePrimaryPath(ref curNode.parent, curNode.g);
            }
        }

        public void makeSecondaryPath(Map map)
        {
            var it = lppath.GetEnumerator();
            if (!it.MoveNext()) return; // Check if the list is empty

            hppath.Add(it.Current); // Equivalent to push_back in C++
            Node prev = it.Current;
            while (it.MoveNext())
            {
                Node cur = it.Current;
                if (!it.MoveNext())
                {
                    hppath.Add(cur);
                    break;
                }
                Node next = it.Current;
                it.MoveNext();

                // Check for collinearity
                if ((cur.i - prev.i) * (next.j - cur.j) != (cur.j - prev.j) * (next.i - cur.i))
                {
                    hppath.Add(cur);
                }
                prev = cur;
            }
        }

        public SearchResult startSearch(Map map, AgentSet agentSet,
                                int start_i, int start_j, int goal_i, int goal_j,
                                Func<Node, Node, Map, AgentSet, bool> isGoal,
                                bool freshStart = true, bool returnPath = true, int startTime = 0, int goalTime = -1, int maxTime = -1,
                                HashSet<Node>? occupiedNodes = null, ConstraintsSet? constraints = null,
                                bool withCAT = false, ConflictAvoidanceTable? CAT = null,
                                DateTime? globalBegin = null, int globalTimeLimit = -1)
        {

            sresult.pathfound = false;  
            DateTime begin = DateTime.Now;
            //Console.WriteLine($"STARTSEARCH invoked {new StackTrace().GetFrame(2)?.GetMethod().Name} => {new StackTrace().GetFrame(1)?.GetMethod().Name}");
            if (goalTime != -1)
            {
                maxTime = goalTime;
            }

            Node? cur = null; //当前节点
            int agentId = -1; //当前代理id

            if (agentSet.isOccupied(start_i, start_j))
            {
                agentId = agentSet.getAgentId(start_i, start_j); //检查起点是否被其他代理占用，如果被占用，则获取该代理的 ID。
            }

            if (freshStart)  //freshStart is true
            {
                clearLists(); // clear open and close list
                sresult.numberofsteps = 0;
                cur = new Node(start_i, start_j, null, startTime, computeHFromCellToCell(start_i, start_j, goal_i, goal_j));
                setEndTime(ref cur, start_i, start_j, startTime, agentId, constraints);
                addStartNode(cur, map, CAT); //open 是起始队列
                
                addSuboptimalNode(ref cur, map, CAT);
            }
            var statis = 0;

            while (!checkOpenEmpty())  // open 不空
            {
                ++sresult.numberofsteps;

                //if (sresult.numberofsteps % 100000 == 0 && (DateTime.Now - begin).TotalMilliseconds > 300000)
                if (sresult.numberofsteps % 100000 == 0 && (DateTime.Now - begin).TotalMilliseconds > 300000)
                {
                    break;
                   
                }

                if (sresult.numberofsteps % 1000 == 0 && globalTimeLimit != -1)
                {
                    
                    break;
                }
                cur = getCur(map); //获取open第一个节点

                bool goalNode = false;

                if ((isGoal != null && isGoal(new Node(start_i, start_j, null, 0, 0), cur, map, agentSet)) ||
                    (isGoal == null && cur.i == goal_i && cur.j == goal_j))
                {
                    goalNode = true;
                    statis++;
                    
                    if (checkGoal(cur, goalTime, agentId, constraints))
                    {
                        sresult.pathfound = true;                       
                        break;
                    }
                } //是否到达目标节点


                removeCur(cur, map);


                /*
                if (goalNode)
                {
                    subtractFutureConflicts(ref cur);//没用
                }
                */

                close[cur.convolution(map.getMapWidth(), map.getMapHeight(), withTime)] = cur.DeepCopy(); //将close【i*width+j】 对应 cur

                Node curPtr = close[cur.convolution(map.getMapWidth(), map.getMapHeight(), withTime)];
                
                if (maxTime == -1 || cur.g < maxTime)
                {
                    var successors = findSuccessors(cur, map, goal_i, goal_j, agentId, occupiedNodes, constraints, withCAT, CAT);
                    foreach (var neigh in successors)
                    {
                        if (!close.ContainsKey(neigh.convolution(map.getMapWidth(), map.getMapHeight(), withTime)))// 在close中找到了 意味着去过的点，直接抛弃，否则加入open
                        {
                            neigh.parent = curPtr;
                            if (!updateFocal(neigh, map))  //true
                            {
                                open.Insert(map, neigh, withTime); //插入领居节点
                            }
                        }
                    }
                }
               

            }            
            sresult.time = (DateTime.Now - begin).TotalSeconds;
            sresult.nodescreated = (uint)(open.Size() + close.Count + getFocalSize());
            sresult.nodesexpanded = (uint)(close.Count);

            if (sresult.pathfound)
            {
                sresult.pathlength = cur.g;
                sresult.minF = Math.Min(cur.F, getMinFocalF());
                sresult.lastNode = cur;
                if (returnPath)
                {
                    lppath.Clear();
                    hppath.Clear();
                    makePrimaryPath(ref cur, goalTime == -1 ? -1 : goalTime + 1);
                    makeSecondaryPath(map);
                    sresult.hppath = hppath;
                    sresult.lppath = lppath;
                }
            }

            return sresult;
        }

        public virtual void subtractFutureConflicts(ref Node node)
        {
            // Intentionally left empty, as in the original code
        }


        public virtual void addSuboptimalNode(ref Node node, Map map, ConflictAvoidanceTable CAT)
        {
            // Intentionally left empty, as in the original code
        }

        public virtual int getFocalSize()
        {
            return 0;
        }


        public void createSuccessorsFromNode(Node cur, ref Node neigh, ref List<Node> successors,
                                     int agentId, ConstraintsSet constraints,
                                     ConflictAvoidanceTable CAT, bool isGoal)
        {
            //if (!constraints.hasNodeConstraint(neigh.i, neigh.j, neigh.g, agentId) &&
            //    !constraints.hasEdgeConstraint(neigh.i, neigh.j, neigh.g, agentId, cur.i, cur.j))
            {
                //setHC(ref neigh, cur, CAT, isGoal);
                successors.Add(neigh);
            }
        }

        public virtual void setHC(ref Node neigh, Node cur, ConflictAvoidanceTable CAT, bool isGoal)
        {
            // Intentionally left empty, as in the original code
        }

        public virtual int GetSize()
        {
            return open.Size() + close.Count;
        }



        public virtual bool CanStay()
        {
            return false;
        }

        public virtual int GetFocalSize()
        {
            return 0;
        }

        public virtual Node GetCur(Map map)
        {
            return default;
        }


        public virtual void SubtractFutureConflicts(ref Node node) { }

        public virtual bool UpdateFocal(Node neigh, Map map)
        {
            return false;
        }



        public virtual void CheckFocal() { }

        public virtual void AddTime(int time) { }

        // Static fields
        public static int T;
        public static int P;

        // Protected fields
        public SearchResult sresult = new SearchResult();
        public List<Node> lppath = new List<Node>();
        public List<Node> hppath = new List<Node>();
        public double hweight; // weight of h-value
        public bool breakingties; // flag for priority of nodes in addOpen when F-values are equal
        public SearchQueue open = new SearchQueue();
        public Dictionary<int, Node> close = new Dictionary<int, Node>();
        public bool withTime;
    }


}

