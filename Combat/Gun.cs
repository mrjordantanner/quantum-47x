using UnityEngine;
using System.Collections;
using UnityEngine.Pool;
using System;

public class Gun : MonoBehaviour
{
    public enum ShotPattern { Straight, Angle }

    [ReadOnly] public bool onCooldown;
    public GunData data;
    [ReadOnly] public FiringPoint firingPoint;
    [ReadOnly] public BulletPool ActiveBulletPool;
    public string bulletPoolName = "S_Ellipse";

    private void Start()
    {
        ActiveBulletPool = LoadBulletPool(bulletPoolName);
        firingPoint = GetComponentInChildren<FiringPoint>();
    }

    public BulletPool LoadBulletPool(string poolName)
    {
        var pool = BulletPoolController.Instance.GetBulletPool(poolName);
        if (pool != null)
        {
            ActiveBulletPool = pool;
            //print($"Gun: Loaded Bullet Pool: {poolName} for gun {data.gunName}");
            return pool;
        }

        Debug.LogError($"Gun: Error loading bullet pool {poolName}");
        return null;
    }

    GameObject GetAndSetProjectile(Vector3 position, Quaternion rotation)
    {
        if (ActiveBulletPool == null)
        {
            LoadBulletPool(bulletPoolName);
        }

        var ProjectileObject = ActiveBulletPool.Pool.Get();
        ProjectileObject.transform.SetPositionAndRotation(position, rotation);
        ProjectileObject.tag = "EnemyProjectile";

        var projectile = ProjectileObject.GetComponent<Projectile>();

        projectile.rotation = data.rotation;
        projectile.lifespan = 25 / data.bulletSpeed;
        projectile.Damage = data.damage;

        ConfigureGraphicsOptions(projectile);
        var bulletColor = Color.cyan;
        projectile.AssignColor(bulletColor);

        return ProjectileObject;
    }

    void ConfigureGraphicsOptions(Projectile projectile)
    {
        if (data.spriteTrails)
        {
            if (projectile.spriteTrails != null)
            {
                projectile.spriteTrails.on = true;
                projectile.spriteTrails.repeatRate = data.spriteTrailsRepeatRate;
                projectile.spriteTrails.duration = data.spriteTrailsDuration;
                projectile.spriteTrails.trailOpacity = data.spriteTrailsOpacity;
                projectile.spriteTrails.trailColorBlue = data.spriteTrailColorBlue;
                projectile.spriteTrails.trailColorRed = data.spriteTrailColorRed;
                projectile.spriteTrails.trailColorGreen = data.spriteTrailColorGreen;
            }
        }
        else
        {
            if (projectile.spriteTrails != null)
            {
                projectile.spriteTrails.on = false;
            }
        }

        if (data.sineMotion)
        {
            if (projectile.sineMotion != null)
            {
                projectile.sineMotion.Init();
                projectile.sineMotion.isActive = true;
                projectile.sineMotion.sineMotion = true;
                projectile.sineMotion.amplitude = data.sineAmplitude;
                projectile.sineMotion.frequency = data.sineFrequency;
                projectile.sineMotion.speed = data.sineSpeed;
            }
        }
        else
        {
            if (projectile.sineMotion != null)
            {
                projectile.sineMotion.isActive = false;
                projectile.sineMotion.sineMotion = false;
            }
        }


    }

    public void Fire(Vector2 shotDirection)
    {
        if (firingPoint == null) return;

        GameObject ProjectileObject;
        var bulletsPerShot = data.shots;

        switch (data.shotPattern)
        {
            case ShotPattern.Straight:
                // Create a rotation that points toward the player
                Quaternion startRot = Quaternion.LookRotation(Vector3.forward, shotDirection) * Quaternion.Euler(0f, 0f, -90f);  // Adjust by -90 degrees

                if (bulletsPerShot == 1)
                {
                    ProjectileObject = GetAndSetProjectile(firingPoint.transform.position, startRot);
                    AddVelocity(ProjectileObject, shotDirection);  // Use shotDirection directly
                    break;
                }

                var adjustedShotWidth = data.multiStraightShotWidth;
                var adjustedSpacing = Mathf.Min(adjustedShotWidth, adjustedShotWidth / (bulletsPerShot - 1));

                Vector3 startPos = firingPoint.transform.position;
                Vector3 axis = firingPoint.transform.right;

                for (int i = 0; i < bulletsPerShot; i++)
                {
                    Vector3 offset = axis * (i * adjustedSpacing - (adjustedShotWidth / 2f));
                    ProjectileObject = GetAndSetProjectile(startPos + offset, startRot);
                    AddVelocity(ProjectileObject, shotDirection);  // Use shotDirection directly
                }
                break;

            case ShotPattern.Angle:
                // Create a base rotation that looks in the shotDirection
                Quaternion baseRotation = Quaternion.LookRotation(Vector3.forward, shotDirection) * Quaternion.Euler(0f, 0f, 90f);  // Adjust by 90 degrees

                // Calculate the angle spread for each bullet
                float angleBetweenBullets = data.maxSpreadAngle / (bulletsPerShot - 1);
                var initialBulletRotation = Quaternion.AngleAxis(-data.maxSpreadAngle / 2f, Vector3.forward) * baseRotation;  // Spread starts here

                for (int i = 0; i < bulletsPerShot; i++)
                {
                    // Apply a rotation for each bullet in the spread
                    var bulletRotation = initialBulletRotation * Quaternion.AngleAxis(angleBetweenBullets * i, Vector3.forward);
                    ProjectileObject = GetAndSetProjectile(firingPoint.transform.position, bulletRotation);

                    // Calculate the direction for each bullet from its rotation
                    Vector3 angledShotDirection = bulletRotation * Vector3.right;  // Right is local forward for the bullet
                    AddVelocity(ProjectileObject, angledShotDirection);  // Use the calculated angled direction
                }
                break;
        }

        StartCoroutine(ShotCooldown());
    }

    void AddVelocity(GameObject ProjectileObject, Vector3 shotDirection)
    {
        var rb = ProjectileObject.GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;
        rb.velocity = shotDirection.normalized * data.bulletSpeed;  // Use the specific shot direction for each bullet

        //print($"Gun: AddVelocity {rb.velocity} to ProjectileObject: {ProjectileObject.name}");
    }




    IEnumerator ShotCooldown()
    {
        //print("Gun: Shot Cooldown");

        onCooldown = true;
        yield return new WaitForSeconds(data.cooldown);

        onCooldown = false;
    }
}
