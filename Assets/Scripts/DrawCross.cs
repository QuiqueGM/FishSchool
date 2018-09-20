using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCross : MonoBehaviour
{
    private float size = 1;

    public void Init(float size)
    {
        this.size = size;
    }

    private void OnDrawGizmos()
    {
        Vector3 pos = gameObject.transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(Vector3.up * size + pos, Vector3.down * size + pos);
        Gizmos.DrawLine(Vector3.left * size + pos, Vector3.right * size + pos);
        Gizmos.DrawLine(Vector3.forward * size + pos, Vector3.back * size + pos);
    }
}
