using UnityEngine;
using TMPro;
using System.Collections;

public class UICollectibleHint : MonoBehaviour
{
    public static UICollectibleHint Instance { get; private set; }

    [Header("Referencias UI")]
    public TextMeshProUGUI messageText; // si usas TextMeshPro cambia a TMP y type
    public TextMeshProUGUI counterText;
    public float messageDuration = 2f;

    Coroutine current;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (GameManagerCollectibles.Instance != null)
        {
            GameManagerCollectibles.Instance.OnCollectChanged += UpdateCounter;
            GameManagerCollectibles.Instance.OnAllCollected += OnAllCollected;
            UpdateCounter(GameManagerCollectibles.Instance.GetCollected(), GameManagerCollectibles.Instance.GetTotalToCollect());
        }
        if (messageText != null) messageText.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (GameManagerCollectibles.Instance != null)
        {
            GameManagerCollectibles.Instance.OnCollectChanged -= UpdateCounter;
            GameManagerCollectibles.Instance.OnAllCollected -= OnAllCollected;
        }
    }

    public void ShowTemporaryMessage(string text)
    {
        if (messageText == null) return;
        if (current != null) StopCoroutine(current);
        current = StartCoroutine(ShowForSeconds(text, messageDuration));
    }

    IEnumerator ShowForSeconds(string text, float seconds)
    {
        messageText.gameObject.SetActive(true);
        messageText.text = text;
        yield return new WaitForSeconds(seconds);
        messageText.gameObject.SetActive(false);
        current = null;
    }

    void UpdateCounter(int col, int total)
    {
        if (counterText != null)
            counterText.text = $"{col}/{total}";
    }

    void OnAllCollected()
    {
        ShowTemporaryMessage("¡Puerta desbloqueada!");
    }
}
