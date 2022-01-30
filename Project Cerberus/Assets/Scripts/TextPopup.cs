/*
 * TextPopup is used to display simple text via a TMP component. It can be conveniently instantiated anytime through
 * Create().
 */
using System.Collections;
using TMPro;
using UnityEngine;

public class TextPopup : MonoBehaviour
{
    public AnimationCurve riseCurve;
    public AnimationCurve fadeCurve;
    public float duration;
    private TextMeshPro _textMeshPro;

    private void Awake()
    {
        _textMeshPro = gameObject.GetComponent<TextMeshPro>();
    }

    public static TextPopup Create(string text, Color color)
    {
        var gameObject = Instantiate(CustomProjectSettings.i.textPopupPrefab);
        var textPopup = gameObject.GetComponent<TextPopup>();
        textPopup._textMeshPro.text = text;
        textPopup._textMeshPro.color = color;
        return textPopup;
    }

    public void PlayRiseAndFadeAnimation(float delay = 0f)
    {
        StartCoroutine(RiseAndFade(delay));
    }

    private IEnumerator RiseAndFade(float delay)
    {
        var cachedColor = _textMeshPro.color;
        var destinationColor = new Color(_textMeshPro.color.r, _textMeshPro.color.g, _textMeshPro.color.b, 0);
        var cachedPosition = transform.position;
        var timeEllapsed = 0f;
        yield return new WaitForSeconds(delay);
        while (timeEllapsed < duration)
        {
            timeEllapsed += Time.fixedDeltaTime;
            var t = timeEllapsed / duration;
            transform.position = cachedPosition + Vector3.up * riseCurve.Evaluate(t);
            _textMeshPro.color = Color.Lerp(cachedColor, destinationColor, fadeCurve.Evaluate(t));
            yield return new WaitForFixedUpdate();
        }
        Destroy(gameObject);
    }
}