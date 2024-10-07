using Unity.Services.CloudSave.Models.Data.Player;
using UnityEngine;


public class PlayerSpawnPoint : MonoBehaviour
{
    [Header("Gizmos")]
    public float gizmoSize = 0.5f;
    public Color gizmoColor = Color.green;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        // Draw the diagonals to form an "X"
        Gizmos.DrawLine(transform.position + new Vector3(-1, 1, 0) * gizmoSize,
                        transform.position + new Vector3(1, -1, 0) * gizmoSize);

        Gizmos.DrawLine(transform.position + new Vector3(-1, -1, 0) * gizmoSize,
                        transform.position + new Vector3(1, 1, 0) * gizmoSize);
    }
}