using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dir : MonoBehaviour
{
    public Color arrowColor = new Color(0.31f, 0.78f, 0.71f, 1.0f);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmos()
    {
        Gizmos.color = arrowColor;
        Gizmos.DrawSphere(transform.position, 0.02f);
    }
}
