using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;

    [Header("Settings")]
    public bool autoSaveOnCheckpoint = true;
    
    private string _savePath;
    private SaveData _currentSave;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _savePath = Path.Combine(Application.persistentDataPath, "checkpoint_save.json");
        LoadSaveFromDisk();
    }

    public void SaveCheckpoint(string sceneName, Vector3 position)
    {
        Debug.Log("─────────────────────────────────");
        Debug.Log($"[SaveSystem] 💾 СОХРАНЕНИЕ ЧЕКПОИНТА");
        Debug.Log($"  • Сцена: {sceneName}");
        Debug.Log($"  • Позиция: X={position.x:F2}, Y={position.y:F2}, Z={position.z:F2}");
        
        _currentSave = new SaveData
        {
            sceneName = sceneName,
            playerPositionX = position.x,
            playerPositionY = position.y,
            playerPositionZ = position.z,
            hasCheckpoint = true
        };

        // Сохраняем здоровье игрока если есть
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                _currentSave.playerHealth = health.CurrentHp;
                Debug.Log($"  • Здоровье игрока: {health.CurrentHp}/{health.MaxHp}");
            }
            else
            {
                Debug.LogWarning($"  • PlayerHealth не найден на игроке");
            }
        }
        else
        {
            Debug.LogWarning($"  • Игрок с тегом 'Player' не найден на сцене");
        }

        SaveToDisk();
        
        Debug.Log($"[SaveSystem] ✅ Чекпоинт сохранён в файл: {_savePath}");
        Debug.Log("─────────────────────────────────");
    }

    public bool HasCheckpoint()
    {
        return _currentSave != null && _currentSave.hasCheckpoint;
    }

    public void LoadCheckpoint()
    {
        if (!HasCheckpoint())
        {
            Debug.LogError("Нет сохранённого чекпоинта!");
            return;
        }

        string sceneToLoad = _currentSave.sceneName;
        _pendingRespawnPosition = new Vector3(
            _currentSave.playerPositionX,
            _currentSave.playerPositionY,
            _currentSave.playerPositionZ
        );
        _pendingHealth = _currentSave.playerHealth;

        SceneManager.sceneLoaded += OnSceneLoadedForRespawn;
        SceneManager.LoadScene(sceneToLoad);
    }

    private Vector3 _pendingRespawnPosition;
    private int _pendingHealth;

    private void OnSceneLoadedForRespawn(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoadedForRespawn;
        
        StartCoroutine(RespawnPlayerAfterLoad());
    }

    private System.Collections.IEnumerator RespawnPlayerAfterLoad()
    {
        yield return null;
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = _pendingRespawnPosition;
            
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.position = _pendingRespawnPosition;
                rb.velocity = Vector2.zero;
            }
            
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null && _pendingHealth > 0)
            {
                health.SetCurrentHp(_pendingHealth);
            }
            
            Debug.Log("Игрок перемещён на чекпоинт");
        }
    }

    public void SaveCurrentState()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            SaveCheckpoint(currentScene, player.transform.position);
        }
    }

    private void SaveToDisk()
    {
        try
        {
            string json = JsonUtility.ToJson(_currentSave, true);
            File.WriteAllText(_savePath, json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Ошибка сохранения: {e.Message}");
        }
    }

    private void LoadSaveFromDisk()
    {
        if (File.Exists(_savePath))
        {
            try
            {
                string json = File.ReadAllText(_savePath);
                _currentSave = JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Ошибка загрузки: {e.Message}");
                _currentSave = null;
            }
        }
    }

    public void DeleteSave()
    {
        _currentSave = null;
        if (File.Exists(_savePath))
        {
            File.Delete(_savePath);
        }
    }
}