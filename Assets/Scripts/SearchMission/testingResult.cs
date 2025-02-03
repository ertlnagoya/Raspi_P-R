using System;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mission
{
    public class TestingResults
    {
        public Dictionary<string, Dictionary<int, List<double>>> data { get; set; }
        public Dictionary<int, int> finalTotalNodes { get; set; }
        public Dictionary<int, int> finalHLNodes { get; set; }
        public Dictionary<int, int> finalHLNodesStart { get; set; }
        public Dictionary<int, int> finalHLExpansions { get; set; }
        public Dictionary<int, int> finalHLExpansionsStart { get; set; }

        public TestingResults()
        {
            data = new Dictionary<string, Dictionary<int, List<double>>>
        {
            { Constants.CNS_TAG_ATTR_TIME, new Dictionary<int, List<double>>() },
            { Constants.CNS_TAG_ATTR_MAKESPAN, new Dictionary<int, List<double>>() },
            { Constants.CNS_TAG_ATTR_FLOWTIME, new Dictionary<int, List<double>>() },
            { Constants.CNS_TAG_ATTR_LLE, new Dictionary<int, List<double>>() },
            { Constants.CNS_TAG_ATTR_LLN, new Dictionary<int, List<double>>() },
            { Constants.CNS_TAG_ATTR_HLE, new Dictionary<int, List<double>>() },
            { Constants.CNS_TAG_ATTR_HLN, new Dictionary<int, List<double>>() },
            { Constants.CNS_TAG_FOCAL_W, new Dictionary<int, List<double>>() },
            { Constants.CNS_TAG_ATTR_TN, new Dictionary<int, List<double>>() }
        };

            finalTotalNodes = new Dictionary<int, int>();
            finalHLNodes = new Dictionary<int, int>();
            finalHLNodesStart = new Dictionary<int, int>();
            finalHLExpansions = new Dictionary<int, int>();
            finalHLExpansionsStart = new Dictionary<int, int>();
        }

        public List<string> getKeys()
        {
            var keys = new List<string>();
            foreach (var pair in data)
            {
                keys.Add(pair.Key);
            }
            return keys;
        }
    }

}


