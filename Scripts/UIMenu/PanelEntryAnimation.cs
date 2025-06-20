using UnityEngine;
using UnityEngine.UI;

public class PanelEntryAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    public float animationDuration = 0.8f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 targetScale;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        // Si no existe CanvasGroup, crear uno
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Guardar escala objetivo y empezar pequeño
        targetScale = rectTransform.localScale;
        rectTransform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;
        
        // Iniciar animación
        StartCoroutine(AnimateEntry());
    }
    
    System.Collections.IEnumerator AnimateEntry()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration)
        {
            float progress = elapsedTime / animationDuration;
            float curveValue = scaleCurve.Evaluate(progress);
            
            // Animar escala
            rectTransform.localScale = Vector3.Lerp(Vector3.zero, targetScale, curveValue);
            
            // Animar transparencia
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Asegurar valores finales
        rectTransform.localScale = targetScale;
        canvasGroup.alpha = 1f;
    }
}