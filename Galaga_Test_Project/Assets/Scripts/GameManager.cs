using System.Collections;
using System.Collections.Generic;
using Gulaga.BaseClass;
using Gulaga.Enemy;
using Gulaga.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Player Controller")]
    [SerializeField] private Text scoreText = null;
    [SerializeField] private Text highScoreText = null;
    [SerializeField] private GameObject liveUiPref = null;
    [SerializeField] private Transform liveUiParent = null;
    [SerializeField] private int maxPlayerLive = 0;
    [SerializeField] private GameObject playerPrefab = null;
    [SerializeField] private Vector2 playerMainSpawnPos = Vector2.zero;
    [SerializeField] private float playerSpawnTime = 0;
    [SerializeField] private GameObject RestartTextObj = null;
    [SerializeField] private GameObject NextLevelTextObj = null;

    [Header("Enemy Controller")]
    [SerializeField] private Vector2 engageIntervalMinMax = Vector2.zero;
    [SerializeField] private Vector2 engageAmountMinMax = Vector2.zero;
    [SerializeField] private GameObject[] enemyPrefs = null;
    /// <summary>
    /// Define enemy position in the game.
    /// </summary>
    [SerializeField] private List<EnemyRow> enemyRows = null;

    public static GameManager Instance;
    public const float effectDestroyTime = 2f;
    public GameObject CurrentPlayer { get; private set; }
    public Vector3 PlayerPos
    {
        get
        {
            if (CurrentPlayer != null)
            {
                return CurrentPlayer.transform.position;
            }
            else
            {
                return playerMainSpawnPos;
            }
        }
        set
        {
            CurrentPlayer.transform.position = value;
        }
    }
    public int PlayerCurrentLive { get; private set; }

    private const string enemyLevelDataPath = "EnemyLevelData";
    /// <summary>
    /// Delay before go next level.
    /// </summary>
    private const float delayForNextLevel = 5f;
    private const float enemySpawnPosXRange = 30f;
    private const float enemySpawnPosYMin = -5f;
    private const float enemySpawnPosYMax = 30f;
    /// <summary>
    /// Prevent enemy spawn in screen on x axis relative with world space.
    /// </summary>
    private const float screenBorderX = 12f;
    /// <summary>
    /// Visualize player's live image on ui. Value by player's sprite size.
    /// </summary>
    private const float playerImageSize = 30;
    private const float xPlayerImageOffset = 18;
    private const float yPlayerImageOffset = 18;
    private List<GameObject> liveUiObj;
    private List<BaseEnemyController> enemyList;
    /// <summary>
    /// These levels will get from resources folder.
    /// </summary>
    private EnemyLevelData[] enemylevels = null;
    private int currentScore = 0;
    private int currentLevel = 0;
    private float nextEngageTime = 2f;
    private float engageTimer = 0;
    private bool isLevelStarted = false;
    private bool isGameover = false;

    private void Awake()
    {
        Instance = this;
        liveUiObj = new List<GameObject>();
        enemyList = new List<BaseEnemyController>();

        if (RestartTextObj != null)
        {
            RestartTextObj.SetActive(isGameover);
        }
        if (NextLevelTextObj != null)
        {
            NextLevelTextObj.SetActive(false);
        }

        enemylevels = Resources.LoadAll<EnemyLevelData>(enemyLevelDataPath);
    }

    private void Start()
    {
        PlayerCurrentLive = maxPlayerLive;
        SpawnPlayer(playerMainSpawnPos);

        UpdateScore();
        UpdateHighScore();
        InitLiveUi();
        InitLevel();
    }

    private void Update()
    {
        if (isGameover)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene(0);
                isGameover = false;
            }

            return;
        }

        if (engageTimer > nextEngageTime && CurrentPlayer != null)
        {
            RandEnemyToEngage();
            RandNewEngageTime();
        }
        else
        {
            engageTimer += Time.deltaTime;
        }

        if (isLevelStarted)
        {
            if (enemyList.Count <= 0)
            {
                StartCoroutine(OnALevelWasEnd(delayForNextLevel));
                isLevelStarted = false;
            }
        }
    }

    public void UpdateScore(int value = 0)
    {
        currentScore += value;
        scoreText.text = currentScore.ToString();
        UpdateHighScore();
    }

    public void CurrentPlayerDie(Transform dieTrans)
    {
        if (CurrentPlayer != null)
        {
            CurrentPlayer = null;
        }
        
        StartCoroutine(RequestNewPlayer(dieTrans.position, playerSpawnTime));
    }

    private void UpdateHighScore()
    {
        var highScore = PlayerPrefs.GetInt("HighScore", 0);

        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
        }

        if (highScoreText != null)
        {
            highScoreText.text = highScore.ToString();
        }
    }

    IEnumerator RequestNewPlayer(Vector2 diePos, float delay)
    {
        var timer = 0f;

        while (timer < delay)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        while (BaseEnemyController.CurrentInvadeTyrant != null) //Prevent from strong attack.
        {
            yield return null;
        }

        if (CurrentPlayer == null)
        {
            if (IsPlayerAlive())
            {
                engageTimer = 1.5f; //Give player some breath.
                SpawnPlayer(diePos);
                PlayerCurrentLive--;
            }
            else
            {
                if (RestartTextObj != null)
                {
                    RestartTextObj.SetActive(true);
                }
                isGameover = true;
                HideAllenemy();
            }
        }

        yield return null;
    }

    private void SpawnPlayer(Vector2 spawnPos)
    {
        CurrentPlayer = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

        if (CurrentPlayer != null)
        {
            var player = CurrentPlayer.GetComponent<PlayerController>();
            player.OnPlayerDie += CurrentPlayerDie;
        }
    }

    private void InitLiveUi()
    {
        if (liveUiObj == null)
        {
            return;
        }

        if (liveUiObj.Count > 0)
        {
            foreach (var ui in liveUiObj)
            {
                Destroy(ui);
            }

            liveUiObj.Clear();
        }

        for (int i = 0; i < PlayerCurrentLive; i++)
        {
            var newUi = Instantiate(liveUiPref, liveUiParent);
            liveUiObj.Add(newUi);
        }
    }

    private bool IsPlayerAlive()
    {
        var liveIndex = liveUiObj.Count - 1;

        if (liveIndex + 1 <= 0)
        {
            return false;
        }

        if (liveUiObj[liveIndex] != null)
        {
            Destroy(liveUiObj[liveIndex]);
            liveUiObj.RemoveAt(liveIndex);
        }

        return true;
    }

    private IEnumerator OnALevelWasEnd(float delay)
    {
        if (NextLevelTextObj != null)
        {
            NextLevelTextObj.SetActive(true);
        }

        var timer = 0f;

        while (timer < delay)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        PlayerCurrentLive++;
        InitLiveUi();
        InitLevel();
    }

    private void InitLevel()
    {
        if (!isLevelStarted)
        {
            InitEnemyFormation(currentLevel);
            isLevelStarted = true;
        }

        currentLevel++;

        if (enemylevels != null)
        {
            if (currentLevel > enemylevels.Length - 1)
            {
                currentLevel = 0;
            }
        }

        if (NextLevelTextObj != null)
        {
            NextLevelTextObj.SetActive(false);
        }
    }

    #region Enemy Control

    private void InitEnemyFormation(int levelIndex)
    {
        if (enemylevels[levelIndex] == null)
        {
            return;
        }

        var targetLevel = enemylevels[levelIndex];

        for (int i = 0; i < targetLevel.rows.Length; i++) //Spawn row by row.
        {
            InitNewEnemyRow(targetLevel.rows[i], i);
        }
    }

    private void InitNewEnemyRow(EnemyRowData rowData, int rawIndex)
    {
        foreach (var spawnTrans in enemyRows[rawIndex].EnemyPos)
        {
            if (spawnTrans == null)
            {
                return;
            }

            var randPosX = Random.Range(-enemySpawnPosXRange, enemySpawnPosXRange);
            var randPosY = Random.Range(enemySpawnPosYMin, enemySpawnPosYMax);

            if (randPosX <= screenBorderX && randPosX > 0)
            {
                randPosX = screenBorderX;
            }
            else if (randPosX >= -screenBorderX && randPosX <= 0)
            {
                randPosX = -screenBorderX;
            }

            var trueSpawnPos = new Vector2(randPosX, randPosY);
            var prefIndex = (int) rowData.enemyType;
            var newObj = Instantiate(enemyPrefs[prefIndex], trueSpawnPos, Quaternion.identity);

            if (newObj != null)
            {
                var newEnemy = newObj.GetComponent<BaseEnemyController>();

                if (newEnemy != null)
                {
                    newEnemy.SetWaitingPos(spawnTrans);
                    enemyList.Add(newEnemy);
                }
            }
        }
    }

    private void RandEnemyToEngage()
    {
        var amount = Random.Range(engageAmountMinMax.x, engageAmountMinMax.y);

        if (enemyList.Count <= 0)
        {
            return;
        }

        for (int i = 0; i < amount; i++)
        {
            var index = Random.Range(0, enemyList.Count);

            if (index > enemyList.Count - 1 || index < 0)
            {
                continue;
            }

            if (enemyList[index] != null)
            {
                if (enemyList[index].IsReadyToInvade)
                {
                    var engageEnemy = enemyList[index];
                    engageEnemy.SetAiState(BaseEnemyController.AiState.ReadyToInvade);
                }
            }
            else
            {
                if (enemyList.Count > 0) //If null just find another enemy.
                {
                    amount++;
                }
            }
        }
    }

    private void RandNewEngageTime()
    {
        nextEngageTime = Random.Range(engageIntervalMinMax.x, engageIntervalMinMax.y);
        engageTimer = 0;
    }

    private void HideAllenemy()
    {
        foreach (var enemy in enemyList)
        {
            enemy.gameObject.SetActive(false);
        }
    }

    public void ClearEnemy(GameObject enemyObj)
    {
        var enemy = enemyObj.GetComponent<BaseEnemyController>();
        enemyList.Remove(enemy);
    }

    #endregion
}