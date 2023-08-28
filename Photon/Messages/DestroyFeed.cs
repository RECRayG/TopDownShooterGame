using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DestroyFeed : MonoBehaviour
{
    private Text textObject;
    [SerializeField]
    private float fadeDuration = 3.9f;
    [SerializeField]
    private float destroyDelay = 0.1f;

    //private bool newMessageIsCome;

    void Start()
    {
        //newMessageIsCome = false;
        textObject = GetComponent<Text>();
        StartCoroutine(FadeOutAndDestroy());
    }
    private IEnumerator FadeOutAndDestroy()
    {
        Color initialColor = textObject.color;
        Color targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            // Если появляетсяя несколько сообщений в одно время, то уничтожаем текущее сообщение,
            // чтобы уведомления не мешали играть
            /*if (gameObject.transform.parent.childCount > 1)
            {
                newMessageIsCome = true;
                break;
            }*/

            float normalizedTime = elapsedTime / fadeDuration;
            textObject.color = Color.Lerp(initialColor, targetColor, normalizedTime);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        /*if (newMessageIsCome)
        {
            Destroy(gameObject);
        }
        else
        {*/
            textObject.color = targetColor;

            yield return new WaitForSeconds(destroyDelay);

            Destroy(gameObject);
        //}
    }
}
