using System.Text;
using System;
using NATS.Client;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Mission;

namespace LineTrace
{
    public class Nats
    {
        private readonly IConnection connection;
        private readonly string subject = "path";  // 监听的频道

        public Nats()
        {
            var opts = ConnectionFactory.GetDefaultOptions();
            opts.Url = "nats://localhost:4222";
            connection = new ConnectionFactory().CreateConnection(opts);
            Debug.Log($"pilot connection building:");
        }

        /// <summary>
        /// 订阅 NATS 消息，并解析为 int[]
        /// </summary>
        /// <param name="onMessageReceived">回调函数，接收 int[] 目标点序号</param>
        /// <summary>
        /// 订阅 NATS 消息，并解析为 int[]（使用 JsonUtility）
        /// </summary>
        /// <param name="onMessageReceived">回调函数，接收 int[] 目标点序号</param>
        public void Subscribe(Action<int[]> onMessageReceived, int robotId)
        {
            IAsyncSubscription sub = connection.SubscribeAsync(subject);

            sub.MessageHandler += (sender, args) =>
            {
                try
                {
                    string message = Encoding.UTF8.GetString(args.Message.Data);
                    Debug.Log($"Received NATS message: {message}");

                    // 解析 JSON 数组
                    RobotPathsWrapper receivedPaths = JsonUtility.FromJson<RobotPathsWrapper>(message);
                    RobotPath robotPath = receivedPaths.paths.Find(path => path.id == robotId);

                    if (robotPath != null && robotPath.path.Count > 0)  
                    {
                        onMessageReceived?.Invoke(robotPath.path.ToArray());  
                    }
                    else
                    {
                        Debug.LogError("Received empty or invalid command array.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error processing NATS message: {ex.Message}");
                }
            };

            sub.Start();
            Debug.Log("pilot NATS Listener started.");
        }

        public void SubscribeSync(Action<int> onMessageReceived, int robotId)
        {
            IAsyncSubscription sub = connection.SubscribeAsync(subject);

            sub.MessageHandler += (sender, args) =>
            {
                try
                {
                    string message = Encoding.UTF8.GetString(args.Message.Data);
                    

                    // 解析 JSON 数据
                    AgentNextPositionWrapper receivedData = JsonUtility.FromJson<AgentNextPositionWrapper>(message);
                    AgentNextPosition agentData = receivedData.agents.Find(agent => agent.id == robotId);
                    //Debug.Log($"Received NATS message: 'id': {agentData.id} next is {agentData.nextPosition}");
                    if (agentData != null)
                    {
                        onMessageReceived?.Invoke(agentData.nextPosition);
                    }
                    else
                    {
                        Debug.LogError($"No data found for robot ID {robotId}.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error processing NATS message: {ex.Message}");
                }
            };

            sub.Start();
            Debug.Log("pilot NATS Listener started.");
        }

        public void SendRaw(string sub, Demand demand)
        {
            connection.Publish(sub, Encoding.UTF8.GetBytes(JsonUtility.ToJson(demand)));
            return;
        }

        public void Send(string sub, Demand demand)
        {
            if(sub == "goal")
            SendRaw(sub, demand);
            return;
        }

        public string SendRaw_CSoS(string sub, Demand_CSoS demand)
        {
            var msg = connection.Request(
                sub, Encoding.UTF8.GetBytes(JsonUtility.ToJson(demand)),
                1000 * 10);
            
            // return Encoding.UTF8.GetString(msg.Data);
            return int.Parse(Encoding.UTF8.GetString(msg.Data)).ToString();
        }
        public int Send_CSoS(string sub, Demand_CSoS demand)
        {
            return int.Parse(SendRaw_CSoS(sub, demand));
        }

        public Demand_Reply SendRaw_Resource(string sub, Demand_Resource demand)
        {
            var msg = connection.Request(
                sub, Encoding.UTF8.GetBytes(JsonUtility.ToJson(demand)),
                1000 * 10); // タイムアウト10秒

            string jsonString = Encoding.UTF8.GetString(msg.Data);
            Debug.Log($"Received JSON: {jsonString}");

            // フラットなリストを扱うためのクラス
            var flatReply = JsonUtility.FromJson<Demand_Reply>(jsonString);
            // Debug.Log($"flatReply.ok: {flatReply.ok}");
            // Debug.Log($"flatReply.edgeFlags: {string.Join(",", flatReply.edgeFlags)}");
            // Debug.Log($"flatReply.crossFlags: {string.Join(",", flatReply.crossFlags)}");
            // Debug.Log($"flatReply.edgeFlagRow: {flatReply.edgeFlagRow}");

            return flatReply;
        }
        public Demand_Reply Send_Resource(string sub, Demand_Resource demand)
        {
            // SendRaw_Resourceの結果をそのまま返す
            return SendRaw_Resource(sub, demand);
        }

        public async Task<string> SendRaw_CSoS_Async(string sub, Demand_CSoS demand)
        {
        try
        {
            var msg = await connection.RequestAsync(
                sub, Encoding.UTF8.GetBytes(JsonUtility.ToJson(demand)),
                1000 * 10); // タイムアウト10秒

            return Encoding.UTF8.GetString(msg.Data);
        }
        catch (NATSTimeoutException ex)
        {
            Debug.LogError($"Timeout occurred while sending CSoS: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error occurred while sending CSoS: {ex.Message}");
            throw;
        }
        }

        public async Task<int> Send_CSoS_Async(string sub, Demand_CSoS demand)
        {
        string rawResult = await SendRaw_CSoS_Async(sub, demand);
        return int.Parse(rawResult);
        }

        public async Task<Demand_Reply> SendRaw_Resource_Async(string sub, Demand_Resource demand)
        {
        try
        {
            var msg = await connection.RequestAsync(
                sub, Encoding.UTF8.GetBytes(JsonUtility.ToJson(demand)),
                1000 * 10); // タイムアウト10秒

            string jsonString = Encoding.UTF8.GetString(msg.Data);
            Debug.Log($"Received JSON: {jsonString}");

            // JSON をオブジェクトにデシリアライズ
            return JsonUtility.FromJson<Demand_Reply>(jsonString);
        }
        catch (NATSTimeoutException ex)
        {
            Debug.LogError($"Timeout occurred while sending resource request: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error occurred while sending resource request: {ex.Message}");
            throw;
        }
        }

        public async Task<Demand_Reply> Send_Resource_Async(string sub, Demand_Resource demand)
        {
        return await SendRaw_Resource_Async(sub, demand);
        }

        [Serializable]
        private class RobotPathsWrapper
        {
            public List<RobotPath> paths = new List<RobotPath>();

            public RobotPathsWrapper(Dictionary<int, List<int>> robotPaths)
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
            public List<int> path;

            public RobotPath(int id, List<int> path)
            {
                this.id = id;
                this.path = path;
            }
        }

        [Serializable]
        private class AgentNextPositionWrapper
        {
            public List<AgentNextPosition> agents = new List<AgentNextPosition>();

            public AgentNextPositionWrapper(Dictionary<int, int> agentPositions)
            {
                foreach (var kvp in agentPositions)
                {
                    agents.Add(new AgentNextPosition(kvp.Key, kvp.Value));
                }
            }
        }

        [Serializable]
        private class AgentNextPosition
        {
            public int id;
            public int nextPosition;

            public AgentNextPosition(int id, int nextPosition)
            {
                this.id = id;
                this.nextPosition = nextPosition;
            }
        }
    }
}
