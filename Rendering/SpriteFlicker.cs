using UnityEngine;


public class SpriteFlicker : MonoBehaviour
{
    public bool flicker = false;
    public float rate;
    public float duration = 2;
    float flickerTimer, durationTimer;
    public SpriteRenderer spriteRenderer;

    void Start()
    {
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        flickerTimer = rate;
        durationTimer = duration;
    }

    void FixedUpdate()
    {
        if (flicker && spriteRenderer != null)
        {
            HandleFlicker();
        }
    }

    public void On(float duration = 2)
    {
        flicker = true;
        durationTimer = duration;
    }

    public void Off()
    {
        flicker = false;
        if (spriteRenderer) spriteRenderer.enabled = true;
    }

    void HandleFlicker()
    {
        flickerTimer -= Time.fixedDeltaTime;

        if (flickerTimer <= 0)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            flickerTimer = rate;
        }

        durationTimer -= Time.fixedDeltaTime;

        if (durationTimer <= 0)
        {
            flicker = false;
            spriteRenderer.enabled = true;
            durationTimer = duration;
        }
    }
}
