using System;
using UnityEngine;

namespace LineTrace
{
    public class Cross : MonoBehaviour
    {
        [HideInInspector] public int number;

        public int[] nexts = {-1, -1, -1, -1};
        private Vector3[] _pivs = new Vector3[4];

        private void Awake()
        {
            number = int.Parse(name.Substring(5, name.Length - 5));
            for (int i = 0; i < 4; i++)
            {
                if (nexts[i] == -1) continue;
                _pivs[i] = transform.Find($"dir{nexts[i]}").position;
            }
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

            throw new Exception($"{number} -/-> {next}");
        }
    }
}