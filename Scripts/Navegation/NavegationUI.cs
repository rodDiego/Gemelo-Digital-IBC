using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Maneja la interfaz de usuario para el sistema de navegación del gemelo digital.
/// Genera dinámicamente botones para waypoints, gestiona el panel de destinos
/// y coordina con NavigationManager para iniciar navegación.
/// 
/// Incluye soporte completo para CEE (Centro de Estudiantes) y diseño
/// responsive optimizado para el proyecto universitario.
/// </summary>
public class NavigationUI : MonoBehaviour
{
    #region Inspector References
    [Header("UI References")]
    [Tooltip("Panel principal que contiene la lista de destinos")]
    public GameObject destinationPanel;
    
    [Tooltip("Transform padre donde se generan los botones (Content del ScrollView)")]
    public Transform contentParent;
    
    [Tooltip("Botón para cerrar el panel de navegación")]
    public Button closeButton;
    
    [Tooltip("Prefab del botón de destino (opcional - se crea dinámicamente si es null)")]
    public GameObject destinationButtonPrefab;

    [Header("Player Reference")]
    [Tooltip("Transform del jugador para cálculo de distancias")]
    public Transform playerTransform;
    
    [Tooltip("Script controlador FPS del jugador")]
    public MonoBehaviour fpsController;

    [Header("Message Controller")]
    [Tooltip("Controlador para mensajes de feedback al usuario")]
    public UIMessageController messageController;

    [Header("Visual Settings")]
    [Tooltip("Color primario de los botones (#4fc3f7)")]
    public Color primaryButtonColor = new Color(0.31f, 0.76f, 0.97f, 1f);
    
    [Tooltip("Color de hover de los botones (#29b6f6)")]
    public Color hoverButtonColor = new Color(0.16f, 0.71f, 0.96f, 1f);
    
    [Tooltip("Color del texto")]
    public Color textColor = Color.white;
    
    [Tooltip("Fuente para los botones")]
    public Font buttonFont;
    #endregion

    #region Private State
    /// <summary>
    /// Lista de botones generados dinámicamente para limpieza posterior
    /// </summary>
    private List<Button> destinationButtons = new List<Button>();
    
    /// <summary>
    /// Mapeo de tipos de sala a iconos de texto para identificación visual.
    /// Incluye soporte completo para CEE (Centro de Estudiantes).
    /// </summary>
    private Dictionary<RoomType, string> roomIcons = new Dictionary<RoomType, string>();
    #endregion

    #region Unity Lifecycle
    void Start()
    {
        InitializeRoomIcons();
        SetupUI();
        // PopulateDestinationList se ejecuta solo cuando se abre el panel para mejor rendimiento
    }
    #endregion

    #region Initialization
    /// <summary>
    /// Inicializa el mapeo de iconos para cada tipo de sala.
    /// Utiliza símbolos cortos y distintivos, incluyendo CEE.
    /// </summary>
    void InitializeRoomIcons()
    {
        roomIcons[RoomType.Aula] = "A•";
        roomIcons[RoomType.Laboratorio] = "L•";
        roomIcons[RoomType.Oficina] = "O•";
        roomIcons[RoomType.Sala_Reuniones] = "R•";
        roomIcons[RoomType.Biblioteca] = "B•";
        roomIcons[RoomType.Auditorio] = "T•";
        roomIcons[RoomType.CEE] = "CEE•"; // Centro de Estudiantes
        roomIcons[RoomType.Otro] = "?•";
    }

    /// <summary>
    /// Obtiene el icono correspondiente a un tipo de sala
    /// </summary>
    /// <param name="roomType">Tipo de sala</param>
    /// <returns>String del icono o "?•" por defecto</returns>
    string GetRoomIcon(RoomType roomType)
    {
        return roomIcons.ContainsKey(roomType) ? roomIcons[roomType] : "?•";
    }

    /// <summary>
    /// Configura los elementos básicos de UI y eventos
    /// </summary>
    void SetupUI()
    {
        // Configurar botón cerrar
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseNavigationPanel);
        }

        // Panel inicialmente oculto para mejor rendimiento
        if (destinationPanel != null)
        {
            destinationPanel.SetActive(false);
            Debug.Log("🔒 Panel configurado como inactivo en SetupUI()");
        }
    }
    #endregion

    #region Public Panel Control
    /// <summary>
    /// Abre el panel de navegación y genera la lista de destinos.
    /// Maneja el estado del cursor y FPS controller.
    /// </summary>
    public void OpenNavigationPanel()
    {
        if (destinationPanel != null)
        {
            destinationPanel.SetActive(true);
            PopulateDestinationList(); // Generar contenido solo al abrir
            
            // Configurar cursor para interacción UI
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Desactivar controlador FPS durante navegación UI
            if (fpsController != null)
            {
                fpsController.enabled = false;
            }
            
            // Notificar apertura a MessageController
            if (messageController != null)
            {
                messageController.OnNavigationUIOpened();
            }
            
            Debug.Log("✅ Panel abierto correctamente");
        }
    }

    /// <summary>
    /// Cierra el panel de navegación y restaura el control FPS.
    /// Limpia la ruta actual del PathfindingSystem.
    /// </summary>
    public void CloseNavigationPanel()
    {
        if (destinationPanel != null)
        {
            destinationPanel.SetActive(false);
            
            // Restaurar cursor para juego
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            // Reactivar controlador FPS
            if (fpsController != null)
            {
                fpsController.enabled = true;
            }
            
            // Notificar cierre a MessageController
            if (messageController != null)
            {
                messageController.OnNavigationUIClosed();
            }
            
            Debug.Log("✅ Panel cerrado correctamente");
        }

        // Limpiar ruta visual actual
        if (PathfindingSystem.Instance != null)
        {
            PathfindingSystem.Instance.ClearPath();
        }
    }

    /// <summary>
    /// Método público para compatibilidad con botones UI (wrapper de CloseNavigationPanel)
    /// </summary>
    public void ClosePanel()
    {
        CloseNavigationPanel();
    }
    #endregion

    #region Dynamic Content Generation
    /// <summary>
    /// Genera dinámicamente la lista de botones de destino basada en waypoints activos.
    /// Solo se ejecuta cuando se abre el panel para optimizar rendimiento.
    /// </summary>
    void PopulateDestinationList()
    {
        ClearDestinationButtons();
        SetupContentLayout();

        // Verificar disponibilidad de WaypointManager
        if (WaypointManager.Instance == null)
        {
            WaypointManager wayManager = FindObjectOfType<WaypointManager>();
            if (wayManager == null)
            {
                Debug.LogError("No se encontró WaypointManager en la escena!");
                return;
            }
            Debug.Log("Usando WaypointManager encontrado en la escena");
        }

        Debug.Log($"Total waypoints encontrados: {WaypointManager.Instance.allWaypoints.Count}");

        // Crear botón para cada waypoint activo
        foreach (Waypoint waypoint in WaypointManager.Instance.allWaypoints)
        {
            if (waypoint.isActive)
            {
                Debug.Log($"Creando botón para: {waypoint.GetDisplayName()}");
                CreateDestinationButton(waypoint);
            }
        }
    }

    /// <summary>
    /// Configura el layout del contenedor para distribución óptima de botones
    /// </summary>
    void SetupContentLayout()
    {
        if (contentParent != null)
        {
            // Configurar VerticalLayoutGroup para organización vertical
            VerticalLayoutGroup contentLayout = contentParent.GetComponent<VerticalLayoutGroup>();
            if (contentLayout == null)
            {
                contentLayout = contentParent.gameObject.AddComponent<VerticalLayoutGroup>();
            }
            
            contentLayout.childAlignment = TextAnchor.UpperCenter;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = false;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.spacing = 10;
            contentLayout.padding = new RectOffset(5, 5, 10, 10);
            
            // ContentSizeFitter para ajuste automático de altura
            ContentSizeFitter sizeFitter = contentParent.GetComponent<ContentSizeFitter>();
            if (sizeFitter == null)
            {
                sizeFitter = contentParent.gameObject.AddComponent<ContentSizeFitter>();
            }
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // Optimizar ancho del ScrollView para mejor visualización
            OptimizeScrollViewWidth();
        }
    }

    /// <summary>
    /// Optimiza el ancho del ScrollView para mejor presentación
    /// </summary>
    void OptimizeScrollViewWidth()
    {
        ScrollRect scrollRect = contentParent.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            RectTransform scrollRectTransform = scrollRect.GetComponent<RectTransform>();
            if (scrollRectTransform != null)
            {
                Vector2 currentSize = scrollRectTransform.sizeDelta;
                scrollRectTransform.sizeDelta = new Vector2(380, currentSize.y);
                Debug.Log($"ScrollView redimensionado a: {scrollRectTransform.sizeDelta}");
            }
        }
    }

    /// <summary>
    /// Crea un botón de destino individual para un waypoint.
    /// Usa prefab si está disponible, o crea uno estilizado dinámicamente.
    /// </summary>
    /// <param name="waypoint">Waypoint para el cual crear el botón</param>
    void CreateDestinationButton(Waypoint waypoint)
    {
        GameObject buttonObj;

        if (destinationButtonPrefab != null)
        {
            buttonObj = Instantiate(destinationButtonPrefab, contentParent);
            UpdateButtonContent(buttonObj, waypoint);
        }
        else
        {
            // Crear botón estilizado si no hay prefab
            buttonObj = CreateStyledButton(waypoint);
        }

        // Configurar evento de navegación
        Button button = buttonObj.GetComponent<Button>();
        button.onClick.AddListener(() => NavigateToDestination(waypoint));

        destinationButtons.Add(button);
    }
    #endregion

    #region Dynamic Button Creation
    /// <summary>
    /// Crea un botón completamente estilizado siguiendo el diseño del proyecto.
    /// Incluye layout horizontal, iconos y información de sala.
    /// </summary>
    /// <param name="waypoint">Waypoint para el botón</param>
    /// <returns>GameObject del botón creado</returns>
    GameObject CreateStyledButton(Waypoint waypoint)
    {
        GameObject buttonObj = new GameObject("DestinationButton");
        buttonObj.transform.SetParent(contentParent);

        // Configurar RectTransform con altura optimizada
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(340, 80);
        buttonRect.anchorMin = new Vector2(0, 1);
        buttonRect.anchorMax = new Vector2(1, 1);

        // Componentes básicos del botón
        Image buttonImage = buttonObj.AddComponent<Image>();
        Button button = buttonObj.AddComponent<Button>();

        // Aplicar esquema de colores
        buttonImage.color = primaryButtonColor;
        ConfigureButtonColors(button);

        // Layout horizontal para organizar contenido
        HorizontalLayoutGroup layout = buttonObj.AddComponent<HorizontalLayoutGroup>();
        ConfigureHorizontalLayout(layout);

        // Crear elementos internos del botón
        GameObject iconObj = CreateIconText(buttonObj.transform, waypoint);
        GameObject infoObj = CreateInfoArea(buttonObj.transform, waypoint);

        return buttonObj;
    }

    /// <summary>
    /// Configura los colores del botón para diferentes estados
    /// </summary>
    /// <param name="button">Botón a configurar</param>
    void ConfigureButtonColors(Button button)
    {
        ColorBlock colorBlock = button.colors;
        colorBlock.normalColor = primaryButtonColor;
        colorBlock.highlightedColor = hoverButtonColor;
        colorBlock.pressedColor = new Color(0.1f, 0.6f, 0.9f, 1f);
        colorBlock.selectedColor = primaryButtonColor;
        colorBlock.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        button.colors = colorBlock;
    }

    /// <summary>
    /// Configura el layout horizontal del botón
    /// </summary>
    /// <param name="layout">HorizontalLayoutGroup a configurar</param>
    void ConfigureHorizontalLayout(HorizontalLayoutGroup layout)
    {
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.padding = new RectOffset(25, 20, 15, 15);
        layout.spacing = 15;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
    }

    /// <summary>
    /// Crea el área de icono del botón con tamaño dinámico según el tipo de sala
    /// </summary>
    /// <param name="parent">Transform padre</param>
    /// <param name="waypoint">Waypoint para determinar el icono</param>
    /// <returns>GameObject del icono</returns>
    GameObject CreateIconText(Transform parent, Waypoint waypoint)
    {
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(parent);

        // Tamaño dinámico según tipo de sala (CEE necesita más espacio)
        float iconWidth = waypoint.roomType == RoomType.CEE ? 50 : 35;
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(iconWidth, 40);

        TextMeshProUGUI iconText = iconObj.AddComponent<TextMeshProUGUI>();
        iconText.text = GetRoomIcon(waypoint.roomType);
        iconText.fontSize = waypoint.roomType == RoomType.CEE ? 16 : 20; // Texto más pequeño para CEE
        iconText.color = new Color(1f, 1f, 1f, 0.9f);
        iconText.alignment = TextAlignmentOptions.Center;
        iconText.fontStyle = FontStyles.Bold;

        // Aplicar color específico según tipo de sala
        ApplyIconStyling(iconText, waypoint.roomType);

        return iconObj;
    }

    /// <summary>
    /// Aplica colores específicos por tipo de sala para mejor identificación visual.
    /// Incluye soporte completo para CEE (Centro de Estudiantes).
    /// </summary>
    /// <param name="iconText">Componente de texto del icono</param>
    /// <param name="roomType">Tipo de sala</param>
    void ApplyIconStyling(TextMeshProUGUI iconText, RoomType roomType)
    {
        switch (roomType)
        {
            case RoomType.Aula:
                iconText.color = new Color(0.2f, 1f, 0.2f, 1f); // Verde claro
                break;
            case RoomType.Laboratorio:
                iconText.color = new Color(1f, 0.8f, 0.2f, 1f); // Amarillo/naranja
                break;
            case RoomType.Oficina:
                iconText.color = new Color(0.8f, 0.8f, 1f, 1f); // Azul claro
                break;
            case RoomType.Sala_Reuniones:
                iconText.color = new Color(1f, 0.6f, 1f, 1f); // Rosa claro
                break;
            case RoomType.Biblioteca:
                iconText.color = new Color(0.6f, 0.8f, 1f, 1f); // Celeste
                break;
            case RoomType.Auditorio:
                iconText.color = new Color(1f, 0.4f, 0.4f, 1f); // Rojo claro
                break;
            case RoomType.CEE: // Centro de Estudiantes
                iconText.color = new Color(0.8f, 0.8f, 1f, 1f); // Azul claro (como oficina)
                break;
            default:
                iconText.color = new Color(1f, 1f, 1f, 0.8f); // Blanco por defecto
                break;
        }
    }

    /// <summary>
    /// Crea el área de información del botón con nombre y tipo de sala
    /// </summary>
    /// <param name="parent">Transform padre</param>
    /// <param name="waypoint">Waypoint con la información</param>
    /// <returns>GameObject del área de información</returns>
    GameObject CreateInfoArea(Transform parent, Waypoint waypoint)
    {
        GameObject infoObj = new GameObject("Info");
        infoObj.transform.SetParent(parent);

        // Ancho dinámico compensando el espacio del icono
        float infoWidth = waypoint.roomType == RoomType.CEE ? 245 : 260;
        RectTransform infoRect = infoObj.AddComponent<RectTransform>();
        infoRect.sizeDelta = new Vector2(infoWidth, 60);

        // Layout vertical para organizar información
        VerticalLayoutGroup verticalLayout = infoObj.AddComponent<VerticalLayoutGroup>();
        verticalLayout.childAlignment = TextAnchor.UpperLeft;
        verticalLayout.childForceExpandHeight = false;
        verticalLayout.childForceExpandWidth = true;
        verticalLayout.spacing = 3;
        verticalLayout.padding = new RectOffset(0, 0, 8, 8);

        // Crear línea principal (nombre de sala)
        CreateRoomNameText(infoObj.transform, waypoint, infoWidth);
        
        // Crear línea secundaria (tipo de sala)
        CreateRoomTypeText(infoObj.transform, waypoint, infoWidth);

        return infoObj;
    }

    /// <summary>
    /// Crea el texto principal con nombre y código de sala
    /// </summary>
    void CreateRoomNameText(Transform parent, Waypoint waypoint, float width)
    {
        GameObject nameObj = new GameObject("RoomName");
        nameObj.transform.SetParent(parent);

        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = $"{waypoint.roomCode} - {waypoint.roomName}";
        nameText.fontSize = 16;
        nameText.fontStyle = FontStyles.Bold;
        nameText.color = textColor;
        nameText.alignment = TextAlignmentOptions.Left;
        nameText.enableWordWrapping = false;

        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.sizeDelta = new Vector2(width, 25);
    }

    /// <summary>
    /// Crea el texto secundario con tipo de sala
    /// </summary>
    void CreateRoomTypeText(Transform parent, Waypoint waypoint, float width)
    {
        GameObject typeObj = new GameObject("RoomType");
        typeObj.transform.SetParent(parent);

        TextMeshProUGUI typeText = typeObj.AddComponent<TextMeshProUGUI>();
        typeText.text = GetRoomTypeDisplayName(waypoint.roomType);
        typeText.fontSize = 12;
        typeText.color = new Color(1f, 1f, 1f, 0.85f);
        typeText.alignment = TextAlignmentOptions.Left;
        typeText.enableWordWrapping = false;
        typeText.fontStyle = FontStyles.Italic;

        RectTransform typeRect = typeObj.GetComponent<RectTransform>();
        typeRect.sizeDelta = new Vector2(width, 18);
    }

    /// <summary>
    /// Convierte el enum RoomType a nombre legible para mostrar al usuario.
    /// Incluye soporte completo para CEE (Centro de Estudiantes).
    /// </summary>
    /// <param name="roomType">Tipo de sala</param>
    /// <returns>Nombre legible del tipo de sala</returns>
    string GetRoomTypeDisplayName(RoomType roomType)
    {
        switch (roomType)
        {
            case RoomType.Aula: return "Aula de Clases";
            case RoomType.Laboratorio: return "Laboratorio";
            case RoomType.Oficina: return "Oficina Administrativa";
            case RoomType.Sala_Reuniones: return "Sala de Reuniones";
            case RoomType.Biblioteca: return "Biblioteca";
            case RoomType.Auditorio: return "Auditorio";
            case RoomType.CEE: return "Centro de Estudiantes"; // Soporte para CEE
            case RoomType.Otro: return "Área General";
            default: return "Espacio";
        }
    }

    /// <summary>
    /// Actualiza el contenido de un botón creado desde prefab
    /// </summary>
    /// <param name="buttonObj">GameObject del botón</param>
    /// <param name="waypoint">Waypoint con la información</param>
    void UpdateButtonContent(GameObject buttonObj, Waypoint waypoint)
    {
        // Buscar componentes específicos en el prefab
        Transform iconText = buttonObj.transform.Find("IconText");
        Transform infoArea = buttonObj.transform.Find("InfoArea");
        
        if (iconText != null)
        {
            TextMeshProUGUI iconComponent = iconText.GetComponent<TextMeshProUGUI>();
            if (iconComponent != null)
            {
                iconComponent.text = GetRoomIcon(waypoint.roomType);
                ApplyIconStyling(iconComponent, waypoint.roomType);
            }
        }
        
        if (infoArea != null)
        {
            UpdatePrefabInfoArea(infoArea, waypoint);
        }
    }

    /// <summary>
    /// Actualiza el área de información en un prefab
    /// </summary>
    void UpdatePrefabInfoArea(Transform infoArea, Waypoint waypoint)
    {
        Transform roomNameText = infoArea.Find("RoomNameText");
        Transform roomTypeText = infoArea.Find("RoomTypeText");
        
        if (roomNameText != null)
        {
            TextMeshProUGUI nameComponent = roomNameText.GetComponent<TextMeshProUGUI>();
            if (nameComponent != null)
            {
                nameComponent.text = $"{waypoint.roomCode} - {waypoint.roomName}";
            }
        }
        
        if (roomTypeText != null)
        {
            TextMeshProUGUI typeComponent = roomTypeText.GetComponent<TextMeshProUGUI>();
            if (typeComponent != null)
            {
                typeComponent.text = GetRoomTypeDisplayName(waypoint.roomType);
            }
        }
    }
    #endregion

    #region Navigation Integration
    /// <summary>
    /// Inicia la navegación hacia un destino seleccionado.
    /// Utiliza NavigationManager para coordinar con PathfindingSystem.
    /// </summary>
    /// <param name="destination">Waypoint de destino seleccionado</param>
    void NavigateToDestination(Waypoint destination)
    {
        if (NavigationManager.Instance != null)
        {
            bool success = NavigationManager.Instance.StartNavigationToWaypoint(destination);

            if (success)
            {
                Debug.Log($"Navegando hacia: {destination.GetDisplayName()}");
                CloseNavigationPanel(); // Cerrar panel tras selección exitosa
            }
            else
            {
                Debug.LogWarning($"No se pudo calcular ruta hacia: {destination.GetDisplayName()}");
            }
        }
        else
        {
            Debug.LogError("NavigationManager.Instance es null!");
        }
    }
    #endregion

    #region Cleanup
    /// <summary>
    /// Limpia todos los botones generados dinámicamente para regeneración
    /// </summary>
    void ClearDestinationButtons()
    {
        foreach (Button button in destinationButtons)
        {
            if (button != null)
            {
                DestroyImmediate(button.gameObject);
            }
        }
        destinationButtons.Clear();
    }
    #endregion
}