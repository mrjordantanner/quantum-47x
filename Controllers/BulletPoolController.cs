using UnityEngine;
using UnityEngine.Pool;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Holds references to projectile prefabs and manages their object pools
/// </summary>
public class BulletPoolController : MonoBehaviour, IInitializable
{
    #region Singleton
    public static BulletPoolController Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
    #endregion

    public string Name { get { return "Bullet Pools"; } }

    public bool fillPoolsOnStart;
    public BulletPool[] BulletPools;
    // public GameObject[] Bullets;

    public IEnumerator Init()
    {
        BulletPools = GetComponents<BulletPool>();
        InitializePools();
        yield return new WaitForSeconds(0);
    }

    public void InitializePools()
    {
        foreach (var pool in BulletPools)
        {
            pool.CreatePool();
        }

    }

    public BulletPool GetBulletPool(string poolName)
    {
        return BulletPools.FirstOrDefault(bp => bp.PoolName == poolName);
    }

}

