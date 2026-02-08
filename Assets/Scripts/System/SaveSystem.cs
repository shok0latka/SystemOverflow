using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class PlayerSaveData
{
    public float posX;
    public float posY;
    public int currentHp;
}

[Serializable]
public class SaveData
{
    public string sceneName;
    public PlayerSaveData player = new PlayerSaveData();
    public List<EnemyRuntimeSaveData> enemies = new List<EnemyRuntimeSaveData>();
}

public class SaveSystem : MonoBehaviour
{
    [Header("Behavior")]
    public bool autoLoadOnStart = true;
    public bool autoSave = true;
    public float autoSaveInterval = 5f;

    [Header("Refs")]
    public Transform player;

    private string savePath;
    private float saveTimer;

    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "save.json");
    }

    private void Start()
    {
        saveTimer = Mathf.Max(0.2f, autoSaveInterval);

        if (autoLoadOnStart)
        {
            LoadCurrentScene();
        }
    }

    private void Update()
    {
        if (!autoSave)
        {
            return;
        }

        saveTimer -= Time.unscaledDeltaTime;
        if (saveTimer > 0f)
        {
            return;
        }

        SaveCurrentScene();
        saveTimer = Mathf.Max(0.2f, autoSaveInterval);
    }

    public void SaveCurrentScene()
    {
        SaveData data = new SaveData();
        data.sceneName = SceneManager.GetActiveScene().name;

        Transform playerTransform = ResolvePlayer();
        if (playerTransform != null)
        {
            data.player.posX = playerTransform.position.x;
            data.player.posY = playerTransform.position.y;

            PlayerHealth health = playerTransform.GetComponent<PlayerHealth>();
            data.player.currentHp = health != null ? health.CurrentHp : 0;
        }

        EnemyAI2D[] enemies = FindObjectsByType<EnemyAI2D>(FindObjectsSortMode.None);
        foreach (EnemyAI2D enemy in enemies)
        {
            data.enemies.Add(enemy.CaptureRuntimeState());
        }

        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Save failed: " + exception.Message);
        }
    }

    public void LoadCurrentScene()
    {
        if (!File.Exists(savePath))
        {
            return;
        }

        SaveData data;
        try
        {
            data = JsonUtility.FromJson<SaveData>(File.ReadAllText(savePath));
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Load failed: " + exception.Message);
            return;
        }

        if (data == null || string.IsNullOrEmpty(data.sceneName))
        {
            return;
        }

        if (data.sceneName != SceneManager.GetActiveScene().name)
        {
            return;
        }

        Transform playerTransform = ResolvePlayer();
        if (playerTransform != null && data.player != null)
        {
            Vector2 loadedPosition = new Vector2(data.player.posX, data.player.posY);
            playerTransform.position = loadedPosition;

            Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.position = loadedPosition;
                playerRb.velocity = Vector2.zero;
            }

            PlayerHealth health = playerTransform.GetComponent<PlayerHealth>();
            if (health != null)
            {
                int hpToRestore = data.player.currentHp <= 0 ? health.MaxHp : data.player.currentHp;
                health.SetCurrentHp(hpToRestore);
            }
        }

        EnemyAI2D[] sceneEnemies = FindObjectsByType<EnemyAI2D>(FindObjectsSortMode.None);
        Dictionary<string, EnemyAI2D> enemyBySaveId = new Dictionary<string, EnemyAI2D>();
        foreach (EnemyAI2D enemy in sceneEnemies)
        {
            enemyBySaveId[enemy.SaveId] = enemy;
        }

        if (data.enemies == null)
        {
            return;
        }

        foreach (EnemyRuntimeSaveData enemyData in data.enemies)
        {
            if (enemyData == null || string.IsNullOrEmpty(enemyData.saveId))
            {
                continue;
            }

            if (enemyBySaveId.TryGetValue(enemyData.saveId, out EnemyAI2D sceneEnemy))
            {
                sceneEnemy.RestoreRuntimeState(enemyData);
            }
        }
    }

    public void SavePlayer(GameObject playerObject)
    {
        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        SaveCurrentScene();
    }

    public void LoadPlayer(GameObject playerObject)
    {
        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        LoadCurrentScene();
    }

    private Transform ResolvePlayer()
    {
        if (player != null)
        {
            return player;
        }

        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            player = playerMovement.transform;
        }

        return player;
    }
}
