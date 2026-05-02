using UnityEngine;
using TMPro;
using System.Collections;
using DG.Tweening;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TextMeshProUGUI dialogText;

    [Header("Typing Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float commaPause = 0.2f;
    [SerializeField] private float dotPause = 0.5f;

    private Coroutine _typingCoroutine;
    private bool _isTyping;
    private bool _canClose;
    private string _currentFullText;
    private float _closeDelay = 0.3f; 

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
        }
    }

    private void Start()
    {
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
            dialogPanel.transform.localScale = Vector3.zero;
        }
        
        _canClose = false;
        _isTyping = false;
    }

    public void ShowDialog(string text)
    {
        if (dialogPanel == null || dialogText == null)
        {
            Debug.LogError("DialogPanel или DialogText не назначены!");
            return;
        }

        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }

        _canClose = false;
        _isTyping = false;

        dialogText.text = "";

        dialogPanel.SetActive(true);
        dialogPanel.transform.localScale = Vector3.zero;
        
        dialogPanel.transform.DOScale(Vector3.one, 0.5f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                _typingCoroutine = StartCoroutine(TypeText(text));
            });
    }

    private IEnumerator TypeText(string text)
    {
        _isTyping = true;
        _currentFullText = text;

        foreach (char c in text)
        {
            dialogText.text += c;

            if (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                dialogText.text = text;
                break;
            }

            if (c == ',')
            {
                yield return new WaitForSeconds(commaPause);
            }
            else if (c == '.' || c == '!' || c == '?')
            {
                yield return new WaitForSeconds(dotPause);
            }
            else
            {
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        _isTyping = false;
        
        yield return new WaitForSeconds(_closeDelay);
        _canClose = true;
    }

    public void HideDialog()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }

        _isTyping = false;
        _canClose = false;

        dialogPanel.transform.DOScale(Vector3.zero, 0.3f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                dialogPanel.SetActive(false);
            });
    }

    private void Update()
    {
        if (_isTyping && Input.GetKeyDown(KeyCode.Space))
        {
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
            }
            dialogText.text = _currentFullText;
            _isTyping = false;
            StartCoroutine(EnableCloseAfterDelay());
        }

        if (_canClose && !_isTyping && dialogPanel != null && dialogPanel.activeSelf && Input.GetKeyDown(KeyCode.Space))
        {
            HideDialog();
        }
    }

    private IEnumerator EnableCloseAfterDelay()
    {
        yield return new WaitForSeconds(_closeDelay);
        _canClose = true;
    }
}