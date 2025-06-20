using UnityEngine;

/// <summary>
/// ScriptableObject que centraliza toda la configuración del sistema de navegación.
/// Permite crear presets reutilizables y ajustar el comportamiento sin tocar código.
/// 
/// Incluye soporte completo para CEE (Centro de Estudiantes) y optimizaciones
/// específicas para el proyecto de gemelo digital universitario.
/// </summary>
[CreateAssetMenu(fileName = "NavigationConfig", menuName = "Navigation/Navigation Config")]
public class NavigationConfig : ScriptableObject
{
    #region General Configuration
    [Header("=== CONFIGURACIÓN GENERAL ===")]
    [Space(5)]
    
    [Header("Modo de Visualización")]
    [Tooltip("Usar línea para mostrar la ruta")]
    public bool useLineRenderer = true;
    
    [Tooltip("Usar partículas para mostrar la ruta")]
    public bool useParticleTrail = true;
    
    [Tooltip("Mostrar tanto línea como partículas simultáneamente")]
    public bool showBothEffects = false;
    #endregion
    
    #region Line Renderer Configuration
    [Space(10)]
    [Header("=== CONFIGURACIÓN DE LÍNEA ===")]
    [Space(5)]
    
    [Tooltip("Material para la línea de la ruta")]
    public Material pathMaterial;
    
    [Tooltip("Ancho de la línea de la ruta")]
    [Range(0.1f, 1.0f)]
    public float pathWidth = 0.3f;
    
    [Tooltip("Color de la línea de la ruta (#1A237E - azul universitario)")]
    public Color pathColor = new Color(0.102f, 0.137f, 0.494f, 1f);
    
    [Tooltip("Altura sobre el suelo para la línea")]
    [Range(0.05f, 1.0f)]
    public float lineHeight = 0.3f;
    #endregion
    
    #region Particle System Configuration
    [Space(10)]
    [Header("=== CONFIGURACIÓN DE PARTÍCULAS ===")]
    [Space(5)]
    
    [Tooltip("Número de partículas activas simultáneamente")]
    [Range(5, 50)]
    public int particleCount = 20;
    
    [Tooltip("Velocidad de movimiento de las partículas")]
    [Range(0.5f, 10.0f)]
    public float particleSpeed = 2.0f;
    
    [Tooltip("Tamaño de las partículas")]
    [Range(0.05f, 0.5f)]
    public float particleSize = 0.12f;
    
    [Tooltip("Color de las partículas")]
    public Color particleColor = new Color(0.102f, 0.137f, 0.494f, 1f);
    
    [Tooltip("Altura sobre el suelo para las partículas")]
    [Range(0.1f, 2.0f)]
    public float particleHeight = 0.5f;
    
    [Tooltip("Distancia entre puntos de partículas en la ruta")]
    [Range(0.5f, 3.0f)]
    public float particleSpacing = 1.0f;
    
    [Tooltip("Tiempo entre lanzamiento de partículas")]
    [Range(0.01f, 0.5f)]
    public float particleDelay = 0.1f;
    #endregion
    
    #region Visual Effects
    [Space(10)]
    [Header("=== EFECTOS VISUALES ===")]
    [Space(5)]
    
    [Tooltip("Usar efecto de brillo en partículas")]
    public bool useGlow = true;
    
    [Tooltip("Intensidad del brillo")]
    [Range(0.5f, 5.0f)]
    public float glowIntensity = 2.5f;
    
    [Tooltip("Usar efecto de rastro en partículas (DESHABILITADO por defecto)")]
    public bool useTrail = false;
    
    [Tooltip("Duración del rastro")]
    [Range(0.1f, 2.0f)]
    public float trailDuration = 0.5f;
    
    [Tooltip("Tiempo de desvanecimiento")]
    [Range(0.5f, 3.0f)]
    public float fadeOutTime = 1.0f;
    #endregion
    
    #region UI Configuration
    [Space(10)]
    [Header("=== CONFIGURACIÓN DE UI ===")]
    [Space(5)]

    [Tooltip("Tecla para abrir/cerrar el panel de navegación")]
    public KeyCode navigationKey = KeyCode.N;

    [Tooltip("Tecla para cancelar la navegación actual")]
    public KeyCode cancelNavigationKey = KeyCode.X;

    [Tooltip("Bloquear apertura del panel durante navegación activa")]
    public bool blockNavigationDuringActive = true;

    [Tooltip("Color primario de los botones")]
    public Color primaryButtonColor = new Color(0.31f, 0.76f, 0.97f, 1f);
    
    [Tooltip("Color de hover de los botones")]
    public Color hoverButtonColor = new Color(0.16f, 0.71f, 0.96f, 1f);
    
    [Tooltip("Color del texto")]
    public Color textColor = Color.white;
    #endregion
    
    #region Navigation Behavior
    [Space(10)]
    [Header("=== CONFIGURACIÓN DE NAVEGACIÓN ===")]
    [Space(5)]
    
    [Tooltip("Distancia mínima para considerar que se llegó al destino")]
    [Range(1.0f, 5.0f)]
    public float arrivalThreshold = 2.0f;
    
    [Tooltip("Recalcular ruta automáticamente si el jugador se aleja mucho")]
    public bool autoRecalculatePath = true;
    
    [Tooltip("Distancia máxima del jugador a la ruta antes de recalcular")]
    [Range(2.0f, 10.0f)]
    public float maxDeviationDistance = 5.0f;
    #endregion
    
    #region Audio Configuration
    [Space(10)]
    [Header("=== CONFIGURACIÓN DE AUDIO ===")]
    [Space(5)]
    
    [Tooltip("Reproducir sonido al iniciar navegación")]
    public bool playNavigationStartSound = true;
    
    [Tooltip("Reproducir sonido al llegar al destino")]
    public bool playArrivalSound = true;
    
    [Tooltip("Clip de audio para inicio de navegación")]
    public AudioClip navigationStartClip;
    
    [Tooltip("Clip de audio para llegada")]
    public AudioClip arrivalClip;
    
    [Tooltip("Volumen de los efectos de sonido")]
    [Range(0.0f, 1.0f)]
    public float soundVolume = 0.5f;
    #endregion
    
    #region Performance Settings
    [Space(10)]
    [Header("=== RENDIMIENTO ===")]
    [Space(5)]
    
    [Tooltip("Actualizar ruta cada X segundos (0 = sin actualización automática)")]
    [Range(0.0f, 10.0f)]
    public float pathUpdateInterval = 0.0f;
    
    [Tooltip("Número máximo de partículas simultáneas por rendimiento")]
    [Range(10, 100)]
    public int maxParticlesForPerformance = 50;
    
    [Tooltip("Usar LOD para partículas (menos partículas cuando está lejos)")]
    public bool useParticleLOD = true;
    
    [Tooltip("Distancia para reducir calidad de partículas")]
    [Range(10.0f, 50.0f)]
    public float particleLODDistance = 25.0f;
    #endregion
    
    #region Debug Settings
    [Space(10)]
    [Header("=== DEPURACIÓN ===")]
    [Space(5)]
    
    [Tooltip("Mostrar información de debug en consola")]
    public bool enableDebugLogging = true;
    
    [Tooltip("Mostrar gizmos de waypoints en Scene View")]
    public bool showWaypointGizmos = true;
    
    [Tooltip("Mostrar información de ruta en pantalla")]
    public bool showPathInfo = false;
    
    [Tooltip("Color de los gizmos de waypoints")]
    public Color waypointGizmoColor = Color.yellow;
    #endregion
    
    #region Room Type Colors (Including CEE)
    [Space(15)]
    [Header("=== CONFIGURACIONES AVANZADAS ===")]
    [Space(5)]
    
    [Header("Colores por Tipo de Sala")]
    [Tooltip("Color para aulas")]
    public Color aulaColor = new Color(0.2f, 1f, 0.2f, 1f);
    
    [Tooltip("Color para laboratorios")]
    public Color laboratorioColor = new Color(1f, 0.8f, 0.2f, 1f);
    
    [Tooltip("Color para oficinas")]
    public Color oficinaColor = new Color(0.8f, 0.8f, 1f, 1f);
    
    [Tooltip("Color para salas de reuniones")]
    public Color salaReunionesColor = new Color(1f, 0.6f, 1f, 1f);
    
    [Tooltip("Color para biblioteca")]
    public Color bibliotecaColor = new Color(0.6f, 0.8f, 1f, 1f);
    
    [Tooltip("Color para auditorio")]
    public Color auditorioColor = new Color(1f, 0.4f, 0.4f, 1f);
    
    [Tooltip("Color para Centro de Estudiantes (CEE)")]
    public Color ceeColor = new Color(0.8f, 0.8f, 1f, 1f);
    #endregion
    
    #region Room Type Icons (Including CEE)
    [Space(10)]
    [Header("Personalización de Iconos")]
    [Tooltip("Iconos personalizados para cada tipo de sala")]
    public string aulaIcon = "A•";
    public string laboratorioIcon = "L•";
    public string oficinaIcon = "O•";
    public string salaReunionesIcon = "R•";
    public string bibliotecaIcon = "B•";
    public string auditorioIcon = "T•";
    public string ceeIcon = "CEE•"; // Icono específico para Centro de Estudiantes
    public string otroIcon = "?•";
    #endregion
    
    #region Animation Settings
    [Space(10)]
    [Header("=== CONFIGURACIÓN DE ANIMACIONES ===")]
    [Space(5)]
    
    [Tooltip("Duración de la animación de aparición del panel")]
    [Range(0.1f, 1.0f)]
    public float panelAnimationDuration = 0.3f;
    
    [Tooltip("Tipo de curva para animaciones")]
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Tooltip("Escala inicial del panel al aparecer")]
    [Range(0.1f, 1.0f)]
    public float panelStartScale = 0.8f;
    #endregion
    
    #region Message Configuration
    [Space(10)]
    [Header("=== CONFIGURACIÓN DE MENSAJES ===")]
    [Space(5)]
    
    [Tooltip("Mostrar mensajes de confirmación")]
    public bool showConfirmationMessages = true;
    
    [Tooltip("Duración de los mensajes temporales")]
    [Range(1.0f, 10.0f)]
    public float messageDuration = 3.0f;
    
    [TextArea(3, 5)]
    [Tooltip("Mensaje al iniciar navegación")]
    public string startNavigationMessage = "🧭 Navegando hacia {destination}...";
    
    [TextArea(3, 5)]
    [Tooltip("Mensaje al llegar al destino")]
    public string arrivalMessage = "🎯 ¡Has llegado a {destination}!";
    
    [TextArea(3, 5)]
    [Tooltip("Mensaje al cancelar navegación")]
    public string cancelNavigationMessage = "❌ Navegación cancelada";

    [TextArea(3, 5)]
    [Tooltip("Mensaje informativo sobre cancelación disponible")]
    public string cancelInstructionMessage = "💡 Presiona '{cancelKey}' para cancelar el recorrido actual";

    [TextArea(3, 5)]
    [Tooltip("Mensaje cuando NavigationUI está bloqueado")]
    public string navigationBlockedMessage = "🔒 Navegación en curso. Presiona '{cancelKey}' para cancelar primero.";
    #endregion
    
    #region Experimental Features
    [Space(10)]
    [Header("=== CONFIGURACIÓN EXPERIMENTAL ===")]
    [Space(5)]
    
    [Tooltip("Usar físicas avanzadas para partículas")]
    public bool useAdvancedParticlePhysics = false;
    
    [Tooltip("Gravedad aplicada a las partículas")]
    [Range(-10.0f, 10.0f)]
    public float particleGravity = 0.0f;
    
    [Tooltip("Usar colisiones de partículas con el entorno")]
    public bool useParticleCollisions = false;
    
    [Tooltip("Crear partículas reactivas al movimiento del jugador")]
    public bool useReactiveParticles = false;
    
    [Tooltip("Radio de influencia del jugador sobre las partículas")]
    [Range(1.0f, 10.0f)]
    public float playerInfluenceRadius = 3.0f;
    #endregion
    
    #region Unity 6 Compatibility
    [Space(10)]
    [Header("=== CONFIGURACIÓN DE UNITY 6 ===")]
    [Space(5)]
    
    [Tooltip("Usar compatibilidad específica para Unity 6")]
    public bool useUnity6Compatibility = true;
    
    [Tooltip("Optimizar para dispositivos móviles")]
    public bool optimizeForMobile = false;
    
    [Tooltip("Usar nuevas características de Unity 6")]
    public bool useUnity6Features = true;
    #endregion
    
    #region Public API Methods
    /// <summary>
    /// Obtiene el color configurado para un tipo específico de sala.
    /// Incluye soporte completo para CEE (Centro de Estudiantes).
    /// </summary>
    /// <param name="roomType">Tipo de sala</param>
    /// <returns>Color asignado al tipo de sala</returns>
    public Color GetColorForRoomType(RoomType roomType)
    {
        switch (roomType)
        {
            case RoomType.Aula: return aulaColor;
            case RoomType.Laboratorio: return laboratorioColor;
            case RoomType.Oficina: return oficinaColor;
            case RoomType.Sala_Reuniones: return salaReunionesColor;
            case RoomType.Biblioteca: return bibliotecaColor;
            case RoomType.Auditorio: return auditorioColor;
            case RoomType.CEE: return ceeColor; // Soporte para Centro de Estudiantes
            default: return Color.white;
        }
    }
    
    /// <summary>
    /// Obtiene el icono configurado para un tipo específico de sala.
    /// Incluye soporte completo para CEE (Centro de Estudiantes).
    /// </summary>
    /// <param name="roomType">Tipo de sala</param>
    /// <returns>String del icono asignado</returns>
    public string GetIconForRoomType(RoomType roomType)
    {
        switch (roomType)
        {
            case RoomType.Aula: return aulaIcon;
            case RoomType.Laboratorio: return laboratorioIcon;
            case RoomType.Oficina: return oficinaIcon;
            case RoomType.Sala_Reuniones: return salaReunionesIcon;
            case RoomType.Biblioteca: return bibliotecaIcon;
            case RoomType.Auditorio: return auditorioIcon;
            case RoomType.CEE: return ceeIcon; // Soporte para Centro de Estudiantes
            default: return otroIcon;
        }
    }
    
    /// <summary>
    /// Formatea el mensaje de inicio de navegación con el nombre del destino
    /// </summary>
    /// <param name="destinationName">Nombre del destino</param>
    /// <returns>Mensaje formateado</returns>
    public string GetFormattedStartMessage(string destinationName)
    {
        if (string.IsNullOrEmpty(destinationName))
            return startNavigationMessage.Replace("{destination}", "destino desconocido");
            
        return startNavigationMessage.Replace("{destination}", destinationName);
    }
    
    /// <summary>
    /// Formatea el mensaje de llegada con el nombre del destino
    /// </summary>
    /// <param name="destinationName">Nombre del destino</param>
    /// <returns>Mensaje formateado</returns>
    public string GetFormattedArrivalMessage(string destinationName)
    {
        if (string.IsNullOrEmpty(destinationName))
            return arrivalMessage.Replace("{destination}", "destino");
            
        return arrivalMessage.Replace("{destination}", destinationName);
    }
    
    /// <summary>
    /// Obtiene el mensaje de cancelación formateado
    /// </summary>
    /// <returns>Mensaje de cancelación</returns>
    public string GetFormattedCancelMessage()
    {
        return cancelNavigationMessage;
    }

    /// <summary>
    /// Formatea el mensaje de instrucción de cancelación con la tecla configurada
    /// </summary>
    /// <returns>Mensaje con tecla de cancelación</returns>
    public string GetFormattedCancelInstructionMessage()
    {
        if (string.IsNullOrEmpty(cancelInstructionMessage))
            return $"💡 Presiona '{cancelNavigationKey}' para cancelar el recorrido actual";
            
        return cancelInstructionMessage.Replace("{cancelKey}", cancelNavigationKey.ToString());
    }

    /// <summary>
    /// Formatea el mensaje de NavigationUI bloqueado con la tecla configurada
    /// </summary>
    /// <returns>Mensaje de UI bloqueado</returns>
    public string GetFormattedNavigationBlockedMessage()
    {
        if (string.IsNullOrEmpty(navigationBlockedMessage))
            return $"🔒 Navegación en curso. Presiona '{cancelNavigationKey}' para cancelar primero.";
            
        return navigationBlockedMessage.Replace("{cancelKey}", cancelNavigationKey.ToString());
    }

    /// <summary>
    /// Obtiene el mensaje de desbloqueo de NavigationUI
    /// </summary>
    /// <returns>Mensaje de desbloqueo</returns>
    public string GetNavigationUnblockedMessage()
    {
        return $"🔓 NavigationUI desbloqueado. Presiona '{navigationKey}' para seleccionar nuevo destino.";
    }
    
    /// <summary>
    /// Verifica si la configuración actual es válida y completa
    /// </summary>
    /// <returns>True si la configuración es válida</returns>
    public bool IsConfigurationValid()
    {
        // Verificar que al menos un modo de visualización esté activo
        if (!useLineRenderer && !useParticleTrail)
            return false;

        // Verificar rangos válidos
        if (particleCount <= 0 || particleSpeed <= 0 || arrivalThreshold <= 0)
            return false;

        // Verificar que los mensajes no estén vacíos
        if (string.IsNullOrEmpty(startNavigationMessage) ||
            string.IsNullOrEmpty(arrivalMessage) ||
            string.IsNullOrEmpty(cancelNavigationMessage))
            return false;

        return true;
    }
    #endregion
    
    #region Platform Optimization
    /// <summary>
    /// Aplica optimizaciones automáticas según la plataforma actual
    /// </summary>
    public void OptimizeForCurrentPlatform()
    {
        #if UNITY_ANDROID || UNITY_IOS
        ApplyMobileOptimizations();
        #elif UNITY_WEBGL
        ApplyWebGLOptimizations();
        #else
        ApplyDesktopOptimizations();
        #endif
    }
    
    /// <summary>
    /// Aplica configuración optimizada para dispositivos móviles
    /// </summary>
    void ApplyMobileOptimizations()
    {
        optimizeForMobile = true;
        particleCount = Mathf.Min(particleCount, 15);
        useGlow = false;
        useTrail = false;
        useAdvancedParticlePhysics = false;
        maxParticlesForPerformance = 25;
        useParticleLOD = true;
        
        Debug.Log("✅ NavigationConfig: Optimizaciones móviles aplicadas");
    }
    
    /// <summary>
    /// Aplica configuración optimizada para WebGL
    /// </summary>
    void ApplyWebGLOptimizations()
    {
        particleCount = Mathf.Min(particleCount, 10);
        useGlow = false;
        useAdvancedParticlePhysics = false;
        maxParticlesForPerformance = 20;
        pathUpdateInterval = Mathf.Max(pathUpdateInterval, 1.0f);
        
        Debug.Log("✅ NavigationConfig: Optimizaciones WebGL aplicadas");
    }
    
    /// <summary>
    /// Aplica configuración optimizada para escritorio
    /// </summary>
    void ApplyDesktopOptimizations()
    {
        // Mantener configuración completa para desktop
        if (particleCount < 20) particleCount = 20;
        maxParticlesForPerformance = 60;
        
        Debug.Log("✅ NavigationConfig: Configuración desktop optimizada");
    }
    #endregion
    
    #region Configuration Presets
    /// <summary>
    /// Aplica preset optimizado para máximo rendimiento
    /// </summary>
    [ContextMenu("🚀 Aplicar Preset de Rendimiento")]
    public void ApplyPerformancePreset()
    {
        useLineRenderer = true;
        useParticleTrail = false; // Solo línea para máximo rendimiento
        showBothEffects = false;
        
        particleCount = 10;
        particleSpeed = 1.5f;
        particleSize = 0.08f;
        useGlow = false;
        useTrail = false;
        useParticleLOD = true;
        maxParticlesForPerformance = 20;
        pathUpdateInterval = 2.0f;
        useAdvancedParticlePhysics = false;
        useParticleCollisions = false;
        useReactiveParticles = false;
        enableDebugLogging = false; // Reducir logs para rendimiento
        
        Debug.Log("🚀 NavigationConfig: Preset de rendimiento aplicado");
    }
    
    /// <summary>
    /// Aplica preset optimizado para máxima calidad visual
    /// </summary>
    [ContextMenu("✨ Aplicar Preset de Calidad")]
    public void ApplyQualityPreset()
    {
        useLineRenderer = true;
        useParticleTrail = true;
        showBothEffects = true; // Ambos efectos para máxima calidad
        
        particleCount = 30;
        particleSpeed = 2.5f;
        particleSize = 0.15f;
        useGlow = true;
        useTrail = false; // Orbes limpios sin rastro
        glowIntensity = 3.0f;
        trailDuration = 0.8f;
        useParticleLOD = false;
        maxParticlesForPerformance = 60;
        pathUpdateInterval = 0.5f;
        useAdvancedParticlePhysics = true;
        enableDebugLogging = true;
        
        Debug.Log("✨ NavigationConfig: Preset de calidad aplicado");
    }
    
    /// <summary>
    /// Aplica preset balanceado entre rendimiento y calidad
    /// </summary>
    [ContextMenu("⚖️ Aplicar Preset Balanceado")]
    public void ApplyBalancedPreset()
    {
        useLineRenderer = true;
        useParticleTrail = true;
        showBothEffects = false; // Alternar entre efectos
        
        particleCount = 20;
        particleSpeed = 2.0f;
        particleSize = 0.12f;
        useGlow = true;
        useTrail = false;
        glowIntensity = 2.5f;
        trailDuration = 0.5f;
        useParticleLOD = true;
        maxParticlesForPerformance = 40;
        pathUpdateInterval = 1.0f;
        useAdvancedParticlePhysics = false;
        enableDebugLogging = true;
        
        Debug.Log("⚖️ NavigationConfig: Preset balanceado aplicado");
    }
    
    /// <summary>
    /// Aplica preset específico para dispositivos móviles
    /// </summary>
    [ContextMenu("📱 Aplicar Preset Móvil")]
    public void ApplyMobilePreset()
    {
        ApplyPerformancePreset(); // Base de rendimiento
        
        optimizeForMobile = true;
        useUnity6Compatibility = true;
        particleCount = 8;
        useGlow = false;
        useTrail = false;
        maxParticlesForPerformance = 15;
        
        // UI optimizada para touch
        primaryButtonColor = new Color(0.31f, 0.76f, 0.97f, 0.9f);
        panelAnimationDuration = 0.2f; // Animaciones más rápidas
        
        Debug.Log("📱 NavigationConfig: Preset móvil aplicado");
    }
    #endregion
    
    #region Default Configuration Reset
    /// <summary>
    /// Restaura toda la configuración a los valores por defecto del proyecto.
    /// Incluye soporte completo para CEE y optimizaciones para Unity 6.
    /// </summary>
    [ContextMenu("🔄 Resetear a Configuración por Defecto")]
    public void ResetToDefault()
    {
        // Configuración general
        useLineRenderer = true;
        useParticleTrail = true;
        showBothEffects = false;
        
        // Configuración de línea con color universitario #1A237E
        pathWidth = 0.3f;
        pathColor = new Color(0.102f, 0.137f, 0.494f, 1f);
        lineHeight = 0.3f;
        
        // Configuración de partículas optimizada
        particleCount = 20;
        particleSpeed = 2.0f;
        particleSize = 0.12f;
        particleColor = new Color(0.102f, 0.137f, 0.494f, 1f);
        particleHeight = 0.5f;
        particleSpacing = 1.0f;
        particleDelay = 0.1f;
        
        // Efectos visuales (sin rastro por defecto)
        useGlow = true;
        glowIntensity = 2.5f;
        useTrail = false;
        trailDuration = 0.5f;
        fadeOutTime = 1.0f;
        
        // Configuración de UI con nuevas teclas
        navigationKey = KeyCode.N;
        cancelNavigationKey = KeyCode.X;
        blockNavigationDuringActive = true;
        primaryButtonColor = new Color(0.31f, 0.76f, 0.97f, 1f);
        hoverButtonColor = new Color(0.16f, 0.71f, 0.96f, 1f);
        textColor = Color.white;
        
        // Configuración de navegación
        arrivalThreshold = 2.0f;
        autoRecalculatePath = true;
        maxDeviationDistance = 5.0f;
        
        // Configuración de audio
        playNavigationStartSound = true;
        playArrivalSound = true;
        soundVolume = 0.5f;
        
        // Configuración de rendimiento
        pathUpdateInterval = 0.0f;
        maxParticlesForPerformance = 50;
        useParticleLOD = true;
        particleLODDistance = 25.0f;
        
        // Configuración de debug
        enableDebugLogging = true;
        showWaypointGizmos = true;
        showPathInfo = false;
        waypointGizmoColor = Color.yellow;
        
        // Colores por tipo de sala (INCLUYE CEE)
        aulaColor = new Color(0.2f, 1f, 0.2f, 1f);
        laboratorioColor = new Color(1f, 0.8f, 0.2f, 1f);
        oficinaColor = new Color(0.8f, 0.8f, 1f, 1f);
        salaReunionesColor = new Color(1f, 0.6f, 1f, 1f);
        bibliotecaColor = new Color(0.6f, 0.8f, 1f, 1f);
        auditorioColor = new Color(1f, 0.4f, 0.4f, 1f);
        ceeColor = new Color(0.8f, 0.8f, 1f, 1f); // Color específico para CEE
        
        // Iconos por tipo de sala (INCLUYE CEE)
        aulaIcon = "A•";
        laboratorioIcon = "L•";
        oficinaIcon = "O•";
        salaReunionesIcon = "R•";
        bibliotecaIcon = "B•";
        auditorioIcon = "T•";
        ceeIcon = "CEE•"; // Icono específico para Centro de Estudiantes
        otroIcon = "?•";
        
        // Configuración de animaciones
        panelAnimationDuration = 0.3f;
        animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        panelStartScale = 0.8f;
        
        // Mensajes con emojis y soporte para nuevas teclas
        showConfirmationMessages = true;
        messageDuration = 3.0f;
        startNavigationMessage = "🧭 Navegando hacia {destination}...";
        arrivalMessage = "🎯 ¡Has llegado a {destination}!";
        cancelNavigationMessage = "❌ Navegación cancelada";
        cancelInstructionMessage = "💡 Presiona '{cancelKey}' para cancelar el recorrido actual";
        navigationBlockedMessage = "🔒 Navegación en curso. Presiona '{cancelKey}' para cancelar primero.";
        
        // Configuración experimental
        useAdvancedParticlePhysics = false;
        particleGravity = 0.0f;
        useParticleCollisions = false;
        useReactiveParticles = false;
        playerInfluenceRadius = 3.0f;
        
        // Configuración Unity 6
        useUnity6Compatibility = true;
        optimizeForMobile = false;
        useUnity6Features = true;
        
        Debug.Log("🔄 NavigationConfig: Configuración restaurada con soporte completo para CEE y Unity 6");
    }
    #endregion
    
    #region Validation and OnValidate
    /// <summary>
    /// Validación automática cuando se modifica el ScriptableObject en el Inspector.
    /// Asegura valores válidos y configuración consistente.
    /// </summary>
    void OnValidate()
    {
        // Validar rangos de valores críticos
        particleCount = Mathf.Clamp(particleCount, 5, maxParticlesForPerformance);
        pathWidth = Mathf.Max(0.1f, pathWidth);
        particleSize = Mathf.Max(0.05f, particleSize);
        particleSpeed = Mathf.Max(0.1f, particleSpeed);
        arrivalThreshold = Mathf.Max(0.5f, arrivalThreshold);
        
        // Validar lógica de efectos visuales
        if (showBothEffects)
        {
            useLineRenderer = true;
            useParticleTrail = true;
        }
        
        // Asegurar que al menos un efecto visual esté habilitado
        if (!useLineRenderer && !useParticleTrail)
        {
            useLineRenderer = true;
            Debug.LogWarning("⚠️ NavigationConfig: Al menos un efecto visual debe estar habilitado. Se habilitó LineRenderer.");
        }
        
        // Validar configuración de audio
        ValidateAudioConfiguration();
        
        // Validar configuración de LOD
        if (useParticleLOD && particleLODDistance <= 0)
        {
            particleLODDistance = 25.0f;
            Debug.LogWarning("⚠️ NavigationConfig: Distancia de LOD debe ser mayor que 0. Se estableció en 25.");
        }
        
        // Validar y corregir mensajes
        ValidateMessages();
        
        // Validar curva de animación
        if (animationCurve == null || animationCurve.keys.Length == 0)
        {
            animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }
        
        // Validar colores (no transparentes)
        ValidateColors();
    }

    /// <summary>
    /// Valida la configuración de audio y muestra advertencias si es necesario
    /// </summary>
    void ValidateAudioConfiguration()
    {
        if (playNavigationStartSound && navigationStartClip == null)
        {
            Debug.LogWarning("⚠️ NavigationConfig: Sonido de inicio habilitado pero no hay clip asignado.");
        }
        
        if (playArrivalSound && arrivalClip == null)
        {
            Debug.LogWarning("⚠️ NavigationConfig: Sonido de llegada habilitado pero no hay clip asignado.");
        }
    }
    
    /// <summary>
    /// Valida y corrige los mensajes del sistema, incluyendo placeholders
    /// </summary>
    void ValidateMessages()
    {
        // Validar existencia de mensajes básicos
        if (string.IsNullOrEmpty(startNavigationMessage))
        {
            startNavigationMessage = "🧭 Navegando hacia {destination}...";
        }
        
        if (string.IsNullOrEmpty(arrivalMessage))
        {
            arrivalMessage = "🎯 ¡Has llegado a {destination}!";
        }
        
        if (string.IsNullOrEmpty(cancelNavigationMessage))
        {
            cancelNavigationMessage = "❌ Navegación cancelada";
        }

        // Validar nuevos mensajes de cancelación
        if (string.IsNullOrEmpty(cancelInstructionMessage))
        {
            cancelInstructionMessage = "💡 Presiona '{cancelKey}' para cancelar el recorrido actual";
        }

        if (string.IsNullOrEmpty(navigationBlockedMessage))
        {
            navigationBlockedMessage = "🔒 Navegación en curso. Presiona '{cancelKey}' para cancelar primero.";
        }
        
        // Asegurar que los mensajes contengan placeholders correctos
        ValidatePlaceholders();
    }

    /// <summary>
    /// Valida que los mensajes contengan los placeholders necesarios
    /// </summary>
    void ValidatePlaceholders()
    {
        // Validar placeholder {destination}
        if (!startNavigationMessage.Contains("{destination}"))
        {
            startNavigationMessage += " - {destination}";
        }
        
        if (!arrivalMessage.Contains("{destination}"))
        {
            arrivalMessage = arrivalMessage.Replace("!", " a {destination}!");
        }

        // Validar placeholder {cancelKey}
        if (!cancelInstructionMessage.Contains("{cancelKey}"))
        {
            cancelInstructionMessage = "💡 Presiona '{cancelKey}' para cancelar el recorrido actual";
        }

        if (!navigationBlockedMessage.Contains("{cancelKey}"))
        {
            navigationBlockedMessage = "🔒 Navegación en curso. Presiona '{cancelKey}' para cancelar primero.";
        }
    }
    
    /// <summary>
    /// Valida que los colores no sean completamente transparentes
    /// </summary>
    void ValidateColors()
    {
        // Colores principales del sistema
        if (pathColor.a < 0.1f) pathColor.a = 1.0f;
        if (particleColor.a < 0.1f) particleColor.a = 1.0f;
        if (primaryButtonColor.a < 0.1f) primaryButtonColor.a = 1.0f;
        if (hoverButtonColor.a < 0.1f) hoverButtonColor.a = 1.0f;
        if (textColor.a < 0.1f) textColor.a = 1.0f;
        
        // Colores específicos por tipo de sala (incluye CEE)
        if (aulaColor.a < 0.1f) aulaColor.a = 1.0f;
        if (laboratorioColor.a < 0.1f) laboratorioColor.a = 1.0f;
        if (oficinaColor.a < 0.1f) oficinaColor.a = 1.0f;
        if (salaReunionesColor.a < 0.1f) salaReunionesColor.a = 1.0f;
        if (bibliotecaColor.a < 0.1f) bibliotecaColor.a = 1.0f;
        if (auditorioColor.a < 0.1f) auditorioColor.a = 1.0f;
        if (ceeColor.a < 0.1f) ceeColor.a = 1.0f; // Validar CEE también
    }
    #endregion
}