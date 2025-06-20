using UnityEngine;
using TMPro;

public class CancelMessageController : MonoBehaviour
{
    [Header("Mensaje de Cancelación")]
    [Tooltip("Referencia al texto del mensaje 'Presiona X para cancelar'")]
    public TextMeshProUGUI cancelMessage;
    
    [Header("Configuración")]
    [Tooltip("Velocidad del fade in/out")]
    public float fadeSpeed = 2f;
    
    [Tooltip("Mostrar mensaje inmediatamente sin fade")]
    public bool showInstantly = false;
    
    // Variables internas
    private bool isNavigating = false;
    private Coroutine currentFade;
    
    void Start()
    {
        // Asegurar que el mensaje esté oculto al inicio
        HideCancelMessage();
    }
    
    // ========================================
    // MÉTODOS PÚBLICOS - LLAMADOS DESDE NavigationManager
    // ========================================
    
    /// <summary>
    /// Llamado cuando se inicia una navegación (usuario selecciona destino)
    /// </summary>
    public void OnNavigationStarted()
    {
        isNavigating = true;
        ShowCancelMessage();
        
        Debug.Log("📝 CancelMessageController: Navegación iniciada - Mostrando mensaje X");
    }
    
    /// <summary>
    /// Llamado cuando se cancela la navegación (tecla X) o se llega al destino
    /// </summary>
    public void OnNavigationEnded()
    {
        isNavigating = false;
        HideCancelMessage();
        
        Debug.Log("📝 CancelMessageController: Navegación terminada - Ocultando mensaje X");
    }
    
    // ========================================
    // MÉTODOS PRIVADOS DE CONTROL
    // ========================================
    
    /// <summary>
    /// Muestra el mensaje de cancelación
    /// </summary>
    private void ShowCancelMessage()
    {
        if (cancelMessage == null) return;
        
        // Detener fade anterior si existe
        if (currentFade != null)
        {
            StopCoroutine(currentFade);
        }
        
        // Activar el GameObject
        cancelMessage.gameObject.SetActive(true);
        
        if (showInstantly)
        {
            // Mostrar inmediatamente
            Color color = cancelMessage.color;
            color.a = 1f;
            cancelMessage.color = color;
        }
        else
        {
            // Mostrar con fade in
            currentFade = StartCoroutine(FadeIn());
        }
        
        Debug.Log("📝 CancelMessageController: Mensaje X mostrado");
    }
    
    /// <summary>
    /// Oculta el mensaje de cancelación
    /// </summary>
    private void HideCancelMessage()
    {
        if (cancelMessage == null) return;
        
        // Detener fade anterior si existe
        if (currentFade != null)
        {
            StopCoroutine(currentFade);
        }
        
        if (showInstantly)
        {
            // Ocultar inmediatamente
            Color color = cancelMessage.color;
            color.a = 0f;
            cancelMessage.color = color;
            cancelMessage.gameObject.SetActive(false);
        }
        else
        {
            // Ocultar con fade out
            currentFade = StartCoroutine(FadeOut());
        }
        
        Debug.Log("📝 CancelMessageController: Mensaje X ocultado");
    }
    
    // ========================================
    // CORRUTINAS DE FADE
    // ========================================
    
    private System.Collections.IEnumerator FadeIn()
    {
        Color color = cancelMessage.color;
        color.a = 0f;
        cancelMessage.color = color;
        
        while (color.a < 1f)
        {
            color.a += Time.deltaTime * fadeSpeed;
            color.a = Mathf.Clamp01(color.a);
            cancelMessage.color = color;
            yield return null;
        }
        
        color.a = 1f;
        cancelMessage.color = color;
        currentFade = null;
    }
    
    private System.Collections.IEnumerator FadeOut()
    {
        Color color = cancelMessage.color;
        
        while (color.a > 0f)
        {
            color.a -= Time.deltaTime * fadeSpeed;
            color.a = Mathf.Clamp01(color.a);
            cancelMessage.color = color;
            yield return null;
        }
        
        color.a = 0f;
        cancelMessage.color = color;
        cancelMessage.gameObject.SetActive(false);
        currentFade = null;
    }
    
    // ========================================
    // MÉTODOS DE DEBUG (OPCIONALES)
    // ========================================
    
    [ContextMenu("🔄 Mostrar Mensaje X")]
    public void TestShowMessage()
    {
        ShowCancelMessage();
    }
    
    [ContextMenu("🔄 Ocultar Mensaje X")]
    public void TestHideMessage()
    {
        HideCancelMessage();
    }
    
    [ContextMenu("📊 Estado Actual")]
    public void LogCurrentState()
    {
        if (cancelMessage != null)
        {
            Debug.Log($"📊 CancelMessageController Estado:");
            Debug.Log($"   - Is Navigating: {isNavigating}");
            Debug.Log($"   - GameObject Active: {cancelMessage.gameObject.activeSelf}");
            Debug.Log($"   - Text Alpha: {cancelMessage.color.a}");
            Debug.Log($"   - Current Fade: {(currentFade != null ? "En progreso" : "Ninguna")}");
        }
        else
        {
            Debug.LogWarning("⚠️ CancelMessage no está asignado!");
        }
    }
    
    // ========================================
    // VALIDACIÓN
    // ========================================
    
    void OnValidate()
    {
        // Verificar que fadeSpeed sea válido
        if (fadeSpeed <= 0)
        {
            fadeSpeed = 2f;
            Debug.LogWarning("⚠️ CancelMessageController: fadeSpeed debe ser mayor que 0. Establecido en 2.");
        }
        
        // Verificar que cancelMessage esté asignado
        if (cancelMessage == null)
        {
            Debug.LogWarning("⚠️ CancelMessageController: cancelMessage no está asignado. Asigna la referencia en el Inspector.");
        }
    }
    
    // ========================================
    // GETTERS PÚBLICOS
    // ========================================
    
    public bool IsNavigating => isNavigating;
    public bool IsMessageVisible => cancelMessage != null && cancelMessage.gameObject.activeSelf && cancelMessage.color.a > 0;
}