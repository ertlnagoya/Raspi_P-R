using LineTrace;
using UnityEngine;

namespace Display
{
    public class EdgeDisp : MonoBehaviour
    {
        public Color color;

        public int num0;
        public int num1;

        private Material myMat;

        private Nats nats;

        private void Awake()
        {
            //myMat = GetComponent<MeshRenderer>().material;
            //nats = new Nats();

            //var tmp = name.Split("-");
            //num0 = int.Parse(tmp[0]);
            //num1 = int.Parse(tmp[1]);
        }

        private void Update()
        {
            //var n = nats.Send("disp", new Demand {Src = num0, Dst = num1});
           // myMat.color = n == 1 ? color : Color.white;
        }

        private void OnDestroy()
        {
            Destroy(myMat);
        }
    }
}