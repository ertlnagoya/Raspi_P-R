using System;
using UnityEngine;

namespace LineTrace
{
    public class Road : MonoBehaviour
    {
        private Nats nats;

        private void Awake()
        {
            nats = new Nats();
        }

        private void OnDestroy()
        {
            nats.Send("fin", new Demand());
        }
    }
}