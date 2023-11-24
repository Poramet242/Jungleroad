using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridDisplay : MonoBehaviour
{
    public Vector2Int myGridPoint;
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = GridDatabase.Instance.RequestGridData(myGridPoint);
        Gizmos.DrawCube(transform.position + new Vector3(0, 0.25f, 0), new Vector3(0.1f, 0.1f, 0));
    }
#endif
}