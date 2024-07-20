using UnityEngine;
using TMPro;
using System.Collections;

public class TextManager : MonoBehaviour
{
    public static TextManager instance;

    public TextMeshProUGUI correctText;
    public TextMeshProUGUI incorrectText;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowCorrectText()
    {
        if (correctText != null)
        {
            correctText.gameObject.SetActive(true);
            StartCoroutine(DeactivateAfterDelay(correctText.gameObject));
        }
    }

    public void ShowIncorrectText()
    {
        if (incorrectText != null)
        {
            incorrectText.gameObject.SetActive(true);
            StartCoroutine(DeactivateAfterDelay(incorrectText.gameObject));
        }
    }

    private IEnumerator DeactivateAfterDelay(GameObject textObject)
    {
        yield return new WaitForSeconds(1f);
        textObject.SetActive(false);
    }
}
