using UnityEngine;


public class Combat : MonoBehaviour
{
    #region Singleton
    public static Combat Instance;
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

    //public float damageRange = 0.05f;
    public float contactDamage = 1;
    public float explodeChance;
    public float explodeDamage;
    public float explodeSize;
    public float explodeKnockbackForce;
    public GameObject ExplosionPrefab;
    public GameObject ExplosionVFX;

}

public enum DamageId
{
    Friendly,
    Hostile
}

public interface IDamageable
{
    public DamageId DamageId { get; }
    public virtual void TakeDamage(float value) { }
}

public interface IDamager
{
    public float Damage { get; }
    public DamageId DamageId { get; }

}
