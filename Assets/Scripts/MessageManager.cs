using UnityEngine;
using TMPro;
using System.Collections;

public class MessageManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI messageText;

    [Header("Settings")]
    [SerializeField] private float autoClearTime = 3f;

    private Coroutine clearCoroutine;

    // --------------------------------------------------
    // Show Message
    // --------------------------------------------------
    public void ShowMessage(string message)
    {
        if (messageText == null)
            return;

        messageText.text = message;

        if (autoClearTime > 0f)
        {
            if (clearCoroutine != null)
                StopCoroutine(clearCoroutine);

            clearCoroutine = StartCoroutine(ClearMessageAfterDelay());
        }
    }

    // --------------------------------------------------
    // Clear Message
    // --------------------------------------------------
    public void ClearMessage()
    {
        if (messageText != null)
            messageText.text = string.Empty;

        if (clearCoroutine != null)
        {
            StopCoroutine(clearCoroutine);
            clearCoroutine = null;
        }
    }

    private IEnumerator ClearMessageAfterDelay()
    {
        yield return new WaitForSeconds(autoClearTime);

        ClearMessage();
    }
}