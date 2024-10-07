using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UIElements;


public enum FiringStyle { SingleShot, RapidBurst };

public class EnemyCharacter : Character
{ 
    public bool isAggroed = true;
    public EnemyData Data;
    public EnemyController controller;
    public SpriteRenderer spriteRenderer;
    [HideInInspector] public EnemySpawner parentSpawner;

    [Header("Firing")]
    [ReadOnly] public float currentAttackCooldown;
    public FiringStyle firingStyle;
    public Gun gun;
    [ReadOnly] public Vector2 Target;
    [ReadOnly] public float rapidFireTimer;
    [ReadOnly] public float totalFiringTimer;
    [ReadOnly] public bool onCooldown;
    [ReadOnly] public bool isRapidFiring;

    float RandomCooldown
    {
        get
        {
            return Random.Range(Data.attackCooldownMin, Data.attackCooldownMax);
        }
    }

    public override void Init()
    {
        base.Init();

        if (!controller) controller = GetComponent<EnemyController>();

        var randomStartDelay = Random.Range(Data.startDelayMin, Data.startDelayMax);
        StartCoroutine(StartAttackCooldown(randomStartDelay));

    }

    public void InitializeEnemyData(EnemyData data)
    {
        //print($"Init enemy data for {data.enemyName}");
        Data = data;

        maxHealth = Data.maxHealth;
        characterSpriteRenderer.sprite = Data.sprite;
    }

    private void Update()
    {
        if (!onCooldown && isAggroed && GameManager.Instance.gameRunning)
        {
            HandleFiringStyle();
        }

       // HandleTestInput();
    }


    void HandleFiringStyle()
    {
        // SINGLE SHOT
        if (Data.firingStyle == FiringStyle.SingleShot && !onCooldown)
        {
            Target = DetermineTarget();
            if (Target == Vector2.zero) return;

            StartCoroutine(ShootFlash());
            gun.Fire(Target);
            StartCoroutine(StartAttackCooldown(RandomCooldown));
        }

        // RAPID FIRE
        if (Data.firingStyle == FiringStyle.RapidBurst && !onCooldown)
        {
            // Start rapid fire
            if (!isRapidFiring)
            {
                Target = DetermineTarget();
                if (Target == Vector2.zero) return;

                isRapidFiring = true;
                StartCoroutine(ShootFlash());
            }

            // During rapid fire
            if (totalFiringTimer < Data.attackDuration)
            {
                totalFiringTimer += Time.deltaTime;
                rapidFireTimer += Time.deltaTime;

                if (rapidFireTimer >= Data.rapidFireInterval)
                {
                    rapidFireTimer = 0;
                    gun.Fire(Target);
                }
            }

            // Rapid Fire complete, start cooldown
            if (totalFiringTimer >= Data.attackDuration)
            {
                isRapidFiring = false;
                totalFiringTimer = 0;
                rapidFireTimer = 0;
                StartCoroutine(StartAttackCooldown(RandomCooldown));
            }
        }
    }

    Vector3 DetermineTarget()
    {
        var targetDir =  PlayerManager.Instance.player == null ? Vector2.zero : Utils.GetDirectionNormalized(PlayerManager.Instance.player.transform.position, gun.firingPoint.transform.position);
        return targetDir;
    }

    //void HandleTestInput()
    //{
    //    //TEST SHOOT PLAYER
    //    if (Input.GetKeyDown(KeyCode.T) && PlayerManager.Instance.player)
    //    {
    //        var direction = DetermineTarget();
    //        gun.Fire(direction);
    //    }

    //    // TEST DAMAGE
    //    //if (Input.GetKeyDown(KeyCode.H))
    //    //{
    //    //    HitByProjectile(1);
    //    //}
    //}

    IEnumerator StartAttackCooldown(float cooldown)
    {
        //print($"StartAttackCooldown for {Data.enemyName}: {cooldown}");
        onCooldown = true;
        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
    }

    public override void TakeDamage(float damageAmount)
    {
        base.TakeDamage(damageAmount);
        transform.DOShakePosition(0.15f, 0.5f, 5);
    }

    public override void Die()
    {
        LevelController.Instance.OnEnemyDeath(this);
        AudioManager.Instance.soundBank.EnemyDie.Play();
        CameraShaker.Instance.Shake(CameraShaker.ShakeStyle.Medium);

        base.Die();
    }


    public float shootFlashDuration = 0.03f;
    public IEnumerator ShootFlash()
    {
        // ShootFlash twice, then Fire
        characterSpriteRenderer.material = VFX.Instance.shootFlashMaterial;
        yield return new WaitForSeconds(shootFlashDuration);

        characterSpriteRenderer.material = startingMaterial;
        yield return new WaitForSeconds(shootFlashDuration);

        characterSpriteRenderer.material = VFX.Instance.shootFlashMaterial;
        yield return new WaitForSeconds(shootFlashDuration);

        characterSpriteRenderer.material = startingMaterial;
        yield return new WaitForSeconds(shootFlashDuration);
    }

    public override void HitFlash()
    {
        StartCoroutine(SpriteFlash(VFX.Instance.hitFlashMaterial));
    }

    public override IEnumerator SpriteFlash(Material material)
    {
        // Intentionally overrides and does not call base()

        spriteRenderer.material = startingMaterial; // Assuming both renderers use the same material

        if (spriteRenderer != null)
        {
            spriteRenderer.material = material;
            yield return new WaitForSeconds(hitFlashDuration);
        }
        if (spriteRenderer != null)
        {
            spriteRenderer.material = startingMaterial;
        }
    }

}
