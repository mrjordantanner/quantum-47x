using System.Collections;
using UnityEngine;
using UnityEngine.Pool;


/// <summary>
/// Object pool for a projectile.  Each projectile will need its own dedicated pool.
/// </summary>
public class BulletPool : MonoBehaviour
{


    public string PoolName;

    public GameObject ProjectilePrefab;
    public int defaultCapacity = 250;
    public int maxSize = 500;
    [ReadOnly]
    public int bulletsCreated;
    [ReadOnly]
    public int bulletsDestroyed;

    [HideInInspector]
    public ObjectPool<GameObject> Pool;

    GameObject BulletContainer;


    private void Start()
    {
        BulletContainer = new GameObject($"BulletContainer_{Utils.ShortName(ProjectilePrefab.name)}");
    }

    public void CreatePool()
    {
        //print($"Creating Bullet Pool: {defaultCapacity}");

        Pool = new ObjectPool<GameObject>(
        createFunc: () => CreateProjectile(),
        actionOnGet: (obj) => GetProjectile(obj),
        actionOnRelease: (obj) => ReleaseProjectile(obj),
        actionOnDestroy: (obj) => DestroyProjectile(obj),
        collectionCheck: false,
        defaultCapacity: defaultCapacity,
        maxSize: maxSize);

        if (BulletPoolController.Instance.fillPoolsOnStart)
        {
            FillPoolToDefaultSize();
        }
    }

    public void FillPoolToDefaultSize()
    {
        //print($"Filling bullet pool, Default size: {defaultCapacity}");

        // Instantiate default capacity
        for (int i = 0; i < defaultCapacity; i++)
        {
            var obj = CreateProjectile();
            ReleaseProjectile(obj);
        }
    }

    GameObject CreateProjectile()
    {
        //print($"Create Projectile");
        bulletsCreated++;

        var ProjectileObject = Instantiate(ProjectilePrefab, transform.position, Quaternion.identity, BulletContainer.transform);
        var projectile = ProjectileObject.GetComponent<Projectile>();
        projectile.AssignColor(Color.white);
        projectile.pool = Pool;

        return ProjectileObject;
    }


    void GetProjectile(GameObject obj)
    {
        //print($"Get Projectile {obj.name}");
        obj.SetActive(true);
    }

    void ReleaseProjectile(GameObject obj)
    {
        //print($"Release Projectile {obj.name}");
        obj.SetActive(false);
    }

    void DestroyProjectile(GameObject obj)
    {
        //print($"Destroy Projectile {obj.name}");
        bulletsDestroyed++;
        Destroy(obj);
    }
}

