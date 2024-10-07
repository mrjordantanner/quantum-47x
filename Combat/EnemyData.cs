using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

[CreateAssetMenu(menuName = "Enemy/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Enemy")]
    public string enemyName;
    public int maxHealth;
    public Sprite sprite;

    [Header("Attack Timing")]
    public FiringStyle firingStyle;
    public float startDelayMin = 0;
    public float startDelayMax = 1.5f;
    public float attackCooldownMin = 3, attackCooldownMax = 5;
    public float startDelay;
    public float rapidFireInterval;
    public float attackDuration;







}