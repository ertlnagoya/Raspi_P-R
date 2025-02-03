using System;
using LineTrace;
using NATS.Client;
using UnityEngine;

namespace Mission
{
    public class Arbitrator : MonoBehaviour
    {
        private Nats nats;

        void Start()
        {
            nats = new Nats();
        }

        void ProcessRobotLocation(string message)
        {

        }

        void OnApplicationQuit()
        {

        }
    }
}


