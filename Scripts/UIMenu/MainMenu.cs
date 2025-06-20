using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject mainMenuPanel;
    public Button continueButton;
    public Button exitButton;
    
    [Header("Player Reference")]
    public MonoBehaviour fpsController; // Referencia al FPS Controller
    
    [Header("Other Controllers")]
    public UIMessageController messageController;
    public NavigationUI navigationUI;
    
    private bool isMenuOpen = false;
    private bool navigationUIOpen = false;
    
    void Start()
    {
        SetupMenu();
        
        // Asegurar estado inicial correcto
        isMenuOpen = false;
        navigationUIOpen = false;
        
        // Configurar cursor inicial
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        // Debug temporal
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log($"ESC presionado - NavigationUI abierto: {navigationUIOpen}, Menu abierto: {isMenuOpen}");
        }
        
        // Detectar ESC solo cuando NavigationUI está cerrado
        if (Input.GetKeyDown(KeyCode.Escape) && !navigationUIOpen)
        {
            if (isMenuOpen)
            {
                Debug.Log("Cerrando menú principal...");
                CloseMainMenu();
            }
            else
            {
                Debug.Log("Abriendo menú principal...");
                OpenMainMenu();
            }
        }
        
        // Detectar si NavigationUI está abierto (para evitar conflictos)
        CheckNavigationUIStatus();
    }
    
    void SetupMenu()
    {
        // Configurar botones
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(CloseMainMenu);
        }
        
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitApplication);
        }
        
        // Panel inicialmente oculto
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
        
        Debug.Log("🎮 MainMenu configurado correctamente");
    }
    
    public void OpenMainMenu()
    {
        if (mainMenuPanel != null && !navigationUIOpen)
        {
            mainMenuPanel.SetActive(true);
            isMenuOpen = true;
            
            // Desbloquear cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Desactivar FPS Controller
            if (fpsController != null)
            {
                fpsController.enabled = false;
            }
            
            // Ocultar mensaje inicial
            if (messageController != null)
            {
                messageController.HideInitialMessage();
            }
            
            Debug.Log("🎮 Menú principal abierto");
        }
    }
    
    public void CloseMainMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
            isMenuOpen = false;
            
            // Bloquear cursor nuevamente
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            // Reactivar FPS Controller
            if (fpsController != null)
            {
                fpsController.enabled = true;
            }
            
            // Mostrar mensaje inicial nuevamente
            if (messageController != null)
            {
                messageController.ShowInitialMessage();
            }
            
            Debug.Log("🎮 Menú principal cerrado - Cursor bloqueado");
        }
    }
    
    public void ExitApplication()
    {
        Debug.Log("🚪 Saliendo del sistema...");
        
        // En el editor de Unity
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // En build final
            Application.Quit();
        #endif
    }
    
    void CheckNavigationUIStatus()
    {
        // Verificar si NavigationUI está abierto
        if (navigationUI != null && navigationUI.destinationPanel != null)
        {
            navigationUIOpen = navigationUI.destinationPanel.activeSelf;
        }
        else
        {
            // Si no hay NavigationUI, asegurar que está cerrado
            navigationUIOpen = false;
        }
    }
    
    // Métodos públicos para ser llamados desde otros scripts
    public void OnNavigationUIOpened()
    {
        navigationUIOpen = true;
        // Si el menú principal está abierto, cerrarlo
        if (isMenuOpen)
        {
            CloseMainMenu();
        }
    }
    
    public void OnNavigationUIClosed()
    {
        navigationUIOpen = false;
    }
    
    public bool IsMenuOpen()
    {
        return isMenuOpen;
    }
}