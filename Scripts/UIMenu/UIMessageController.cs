using UnityEngine;
using TMPro;

public class UIMessageController : MonoBehaviour
{
    [Header("UI Messages")]
    public TextMeshProUGUI initialMessage;
   
    [Header("Settings")]
    public float fadeSpeed = 2f;
   
    // Estados internos
    private bool navigationUIOpen = false;
    private bool isNavigating = false; // NUEVO: Controla si hay navegación activa
   
    void Start()
    {
        // Mostrar mensaje inicial al comenzar (solo si no hay navegación)
        ShowInitialMessage();
    }
   
    void Update()
    {
        // Ya no necesitamos detectar teclas aquí
        // NavigationManager.cs se encargará de notificarnos
    }
   
    // ========================================
    // MÉTODOS PÚBLICOS PARA CONTROLAR EL MENSAJE
    // ========================================
   
    public void ShowInitialMessage()
    {
        // CAMBIO: Solo mostrar si NO hay navegación activa
        if (initialMessage != null && !isNavigating)
        {
            initialMessage.gameObject.SetActive(true);
            StartCoroutine(FadeIn(initialMessage));
            Debug.Log("📝 UIMessageController: Mostrando mensaje inicial");
        }
        else if (isNavigating)
        {
            Debug.Log("📝 UIMessageController: No se muestra mensaje inicial - navegación activa");
        }
    }
   
    public void HideInitialMessage()
    {
        if (initialMessage != null)
        {
            StartCoroutine(FadeOut(initialMessage));
            Debug.Log("📝 UIMessageController: Ocultando mensaje inicial");
        }
    }
   
    // ========================================
    // MÉTODOS DE NOTIFICACIÓN DESDE NavigationUI
    // ========================================
   
    // Método que será llamado desde NavigationUI cuando se cierre
    public void OnNavigationUIClosed()
    {
        navigationUIOpen = false;
        
        // CAMBIO: Solo mostrar mensaje si NO hay navegación activa
        if (!isNavigating)
        {
            ShowInitialMessage();
            Debug.Log("📝 UIMessageController: NavigationUI cerrado - Mostrando mensaje inicial");
        }
        else
        {
            Debug.Log("📝 UIMessageController: NavigationUI cerrado - Pero hay navegación activa, no mostrar mensaje");
        }
    }
   
    // Método que será llamado desde NavigationUI cuando se abra
    public void OnNavigationUIOpened()
    {
        HideInitialMessage();
        navigationUIOpen = true;
        Debug.Log("📝 UIMessageController: NavigationUI abierto - Ocultando mensaje inicial");
    }
   
    // ========================================
    // NUEVOS MÉTODOS DE NOTIFICACIÓN DESDE NavigationManager
    // ========================================
   
    /// <summary>
    /// Llamado cuando se inicia una navegación
    /// El mensaje inicial NO debe aparecer hasta que termine la navegación
    /// </summary>
    public void OnNavigationStarted()
    {
        isNavigating = true;
        
        // Ocultar mensaje inicial si está visible
        HideInitialMessage();
        
        Debug.Log("📝 UIMessageController: Navegación iniciada - Mensaje inicial bloqueado");
    }
   
    /// <summary>
    /// Llamado cuando se cancela o completa una navegación
    /// Ahora el mensaje inicial puede aparecer de nuevo
    /// </summary>
    public void OnNavigationEnded()
    {
        isNavigating = false;
        
        // Solo mostrar mensaje inicial si NavigationUI no está abierto
        if (!navigationUIOpen)
        {
            ShowInitialMessage();
            Debug.Log("📝 UIMessageController: Navegación terminada - Mostrando mensaje inicial");
        }
        else
        {
            Debug.Log("📝 UIMessageController: Navegación terminada - Pero NavigationUI está abierto, no mostrar mensaje");
        }
    }
   
    // ========================================
    // EFECTOS DE FADE (SIN CAMBIOS)
    // ========================================
   
    private System.Collections.IEnumerator FadeIn(TextMeshProUGUI text)
    {
        Color color = text.color;
        color.a = 0f;
        text.color = color;
       
        while (color.a < 1f)
        {
            color.a += Time.deltaTime * fadeSpeed;
            text.color = color;
            yield return null;
        }
       
        color.a = 1f;
        text.color = color;
    }
   
    private System.Collections.IEnumerator FadeOut(TextMeshProUGUI text)
    {
        Color color = text.color;
       
        while (color.a > 0f)
        {
            color.a -= Time.deltaTime * fadeSpeed;
            text.color = color;
            yield return null;
        }
       
        color.a = 0f;
        text.color = color;
        text.gameObject.SetActive(false);
    }
    
    // ========================================
    // MÉTODOS DE DEBUG (OPCIONALES)
    // ========================================
    
    [ContextMenu("📊 Mostrar Estado Actual")]
    public void LogCurrentState()
    {
        Debug.Log($"📊 UIMessageController Estado:");
        Debug.Log($"   - Navigation UI Open: {navigationUIOpen}");
        Debug.Log($"   - Is Navigating: {isNavigating}");
        Debug.Log($"   - Message Active: {(initialMessage != null ? initialMessage.gameObject.activeSelf : false)}");
        Debug.Log($"   - Message Alpha: {(initialMessage != null ? initialMessage.color.a : 0)}");
    }
    
    /// <summary>
    /// Forzar mostrar mensaje (para debugging)
    /// </summary>
    [ContextMenu("🔄 Forzar Mostrar Mensaje")]
    public void ForceShowMessage()
    {
        if (initialMessage != null)
        {
            initialMessage.gameObject.SetActive(true);
            StartCoroutine(FadeIn(initialMessage));
            Debug.Log("📝 UIMessageController: Mensaje forzado a mostrar");
        }
    }
    
    /// <summary>
    /// Forzar ocultar mensaje (para debugging)
    /// </summary>
    [ContextMenu("🔄 Forzar Ocultar Mensaje")]
    public void ForceHideMessage()
    {
        HideInitialMessage();
        Debug.Log("📝 UIMessageController: Mensaje forzado a ocultar");
    }
    
    // ========================================
    // GETTERS PÚBLICOS
    // ========================================
    
    public bool IsNavigationUIOpen => navigationUIOpen;
    public bool IsNavigating => isNavigating;
    public bool IsMessageVisible => initialMessage != null && initialMessage.gameObject.activeSelf && initialMessage.color.a > 0;
}