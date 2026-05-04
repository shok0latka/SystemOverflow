using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    
    public Vector3 RespawnPosition => respawnPoint != null ? respawnPoint.position : transform.position;

    [Header("Visuals")]
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Sprite inactiveSprite;
    private SpriteRenderer spriteRenderer;
    
    private static Checkpoint _lastActivatedCheckpoint;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning($"[Checkpoint] SpriteRenderer не найден на {gameObject.name}");
        }
    }

    private void Start()
    {
        if (spriteRenderer != null && activeSprite != null)
        {
            spriteRenderer.sprite = activeSprite;
        }
        
        Debug.Log($"[Checkpoint] Инициализирован на сцене '{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}'. Позиция респавна: {RespawnPosition}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) 
        {
            Debug.Log($"[Checkpoint] Объект {other.name} вошёл в триггер, но это не Player (тег: {other.tag})");
            return;
        }
        
        Debug.Log($"[Checkpoint] Игрок вошёл в чекпоинт: {gameObject.name}");
        
        if (_lastActivatedCheckpoint != null && _lastActivatedCheckpoint != this)
        {
            Debug.Log($"[Checkpoint] Деактивируем предыдущий чекпоинт: {_lastActivatedCheckpoint.name}");
            _lastActivatedCheckpoint.DeactivateVisual();
        }
        
        ActivateCheckpoint();
    }

    private void ActivateCheckpoint()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        Debug.Log("═══════════════════════════════════");
        Debug.Log($"[Checkpoint] 🔄 АКТИВАЦИЯ ЧЕКПОИНТА");
        Debug.Log($"  • Объект: {gameObject.name}");
        Debug.Log($"  • Сцена: {currentScene}");
        Debug.Log($"  • Позиция респавна: {RespawnPosition}");
        
        SaveSystem saveSystem = SaveSystem.Instance;
        
        if (saveSystem != null)
        {
            saveSystem.SaveCheckpoint(currentScene, RespawnPosition);
            Debug.Log($"  • SaveSystem: ✅ Сохранено успешно");
        }
        else
        {
            Debug.LogError($"[Checkpoint] ❌ SaveSystem.Instance не найден! Сохранение не выполнено!");
        }
        
        GameStateManager gameState = GameStateManager.Instance;
        if (gameState != null)
        {
            gameState.UpdateCheckpoint(this);
            Debug.Log($"  • GameStateManager: ✅ Обновлён для быстрого респавна");
        }
        else
        {
            Debug.LogWarning($"[Checkpoint] ⚠️ GameStateManager не найден. Быстрый респавн не будет работать.");
        }
        
        _lastActivatedCheckpoint = this;
        
        if (spriteRenderer != null && inactiveSprite != null)
        {
            spriteRenderer.sprite = inactiveSprite;
            Debug.Log($"  • Визуал: спрайт изменён на неактивный");
        }
        
        Debug.Log($"[Checkpoint] ✅ Чекпоинт успешно активирован и сохранён!");
        Debug.Log("═══════════════════════════════════");
    }

    private void DeactivateVisual()
    {
        if (spriteRenderer != null && inactiveSprite != null)
        {
            spriteRenderer.sprite = inactiveSprite;
            Debug.Log($"[Checkpoint] Визуально деактивирован: {gameObject.name}");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(RespawnPosition, 0.3f);
        Gizmos.DrawLine(transform.position, RespawnPosition);
    }
}