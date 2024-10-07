using UnityEngine;


public class FiringPoint : MonoBehaviour
{
    [Header("Gizmos")]
    public float gizmoSize = 0.25f;
    public Color gizmoColor = Color.cyan;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * gizmoSize);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.left * gizmoSize);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * gizmoSize);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * gizmoSize);
    }

}
