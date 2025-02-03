using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mission
{
    class MultiagentSearchResult
    {
        public bool pathfound;
        public List<AgentMove> agentsMoves;
        public List<List<Node>> agentsPaths;
        public List<double> time = new List<double>();
        public List<double> AvgLLExpansions = new List<double>();
        public List<double> AvgLLNodes = new List<double>();
        public List<int> cost = new List<int>();
        public List<double> HLExpansions = new List<double>();
        public List<double> HLExpansionsStart = new List<double>();
        public List<double> HLNodes = new List<double>();
        public List<double> HLNodesStart = new List<double>();
        public double finalHLExpansions = 0.0;
        public double finalHLExpansionsStart = 0.0;
        public double finalHLNodes = 0.0;
        public double finalHLNodesStart = 0.0;
        public List<double> focalW = new List<double>();
        public List<double> flowtime = new List<double>();
        public List<double> makespan = new List<double>();
        public List<double> totalNodes = new List<double>();
        public int finalTotalNodes;

        public MultiagentSearchResult(bool Pathfound = false)
        {
            pathfound = Pathfound;
        }

        public MultiagentSearchResult Add(MultiagentSearchResult other)
        {
            pathfound = other.pathfound;
            agentsPaths = other.agentsPaths;
            agentsMoves = other.agentsMoves;

            time.AddRange(other.time);
            AvgLLExpansions.AddRange(other.AvgLLExpansions);
            AvgLLNodes.AddRange(other.AvgLLNodes);
            cost.AddRange(other.cost);
            HLExpansionsStart.AddRange(other.HLExpansionsStart);
            HLNodesStart.AddRange(other.HLNodesStart);
            HLExpansions.AddRange(other.HLExpansions);
            HLNodes.AddRange(other.HLNodes);
            focalW.AddRange(other.focalW);
            flowtime.AddRange(other.flowtime);
            makespan.AddRange(other.makespan);
            totalNodes.AddRange(other.totalNodes);
            return this;
        }

        public (int makespan, int timeflow) getCosts()
        {
            int makespan = 0, timeflow = 0;
            for (int i = 0; i < agentsPaths.Count; ++i)
            {
                makespan = Math.Max(makespan, agentsPaths[i].Count - 1);
                int lastMove;
                for (lastMove = agentsPaths[i].Count - 1;
                     lastMove > 1 && agentsPaths[i][lastMove].Equals(agentsPaths[i][lastMove - 1]);
                     --lastMove) ;
                timeflow += lastMove;
            }
            return (makespan, timeflow);
        }
    }

}