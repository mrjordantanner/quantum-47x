using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using static Cinemachine.DocumentationSortingAttribute;

/// <summary>
/// Controls the level progression.
/// </summary>
public class LevelController : MonoBehaviour
{
    #region Singleton
    public static LevelController Instance;
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

    // Manage all Levels, which are individual rooms/scenes
    // Load and init new levels, unload previous ones
    // Level is notified of each enemyDeath. When all enemies defeated, level complete
    // When Level complete, LevelController is notified.

    // For now, references to Levels will just be the entire GameObject
    // Levels will be setActive(true) and then initialized, and previous level turned off
    // We won't use separate Scenes since WebGL doesn't support additive scenes

    public Level[] Levels;
    public Level CurrentLevel;

    [Header("Scoring")]
    [ReadOnly] public int currentTotalScore;
    [ReadOnly] public int allTimeBestScore;
    public int basePointsForPar = 5000;
    public int consolationPoints = 500;

    public int CalculateLevelScore(int par, int shotsTaken)
    {
        if (shotsTaken > par) return consolationPoints;
        return (int)(basePointsForPar * Mathf.Pow(2, par - shotsTaken));
    }


    private void Start()
    {
        Init();
    }

    public void Init()
    {
        //Levels = Resources.LoadAll<Level>("Levels");
        HUD.Instance.levelNumberLabel.text = "";
        CurrentLevel = GetLevel(1);

    }

    public bool LoadLevel(int levelNumber)
    {
        var newLevel = GetLevel(levelNumber);
        if (newLevel != null)
        {
            print($"Level {levelNumber} Loaded");

            newLevel.gameObject.SetActive(true);
            newLevel.Init();
            CurrentLevel = newLevel;
            CameraController.Instance.SetCameraFollow(CurrentLevel.cameraAnchor.transform);
            return true;
        }
        return false;
    }

    Level GetLevel(int levelNumber)
    {
        return Levels.FirstOrDefault(l => l.levelNumber == levelNumber);
    }

    bool AreAllLevelsCompleted()
    {
        var maxLevel = Levels?.OrderByDescending(level => level.levelNumber).FirstOrDefault();
        return CurrentLevel.levelNumber == maxLevel.levelNumber;
    }

    public void NextLevel()
    {
        if (AreAllLevelsCompleted())
        {
            StartCoroutine(Victory());
           
        }

        var nextLevelNumber = CurrentLevel.levelNumber + 1;
        CurrentLevel.Unload();
        var success = LoadLevel(nextLevelNumber);
        if (success)
        {
            print($"Level {nextLevelNumber} Loaded");
            CameraController.Instance.SetCameraFollow(CurrentLevel.cameraAnchor.transform);
            CurrentLevel.StartLevel();
        }
        else
        {
            Debug.LogError($"LevelController: Unable to load Level {nextLevelNumber}");
        }

    }

    IEnumerator Victory()
    {
        // TODO Calculate final score and send to cloud, etc

        AudioManager.Instance.soundBank.LevelComplete.Play();
        HUD.Instance.ShowMessage($"All Enemies Destroyed. Killer!", 0.25f, 2f, 1f);
        yield return new WaitForSecondsRealtime(2);

        GameManager.Instance.GameOver();


    }

    public void OnEnemyDeath(EnemyCharacter enemy)
    {
        StartCoroutine(CurrentLevel.CheckForLevelComplete());
    }

}
