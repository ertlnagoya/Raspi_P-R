using Display;
using System.Collections.Generic;

namespace Mission
{
    public class Robot
    {
        public int Id { get; set; } // 唯一标识
        public int Src { get; set; } // 当前位置
        public int Goal { get; set; } // 目标位置
        public int Dst { get; set; } // next位置
        public List<int> Path { get; set; } // 规划出的路径
        public RobotStatus Status { get; set; } // 状态
        private List<int> priorityList = new List<int>();  // 按优先级存储机器人ID

        public Robot(int id, int src, int dst, int goal)
        {
            Id = id;
            Src = src;
            Dst = dst; // 初始无目标
            Goal = goal;
            Path = new List<int>();
            Status = RobotStatus.Idle; // 默认空闲
            
        }

        public void UpdateInfo(int newSrc, int newDst, int newGoal)
        {
            Src = newSrc;
            Dst = newDst;
            Goal = newGoal;
        }

        public void UpdateInfo(int newSrc, int newDst)
        {
            Src = newSrc;
            Dst = newDst;
        }
    }

    public enum RobotStatus
    {
        Idle, // 空闲
        Moving, // 移动中
        Waiting, // 等待
        Completed // 任务完成
    }


}
