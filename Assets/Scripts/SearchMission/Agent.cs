using System;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mission
{
    public class Agent
    {
        private int id;
        private int start_i, start_j;
        private int goal_i, goal_j;
        private int cur_i, cur_j;
        private int subgraph;

        public Agent(int start_i = 0, int start_j = 0, int goal_i = 0, int goal_j = 0, int id = 0)
        {
            this.start_i = start_i;
            this.start_j = start_j;
            this.goal_i = goal_i;
            this.goal_j = goal_j;
            this.cur_i = start_i;
            this.cur_j = start_j;
            this.id = id;
            this.subgraph = -1;
        }

        public Agent Clone()
        {
            return new Agent
            {
                id = this.id,
                start_i = this.start_i,
                start_j = this.start_j,
                goal_i = this.goal_i,
                goal_j = this.goal_j,
                cur_i = this.cur_i,
                cur_j = this.cur_j,
                subgraph = this.subgraph
            };
        }

        public int getStart_i()
        {
            return start_i;
        }

        public int getStart_j()
        {
            return start_j;
        }

        public int getGoal_i()
        {
            return goal_i;
        }

        public int getGoal_j()
        {
            return goal_j;
        }

        public int getCur_i()
        {
            return cur_i;
        }

        public int getCur_j()
        {
            return cur_j;
        }

        public int getId()
        {
            return id;
        }

        public int getSubgraph()
        {
            return subgraph;
        }

        public Node getStartPosition()
        {
            return new Node(start_i, start_j);
        }

        public Node getGoalPosition()
        {
            return new Node(goal_i, goal_j);
        }

        public Node getCurPosition()
        {
            return new Node(cur_i, cur_j);
        }

        public void setCurPosition(int i, int j)
        {
            cur_i = i;
            cur_j = j;
        }

        public void setSubgraph(int Subgraph)
        {
            subgraph = Subgraph;
        }

    }
}

