using System;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mission
{
    using System;
    using System.Collections.Generic;

    abstract class MultiagentSearchInterface
    {
        ~MultiagentSearchInterface() { }

        public abstract MultiagentSearchResult startSearch(
            Map map,
            Config config,
            AgentSet agentSet,
            DateTime? globalBegin = null,
            int globalTimeLimit = -1);

        public virtual void clear()
        {
            agentsPaths.Clear(); // 2-d array of nodes. please check the head file of the node
            perfectHeuristic.Clear();
        }

        /*
        public void getPerfectHeuristic(Map map, AgentSet agentSet)
        {
            if (perfectHeuristic.Count > 0)
            {
                return;
            }

            HashSet<int> visited = new HashSet<int>();
            ISearch search = new ISearch(false);
            for (int i = 0; i < agentSet.getAgentCount(); ++i)
            {
                visited.Clear();
                Node goal = new Node(agentSet.getAgent(i).getGoal_i(), agentSet.getAgent(i).getGoal_j());
                Queue<Node> queue = new Queue<Node>();
                queue.Enqueue(goal);
                while (queue.Count > 0)
                {
                    Node cur = queue.Dequeue();
                    if (visited.Contains(cur.convolution(map.getMapWidth(), map.getMapHeight())))
                    {
                        continue;
                    }
                    perfectHeuristic[new KeyValuePair<Node, Node>(cur, goal)] = cur.g;
                    visited.Add(cur.convolution(map.getMapWidth(), map.getMapHeight()));
                    List<Node> successors = search.findSuccessors(cur, map);
                    foreach (Node neigh in successors)
                    {
                        queue.Enqueue(neigh);
                    }
                }
            }
        }
        */

        public List<List<Node>> agentsPaths = new List<List<Node>>();
        public Dictionary<KeyValuePair<Node, Node>, int> perfectHeuristic = new Dictionary<KeyValuePair<Node, Node>, int>(new NodePairComparer());
    }

    // The NodePairComparer class is required for dictionary key comparison.
    class NodePairComparer : IEqualityComparer<KeyValuePair<Node, Node>>
    {
        public bool Equals(KeyValuePair<Node, Node> x, KeyValuePair<Node, Node> y)
        {
            return x.Key.Equals(y.Key) && x.Value.Equals(y.Value);
        }

        public int GetHashCode(KeyValuePair<Node, Node> obj)
        {
            return obj.Key.GetHashCode() ^ obj.Value.GetHashCode();
        }
    }

}

