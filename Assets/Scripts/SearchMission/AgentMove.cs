using System;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mission
{
    public class AgentMove
    {
        public int di { get; set; }
        public int dj { get; set; }
        public int id { get; set; }

        public AgentMove(int x, int y, int Id)
        {
            di = x;
            dj = y;
            id = Id;
        }

        public AgentMove Copy()
        {
            return new AgentMove(this.di, this.dj, this.id);
        }
    }
}

