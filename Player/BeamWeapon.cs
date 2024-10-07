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
    LineRenderer lineRenderer;
    float timeSinceLastUpdate = 0f;
    List<Collider2D> enemiesHit = new List<Collider2D>();


    private void Start()
    {
        lineRenderer = GetComponentInChildren<LineRenderer>();
        maxBeamDistance = maxBeamDistance > maxSafeDistance ? maxSafeDistance : maxBeamDistance;

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

        // Only update the beam if it's held down and enough time has passed to prevent freezing
        if (drawBeam)
        {
            timeSinceLastUpdate += Time.unscaledDeltaTime;

            if (timeSinceLastUpdate >= beamUpdateInterval)
            {
                DrawBeam(transform.position, transform.up);
                timeSinceLastUpdate = 0f;
            }
        }

        if (drawBeam && !PlayerManager.Instance.shotOnCooldown && Input.GetMouseButtonDown(0))
        {
            FireWeapon();
        }
    }

    // Draws the beam reflecting off surfaces
    void DrawBeam(Vector2 startPos, Vector2 direction)
    {
        lineRenderer.positionCount = 1; // Reset position count to 1
        lineRenderer.SetPosition(0, startPos); // Set the starting position

        Vector2 currentPosition = startPos;
        Vector2 currentDirection = direction;

        enemiesHit.Clear();  // Reset the list of enemies hit

        int reflections = 0;

        while (reflections < maxReflections)
        {
            // Raycast to find obstacles
            RaycastHit2D hit = Physics2D.Raycast(currentPosition, currentDirection, maxBeamDistance, obstacleLayer);

            if (hit.collider != null)
            {
                lineRenderer.positionCount += 1; // Increase position count
                lineRenderer.SetPosition(reflections + 1, hit.point); // Set the hit point

                // Reflect the beam if it's an obstacle
                if (((1 << hit.collider.gameObject.layer) & obstacleLayer) != 0)
                {
                    currentDirection = Vector2.Reflect(currentDirection, hit.normal);
                    currentPosition = hit.point;
                    reflections++;
                }
            }
            else
            {
                // No more obstacles, draw the beam to max distance
                lineRenderer.positionCount += 1; // Increase position count for end position
                lineRenderer.SetPosition(reflections + 1, currentPosition + currentDirection * maxBeamDistance);
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

        ClearBeam();

        Vector2 startPosition = transform.position;
        Vector2 direction = transform.up;

        // Reset the list of enemies hit
        enemiesHit.Clear();

        // Check for enemies and obstacles along the beam's path
        for (int i = 0; i <= maxReflections; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(startPosition, direction, maxBeamDistance, enemyLayer | obstacleLayer);

            if (hit.collider != null)
            {
                // If an enemy is hit, add it to the list
                if (((1 << hit.collider.gameObject.layer) & enemyLayer) != 0)
                {
                    if (!enemiesHit.Contains(hit.collider))
                    {
                        enemiesHit.Add(hit.collider);
                    }
                }

                // If an obstacle is hit, reflect and continue the beam
                if (((1 << hit.collider.gameObject.layer) & obstacleLayer) != 0)
                {
                    direction = Vector2.Reflect(direction, hit.normal);
                    startPosition = hit.point;
                }
            }
            else
            {
                // If no more obstacles or enemies are hit, end the loop
                break;
            }
        }

        print($"Beam Weapon hit {enemiesHit.Count} enemies.");

        // Apply damage to all enemies hit
        foreach (var enemyCollider in enemiesHit)
        {
            EnemyCharacter enemy = enemyCollider.GetComponent<EnemyCharacter>();
            if (enemy != null)
            {
                Debug.Log($"Beam Weapon hit Enemy: {enemy.Data.enemyName}, dealing {Damage} damage.");
                enemy.TakeDamage(Damage);
            }
        }

        StartCoroutine(ShotVFX());
        PlayerManager.Instance.StartShotCooldown();
    }



    // Check for enemies along the raycast path
    private void CheckForEnemies(Vector2 startPosition, Vector2 direction)
    {
        const int maxCheckAttempts = 10;
        int attempts = 0;

        RaycastHit2D hit = Physics2D.Raycast(startPosition, direction, maxBeamDistance, enemyLayer);
        while (hit.collider != null && attempts < maxCheckAttempts)
        {
            if (!enemiesHit.Contains(hit.collider))
            {
                enemiesHit.Add(hit.collider);
            }

            // Continue raycasting in the same direction
            hit = Physics2D.Raycast(hit.point, direction, maxBeamDistance, enemyLayer);
            attempts++;
        }
    }

    private void ClearBeam()
    {
        lineRenderer.positionCount = 0;
    }

    IEnumerator ShotVFX()
    {
        lineRenderer.material.color = beamColorFiring;
        yield return new WaitForSeconds(beamColorChangeDuration);

        lineRenderer.material.color = beamColorAiming;
    }
}
