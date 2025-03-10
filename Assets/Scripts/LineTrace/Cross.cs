using System;
using UnityEngine;

namespace LineTrace
{
    public class Cross : MonoBehaviour
    {
        [HideInInspector] public int number;

        public int[] nexts = {-1, -1, -1, -1};
        private Vector3[] _pivs = new Vector3[4];
        public float capsuleRadius = 0.03f;

        private void Awake()
        {
            number = int.Parse(name.Substring(5, name.Length - 5));
            for (int i = 0; i < 4; i++)
            {
                if (nexts[i] == -1) continue;
                Transform target = transform.Find($"dir{nexts[i]}");

                if (target != null)
                {
                    _pivs[i] = target.position;
                }
                else
                {
                    Debug.LogError($"[{gameObject.name}] 找不到子对象 'dir{nexts[i]}'！");
                }
            }
        }

        private void Start()
        {

        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow; 
            Gizmos.DrawSphere(transform.position, 0.02f); 
        }

        public Vector3 GetDir(int next)
        {
            for (int i = 0; i < 4; i++)
            {
                if (nexts[i] == next)
                {
                    return _pivs[i];
                }
            }

            throw new Exception($"try from {number} -/-> {next}");
        }
    }
}