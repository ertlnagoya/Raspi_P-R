using System.Text;
using System;
using NATS.Client;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using Mission;
using NATS.Client.JetStream;
using LineTrace;
using System.Collections.Generic;


namespace Mission
{

    public class Nats
    {
        private readonly IConnection connection;
        private readonly string goalSubject = "goal";  // 处理 goal 请求
        private readonly string retSubject = "ret";    // 处理 ret 请求
        private readonly string responseSubject = "path";  // 发送路径数据的频道
        public event Action<Demand, string> OnDemandReceived;  // 事件，传递 Demand 和主题


        public Nats()
        {
            var opts = ConnectionFactory.GetDefaultOptions();
            opts.Url = "nats://localhost:4222";
            connection = new ConnectionFactory().CreateConnection(opts);
        }


        public void Subscribe()
        {
            // 监听 "goal" 主题
            connection.SubscribeAsync(goalSubject, (sender, args) =>
            {
                ProcessIncomingMessage(args, goalSubject);
            });

            // 监听 "ret" 主题
            connection.SubscribeAsync(retSubject, (sender, args) =>
            {
                ProcessIncomingMessage(args, retSubject);
            });

            Debug.Log("[Nats] NATS Listeners Started.");
        }
        private void ProcessIncomingMessage(MsgHandlerEventArgs args, string subject)
        {
            string message = Encoding.UTF8.GetString(args.Message.Data);
            Debug.Log($"[Nats] Received {subject} Request: {message}");

            Demand demand = JsonUtility.FromJson<Demand>(message);

            // 触发事件，把解析的 Demand 传递给 Arbitrator 处理
            OnDemandReceived?.Invoke(demand, subject);
        }

        public void SendPath(Dictionary<int, int[]> path)
        {
            if (path.Count == 0)
            {
                Debug.LogWarning("No robot paths to send.");
                return;
            }

            // 将 Dictionary<int, int[]> 转换为 JSON
            string jsonData = JsonUtility.ToJson(new RobotPathsWrapper(path));

            // 发送 JSON 数据到 "robot.paths" 频道
            connection.Publish(responseSubject, Encoding.UTF8.GetBytes(jsonData));

            Debug.Log($"Sent Robot Paths: {jsonData}");
        }

        [Serializable]
        private class RobotPathsWrapper
        {
            public List<RobotPath> paths = new List<RobotPath>();

            public RobotPathsWrapper(Dictionary<int, int[]> robotPaths)
            {
                foreach (var kvp in robotPaths)
                {
                    paths.Add(new RobotPath(kvp.Key, kvp.Value));
                }
            }
        }

        [Serializable]
        private class RobotPath
        {
            public int id;
            public int[] path;

            public RobotPath(int id, int[] path)
            {
                this.id = id;
                this.path = path;
            }
        }

        public string SendRaw(string sub, Demand demand)
        {
            var msg = connection.Request(
                sub, Encoding.UTF8.GetBytes(JsonUtility.ToJson(demand)),
                1000 * 10);
            return Encoding.UTF8.GetString(msg.Data);
        }

        public int Send(string sub, Demand demand)
        {
            return int.Parse(SendRaw(sub, demand));
        }

    }
}