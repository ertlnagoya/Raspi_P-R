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
    public class Conflict
    {
        public int id1, id2;
        public Node pos1, pos2;
        public int time;
        public bool edgeConflict;
        public bool conflictFound;

        // 默认构造函数
        public Conflict(bool ConflictFound = false)
        {
            conflictFound = ConflictFound;
        }

        // 带参数的构造函数
        public Conflict(int Id1, int Id2, Node Pos1, Node Pos2, int Time, bool EdgeConflict)
        {
            id1 = Id1;
            id2 = Id2;
            pos1 = Pos1;
            pos2 = Pos2;
            time = Time;
            edgeConflict = EdgeConflict;
            conflictFound = true;
        }
    }

}
