using Cinemachine;
using RasPiMouse.Device;
using UnityEngine;
using UnityEngine.UI;

namespace Display
{
    public class CamOp : MonoBehaviour
    {
        public string search;

        private string preSearch;

        public Transform pivot;
        private Transform pre;
        public RawImage image;
        private Transform empty;


        private void Start()
        {
            empty = new GameObject().transform;
            GetComponent<CinemachineVirtualCamera>().Follow = empty;

        }

        void Update()
        {
            if (search != preSearch)
            {
                preSearch = search;
                var hit = GameObject.Find(search);
                if (hit)
                {
                    pivot = hit.transform;
                }
            }

            if (pre != pivot && pivot)
            {
//                var tmp = pivot.Find("base_footprint");
//                if (tmp)
//                {
//                    tmp = tmp.Find("base_link");
//                    if (tmp)
//                    {
//                        pivot = tmp;
//                    }
//                }

                var mini = pivot.GetComponentInChildren<MiniCamera>();
                if (mini && image) image.texture = mini.GetTexture();

                pre = pivot;
            }

            var tf = transform;
            var eu = tf.eulerAngles;
            if (Input.GetKey(KeyCode.A))
            {
                eu.y += 40 * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                eu.y -= 40 * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.W))
            {
                eu.x += 40 * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                eu.x -= 40 * Time.deltaTime;
            }


            tf.rotation = Quaternion.Euler(eu);
            if (empty)
            {
                eu.x = 0;
                eu.z = 0;
                empty.rotation = Quaternion.Euler(eu);
            }
        }

        private void LateUpdate()
        {
            if (empty && pivot)
            {
                empty.position = pivot.position;
            }
        }
    }
}