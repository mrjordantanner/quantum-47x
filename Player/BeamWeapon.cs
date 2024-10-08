using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class BeamWeapon : MonoBehaviour
{
    [Header("Properties")]
    public float Damage = 1;
    public int maxReflections = 3;
    public float maxBeamDistance = 100f;
    public float maxSafeDistance = 100f;
    public float beamUpdateInterval = 0.1f;
    public LayerMask obstacleLayer;
    public LayerMask enemyLayer;

    [Header("VFX")]
    public float beamColorChangeDuration = 0.25f;
    public Color beamColorAiming, beamColorFiring;

    bool drawBeam;
    bool fireBeam; // New flag to track whether the beam should be firing
    LineRenderer lineRenderer;
    float timeSinceLastUpdate = 0f;
    List<Collider2D> enemiesHit = new List<Collider2D>();

    private void Start()
    {
        lineRenderer = GetComponentInChildren<LineRenderer>();
        maxBeamDistance = Mathf.Min(maxBeamDistance, maxSafeDistance);
        lineRenderer.material.color = beamColorAiming;
    }

    private void Update()
    {
        HUD.Instance.UpdateShotCooldown();
        drawBeam = Input.GetMouseButton(1) && !PlayerManager.Instance.shotOnCooldown;

        if (!drawBeam)
        {
            ClearBeam();
        }

        // Only update the beam if it's held down and enough time has passed
        if (drawBeam)
        {
            timeSinceLastUpdate += Time.unscaledDeltaTime;

            if (timeSinceLastUpdate >= beamUpdateInterval)
            {
                DrawBeam(transform.position, transform.up);
                timeSinceLastUpdate = 0f;
            }
        }

        // Fire the weapon only when the fire button is pressed
        if (drawBeam && !PlayerManager.Instance.shotOnCooldown && Input.GetMouseButtonDown(0))
        {
            fireBeam = true; // Set fire flag when the weapon is fired
            FireWeapon();
        }
    }

    [ReadOnly] public int reflections;

    // Draws the beam reflecting off surfaces (no enemy interaction here)
    void DrawBeam(Vector2 startPos, Vector2 direction)
    {
        lineRenderer.positionCount = 1; // Reset position count to 1
        lineRenderer.SetPosition(0, startPos); // Set the starting position

        Vector2 currentPosition = startPos;
        Vector2 currentDirection = direction;

        reflections = 0;
        float totalDistanceTravelled = 0f; // Track total beam travel distance

        while (reflections < maxReflections && totalDistanceTravelled < maxBeamDistance)
        {
            // Raycast only for obstacles in this method (no enemies)
            RaycastHit2D obstacleHit = Physics2D.Raycast(currentPosition, currentDirection, maxBeamDistance - totalDistanceTravelled, obstacleLayer);

            if (obstacleHit.collider != null)
            {
                lineRenderer.positionCount += 1;
                lineRenderer.SetPosition(reflections + 1, obstacleHit.point);

                // Reflect the beam if it's an obstacle
                currentDirection = Vector2.Reflect(currentDirection, obstacleHit.normal);

                // Move the position slightly forward along the reflection direction to avoid raycast issues
                currentPosition = obstacleHit.point + currentDirection * 0.1f;

                reflections++;
                totalDistanceTravelled += Vector2.Distance(currentPosition, obstacleHit.point);
                PlayerManager.Instance.beamReflections++;
            }
            else
            {
                // No more obstacles, draw the beam to max distance
                lineRenderer.positionCount += 1;
                lineRenderer.SetPosition(reflections + 1, currentPosition + currentDirection * (maxBeamDistance - totalDistanceTravelled));
                break;
            }
        }
    }

    public void FireWeapon()
    {
        AudioManager.Instance.soundBank.BeamWeapon.Play();
        CameraShaker.Instance.Shake(CameraShaker.ShakeStyle.Small);
        LevelController.Instance.CurrentLevel.shotsFiredThisLevel++;
        StartCoroutine(HUD.Instance.TextPop(HUD.Instance.shotsFiredLabel));
        HUD.Instance.UpdateShotsUI();

        Vector2 startPosition = transform.position;
        Vector2 direction = transform.up;

        // Reset the list of enemies hit
        enemiesHit.Clear();
        float totalDistanceTravelled = 0f; // Track total distance the beam has traveled

        // Perform raycasting with reflections and pierce through enemies
        for (int i = 0; i <= maxReflections && totalDistanceTravelled < maxBeamDistance; i++)
        {
            RaycastHit2D obstacleHit = Physics2D.Raycast(startPosition, direction, maxBeamDistance - totalDistanceTravelled, obstacleLayer);
            RaycastHit2D enemyHit = Physics2D.Raycast(startPosition, direction, maxBeamDistance - totalDistanceTravelled, enemyLayer);

            RaycastHit2D hit = GetFirstHit(obstacleHit, enemyHit);

            if (hit.collider != null)
            {
                // If an enemy is hit, add it to the list and continue raycasting without reflecting
                if (((1 << hit.collider.gameObject.layer) & enemyLayer) != 0)
                {
                    EnemyCharacter enemy = hit.collider.GetComponent<EnemyCharacter>();

                    if (!enemiesHit.Contains(hit.collider) && enemy != null)
                    {
                        enemiesHit.Add(hit.collider);
                        Debug.Log($"Beam Weapon hit Enemy: {enemy.Data.enemyName}, dealing {Damage} damage.");
                        enemy.TakeDamage(Damage);
                    }

                    // Move past the enemy
                    startPosition = hit.point + direction * 0.01f;
                    totalDistanceTravelled += Vector2.Distance(startPosition, hit.point);
                }

                // If an obstacle is hit, reflect and continue
                if (((1 << hit.collider.gameObject.layer) & obstacleLayer) != 0)
                {
                    direction = Vector2.Reflect(direction, hit.normal);

                    // Move startPosition slightly forward to avoid getting stuck at reflection points
                    startPosition = hit.point + direction * 0.1f;
                    totalDistanceTravelled += Vector2.Distance(startPosition, hit.point);
                }
            }
            else
            {
                // If no more obstacles or enemies, break out of loop
                break;
            }
        }

        print($"Beam Weapon hit {enemiesHit.Count} enemies.");

        StartCoroutine(ShotVFX());
        PlayerManager.Instance.StartShotCooldown();

        fireBeam = false; // Reset fire flag after firing is complete
    }

    // Helper function to decide which hit occurs first (obstacle or enemy)
    RaycastHit2D GetFirstHit(RaycastHit2D obstacleHit, RaycastHit2D enemyHit)
    {
        if (obstacleHit.collider == null) return enemyHit;
        if (enemyHit.collider == null) return obstacleHit;

        // Return the closest hit (enemy or obstacle)
        return (obstacleHit.distance < enemyHit.distance) ? obstacleHit : enemyHit;
    }

    private void ClearBeam()
    {
        if (GameManager.Instance.gamePaused) return;

        lineRenderer.positionCount = 0;
    }

    IEnumerator ShotVFX()
    {
        lineRenderer.material.color = beamColorFiring;
        yield return new WaitForSeconds(beamColorChangeDuration);

        ClearBeam();
        lineRenderer.material.color = beamColorAiming;
    }
}
