using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [Header("Scene Transition")]
    [SerializeField] private GameObject _fade;
    
    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    
    [Header("Animations")]
    [SerializeField] private ButtonAnimationY continueAnimation;
    [SerializeField] private ButtonAnimationY startAnimation;
    
    private CanvasGroup _continueCanvasGroup;
    
    private void Awake()
    {
        if (continueButton != null)
        {
            _continueCanvasGroup = continueButton.GetComponent<CanvasGroup>();
            if (_continueCanvasGroup == null)
                _continueCanvasGroup = continueButton.gameObject.AddComponent<CanvasGroup>();
        }
    }
    
    private void Start()
    {
        StartCoroutine(InitializeAfterFrame());
    }
    
    private IEnumerator InitializeAfterFrame()
    {
        yield return null;
        
        if (SaveSystem.Instance == null)
            Debug.LogError("[MenuManager] SaveSystem.Instance не найден! Убедись что SaveSystem есть на сцене.");
        
        if (continueButton == null)
            Debug.LogError("[MenuManager] Continue Button не назначен в инспекторе!");
        
        if (newGameButton == null)
            Debug.LogError("[MenuManager] New Game Button не назначен в инспекторе!");
        
        if (_fade == null)
            Debug.LogError("[MenuManager] Fade не назначен в инспекторе!");
        
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(ContinueGame);
        }
        
        if (newGameButton != null)
        {
            newGameButton.onClick.RemoveAllListeners();
            newGameButton.onClick.AddListener(NewGame);
        }
        
        UpdateContinueButton();
        
        if (startAnimation != null)
            startAnimation.enabled = true;
        
        DebugLogState();
    }
    
    private void OnEnable()
    {
        if (continueButton != null)
            UpdateContinueButton();
            
        if (startAnimation != null)
            startAnimation.enabled = true;
    }
    
    private void UpdateContinueButton()
    {
        if (continueButton == null) return;
        
        bool hasSave = SaveSystem.Instance != null && SaveSystem.Instance.HasCheckpoint();
        
        continueButton.interactable = hasSave;
        
        if (_continueCanvasGroup != null)
        {
            _continueCanvasGroup.alpha = hasSave ? 1f : 0.3f;
            _continueCanvasGroup.interactable = hasSave;
            _continueCanvasGroup.blocksRaycasts = hasSave;
        }
        
        if (continueAnimation != null)
            continueAnimation.enabled = hasSave;
        
        Debug.Log($"[MenuManager] Continue обновлена: hasSave={hasSave}, interactable={hasSave}");
    }
    
    public void ContinueGame()
    {
        Debug.Log("[MenuManager] ContinueGame вызван");
        
        if (SaveSystem.Instance == null)
        {
            Debug.LogError("[MenuManager] Невозможно продолжить: SaveSystem отсутствует!");
            return;
        }
        
        if (!SaveSystem.Instance.HasCheckpoint())
        {
            Debug.LogWarning("[MenuManager] Нет сохранения для продолжения!");
            return;
        }
        
        if (continueAnimation != null)
            continueAnimation.enabled = false;
        if (startAnimation != null)
            startAnimation.enabled = false;
        
        SaveSystem.Instance.LoadCheckpoint();
    }
    
    public void NewGame()
    {
        Debug.Log("[MenuManager] NewGame вызван");
        
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.DeleteSave();
            Debug.Log("[MenuManager] Сохранение удалено");
        }
        
        if (continueAnimation != null)
            continueAnimation.enabled = false;
        if (startAnimation != null)
            startAnimation.enabled = false;
        
        if (_fade != null)
        {
            SceneTransition transition = _fade.GetComponent<SceneTransition>();
            if (transition != null)
            {
                transition.ChangeScene("FirstLevel");
                Debug.Log("[MenuManager] Загружаем FirstLevel");
            }
            else
            {
                Debug.LogError("[MenuManager] SceneTransition не найден на _fade!");
            }
        }
        else
        {
            Debug.LogError("[MenuManager] _fade не назначен!");
            UnityEngine.SceneManagement.SceneManager.LoadScene("FirstLevel");
        }
    }
    
    private void DebugLogState()
    {
        Debug.Log("=== MenuManager State ===");
        Debug.Log($"SaveSystem exists: {SaveSystem.Instance != null}");
        Debug.Log($"Has checkpoint: {SaveSystem.Instance?.HasCheckpoint()}");
        Debug.Log($"Continue button assigned: {continueButton != null}");
        Debug.Log($"NewGame button assigned: {newGameButton != null}");
        Debug.Log($"Continue animation assigned: {continueAnimation != null}");
        Debug.Log($"Start animation assigned: {startAnimation != null}");
        Debug.Log($"Fade assigned: {_fade != null}");
    }
}