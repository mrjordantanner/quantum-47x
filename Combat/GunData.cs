using UnityEngine;

[CreateAssetMenu(menuName = "GunData")]
public class GunData : ScriptableObject
{
    public string bulletPoolName;
    public string gunName;
    public string ShotSoundName = "Shot-1";

    [Header("Gun Style")]
    public Gun.ShotPattern shotPattern;

    [Header("Properties")]
    public int shots;
    public float damage;
    public float cooldown;
    public float bulletSpeed;

    [Header("Other properties")]
    public float multiStraightShotWidth = 1.5f;
    public float maxSpreadAngle = 30f;

    [Header("Rotation")]
    public float rotation;

    [Header("Sine Motion")]
    public bool sineMotion;
    public bool alternateDirections;
    public float sineAmplitude = 1f;
    public float sineFrequency = 1f;
    public float sineSpeed = 1f;

    [Header("Sprite Trails")]
    public bool spriteTrails;
    public float spriteTrailsRepeatRate,
        spriteTrailsDuration,
        spriteTrailsOpacity,
        spriteTrailColorBlue,
        spriteTrailColorRed,
        spriteTrailColorGreen;



}

