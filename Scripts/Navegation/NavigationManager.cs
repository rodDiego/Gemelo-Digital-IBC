using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestor principal del sistema de navegación del gemelo digital.
/// Coordina la navegación entre waypoints, maneja el estado del sistema,
/// y controla la integración entre UI, PathfindingSystem y ParticleTrail.
/// 
/// Implementa patrón Singleton para acceso global desde otros sistemas.
/// </summary>
public class NavigationManager : MonoBehaviour
{
    #region Singleton Pattern
    /// <summary>
    /// Instancia singleton del NavigationManager para acceso global
    /// </summary>
    public static NavigationManager Instance { get; private set; }
    #endregion

    #region Inspector Configuration
    [Header("=== CONFIGURACIÓN PRINCIPAL ===")]
    [Tooltip("Configuración del sistema de navegación")]
    public NavigationConfig config;

    [Header("=== REFERENCIAS REQUERIDAS ===")]
    [Tooltip("Transform del jugador")]
    public Transform playerTransform;

    [Tooltip("Controlador FPS del jugador")]
    public MonoBehaviour fpsController;

    [Tooltip("Sistema de navegación UI")]
    public NavigationUI navigationUI;

    [Header("=== CONTROLADOR DE MENSAJES ===")]
    [Tooltip("Controlador unificado de mensajes N y X")]
    public MessageController messageController;

    [Header("=== REFERENCIAS DE AUDIO ===")]
    [Tooltip("AudioSource para efectos de sonido")]
    public AudioSource audioSource;

    [Header("=== UI DE INFORMACIÓN ===")]
    [Tooltip("Panel para mostrar información de la ruta actual")]
    public GameObject pathInfoPanel;

    [Tooltip("Texto para mostrar distancia al destino")]
    public TextMeshProUGUI distanceText;

    [Tooltip("Texto para mostrar nombre del destino")]
    public TextMeshProUGUI destinationText;

    [Tooltip("Botón para cancelar navegación")]
    public Button cancelNavigationButton;

    [Header("=== CONTROL DE NAVEGACIÓN ===")]
    [Tooltip("Tecla para cancelar la navegación actual")]
    public KeyCode cancelNavigationKey = KeyCode.X;

    [Tooltip("Bloquear apertura de NavigationUI durante navegación activa")]
    public bool blockNavigationUIWhileActive = true;
    #endregion

    #region Private State Variables
    /// <summary>
    /// Indica si NavigationUI está bloqueado durante navegación activa
    /// </summary>
    private bool navigationUIBlocked = false;
    
    /// <summary>
    /// Flag para controlar mostrar mensaje de cancelación
    /// </summary>
    private bool showCancelMessage = false;

    /// <summary>
    /// Estado actual de navegación activa
    /// </summary>
    private bool isNavigating = false;
    
    /// <summary>
    /// Waypoint de destino actual
    /// </summary>
    private Waypoint currentDestination;
    
    /// <summary>
    /// Timestamp de la última actualización de ruta para auto-recálculo
    /// </summary>
    private float lastPathUpdateTime = 0f;
    
    /// <summary>
    /// Corrutina para actualización automática de ruta
    /// </summary>
    private Coroutine pathUpdateCoroutine;
    
    /// <summary>
    /// Corrutina para verificación periódica de llegada al destino
    /// </summary>
    private Coroutine arrivalCheckCoroutine;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        // Implementar Singleton con persistencia entre escenas
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        InitializeNavigationManager();
    }

    void Update()
    {
        HandleInput();

        if (isNavigating)
        {
            UpdateNavigationInfo();
            CheckAutoRecalculation();
        }
    }
    #endregion

    #region Initialization
    /// <summary>
    /// Inicializa todos los componentes del NavigationManager
    /// </summary>
    void InitializeNavigationManager()
    {
        // Cargar configuración por defecto si no se asignó
        if (config == null)
        {
            config = Resources.Load<NavigationConfig>("DefaultNavigationConfig");
            if (config == null)
            {
                Debug.LogWarning("NavigationManager: No se encontró configuración. Usando valores por defecto.");
                CreateDefaultConfig();
            }
        }

        SetupAudioSource();
        SetupUI();
        ApplyConfiguration();
        FindMissingReferences();

        Debug.Log("NavigationManager: Sistema inicializado correctamente");
    }

    /// <summary>
    /// Crea configuración por defecto en memoria si no existe archivo
    /// </summary>
    void CreateDefaultConfig()
    {
        config = ScriptableObject.CreateInstance<NavigationConfig>();
        Debug.Log("NavigationManager: Creada configuración por defecto en memoria");
    }

    /// <summary>
    /// Configura el AudioSource para efectos de sonido de navegación
    /// </summary>
    void SetupAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (config != null)
        {
            audioSource.playOnAwake = false;
            audioSource.volume = config.soundVolume;
        }
    }

    /// <summary>
    /// Configura elementos de UI y eventos de botones
    /// </summary>
    void SetupUI()
    {
        // Configurar botón de cancelar navegación
        if (cancelNavigationButton != null)
        {
            cancelNavigationButton.onClick.AddListener(CancelNavigation);
        }

        // Ocultar panel de información inicialmente
        if (pathInfoPanel != null)
        {
            pathInfoPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Aplica la configuración actual a todos los subsistemas
    /// </summary>
    void ApplyConfiguration()
    {
        if (config == null)
        {
            Debug.LogWarning("NavigationManager: No hay configuración asignada");
            return;
        }

        // Aplicar configuración al PathfindingSystem
        if (PathfindingSystem.Instance != null)
        {
            PathfindingSystem.Instance.SetVisualizationMode(
                config.useLineRenderer,
                config.useParticleTrail,
                config.showBothEffects
            );

            PathfindingSystem.Instance.UpdatePathAppearance(
                config.pathColor,
                config.pathWidth,
                config.particleSpeed
            );
        }

        // Aplicar configuración al ParticleTrail
        if (ParticleTrail.Instance != null)
        {
            ParticleTrail.Instance.particleColor = config.particleColor;
            ParticleTrail.Instance.particleSpeed = config.particleSpeed;
            ParticleTrail.Instance.particleSize = config.particleSize;
            ParticleTrail.Instance.particleCount = config.particleCount;
            ParticleTrail.Instance.pathOffset = config.particleHeight;
            ParticleTrail.Instance.particleSpacing = config.particleSpacing;
            ParticleTrail.Instance.loopDelay = config.particleDelay;
            ParticleTrail.Instance.useGlow = config.useGlow;
            ParticleTrail.Instance.glowIntensity = config.glowIntensity;
            ParticleTrail.Instance.useTrail = config.useTrail;
            ParticleTrail.Instance.trailDuration = config.trailDuration;
            ParticleTrail.Instance.fadeOutTime = config.fadeOutTime;
        }

        // Aplicar configuración al NavigationUI
        if (navigationUI != null)
        {
            navigationUI.primaryButtonColor = config.primaryButtonColor;
            navigationUI.hoverButtonColor = config.hoverButtonColor;
            navigationUI.textColor = config.textColor;
        }
    }

    /// <summary>
    /// Busca automáticamente referencias faltantes en la escena
    /// </summary>
    void FindMissingReferences()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                Debug.Log("NavigationManager: Player encontrado automáticamente");
            }
        }

        if (fpsController == null && playerTransform != null)
        {
            fpsController = playerTransform.GetComponent<MonoBehaviour>();
        }

        if (navigationUI == null)
        {
            navigationUI = FindObjectOfType<NavigationUI>();
        }

        if (messageController == null)
        {
            messageController = FindObjectOfType<MessageController>();
            if (messageController != null)
            {
                Debug.Log("NavigationManager: MessageController encontrado automáticamente");
            }
        }
    }
    #endregion

    #region Input Handling
    /// <summary>
    /// Maneja la entrada del usuario para controles de navegación.
    /// Procesa teclas 'N' (abrir panel) y 'X' (cancelar navegación)
    /// </summary>
    void HandleInput()
    {
        if (config == null) return;

        // Manejo prioritario de tecla 'X' para cancelar navegación
        if (Input.GetKeyDown(config.cancelNavigationKey) && isNavigating)
        {
            Debug.Log($"🚫 Tecla {config.cancelNavigationKey} presionada - Cancelando navegación");
            CancelNavigation();
            return;
        }

        // Manejo de tecla 'N' con sistema de bloqueo durante navegación activa
        if (Input.GetKeyDown(config.navigationKey))
        {
            // Verificar si NavigationUI está bloqueado durante navegación
            if (navigationUIBlocked && config.blockNavigationDuringActive)
            {
                Debug.Log($"🔒 NavigationUI bloqueado durante navegación activa. Presiona '{config.cancelNavigationKey}' para cancelar.");
                
                if (config.showConfirmationMessages)
                {
                    Debug.Log(config.GetFormattedNavigationBlockedMessage());
                }
                
                return;
            }

            // Toggle del panel de navegación si no está bloqueado
            Debug.Log($"🔥 Tecla {config.navigationKey} presionada en NavigationManager");

            if (navigationUI != null && navigationUI.destinationPanel != null)
            {
                if (navigationUI.destinationPanel.activeSelf)
                {
                    Debug.Log("📤 Cerrando panel de navegación...");
                    navigationUI.ClosePanel();
                }
                else
                {
                    Debug.Log("📥 Abriendo panel de navegación...");
                    navigationUI.OpenNavigationPanel();
                }
            }
            else
            {
                Debug.LogError("❌ NavigationUI o destinationPanel es null!");
            }
        }
    }
    #endregion

    #region Navigation State Management
    /// <summary>
    /// Actualiza la información de navegación en tiempo real durante el recorrido
    /// </summary>
    void UpdateNavigationInfo()
    {
        if (currentDestination == null || playerTransform == null || config == null) return;

        // Calcular y mostrar distancia actual al destino
        float distance = Vector3.Distance(playerTransform.position, currentDestination.transform.position);

        if (distanceText != null)
        {
            distanceText.text = $"Distancia: {distance:F1}m";
        }

        if (destinationText != null)
        {
            destinationText.text = $"Destino: {currentDestination.GetDisplayName()}";
        }

        // Verificar si el jugador ha llegado al destino
        if (distance <= config.arrivalThreshold)
        {
            OnArrivalAtDestination();
        }
    }

    /// <summary>
    /// Verifica si es necesario recalcular la ruta automáticamente
    /// </summary>
    void CheckAutoRecalculation()
    {
        if (!config.autoRecalculatePath || PathfindingSystem.Instance == null) return;

        if (Time.time - lastPathUpdateTime >= config.pathUpdateInterval && config.pathUpdateInterval > 0)
        {
            RecalculatePathIfNeeded();
            lastPathUpdateTime = Time.time;
        }
    }

    /// <summary>
    /// Recalcula la ruta si el jugador se ha alejado demasiado del camino planificado
    /// </summary>
    void RecalculatePathIfNeeded()
    {
        if (currentDestination == null || playerTransform == null) return;

        PathInfo currentPathInfo = PathfindingSystem.Instance.GetCurrentPathInfo();
        if (currentPathInfo == null) return;

        // Verificar desviación del jugador respecto a la ruta
        float distanceToPath = CalculateDistanceToPath(currentPathInfo.pathPoints);

        if (distanceToPath > config.maxDeviationDistance)
        {
            if (config.enableDebugLogging)
            {
                Debug.Log($"NavigationManager: Recalculando ruta. Distancia a la ruta: {distanceToPath:F1}m");
            }

            StartNavigationToWaypoint(currentDestination);
        }
    }

    /// <summary>
    /// Calcula la distancia mínima del jugador a cualquier segmento de la ruta actual
    /// </summary>
    /// <param name="pathPoints">Puntos que definen la ruta</param>
    /// <returns>Distancia mínima en metros</returns>
    float CalculateDistanceToPath(Vector3[] pathPoints)
    {
        if (pathPoints == null || pathPoints.Length < 2) return float.MaxValue;

        float minDistance = float.MaxValue;
        Vector3 playerPos = playerTransform.position;

        // Evaluar distancia a cada segmento de línea de la ruta
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            Vector3 lineStart = pathPoints[i];
            Vector3 lineEnd = pathPoints[i + 1];

            Vector3 closestPoint = GetClosestPointOnLineSegment(playerPos, lineStart, lineEnd);
            float distance = Vector3.Distance(playerPos, closestPoint);

            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }

        return minDistance;
    }

    /// <summary>
    /// Encuentra el punto más cercano en un segmento de línea a una posición dada
    /// </summary>
    Vector3 GetClosestPointOnLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 lineDirection = lineEnd - lineStart;
        float lineLength = lineDirection.magnitude;
        lineDirection.Normalize();

        Vector3 pointDirection = point - lineStart;
        float projectionLength = Vector3.Dot(pointDirection, lineDirection);

        // Clamping para mantener el punto dentro del segmento
        projectionLength = Mathf.Clamp(projectionLength, 0f, lineLength);

        return lineStart + lineDirection * projectionLength;
    }
    #endregion

    #region Public Navigation API
    /// <summary>
    /// Inicia la navegación hacia un waypoint específico.
    /// Maneja el cambio de estado, bloqueo de UI y notificaciones.
    /// </summary>
    /// <param name="destination">Waypoint de destino</param>
    /// <returns>True si la navegación se inició correctamente</returns>
    public bool StartNavigationToWaypoint(Waypoint destination)
    {
        if (destination == null || playerTransform == null)
        {
            Debug.LogError("❌ NavigationManager: Destination o playerTransform es null");
            return false;
        }

        if (PathfindingSystem.Instance == null)
        {
            Debug.LogError("❌ NavigationManager: PathfindingSystem.Instance es null");
            return false;
        }

        // Cancelar navegación anterior sin limpiar partículas inmediatamente
        if (isNavigating)
        {
            Debug.Log("🔄 Cambiando a nueva navegación...");
            if (arrivalCheckCoroutine != null)
            {
                StopCoroutine(arrivalCheckCoroutine);
                arrivalCheckCoroutine = null;
            }
        }

        // Calcular nueva ruta usando PathfindingSystem
        bool success = PathfindingSystem.Instance.NavigateToWaypoint(playerTransform.position, destination);

        if (success)
        {
            // Actualizar estado de navegación
            isNavigating = true;
            currentDestination = destination;

            // Activar bloqueo de NavigationUI durante navegación
            if (config.blockNavigationDuringActive)
            {
                navigationUIBlocked = true;
                Debug.Log($"🔒 NavigationUI bloqueado. Presiona '{config.cancelNavigationKey}' para cancelar.");
            }

            // Notificar a MessageController sobre inicio de navegación
            if (messageController != null)
            {
                messageController.OnNavigationStarted();
                Debug.Log("📝 NavigationManager: MessageController notificado - navegación iniciada");
            }
            else
            {
                Debug.LogWarning("⚠️ NavigationManager: MessageController no asignado!");
            }

            // Mostrar panel de información si está configurado
            if (pathInfoPanel != null && config != null && config.showPathInfo)
            {
                pathInfoPanel.SetActive(true);
            }

            // Mostrar mensajes de confirmación
            if (config != null && config.showConfirmationMessages)
            {
                string destinationName = destination.GetDisplayName();
                Debug.Log(config.GetFormattedStartMessage(destinationName));
                Debug.Log(config.GetFormattedCancelInstructionMessage());
            }

            PlayNavigationStartSound();

            // Iniciar verificación periódica de llegada
            arrivalCheckCoroutine = StartCoroutine(CheckArrivalPeriodically());

            if (config != null && config.enableDebugLogging)
            {
                Debug.Log($"✅ NavigationManager: Navegación iniciada hacia {destination.GetDisplayName()}");
            }
        }
        else
        {
            Debug.LogError($"❌ NavigationManager: No se pudo calcular ruta hacia {destination.GetDisplayName()}");
            navigationUIBlocked = false; // No bloquear UI si falla la navegación
        }

        return success;
    }

    /// <summary>
    /// Cancela la navegación actual y limpia todos los estados relacionados
    /// </summary>
    public void CancelNavigation()
    {
        if (!isNavigating) 
        {
            Debug.Log("⚠️ NavigationManager: No hay navegación activa para cancelar");
            return;
        }

        // Limpiar estado de navegación
        isNavigating = false;
        currentDestination = null;
        navigationUIBlocked = false;
        showCancelMessage = false;

        // Notificar a MessageController sobre finalización
        if (messageController != null)
        {
            messageController.OnNavigationEnded();
            Debug.Log("📝 NavigationManager: MessageController notificado - navegación terminada");
        }
        else
        {
            Debug.LogWarning("⚠️ NavigationManager: MessageController no asignado!");
        }

        // Limpiar visualización de ruta completamente
        if (PathfindingSystem.Instance != null)
        {
            PathfindingSystem.Instance.ClearPathCompletely();
        }

        // Ocultar panel de información
        if (pathInfoPanel != null)
        {
            pathInfoPanel.SetActive(false);
        }

        // Detener corrutina de verificación
        if (arrivalCheckCoroutine != null)
        {
            StopCoroutine(arrivalCheckCoroutine);
            arrivalCheckCoroutine = null;
        }

        // Mostrar mensajes de confirmación
        if (config != null)
        {
            if (config.showConfirmationMessages)
            {
                Debug.Log(config.GetFormattedCancelMessage());
                Debug.Log(config.GetNavigationUnblockedMessage());
            }
            
            if (config.enableDebugLogging)
            {
                Debug.Log("🚫 NavigationManager: Navegación cancelada completamente");
                Debug.Log("🔓 NavigationUI desbloqueado para nueva selección");
            }
        }
    }

    /// <summary>
    /// Callback llamado cuando una partícula completa la ruta.
    /// No cancela la navegación inmediatamente, permite que el jugador también llegue.
    /// </summary>
    public void OnRouteCompleted()
    {
        if (!isNavigating || currentDestination == null) return;

        string destinationName = currentDestination.GetDisplayName();

        if (config != null && config.enableDebugLogging)
        {
            Debug.Log($"🎯 NavigationManager: ¡Ruta completada! Una partícula llegó a {destinationName}");
        }

        PlayArrivalSound();

        // Mostrar mensaje de que la ruta fue completada
        if (config != null && config.showConfirmationMessages)
        {
            Debug.Log($"✨ {config.GetFormattedArrivalMessage(destinationName)}");
        }
    }
    #endregion

    #region Arrival Handling
    /// <summary>
    /// Corrutina que verifica periódicamente si el jugador ha llegado al destino
    /// </summary>
    IEnumerator CheckArrivalPeriodically()
    {
        while (isNavigating && currentDestination != null)
        {
            if (playerTransform != null && config != null)
            {
                float distance = Vector3.Distance(playerTransform.position, currentDestination.transform.position);

                if (distance <= config.arrivalThreshold)
                {
                    OnArrivalAtDestination();
                    break;
                }
            }

            yield return new WaitForSeconds(0.5f); // Verificar cada medio segundo
        }
    }

    /// <summary>
    /// Maneja la llegada del jugador al destino final
    /// </summary>
    void OnArrivalAtDestination()
    {
        if (!isNavigating || currentDestination == null) return;

        string destinationName = currentDestination.GetDisplayName();

        if (config != null && config.enableDebugLogging)
        {
            Debug.Log($"🎯 NavigationManager: ¡Llegaste a {destinationName}!");
        }

        PlayArrivalSound();
        ShowDestinationInfo();
        
        // Desbloquear NavigationUI al llegar al destino
        navigationUIBlocked = false;
        showCancelMessage = false;
        
        // Notificar finalización a MessageController
        if (messageController != null)
        {
            messageController.OnNavigationEnded();
            Debug.Log("📝 NavigationManager: MessageController notificado - llegada al destino");
        }
        else
        {
            Debug.LogWarning("⚠️ NavigationManager: MessageController no asignado!");
        }
        
        // Mostrar mensajes de confirmación
        if (config != null && config.showConfirmationMessages)
        {
            Debug.Log(config.GetFormattedArrivalMessage(destinationName));
            Debug.Log(config.GetNavigationUnblockedMessage());
        }
        
        // Limpiar estado sin mensajes duplicados
        ClearNavigationState();
    }

    /// <summary>
    /// Limpia el estado de navegación tras llegada exitosa al destino
    /// </summary>
    void ClearNavigationState()
    {
        isNavigating = false;
        currentDestination = null;

        // Limpiar visualización de ruta
        if (PathfindingSystem.Instance != null)
        {
            PathfindingSystem.Instance.ClearPathCompletely();
        }

        // Ocultar panel de información
        if (pathInfoPanel != null)
        {
            pathInfoPanel.SetActive(false);
        }

        // Detener corrutina de verificación
        if (arrivalCheckCoroutine != null)
        {
            StopCoroutine(arrivalCheckCoroutine);
            arrivalCheckCoroutine = null;
        }

        if (config != null && config.enableDebugLogging)
        {
            Debug.Log("✅ NavigationManager: Estado de navegación limpiado tras llegada al destino");
        }
    }

    /// <summary>
    /// Muestra información detallada del destino al llegar (incluye soporte para CEE)
    /// </summary>
    void ShowDestinationInfo()
    {
        if (currentDestination == null) return;

        if (config != null && config.enableDebugLogging)
        {
            string roomTypeDisplay = currentDestination.roomType == RoomType.CEE ? "Centro de Estudiantes" : currentDestination.roomType.ToString();
            Debug.Log($"📍 Has llegado a: {currentDestination.GetDisplayName()}\n" +
                     $"🏷️ Tipo: {roomTypeDisplay}\n" +
                     $"📝 Descripción: {currentDestination.description}");
        }
    }
    #endregion

    #region Audio System
    /// <summary>
    /// Reproduce sonido de inicio de navegación si está configurado
    /// </summary>
    void PlayNavigationStartSound()
    {
        if (config == null || audioSource == null) return;

        if (config.playNavigationStartSound && config.navigationStartClip != null)
        {
            audioSource.PlayOneShot(config.navigationStartClip);
        }
    }

    /// <summary>
    /// Reproduce sonido de llegada al destino si está configurado
    /// </summary>
    void PlayArrivalSound()
    {
        if (config == null || audioSource == null) return;

        if (config.playArrivalSound && config.arrivalClip != null)
        {
            audioSource.PlayOneShot(config.arrivalClip);
        }
    }
    #endregion

    #region Configuration Management
    /// <summary>
    /// Actualiza la configuración del sistema y la aplica a todos los subsistemas
    /// </summary>
    /// <param name="newConfig">Nueva configuración a aplicar</param>
    public void UpdateConfiguration(NavigationConfig newConfig)
    {
        config = newConfig;
        ApplyConfiguration();

        if (config != null && config.enableDebugLogging)
        {
            Debug.Log("🔄 NavigationManager: Configuración actualizada");
        }
    }

    /// <summary>
    /// Cambia el modo de visualización de rutas en tiempo real
    /// </summary>
    /// <param name="useLineRenderer">Usar línea para mostrar ruta</param>
    /// <param name="useParticleTrail">Usar partículas para mostrar ruta</param>
    /// <param name="showBoth">Mostrar ambos efectos simultáneamente</param>
    public void SetVisualizationMode(bool useLineRenderer, bool useParticleTrail, bool showBoth = false)
    {
        if (PathfindingSystem.Instance != null)
        {
            PathfindingSystem.Instance.SetVisualizationMode(useLineRenderer, useParticleTrail, showBoth);
            Debug.Log($"🎨 Modo de visualización cambiado: Línea={useLineRenderer}, Partículas={useParticleTrail}, Ambos={showBoth}");
        }
    }
    #endregion

    #region Public Properties
    /// <summary>
    /// Indica si hay una navegación activa en curso
    /// </summary>
    public bool IsNavigating => isNavigating;
    
    /// <summary>
    /// Waypoint de destino actual (null si no hay navegación activa)
    /// </summary>
    public Waypoint CurrentDestination => currentDestination;
    
    /// <summary>
    /// Configuración actual del sistema
    /// </summary>
    public NavigationConfig Config => config;
    
    /// <summary>
    /// Indica si NavigationUI está bloqueado durante navegación activa
    /// </summary>
    public bool IsNavigationUIBlocked => navigationUIBlocked;
    #endregion

    #region Debug Visualization
    /// <summary>
    /// Dibuja gizmos en Scene View para depuración del sistema de navegación
    /// </summary>
    void OnDrawGizmos()
    {
        if (config == null || !config.showWaypointGizmos) return;

        // Dibujar waypoints disponibles con colores específicos por tipo (incluye CEE)
        if (WaypointManager.Instance != null && WaypointManager.Instance.allWaypoints != null)
        {
            foreach (Waypoint waypoint in WaypointManager.Instance.allWaypoints)
            {
                if (waypoint != null && waypoint.isActive && waypoint.transform != null)
                {
                    // Color específico según tipo de sala (soporte completo para CEE)
                    Gizmos.color = config.GetColorForRoomType(waypoint.roomType);
                    Gizmos.DrawWireSphere(waypoint.transform.position, 0.5f);

                    // Mostrar conexiones entre waypoints
                    if (waypoint.connectedWaypoints != null)
                    {
                        foreach (Waypoint connection in waypoint.connectedWaypoints)
                        {
                            if (connection != null && connection.transform != null)
                            {
                                Gizmos.DrawLine(waypoint.transform.position, connection.transform.position);
                            }
                        }
                    }
                }
            }
        }

        // Dibujar ruta actual si hay navegación activa
        if (isNavigating && PathfindingSystem.Instance != null)
        {
            PathInfo pathInfo = PathfindingSystem.Instance.GetCurrentPathInfo();
            if (pathInfo != null && pathInfo.pathPoints != null && pathInfo.pathPoints.Length > 1)
            {
                Gizmos.color = Color.red;
                for (int i = 0; i < pathInfo.pathPoints.Length - 1; i++)
                {
                    Gizmos.DrawLine(pathInfo.pathPoints[i], pathInfo.pathPoints[i + 1]);
                }
            }
        }

        // Dibujar posición del jugador y radio de llegada
        if (playerTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(playerTransform.position, 0.3f);

            // Radio de llegada al destino
            if (isNavigating && currentDestination != null && currentDestination.transform != null && config != null)
            {
                Gizmos.color = new Color(0, 1, 0, 0.3f);
                Gizmos.DrawWireSphere(currentDestination.transform.position, config.arrivalThreshold);
            }
        }
    }
    #endregion
}