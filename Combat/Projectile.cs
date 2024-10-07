using UnityEngine;
using UnityEngine.Pool;
using System.Collections;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Sits on projectile gameObjects and can control behavior
/// </summary>
public class Projectile : MonoBehaviour//, IDamager
{
    //public DamageId DamageId { get; set; }
    //public float Damage { get; set; }

    [ReadOnly] public float Damage = 1;  // Default is 1. Overidden by Gun when fired.

    [Header("Behavior")]
    public float rotation;
    public bool useSineMotion;
    public bool obstacleCollision = true;

    [Header("Impact FX")]
    public GameObject ImpactEffect;
    //public GameObject[] ImpactEffects;
    public Vector2 effectOffset = new();

    [Header("Lights")]
    public Light2D[] lights;

    Rigidbody2D rb;

    [Header("Base Class Properties")]
    [ReadOnly]
    public ObjectPool<GameObject> pool;

    public TrailRenderer trailRenderer;
    public SpriteRenderer spriteRenderer;
    public SineMotion sineMotion;
    public SpriteTrails spriteTrails;

    [ReadOnly]
    public float lifespan = 3f;
    //GameObject PreviousTarget;
    [ReadOnly]
    public float timesPierced;

    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!trailRenderer) trailRenderer = GetComponentInChildren<TrailRenderer>();
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (!spriteTrails) spriteTrails = GetComponentInChildren<SpriteTrails>();
        if (!sineMotion) sineMotion = GetComponent<SineMotion>();
        lights = GetComponentsInChildren<Light2D>();

        if (sineMotion != null)
        {
            sineMotion.Init();
        }

        Invoke(nameof(DestroyProjectile), lifespan);

    }

    private void OnEnable()
    {
        //print($"Projectile: OnEnable");

        if (sineMotion != null)
        {
            sineMotion.Init();
        }

        Invoke(nameof(DestroyProjectile), lifespan);
        timesPierced = 0;
    }

    void Update()
    {
        // PreviousTarget = null;

        if (rotation != 0)
        {
            transform.Rotate(Vector3.forward, rotation * Time.deltaTime);
        }
    }

    public void AssignColor(Color color)
    {
        //print($"Projectile: AssignColor {color}");

        spriteRenderer.color = color;
        var particles = ImpactEffect.GetComponent<ParticleSystem>().main;
        particles.startColor = color;

        foreach (var light in lights)
        {
            light.color = color;
        }
    }

    public void DestroyProjectile()
    {
        CancelInvoke(nameof(DestroyProjectile));
        if (pool != null)
        {
            pool.Release(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }

    //void Explode()
    //{
    //    var Explosion = Instantiate(Combat.Instance.ExplosionPrefab, transform.position, Quaternion.identity);
    //    Explosion.transform.localScale *= Combat.Instance.explodeSize;

    //    var areaDamage = Explosion.GetComponent<AreaDamage>();
    //    areaDamage.Damage = Damage * Combat.Instance.explodeDamage;
    //    areaDamage.knockbackForce = Combat.Instance.explodeKnockbackForce;

    //    AudioManager.Instance.Play("Plasma-Explode");
    //    Instantiate(Combat.Instance.PlasmaExplosionVFX, transform.position, Quaternion.identity);
    //}

    void SpawnImpactEffects()
    {
        if (ImpactEffect != null)
        {
            Instantiate(ImpactEffect, transform.position, Quaternion.identity);
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (obstacleCollision && other.gameObject.layer == 6)
        {
            SpawnImpactEffects();
            DestroyProjectile();
        }

        if (other.CompareTag("Player"))
        {
            //print("Projectile hit Player");
            PlayerManager.Instance.PlayerHit(Damage);

            //var damageable = other.GetComponent<IDamageable>();
            //if (damageable == null)
            //{
            //    print("Damageable not found!");
            //}
            //else
            //{
            //    damageable.TakeDamage(Damage);
            //}

            SpawnImpactEffects();
            DestroyProjectile();

        }
    }

}
