using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mission
{
    public class SearchResult
    {
        public bool pathfound;
        public float pathlength; // if path not found, then pathlength = 0
        public List<Node>? lppath; // path as the sequence of adjacent nodes (see above)
                                            // This is a reference to the list of nodes that is actually created and handled by ISearch class,
                                            // so no need to re-create them, delete them etc. It's just a trick to save some memory
        public List<Node>? hppath; // path as the sequence of non-adjacent nodes: "sections" (see above)
                                            // This is a reference to the list of nodes that is actually created and handled by ISearch class,
                                            // so no need to re-create them, delete them etc. It's just a trick to save some memory
        public uint nodescreated; // |OPEN| + |CLOSE| = total number of nodes saved in memory during search process.
        public uint nodesexpanded;
        public uint numberofsteps; // number of iterations (expansions) made by algorithm to find a solution
        public double time; // runtime of the search algorithm (expanding nodes + reconstructing the path)
        public Node? lastNode;
        public double minF;

        public SearchResult()
        {
            pathfound = false;
            pathlength = 0;
            lppath = null;
            hppath = null;
            nodescreated = 0;
            nodesexpanded = 0;
            numberofsteps = 0;
            time = 0;
            lastNode = default;
            minF = 0;
        }
    }
}

