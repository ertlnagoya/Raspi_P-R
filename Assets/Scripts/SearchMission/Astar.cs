using System;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mission
{
    public class Astar : Dijkstra
    {
        public bool WithTime { get; set; }
        public double HWeight { get; set; } = 1.0;
        public bool BreakingTies { get; set; } = true;

        // Perfect Heuristic Dictionary
        private Dictionary<(Node, Node), int>? perfectHeuristic { get; set; } = null;

        // Constructor
        public Astar(bool withTime = false, double hWeight = 1.0, bool breakingTies = true)
        {
            WithTime = withTime;
            HWeight = hWeight;
            BreakingTies = breakingTies;
        }

        // Set Perfect Heuristic 

        // Compute H from one cell to another

        protected double ManhattanDistance(int x1, int y1, int x2, int y2)
        {
            return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        }


        protected virtual double metric(int x1, int y1, int x2, int y2)
        {
            return ManhattanDistance(x1, y1, x2, y2); // Default Metric: Manhattan Distance
        }

        
        public override double computeHFromCellToCell(int i1, int j1, int i2, int j2)
        {
            if (this.perfectHeuristic != null)
            {
                var key = (new Node(i1, j1), new Node(i2, j2));
                if (this.perfectHeuristic.TryGetValue(key, out var heuristicValue))
                {
                    return heuristicValue;
                }
            }
           
            return metric(i1, j1, i2, j2) * this.hweight;
        }
        

    }



}

