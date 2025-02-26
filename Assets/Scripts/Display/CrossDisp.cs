using System;
using LineTrace;
using UnityEngine;

namespace Display
{
    public class CrossDisp : MonoBehaviour
    {
        public Color color;

        private int num;

        private Material myMat;

        private Nats nats;

        private void Awake()
        {
            //myMat = GetComponent<MeshRenderer>().material;
            //nats = new Nats();

            //num = int.Parse(name);
        }

        private void Update()
        {
            //var n = nats.Send("disp", new Demand {Src = num, Dst = -1});
            //myMat.color = n == 1 ? color : Color.white;
        }

        private void OnDestroy()
        {
            Destroy(myMat);
        }
    }

    [Serializable]
    public struct Question
    {
        public int Cross1;
        public int Cross2;
    }
}