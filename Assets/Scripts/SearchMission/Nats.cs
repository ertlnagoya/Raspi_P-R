using System.Text;
using System;
using NATS.Client;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using Mission;

namespace Mission
{
    public class Nats
    {
        private readonly IConnection connection;

        public Nats()
        {
            var opts = ConnectionFactory.GetDefaultOptions();
            opts.Url = "nats://localhost:4222";
            connection = new ConnectionFactory().CreateConnection(opts);
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

    }
}