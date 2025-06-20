using UnityEngine;
using TMPro;

public class MessageController : MonoBehaviour
{
    [Header("Referencias de Mensajes")]
    [Tooltip("Referencia al texto del mensaje 'Presiona N para abrir navegación'")]
    public TextMeshProUGUI initialMessage;
    
    [Tooltip("Referencia al texto del mensaje 'Presiona X para cancelar'")]
    public TextMeshProUGUI cancelMessage;
    
    [Header("Configuración")]
    [Tooltip("Velocidad del fade in/out")]
    public float fadeSpeed = 2f;
    
    [Tooltip("Mostrar mensaje inmediatamente sin fade")]
    public bool showInstantly = false;
    
    // Variables internas
    private bool isNavigating = false;
    private bool navigationUIOpen = false;
    private Coroutine currentInitialFade;
    private Coroutine currentCancelFade;
    
    void Start()
    {
        // Al inicio: mostrar mensaje inicial, ocultar mensaje de cancelación
        HideCancelMessage();
        ShowInitialMessage();
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
        
        // Ocultar mensaje inicial y mostrar mensaje de cancelación
        HideInitialMessage();
        ShowCancelMessage();
        
        Debug.Log("📝 UnifiedMessageController: Navegación iniciada - N oculto, X mostrado");
    }
    
    /// <summary>
    /// Llamado cuando se cancela la navegación (tecla X) o se llega al destino
    /// </summary>
    public void OnNavigationEnded()
    {
        isNavigating = false;
        
        // Ocultar mensaje de cancelación y mostrar mensaje inicial (si UI no está abierto)
        HideCancelMessage();
        
        if (!navigationUIOpen)
        {
            ShowInitialMessage();
        }
        
        Debug.Log("📝 UnifiedMessageController: Navegación terminada - X oculto, N mostrado (si UI cerrado)");
    }
    
    // ========================================
    // MÉTODOS PÚBLICOS - LLAMADOS DESDE NavigationUI
    // ========================================
    
    /// <summary>
    /// Llamado cuando NavigationUI se abre
    /// </summary>
    public void OnNavigationUIOpened()
    {
        navigationUIOpen = true;
        
        // Ocultar mensaje inicial cuando se abre el panel (no tocar mensaje X si está navegando)
        if (!isNavigating)
        {
            HideInitialMessage();
        }
        
        Debug.Log("📝 UnifiedMessageController: NavigationUI abierto");
    }
    
    /// <summary>
    /// Llamado cuando NavigationUI se cierra
    /// </summary>
    public void OnNavigationUIClosed()
    {
        navigationUIOpen = false;
        
        // Mostrar mensaje apropiado según el estado
        if (!isNavigating)
        {
            ShowInitialMessage(); // Si no hay navegación, mostrar mensaje inicial
        }
        // Si hay navegación activa, el mensaje X ya debería estar visible
        
        Debug.Log("📝 UnifiedMessageController: NavigationUI cerrado");
    }
    
    // ========================================
    // MÉTODOS PRIVADOS DE CONTROL - MENSAJE INICIAL
    // ========================================
    
    /// <summary>
    /// Muestra el mensaje inicial
    /// </summary>
    private void ShowInitialMessage()
    {
        if (initialMessage == null || isNavigating) return;
        
        // Detener fade anterior si existe
        if (currentInitialFade != null)
        {
            StopCoroutine(currentInitialFade);
        }
        
        // Activar el GameObject
        initialMessage.gameObject.SetActive(true);
        
        if (showInstantly)
        {
            // Mostrar inmediatamente
            Color color = initialMessage.color;
            color.a = 1f;
            initialMessage.color = color;
        }
        else
        {
            // Mostrar con fade in
            currentInitialFade = StartCoroutine(FadeInInitial());
        }
        
        Debug.Log("📝 UnifiedMessageController: Mensaje N mostrado");
    }
    
    /// <summary>
    /// Oculta el mensaje inicial
    /// </summary>
    private void HideInitialMessage()
    {
        if (initialMessage == null) return;
        
        // Detener fade anterior si existe
        if (currentInitialFade != null)
        {
            StopCoroutine(currentInitialFade);
        }
        
        if (showInstantly)
        {
            // Ocultar inmediatamente
            Color color = initialMessage.color;
            color.a = 0f;
            initialMessage.color = color;
            initialMessage.gameObject.SetActive(false);
        }
        else
        {
            // Ocultar con fade out
            currentInitialFade = StartCoroutine(FadeOutInitial());
        }
        
        Debug.Log("📝 UnifiedMessageController: Mensaje N ocultado");
    }
    
    // ========================================
    // MÉTODOS PRIVADOS DE CONTROL - MENSAJE CANCELACIÓN
    // ========================================
    
    /// <summary>
    /// Muestra el mensaje de cancelación
    /// </summary>
    private void ShowCancelMessage()
    {
        if (cancelMessage == null) return;
        
        // Detener fade anterior si existe
        if (currentCancelFade != null)
        {
            StopCoroutine(currentCancelFade);
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
            currentCancelFade = StartCoroutine(FadeInCancel());
        }
        
        Debug.Log("📝 UnifiedMessageController: Mensaje X mostrado");
    }
    
    /// <summary>
    /// Oculta el mensaje de cancelación
    /// </summary>
    private void HideCancelMessage()
    {
        if (cancelMessage == null) return;
        
        // Detener fade anterior si existe
        if (currentCancelFade != null)
        {
            StopCoroutine(currentCancelFade);
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
            currentCancelFade = StartCoroutine(FadeOutCancel());
        }
        
        Debug.Log("📝 UnifiedMessageController: Mensaje X ocultado");
    }
    
    // ========================================
    // CORRUTINAS DE FADE - MENSAJE INICIAL
    // ========================================
    
    private System.Collections.IEnumerator FadeInInitial()
    {
        Color color = initialMessage.color;
        color.a = 0f;
        initialMessage.color = color;
        
        while (color.a < 1f)
        {
            color.a += Time.deltaTime * fadeSpeed;
            color.a = Mathf.Clamp01(color.a);
            initialMessage.color = color;
            yield return null;
        }
        
        color.a = 1f;
        initialMessage.color = color;
        currentInitialFade = null;
    }
    
    private System.Collections.IEnumerator FadeOutInitial()
    {
        Color color = initialMessage.color;
        
        while (color.a > 0f)
        {
            color.a -= Time.deltaTime * fadeSpeed;
            color.a = Mathf.Clamp01(color.a);
            initialMessage.color = color;
            yield return null;
        }
        
        color.a = 0f;
        initialMessage.color = color;
        initialMessage.gameObject.SetActive(false);
        currentInitialFade = null;
    }
    
    // ========================================
    // CORRUTINAS DE FADE - MENSAJE CANCELACIÓN
    // ========================================
    
    private System.Collections.IEnumerator FadeInCancel()
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
        currentCancelFade = null;
    }
    
    private System.Collections.IEnumerator FadeOutCancel()
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
        currentCancelFade = null;
    }
    
    // ========================================
    // MÉTODOS DE DEBUG
    // ========================================
    
    [ContextMenu("🔄 Mostrar Mensaje N")]
    public void TestShowInitialMessage()
    {
        ShowInitialMessage();
    }
    
    [ContextMenu("🔄 Ocultar Mensaje N")]
    public void TestHideInitialMessage()
    {
        HideInitialMessage();
    }
    
    [ContextMenu("🔄 Mostrar Mensaje X")]
    public void TestShowCancelMessage()
    {
        ShowCancelMessage();
    }
    
    [ContextMenu("🔄 Ocultar Mensaje X")]
    public void TestHideCancelMessage()
    {
        HideCancelMessage();
    }
    
    [ContextMenu("🔄 Simular Inicio Navegación")]
    public void TestNavigationStart()
    {
        OnNavigationStarted();
    }
    
    [ContextMenu("🔄 Simular Fin Navegación")]
    public void TestNavigationEnd()
    {
        OnNavigationEnded();
    }
    
    [ContextMenu("📊 Estado Actual")]
    public void LogCurrentState()
    {
        Debug.Log($"📊 UnifiedMessageController Estado:");
        Debug.Log($"   - Is Navigating: {isNavigating}");
        Debug.Log($"   - Navigation UI Open: {navigationUIOpen}");
        
        if (initialMessage != null)
        {
            Debug.Log($"   - Initial Message Active: {initialMessage.gameObject.activeSelf}");
            Debug.Log($"   - Initial Message Alpha: {initialMessage.color.a}");
        }
        
        if (cancelMessage != null)
        {
            Debug.Log($"   - Cancel Message Active: {cancelMessage.gameObject.activeSelf}");
            Debug.Log($"   - Cancel Message Alpha: {cancelMessage.color.a}");
        }
        
        Debug.Log($"   - Current Initial Fade: {(currentInitialFade != null ? "En progreso" : "Ninguna")}");
        Debug.Log($"   - Current Cancel Fade: {(currentCancelFade != null ? "En progreso" : "Ninguna")}");
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
            Debug.LogWarning("⚠️ UnifiedMessageController: fadeSpeed debe ser mayor que 0. Establecido en 2.");
        }
        
        // Verificar que los mensajes estén asignados
        if (initialMessage == null)
        {
            Debug.LogWarning("⚠️ UnifiedMessageController: initialMessage no está asignado.");
        }
        
        if (cancelMessage == null)
        {
            Debug.LogWarning("⚠️ UnifiedMessageController: cancelMessage no está asignado.");
        }
    }
    
    // ========================================
    // GETTERS PÚBLICOS
    // ========================================
    
    public bool IsNavigating => isNavigating;
    public bool IsNavigationUIOpen => navigationUIOpen;
    public bool IsInitialMessageVisible => initialMessage != null && initialMessage.gameObject.activeSelf && initialMessage.color.a > 0;
    public bool IsCancelMessageVisible => cancelMessage != null && cancelMessage.gameObject.activeSelf && cancelMessage.color.a > 0;
}