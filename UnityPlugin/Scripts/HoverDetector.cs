using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string labelToSpeak;
    private float timeToWait = 0.5f;
    private TextToSpeech textToSpeech;

    private void Start()
    {
        textToSpeech = FindObjectOfType<TextToSpeech>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Hover");
        StopAllCoroutines();
        StartCoroutine(StartTimer());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Exit Hover");
        StopAllCoroutines();
        HoverManager.OnMouseLoseFocus?.Invoke();
    }

    private void ShowMessage()
    {
        textToSpeech.Speak(labelToSpeak);
    }

    private IEnumerator StartTimer()
    {
        yield return new WaitForSeconds(timeToWait);
        ShowMessage();
    }
}
