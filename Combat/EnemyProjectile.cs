using System.Collections;
using UnityEngine;


public class EnemyProjectile : Projectile
{
    [Header("Seeker")]
    public bool seeker = false;
    public float seekerVelocity, seekerRotation;
    float sensitivity;
    public float homingSensitivity = 0.2f;
    public float initialHomingSensitivity = 1f;
    public Vector3 targetOffset = new Vector3(0, 1, 0);

    public float homingDelay = 1f;
    public bool homingActive = false;
    float homingTimer;

    public void Start()
    {
        Init();
    }

    public void Init()
    {
        homingTimer = homingDelay;
        homingActive = false;
        sensitivity = initialHomingSensitivity;
    }

    public void FixedUpdate()
    {
        if (seeker)
        {
            SeekTarget();
        }

        if (seeker && !homingActive)
        {
            homingTimer -= Time.deltaTime;

            if (homingTimer <= 0)
            {
                homingActive = true;
                sensitivity = homingSensitivity;
            }
        }

    }


    void SeekTarget()
    {
        Vector3 relativePos = PlayerManager.Instance.player.transform.position - transform.position;
        Quaternion rot = Quaternion.LookRotation(relativePos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, sensitivity);
        transform.Translate(0, 0, seekerVelocity * Time.fixedDeltaTime, Space.Self);

    }


    void OnTriggerEnter(Collider other)
    {
        // if (alreadyHit) return;

        if (other.CompareTag("Player"))
        {
            // alreadyHit = true;
            PlayerManager.Instance.PlayerHit(Damage);
            DestroyProjectile();
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacles") && obstacleCollision)
        {
            DestroyProjectile();
        }
    }


}





