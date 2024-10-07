using Unity.Services.CloudSave.Models.Data.Player;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.UI;
using System.Diagnostics.Contracts;
using System.Collections;
using UnityEngine.Rendering;
using System.Xml.Serialization;
using Unity.Services.CloudSave;


/// <summary>
/// Sits at top level of Level gameobject, essentially an entire small Scene.
/// </summary>
public class Level : MonoBehaviour
{
    public int levelNumber;
    public string levelName;
    public bool isCompleted;
    public bool isRunning;
    public PlayerSpawnPoint playerSpawnPoint;
    public CameraAnchor cameraAnchor;

    [Header("Shots")]
    public int shotPar;
    [ReadOnly] public int shotsFiredThisLevel;

    public EnemySpawner[] Spawners;
    //public List<EnemyCharacter> ActiveEnemies = new();

    public void Init()
    {
        isCompleted = false;
        isRunning = false;

        if (!playerSpawnPoint) playerSpawnPoint = GetComponentInChildren<PlayerSpawnPoint>();
        Spawners = GetComponentsInChildren<EnemySpawner>();

        HUD.Instance.levelNumberLabel.text = levelNumber.ToString();
        shotsFiredThisLevel = 0;
        HUD.Instance.UpdateShotsUI();
        Clear();
    }

    public void StartLevel()
    {
        print($"Starting Level {levelNumber}...");

        PlayerManager.Instance.SpawnPlayer();
        shotsFiredThisLevel = 0;
        HUD.Instance.UpdateShotsUI();
        isRunning = true;
        SpawnEnemies();
    }

    IEnumerator CompleteLevel()
    {
        //print($"Level {levelNumber} complete!");
        HUD.Instance.ShowMessage($"Level {levelNumber} complete!", 0.25f, 2f, 1f);
        ClearProjectiles();

        yield return new WaitForSecondsRealtime(1);


        var newPoints = LevelController.Instance.CalculateLevelScore(shotPar, shotsFiredThisLevel);
        PlayerData.Instance.Data.PlayerScore += newPoints;
        StartCoroutine(HUD.Instance.TextPop(HUD.Instance.scoreLabel));
        PlayerData.Instance.SaveAllAsync();

        isCompleted = true;
        isRunning = false;

        yield return new WaitForSecondsRealtime(1);

        LevelController.Instance.NextLevel();
    }


    public void Clear()
    {
        if (Spawners.Length > 0)
        {
            foreach (var spawner in Spawners)
            {
                if (spawner.childEnemy != null)
                {
                    Destroy(spawner.childEnemy.gameObject);
                }
            }
        }

        ClearProjectiles();
    }

    public void ClearProjectiles()
    {
        var projectiles = FindObjectsOfType<Projectile>();
        if (projectiles.Length > 0)
        {
            foreach (var projectile in projectiles)
            {
                Destroy(projectile.gameObject);
            }
        }
    }

    public void SpawnEnemies(bool aggroOnSpawn = true)
    {
        foreach (var spawner in Spawners)
        {
            if (spawner.childEnemy != null)
            {
                Destroy(spawner.childEnemy.gameObject);
            }

            spawner.Spawn(Vector2.zero);
            spawner.childEnemy.isAggroed = aggroOnSpawn;
        }
    }

    // TODO handle this differently?
    public IEnumerator CheckForLevelComplete()
    {
        yield return new WaitForSecondsRealtime(1);

        if (PlayerManager.Instance.State == PlayerState.Dead)
        {
            print("Level: Checked for LevelComplete but Player was dead.");
            yield break;
        }

        print("Checking for level complete...");
        var enemies = FindObjectsOfType<EnemyCharacter>();
        if (enemies.Length > 0)
        {
            print($"Level not complete, found {enemies.Length} enemies remaining:");
            foreach (var enemy in enemies)
            {
                print($"- {enemy.gameObject.name} | {enemy.Data.enemyName}");
            }
            yield break;
        }

        StartCoroutine(CompleteLevel());
    }

    public void Unload()
    {
        PlayerManager.Instance.DespawnPlayer();
        Clear();
        gameObject.SetActive(false);
    }
}