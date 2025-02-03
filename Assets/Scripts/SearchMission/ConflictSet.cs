using Mission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mission
{
    public class ConflictSet
    {
        public List<Conflict> cardinal = new List<Conflict>();
        public List<Conflict> semiCardinal = new List<Conflict>();
        public List<Conflict> nonCardinal = new List<Conflict>();

        public void addCardinalConflict(Conflict conflict)
        {
            cardinal.Add(conflict);
        }

        public void addSemiCardinalConflict(Conflict conflict)
        {
            semiCardinal.Add(conflict);
        }

        public void addNonCardinalConflict(Conflict conflict)
        {
            nonCardinal.Add(conflict);
        }

        public void replaceAgentConflicts(int agentId, ConflictSet agentConflicts)
        {
            List<Conflict> newCardinal = new List<Conflict>();
            List<Conflict> newSemiCardinal = new List<Conflict>();
            List<Conflict> newNonCardinal = new List<Conflict>();

            Predicate<Conflict> pred = conflict => conflict.id1 != agentId && conflict.id2 != agentId;

            newCardinal.AddRange(cardinal.FindAll(pred));
            newSemiCardinal.AddRange(semiCardinal.FindAll(pred));
            newNonCardinal.AddRange(nonCardinal.FindAll(pred));

            newCardinal.AddRange(agentConflicts.cardinal);
            newSemiCardinal.AddRange(agentConflicts.semiCardinal);
            newNonCardinal.AddRange(agentConflicts.nonCardinal);

            cardinal = newCardinal;
            semiCardinal = newSemiCardinal;
            nonCardinal = newNonCardinal;
        }

        public bool empty()
        {
            return cardinal.Count == 0 && semiCardinal.Count == 0 && nonCardinal.Count == 0;
        }

        public Conflict getBestConflict()
        {
            if (cardinal.Count > 0)
            {
                return cardinal[0];
            }
            else if (semiCardinal.Count > 0)
            {
                return semiCardinal[0];
            }
            return nonCardinal[0];
        }

        public int getCardinalConflictCount()
        {
            return cardinal.Count;
        }

        public int getConflictCount()
        {
            return cardinal.Count + semiCardinal.Count + nonCardinal.Count;
        }

        public List<Conflict> getCardinalConflicts()
        {
            return new List<Conflict>(cardinal);
        }

        public int getMatchingHeuristic()
        {
            HashSet<int> matched = new HashSet<int>();
            int res = 0;

            foreach (var conflict in cardinal)
            {
                if (!matched.Contains(conflict.id1) && !matched.Contains(conflict.id2))
                {
                    res++;
                    matched.Add(conflict.id1);
                    matched.Add(conflict.id2);
                }
            }

            return res;
        }

        public int getConflictingPairsCount()
        {
            HashSet<(int, int)> conflictingPairs = new HashSet<(int, int)>();

            foreach (var conflict in nonCardinal.Concat(semiCardinal).Concat(cardinal))
            {
                var pair = (Math.Min(conflict.id1, conflict.id2), Math.Max(conflict.id1, conflict.id2));
                conflictingPairs.Add(pair);
            }

            return conflictingPairs.Count;
        }
    }

}
