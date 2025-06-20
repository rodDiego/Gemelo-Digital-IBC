using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Sistema central de pathfinding que utiliza NavMesh de Unity para calcular rutas.
/// Maneja tanto visualización con LineRenderer como sistema de partículas para mostrar caminos.
/// 
/// Implementa Singleton para acceso global y coordinación con NavigationManager.
/// Optimizado para el proyecto de gemelo digital universitario.
/// </summary>
public class PathfindingSystem : MonoBehaviour
{
    #region Singleton Pattern
    /// <summary>
    /// Instancia singleton para acceso global desde otros sistemas
    /// </summary>
    public static PathfindingSystem Instance { get; private set; }
    #endregion
    
    #region Inspector Configuration - Line Renderer
    [Header("Configuración de Navegación")]
    [Tooltip("LineRenderer para visualizar la ruta calculada")]
    public LineRenderer pathLineRenderer;
    
    [Tooltip("Material para la línea de ruta")]
    public Material pathMaterial;
    
    [Tooltip("Ancho de la línea de ruta")]
    public float pathWidth = 0.3f;
    
    [Tooltip("Color de la línea (#1A237E - azul universitario)")]
    public Color pathColor = new Color(0.102f, 0.137f, 0.494f, 1f);
    #endregion
    
    #region Inspector Configuration - Visualization
    [Header("Configuración Visual")]
    [Tooltip("Usar LineRenderer para mostrar ruta")]
    public bool useLineRenderer = true;
    
    [Tooltip("Usar sistema de partículas para mostrar ruta")]
    public bool useParticleTrail = true;
    
    [Tooltip("Mostrar ambos efectos simultáneamente")]
    public bool showBothEffects = false;
    #endregion
    
    #region Inspector Configuration - Particles
    [Header("Configuración de Partículas")]
    [Tooltip("Color de las partículas (#1A237E)")]
    public Color particleColor = new Color(0.102f, 0.137f, 0.494f, 1f);
    
    [Tooltip("Velocidad de movimiento de partículas")]
    public float particleSpeed = 2.0f;
    
    [Tooltip("Tamaño de las partículas")]
    public float particleSize = 0.12f;
    
    [Tooltip("Número de partículas activas")]
    public int particleCount = 15;
    #endregion
    
    #region Inspector Configuration - Enhanced Line
    [Header("Configuración de Línea Mejorada")]
    [Tooltip("Altura de la línea sobre el suelo para mejor visibilidad")]
    [Range(0.1f, 1.0f)]
    public float lineHeight = 0.3f;
    
    [Tooltip("Usar shader emisivo para línea más visible")]
    public bool useEmissiveShader = true;
    
    [Tooltip("Intensidad de emisión para mejor visibilidad")]
    [Range(0.5f, 5.0f)]
    public float lineEmissionIntensity = 2.0f;
    #endregion
    
    #region Private State
    /// <summary>
    /// Ruta calculada por NavMesh actual
    /// </summary>
    private NavMeshPath currentPath;
    
    /// <summary>
    /// Waypoint de destino actual
    /// </summary>
    private Waypoint currentDestination;
    
    /// <summary>
    /// Array de puntos que definen la ruta calculada
    /// </summary>
    private Vector3[] currentPathPoints;
    #endregion
    
    #region Unity Lifecycle
    void Awake()
    {
        // Implementar Singleton con persistencia
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ PathfindingSystem: Instancia creada");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        SetupPathRenderer();
        SetupParticleSystem();
    }

    void Update()
    {
        // Verificación periódica de integridad del sistema
        ValidateSystemIntegrity();
        
        // Mantener visibilidad de línea si está configurada
        if (useLineRenderer && pathLineRenderer != null && currentPathPoints != null && currentPathPoints.Length > 0)
        {
            if (!pathLineRenderer.enabled)
            {
                Debug.LogWarning("⚠️ PathfindingSystem: LineRenderer se deshabilitó, reactivando...");
                ForceLineVisibility();
            }
        }
    }
    #endregion
    
    #region System Setup
    /// <summary>
    /// Configura el LineRenderer con material emisivo optimizado para Unity 6
    /// </summary>
    void SetupPathRenderer()
    {
        if (pathLineRenderer == null)
        {
            pathLineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        
        // Crear material si no existe
        if (pathMaterial == null)
        {
            pathMaterial = CreateEnhancedPathMaterial();
        }
        
        // Configuración básica del LineRenderer
        pathLineRenderer.material = pathMaterial;
        pathLineRenderer.startColor = pathColor;
        pathLineRenderer.endColor = pathColor;
        pathLineRenderer.startWidth = pathWidth;
        pathLineRenderer.endWidth = pathWidth;
        pathLineRenderer.positionCount = 0;
        pathLineRenderer.useWorldSpace = true;
        pathLineRenderer.gameObject.layer = LayerMask.NameToLayer("Navigation");
        
        // Configuraciones avanzadas para mejor apariencia visual
        pathLineRenderer.numCapVertices = 8; // Extremos más suaves
        pathLineRenderer.numCornerVertices = 8; // Esquinas más suaves
        pathLineRenderer.sortingOrder = 2; // Prioridad sobre partículas
        pathLineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        pathLineRenderer.receiveShadows = false;
        
        // CRÍTICO: Configurar para visibilidad a través de objetos
        pathLineRenderer.allowOcclusionWhenDynamic = false;
        
        Debug.Log("✅ PathfindingSystem: LineRenderer configurado con máxima visibilidad");
    }
    
    /// <summary>
    /// Crea material optimizado para línea de ruta con emisión en Unity 6
    /// </summary>
    /// <returns>Material configurado para máxima visibilidad</returns>
    Material CreateEnhancedPathMaterial()
    {
        Material mat;
        
        if (useEmissiveShader)
        {
            // Material Standard con emisión para Unity 6
            mat = new Material(Shader.Find("Standard"));
            mat.SetFloat("_Mode", 2); // Fade mode para transparencia
            mat.SetOverrideTag("RenderType", "Transparent");
            
            // Configurar colores con el azul universitario #1A237E
            mat.color = new Color(pathColor.r, pathColor.g, pathColor.b, 0.8f);
            mat.SetColor("_EmissionColor", pathColor * lineEmissionIntensity);
            mat.EnableKeyword("_EMISSION");
            
            // Configuración para renderizado correcto y visibilidad máxima
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always); // Siempre visible
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;
            
            Debug.Log("🎨 Material emisivo creado para línea de navegación");
        }
        else
        {
            // Fallback para dispositivos con menos recursos
            mat = new Material(Shader.Find("Unlit/Color"));
            mat.color = pathColor;
        }
        
        return mat;
    }
    
    /// <summary>
    /// Inicializa o verifica el sistema de partículas ParticleTrail
    /// </summary>
    void SetupParticleSystem()
    {
        if (ParticleTrail.Instance == null)
        {
            // Buscar ParticleTrail existente en la escena
            ParticleTrail existingParticleTrail = FindObjectOfType<ParticleTrail>();
            
            if (existingParticleTrail == null)
            {
                // Crear nuevo ParticleTrail con configuración optimizada
                GameObject particleTrailObj = new GameObject("ParticleTrail");
                ParticleTrail particleTrail = particleTrailObj.AddComponent<ParticleTrail>();
                
                // Aplicar configuración del proyecto
                ConfigureParticleTrail(particleTrail);
                
                Debug.Log("✅ PathfindingSystem: ParticleTrail creado con configuración del proyecto");
            }
            else
            {
                Debug.Log("✅ PathfindingSystem: ParticleTrail encontrado en la escena");
            }
        }
        else
        {
            Debug.Log("✅ PathfindingSystem: ParticleTrail ya existe como instancia");
        }
    }

    /// <summary>
    /// Configura ParticleTrail con los valores optimizados para el gemelo digital
    /// </summary>
    /// <param name="particleTrail">Instancia de ParticleTrail a configurar</param>
    void ConfigureParticleTrail(ParticleTrail particleTrail)
    {
        particleTrail.particleColor = particleColor;
        particleTrail.particleSpeed = particleSpeed;
        particleTrail.particleSize = particleSize;
        particleTrail.particleCount = particleCount;
        particleTrail.pathOffset = 0.5f; // Altura elevada para visibilidad
        particleTrail.useTrail = false; // Orbes limpios sin rastro
        particleTrail.stopAtDestination = true; // Comportamiento de llegada
    }

    /// <summary>
    /// Verifica integridad de sistemas críticos
    /// </summary>
    void ValidateSystemIntegrity()
    {
        // Verificar ParticleTrail si debe estar activo
        if (useParticleTrail && ParticleTrail.Instance == null)
        {
            Debug.LogWarning("⚠️ PathfindingSystem: ParticleTrail.Instance se perdió, recreando...");
            SetupParticleSystem();
        }
    }
    #endregion
    
    #region Public Navigation API
    /// <summary>
    /// Inicia navegación hacia un waypoint específico.
    /// Calcula ruta usando NavMesh y activa visualización correspondiente.
    /// </summary>
    /// <param name="startPosition">Posición inicial del jugador</param>
    /// <param name="destination">Waypoint de destino</param>
    /// <returns>True si la ruta se calculó correctamente</returns>
    public bool NavigateToWaypoint(Vector3 startPosition, Waypoint destination)
    {
        if (destination == null)
        {
            Debug.LogError("❌ PathfindingSystem: Destination es null");
            return false;
        }
        
        currentDestination = destination;
        bool success = CalculatePath(startPosition, destination.transform.position);
        
        if (success)
        {
            Debug.Log($"✅ PathfindingSystem: Ruta calculada hacia {destination.GetDisplayName()}");
        }
        else
        {
            Debug.LogError($"❌ PathfindingSystem: No se pudo calcular ruta hacia {destination.GetDisplayName()}");
        }
        
        return success;
    }
    #endregion
    
    #region Path Calculation
    /// <summary>
    /// Calcula ruta usando NavMesh con diagnóstico completo de errores.
    /// Incluye corrección automática de posiciones y validación de superficies.
    /// </summary>
    /// <param name="startPos">Posición de inicio</param>
    /// <param name="endPos">Posición de destino</param>
    /// <returns>True si se calculó una ruta válida</returns>
    bool CalculatePath(Vector3 startPos, Vector3 endPos)
    {
        // Diagnóstico previo: Verificar que ambas posiciones estén en NavMesh
        NavMeshHit startHit, endHit;
        float maxSearchDistance = 5.0f;
        
        bool startOnNavMesh = NavMesh.SamplePosition(startPos, out startHit, maxSearchDistance, NavMesh.AllAreas);
        bool endOnNavMesh = NavMesh.SamplePosition(endPos, out endHit, maxSearchDistance, NavMesh.AllAreas);
        
        LogNavMeshDiagnostics(startPos, endPos, startOnNavMesh, endOnNavMesh, startHit, endHit);
        
        // Validar posiciones corregidas
        if (!startOnNavMesh)
        {
            Debug.LogError($"❌ Posición de inicio no está en NavMesh (radio {maxSearchDistance}m)");
            return false;
        }
        
        if (!endOnNavMesh)
        {
            Debug.LogError($"❌ Posición de destino no está en NavMesh (radio {maxSearchDistance}m)");
            Debug.LogError($"   🔧 SOLUCIÓN: Verifica que el waypoint '{currentDestination?.GetDisplayName()}' esté sobre NavMesh");
            return false;
        }
        
        // Usar posiciones corregidas para cálculo preciso
        Vector3 correctedStart = startOnNavMesh ? startHit.position : startPos;
        Vector3 correctedEnd = endOnNavMesh ? endHit.position : endPos;
        
        NavMeshPath path = new NavMeshPath();
        bool pathFound = NavMesh.CalculatePath(correctedStart, correctedEnd, NavMesh.AllAreas, path);
        
        if (pathFound)
        {
            return ProcessCalculatedPath(path);
        }
        else
        {
            LogPathCalculationError(correctedStart, correctedEnd);
            return false;
        }
    }

    /// <summary>
    /// Registra información de diagnóstico detallada sobre NavMesh
    /// </summary>
    void LogNavMeshDiagnostics(Vector3 startPos, Vector3 endPos, bool startOnNavMesh, bool endOnNavMesh, 
                              NavMeshHit startHit, NavMeshHit endHit)
    {
        Debug.Log($"🔍 DIAGNÓSTICO NavMesh:");
        Debug.Log($"   📍 Inicio ({startPos}): En NavMesh = {startOnNavMesh}");
        if (startOnNavMesh)
            Debug.Log($"   ✅ Posición corregida inicio: {startHit.position} (distancia: {Vector3.Distance(startPos, startHit.position):F2}m)");
        
        Debug.Log($"   🎯 Destino ({endPos}): En NavMesh = {endOnNavMesh}");
        if (endOnNavMesh)
            Debug.Log($"   ✅ Posición corregida destino: {endHit.position} (distancia: {Vector3.Distance(endPos, endHit.position):F2}m)");
    }

    /// <summary>
    /// Procesa la ruta calculada y determina si es válida para uso
    /// </summary>
    /// <param name="path">Ruta calculada por NavMesh</param>
    /// <returns>True si la ruta es utilizable</returns>
    bool ProcessCalculatedPath(NavMeshPath path)
    {
        switch (path.status)
        {
            case NavMeshPathStatus.PathComplete:
                Debug.Log($"✅ Ruta COMPLETA calculada con {path.corners.Length} puntos");
                break;
            case NavMeshPathStatus.PathPartial:
                Debug.LogWarning($"⚠️ Ruta PARCIAL calculada. Solo acceso parcial al destino");
                Debug.LogWarning($"   🔧 SOLUCIÓN: Verifica conexiones de NavMesh cerca del destino");
                break;
            case NavMeshPathStatus.PathInvalid:
                Debug.LogError($"❌ Ruta INVÁLIDA. No se puede calcular camino");
                Debug.LogError($"   🔧 SOLUCIÓN: Verifica que haya NavMesh conectado entre inicio y destino");
                return false;
        }
        
        // Guardar ruta y activar visualización
        currentPath = path;
        currentPathPoints = path.corners;
        
        Debug.Log($"🛤️ PathfindingSystem: Ruta procesada con {currentPathPoints.Length} puntos");
        DisplayPath();
        return true;
    }

    /// <summary>
    /// Registra error detallado cuando falla el cálculo de ruta
    /// </summary>
    void LogPathCalculationError(Vector3 correctedStart, Vector3 correctedEnd)
    {
        Debug.LogError($"❌ NavMesh.CalculatePath() falló completamente");
        Debug.LogError($"   📍 Desde: {correctedStart}");
        Debug.LogError($"   📍 Hacia: {correctedEnd}");
        Debug.LogError($"   🔧 POSIBLES SOLUCIONES:");
        Debug.LogError($"      1. Regenerar NavMesh (Window > AI > Navigation > Bake)");
        Debug.LogError($"      2. Verificar que ambas áreas estén marcadas como 'Navigation Static'");
        Debug.LogError($"      3. Verificar que el agente de NavMesh tenga el tamaño correcto");
        Debug.LogError($"      4. Verificar que no haya obstáculos bloqueando el camino");
    }
    #endregion
    
    #region Path Visualization
    /// <summary>
    /// Activa la visualización de la ruta calculada según configuración.
    /// Coordina entre LineRenderer y sistema de partículas.
    /// </summary>
    void DisplayPath()
    {
        if (currentPathPoints == null || currentPathPoints.Length < 2)
        {
            Debug.LogWarning("⚠️ PathfindingSystem: No hay puntos de ruta válidos para mostrar");
            return;
        }
        
        Debug.Log($"🎨 PathfindingSystem: Mostrando ruta - Línea: {useLineRenderer && (showBothEffects || !useParticleTrail)}, Partículas: {useParticleTrail}");
        
        // Visualización con LineRenderer
        if (useLineRenderer && (showBothEffects || !useParticleTrail))
        {
            DisplayLineRenderer();
        }
        else if (pathLineRenderer != null)
        {
            pathLineRenderer.positionCount = 0; // Ocultar línea si solo usamos partículas
        }
        
        // Visualización con sistema de partículas
        if (useParticleTrail)
        {
            DisplayParticleTrail();
        }
    }
    
    /// <summary>
    /// Configura y muestra la ruta usando LineRenderer
    /// </summary>
    void DisplayLineRenderer()
    {
        if (pathLineRenderer == null)
        {
            Debug.LogWarning("⚠️ PathfindingSystem: pathLineRenderer es null");
            return;
        }
        
        pathLineRenderer.positionCount = currentPathPoints.Length;
        
        // Posicionar puntos con altura elevada para visibilidad
        for (int i = 0; i < currentPathPoints.Length; i++)
        {
            Vector3 point = currentPathPoints[i];
            point.y += lineHeight; // Elevar sobre el suelo
            pathLineRenderer.SetPosition(i, point);
        }
        
        // Asegurar que el LineRenderer esté activo
        pathLineRenderer.enabled = true;
        
        Debug.Log($"📍 PathfindingSystem: LineRenderer configurado con {currentPathPoints.Length} puntos a altura {lineHeight}m");
    }
    
    /// <summary>
    /// Activa el sistema de partículas para mostrar la ruta
    /// </summary>
    void DisplayParticleTrail()
    {
        // Verificación de integridad de ParticleTrail
        if (ParticleTrail.Instance == null)
        {
            Debug.LogError("❌ PathfindingSystem: ParticleTrail.Instance es null! Intentando recrear...");
            SetupParticleSystem();
            
            if (ParticleTrail.Instance == null)
            {
                Debug.LogError("❌ PathfindingSystem: No se pudo crear ParticleTrail.Instance");
                return;
            }
        }
        
        // Actualizar configuración de partículas
        ParticleTrail.Instance.UpdateTrailSettings(particleColor, particleSpeed, particleSize);
        
        // Configurar comportamiento específico del proyecto
        ParticleTrail.Instance.pathOffset = 0.5f; // Altura optimizada
        ParticleTrail.Instance.useTrail = false; // Orbes limpios
        ParticleTrail.Instance.stopAtDestination = true; // Parar en destino
        
        // Iniciar visualización de partículas
        ParticleTrail.Instance.StartTrail(currentPathPoints);
        
        Debug.Log($"🎆 PathfindingSystem: ParticleTrail iniciado con {currentPathPoints.Length} puntos");
    }
    #endregion
    
    #region Path Management
    /// <summary>
    /// Limpia la visualización de ruta (mantiene partículas para transiciones suaves)
    /// </summary>
    public void ClearPath()
    {
        Debug.Log("🧹 PathfindingSystem: Limpiando ruta...");
        
        // Limpiar línea inmediatamente
        if (pathLineRenderer != null)
        {
            pathLineRenderer.positionCount = 0;
            pathLineRenderer.enabled = false;
        }
        
        // Manejo inteligente de partículas para transiciones suaves
        if (ParticleTrail.Instance != null)
        {
            // Solo detener si no hay destino activo (cancelación real)
            if (currentDestination == null)
            {
                ParticleTrail.Instance.StopTrail();
                Debug.Log("🛑 ParticleTrail detenido (cancelación real)");
            }
            else
            {
                Debug.Log("🔄 Manteniendo ParticleTrail activo para nueva ruta");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ PathfindingSystem: ParticleTrail.Instance es null al limpiar");
        }
        
        currentPath = null;
        currentPathPoints = null;
        
        Debug.Log("✅ PathfindingSystem: Ruta de línea limpiada");
    }
    
    /// <summary>
    /// Limpia completamente toda visualización de ruta (para cancelación real)
    /// </summary>
    public void ClearPathCompletely()
    {
        Debug.Log("🧹 PathfindingSystem: Limpiando ruta COMPLETAMENTE...");
        
        // Limpiar línea
        if (pathLineRenderer != null)
        {
            pathLineRenderer.positionCount = 0;
            pathLineRenderer.enabled = false;
        }
        
        // Limpiar partículas completamente
        if (ParticleTrail.Instance != null)
        {
            ParticleTrail.Instance.StopTrail();
        }
        
        currentPath = null;
        currentDestination = null;
        currentPathPoints = null;
        
        Debug.Log("✅ PathfindingSystem: Ruta limpiada completamente (incluye partículas)");
    }
    #endregion
    
    #region Runtime Configuration
    /// <summary>
    /// Cambia el modo de visualización en tiempo real
    /// </summary>
    /// <param name="lineRenderer">Activar LineRenderer</param>
    /// <param name="particleTrail">Activar sistema de partículas</param>
    /// <param name="bothEffects">Mostrar ambos efectos</param>
    public void SetVisualizationMode(bool lineRenderer, bool particleTrail, bool bothEffects = false)
    {
        useLineRenderer = lineRenderer;
        useParticleTrail = particleTrail;
        showBothEffects = bothEffects;
        
        Debug.Log($"🎛️ PathfindingSystem: Modo cambiado - Línea: {useLineRenderer}, Partículas: {useParticleTrail}, Ambos: {showBothEffects}");
        
        // Actualizar visualización si hay ruta activa
        if (currentPathPoints != null && currentPathPoints.Length > 0)
        {
            DisplayPath();
        }
    }
    
    /// <summary>
    /// Actualiza la apariencia visual de las rutas en tiempo real
    /// </summary>
    /// <param name="newColor">Nuevo color para línea y partículas</param>
    /// <param name="newWidth">Nuevo ancho de línea</param>
    /// <param name="newParticleSpeed">Nueva velocidad de partículas</param>
    public void UpdatePathAppearance(Color newColor, float newWidth, float newParticleSpeed)
    {
        // Actualizar configuración de línea
        pathColor = newColor;
        pathWidth = newWidth;
        
        if (pathLineRenderer != null)
        {
            pathLineRenderer.startColor = pathColor;
            pathLineRenderer.endColor = pathColor;
            pathLineRenderer.startWidth = pathWidth;
            pathLineRenderer.endWidth = pathWidth;
            
            // Actualizar material emisivo si está habilitado
            if (pathMaterial != null && useEmissiveShader)
            {
                pathMaterial.color = new Color(pathColor.r, pathColor.g, pathColor.b, 0.8f);
                pathMaterial.SetColor("_EmissionColor", pathColor * lineEmissionIntensity);
            }
        }
        
        // Actualizar configuración de partículas
        particleColor = newColor;
        particleSpeed = newParticleSpeed;
        
        if (ParticleTrail.Instance != null)
        {
            ParticleTrail.Instance.UpdateTrailSettings(particleColor, particleSpeed, particleSize);
        }
        
        Debug.Log($"🎨 PathfindingSystem: Apariencia actualizada - Color: #{ColorUtility.ToHtmlStringRGB(newColor)}, Velocidad: {newParticleSpeed}");
    }

    /// <summary>
    /// Fuerza la visibilidad de la línea (para casos de problemas de renderizado)
    /// </summary>
    public void ForceLineVisibility()
    {
        if (pathLineRenderer != null && currentPathPoints != null && currentPathPoints.Length > 0)
        {
            pathLineRenderer.enabled = true;
            
            // Recrear material si es necesario
            if (pathMaterial == null)
            {
                pathMaterial = CreateEnhancedPathMaterial();
                pathLineRenderer.material = pathMaterial;
            }
            
            // Re-posicionar puntos con altura adecuada
            for (int i = 0; i < currentPathPoints.Length; i++)
            {
                Vector3 point = currentPathPoints[i];
                point.y += lineHeight;
                pathLineRenderer.SetPosition(i, point);
            }
            
            Debug.Log("🔆 PathfindingSystem: Visibilidad de línea forzada");
        }
    }

    /// <summary>
    /// Recrea el sistema de partículas con nueva configuración
    /// </summary>
    public void ForceRecreateParticleSystem()
    {
        Debug.Log("🔄 PathfindingSystem: Recreando sistema de partículas...");
        
        // Limpiar partículas existentes
        if (ParticleTrail.Instance != null)
        {
            ParticleTrail.Instance.ClearAllParticles();
        }
        
        // Reconfigurar sistema
        SetupParticleSystem();
        
        // Restaurar visualización si hay ruta activa
        if (currentPathPoints != null && currentPathPoints.Length > 0)
        {
            DisplayPath();
        }
    }
    #endregion
    
    #region Public Information API
    /// <summary>
    /// Obtiene información completa de la ruta actual
    /// </summary>
    /// <returns>PathInfo con datos de la ruta o null si no hay ruta</returns>
    public PathInfo GetCurrentPathInfo()
    {
        if (currentPathPoints == null || currentDestination == null)
        {
            return null;
        }
        
        float totalDistance = CalculatePathDistance();
        
        return new PathInfo
        {
            destination = currentDestination,
            pathPoints = currentPathPoints,
            totalDistance = totalDistance,
            pointCount = currentPathPoints.Length
        };
    }
    
    /// <summary>
    /// Calcula la distancia total de la ruta actual
    /// </summary>
    /// <returns>Distancia en metros</returns>
    float CalculatePathDistance()
    {
        if (currentPathPoints == null || currentPathPoints.Length < 2) return 0f;
        
        float distance = 0f;
        for (int i = 0; i < currentPathPoints.Length - 1; i++)
        {
            distance += Vector3.Distance(currentPathPoints[i], currentPathPoints[i + 1]);
        }
        
        return distance;
    }
    
    /// <summary>
    /// Verifica si el jugador está cerca del destino actual
    /// </summary>
    /// <param name="playerPosition">Posición actual del jugador</param>
    /// <param name="threshold">Distancia umbral en metros</param>
    /// <returns>True si está dentro del umbral</returns>
    public bool IsNearDestination(Vector3 playerPosition, float threshold = 2.0f)
    {
        if (currentDestination == null) return false;
        
        float distance = Vector3.Distance(playerPosition, currentDestination.transform.position);
        return distance <= threshold;
    }
    #endregion
    
    #region Debug and Gizmos
    /// <summary>
    /// Dibuja gizmos de depuración en Scene View para visualizar rutas y waypoints
    /// </summary>
    void OnDrawGizmos()
    {
        if (currentPathPoints != null && currentPathPoints.Length > 1)
        {
            // Dibujar línea de la ruta en el editor con altura elevada
            Gizmos.color = pathColor;
            for (int i = 0; i < currentPathPoints.Length - 1; i++)
            {
                Vector3 startPoint = currentPathPoints[i];
                Vector3 endPoint = currentPathPoints[i + 1];
                startPoint.y += lineHeight;
                endPoint.y += lineHeight;
                Gizmos.DrawLine(startPoint, endPoint);
            }
            
            // Dibujar puntos individuales de la ruta
            Gizmos.color = Color.yellow;
            foreach (Vector3 point in currentPathPoints)
            {
                Vector3 adjustedPoint = point;
                adjustedPoint.y += lineHeight;
                Gizmos.DrawWireSphere(adjustedPoint, 0.3f);
            }
            
            // Destacar waypoint de destino
            if (currentDestination != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(currentDestination.transform.position, 0.5f);
            }
        }
    }
    #endregion
}

/// <summary>
/// Estructura que contiene información completa sobre una ruta calculada.
/// Utilizada para comunicación entre PathfindingSystem y otros componentes.
/// </summary>
[System.Serializable]
public class PathInfo
{
    /// <summary>
    /// Waypoint de destino de la ruta
    /// </summary>
    public Waypoint destination;
    
    /// <summary>
    /// Array de puntos que definen la ruta completa
    /// </summary>
    public Vector3[] pathPoints;
    
    /// <summary>
    /// Distancia total de la ruta en metros
    /// </summary>
    public float totalDistance;
    
    /// <summary>
    /// Número de puntos en la ruta
    /// </summary>
    public int pointCount;
}