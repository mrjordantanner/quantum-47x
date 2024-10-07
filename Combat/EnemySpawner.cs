using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject EnemyPrefabToSpawn;
    [ReadOnly] public EnemyCharacter childEnemy;


    // ** TODO **
    [Header("Enemy Properties")]
    public FiringStyle firingStyle;
    public GunData gunData;
    public string bulletPoolName;
    //


    [Header("Gizmos")]
    public float gizmoSize = 0.5f;
    public Color gizmoColor = Color.red;

    public void Spawn(Vector3 offset)
    {
        var Enemy = Instantiate(EnemyPrefabToSpawn, transform.position + offset, Quaternion.identity);
        Enemy.transform.SetParent(transform, true);
        childEnemy = Enemy.GetComponent<EnemyCharacter>();
        childEnemy.InitializeEnemyData(childEnemy.Data);
        childEnemy.parentSpawner = this;
        childEnemy.Init();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * gizmoSize);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.left * gizmoSize);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * gizmoSize);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * gizmoSize);
    }


}
