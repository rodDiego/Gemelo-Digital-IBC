using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ENUM MOVIDO FUERA PARA ACCESIBILIDAD GLOBAL
public enum ParticleArrivalMode
{
    Individual, // Cada partícula se borra al llegar
    AllAtOnce   // Todas se borran cuando una llega
}

public class ParticleTrail : MonoBehaviour
{
    public static ParticleTrail Instance { get; private set; }
    
    [Header("=== CONFIGURACIÓN DE PARTÍCULAS ===")]
    [Tooltip("Material para las partículas")]
    public Material particleMaterial; // USAR ESTE EN EL INSPECTOR
    
    [Tooltip("Shader de respaldo si no hay material asignado")]
    public Shader fallbackShader; // NUEVO: Asignar en el Inspector
    
    [Tooltip("Número de partículas activas simultáneamente")]
    [Range(5, 50)]
    public int particleCount = 15;
    
    [Tooltip("Velocidad de movimiento de las partículas")]
    [Range(0.5f, 10.0f)]
    public float particleSpeed = 2.0f;
    
    [Tooltip("Tamaño de las partículas")]
    [Range(0.05f, 0.5f)]
    public float particleSize = 0.12f;
    
    [Tooltip("Color de las partículas (#1A237E)")]
    public Color particleColor = new Color(0.102f, 0.137f, 0.494f, 1f); // #1A237E
    
    [Header("=== CONFIGURACIÓN DE MOVIMIENTO ===")]
    [Tooltip("Altura sobre el suelo")]
    [Range(0.1f, 2.0f)]
    public float pathOffset = 0.5f;
    
    [Tooltip("Distancia entre puntos de partículas")]
    [Range(0.5f, 3.0f)]
    public float particleSpacing = 1.0f;
    
    [Tooltip("Tiempo entre lanzamiento de partículas")]
    [Range(0.01f, 0.5f)]
    public float loopDelay = 0.1f;
    
    [Header("=== EFECTOS VISUALES ===")]
    [Tooltip("Usar efecto de brillo")]
    public bool useGlow = true;
    
    [Tooltip("Intensidad del brillo")]
    [Range(0.5f, 5.0f)]
    public float glowIntensity = 2.5f;
    
    [Tooltip("Usar efecto de rastro (DESHABILITADO - solo orbes)")]
    public bool useTrail = false;
    
    [Tooltip("Duración del rastro")]
    [Range(0.1f, 2.0f)]
    public float trailDuration = 0.5f;
    
    [Tooltip("Tiempo de desvanecimiento")]
    [Range(0.5f, 3.0f)]
    public float fadeOutTime = 1.0f;
    
    [Header("=== CONFIGURACIÓN DE LLEGADA ===")]
    [Tooltip("Detener partículas al llegar al destino")]
    public bool stopAtDestination = true;
    
    [Tooltip("Comportamiento al llegar: Individual o Todas")]
    public ParticleArrivalMode arrivalMode = ParticleArrivalMode.Individual;
    
    // Variables internas
    private List<GameObject> activeParticles = new List<GameObject>();
    private Vector3[] pathPoints;
    private bool isTrailActive = false;
    private Coroutine trailCoroutine;
    private GameObject particlePrefab;
    private List<Vector3> pathSegments;
    private Vector3 destinationPoint;
    private bool routeCompleted = false;
    private int particlesReachedDestination = 0;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        InitializeParticleSystem();
    }
    
    void InitializeParticleSystem()
    {
        LoadParticlePrefab();
    }
    
    void LoadParticlePrefab()
    {
        // CARGAR PREFAB PRE-CREADO DESDE RESOURCES
        particlePrefab = Resources.Load<GameObject>("NavigationParticlePrefab");
        
        if (particlePrefab == null)
        {
            // FALLBACK: Crear dinámicamente si no existe el prefab
            CreateParticlePrefab();
        }
    }
    
    void CreateParticlePrefab()
    {
        // MÉTODO DE EMERGENCIA: Solo se usa si no existe el prefab pre-creado
        particlePrefab = new GameObject("NavigationParticle_Emergency");
        
        // Agregar MeshRenderer y MeshFilter
        MeshRenderer meshRenderer = particlePrefab.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = particlePrefab.AddComponent<MeshFilter>();
        
        // Usar una esfera primitiva simple
        meshFilter.mesh = GetSphereMesh();
        
        // Crear material de emergencia más simple
        Material emergencyMaterial = new Material(Shader.Find("Legacy Shaders/Diffuse"));
        emergencyMaterial.color = particleColor;
        meshRenderer.material = emergencyMaterial;
        
        // Configurar escala correcta
        particlePrefab.transform.localScale = Vector3.one * particleSize;
        
        // Agregar componente de movimiento
        ParticleController controller = particlePrefab.AddComponent<ParticleController>();
        
        // Desactivar prefab
        particlePrefab.SetActive(false);
    }
    
    Mesh GetSphereMesh()
    {
        // Crear una esfera de baja resolución para mejor rendimiento
        GameObject tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Mesh sphereMesh = Instantiate(tempSphere.GetComponent<MeshFilter>().mesh);
        
        // Limpiar el objeto temporal inmediatamente
        if (Application.isPlaying)
        {
            DestroyImmediate(tempSphere);
        }
        else
        {
            DestroyImmediate(tempSphere);
        }
        
        return sphereMesh;
    }
    
    // MÉTODO CORREGIDO PARA UNITY 6 + MONO
    Material CreateOptimizedMaterial()
    {
        Material mat = null;
        
        // PRIMERA OPCIÓN: Intentar usar Standard shader (más compatible)
        Shader standardShader = Shader.Find("Standard");
        if (standardShader != null)
        {
            mat = new Material(standardShader);
            
            if (useGlow)
            {
                // Configuración con glow usando Standard shader
                mat.color = new Color(particleColor.r, particleColor.g, particleColor.b, 0.9f);
                
                // Configurar emisión si está disponible
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.SetColor("_EmissionColor", particleColor * glowIntensity);
                    mat.EnableKeyword("_EMISSION");
                }
                
                // Configurar para transparencia
                mat.SetFloat("_Mode", 2); // Fade mode
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = 3000;
            }
            else
            {
                // Sin glow, solo color
                mat.color = particleColor;
            }
            
            return mat;
        }
        
        // SEGUNDA OPCIÓN: Unlit/Color (más básico pero siempre funciona)
        Shader unlitShader = Shader.Find("Unlit/Color");
        if (unlitShader != null)
        {
            mat = new Material(unlitShader);
            mat.color = particleColor;
            return mat;
        }
        
        // TERCERA OPCIÓN: Diffuse (legacy pero confiable)
        Shader diffuseShader = Shader.Find("Diffuse");
        if (diffuseShader != null)
        {
            mat = new Material(diffuseShader);
            mat.color = particleColor;
            return mat;
        }
        
        // OPCIÓN DE EMERGENCIA: Crear material básico
        try
        {
            // Intentar con cualquier shader disponible
            mat = new Material(Shader.Find("Standard") ?? Shader.Find("Diffuse") ?? Shader.Find("Unlit/Color"));
            if (mat != null)
            {
                mat.color = particleColor;
            }
        }
        catch
        {
            // Si todo falla, al menos crear el objeto
            mat = new Material(Shader.Find("Standard"));
            mat.color = particleColor;
        }
        
        return mat;
    }
    
    // NUEVO MÉTODO: Crear material con shaders Legacy más compatibles
    Material CreateLegacyMaterial()
    {
        Material mat = null;
        
        // Intentar con Legacy Diffuse (más compatible en builds)
        Shader legacyDiffuse = Shader.Find("Legacy Shaders/Diffuse");
        if (legacyDiffuse != null)
        {
            mat = new Material(legacyDiffuse);
            mat.color = particleColor;
            return mat;
        }
        
        // Fallback a Mobile/Diffuse
        Shader mobileDiffuse = Shader.Find("Mobile/Diffuse");
        if (mobileDiffuse != null)
        {
            mat = new Material(mobileDiffuse);
            mat.color = particleColor;
            return mat;
        }
        
        // Fallback final a Unlit/Color
        Shader unlitColor = Shader.Find("Unlit/Color");
        if (unlitColor != null)
        {
            mat = new Material(unlitColor);
            mat.color = particleColor;
            return mat;
        }
        
        // Último recurso: Standard
        mat = new Material(Shader.Find("Standard"));
        mat.color = particleColor;
        return mat;
    }
    
    void SetupTrailEffect(GameObject particle)
    {
        // MÉTODO MANTENIDO PERO NO USADO POR DEFECTO
        LineRenderer lineRenderer = particle.AddComponent<LineRenderer>();
        
        // Configurar LineRenderer para el trail
        lineRenderer.material = particleMaterial;
        lineRenderer.startWidth = particleSize * 0.3f;
        lineRenderer.endWidth = particleSize * 0.1f;
        lineRenderer.startColor = particleColor;
        lineRenderer.endColor = new Color(particleColor.r, particleColor.g, particleColor.b, 0);
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
        lineRenderer.sortingOrder = 1;
        
        // Configuraciones adicionales para mejor apariencia
        lineRenderer.numCapVertices = 3;
        lineRenderer.numCornerVertices = 3;
    }
    
    // ========================================
    // MÉTODOS PÚBLICOS PRINCIPALES
    // ========================================
    
    public void StartTrail(Vector3[] points)
    {
        if (points == null || points.Length < 2)
        {
            return;
        }
        
        // Detener trail anterior si existe
        StopTrail();
        
        // Resetear flag de ruta completada y contador
        routeCompleted = false;
        particlesReachedDestination = 0;
        
        // Ajustar altura de los puntos
        pathPoints = AdjustPathHeight(points);
        
        // Guardar punto de destino final
        destinationPoint = pathPoints[pathPoints.Length - 1];
        
        // Generar segmentos de ruta ANTES de marcar como activo
        pathSegments = GeneratePathSegments(pathPoints, particleSpacing);
        
        // Marcar como activo DESPUÉS de preparar todo
        isTrailActive = true;
        
        // Iniciar corrutina de creación de partículas
        trailCoroutine = StartCoroutine(CreateTrailCoroutine());
    }
    
    public void StopTrail()
    {
        isTrailActive = false;
        
        if (trailCoroutine != null)
        {
            StopCoroutine(trailCoroutine);
            trailCoroutine = null;
        }
        
        // Fade out de partículas existentes en lugar de dejarlas en bucle
        StartCoroutine(FadeOutAllParticles());
    }
    
    public void ClearAllParticles()
    {
        StopTrail();
        
        // Destruir todas las partículas inmediatamente
        foreach (GameObject particle in activeParticles)
        {
            if (particle != null)
            {
                Destroy(particle);
            }
        }
        
        activeParticles.Clear();
    }
    
    public void UpdateTrailSettings(Color newColor, float newSpeed, float newSize)
    {
        particleColor = newColor;
        particleSpeed = newSpeed;
        particleSize = newSize;
        
        // Actualizar material si existe
        if (particleMaterial != null)
        {
            particleMaterial.color = particleColor;
            if (useGlow && particleMaterial.HasProperty("_EmissionColor"))
            {
                particleMaterial.SetColor("_EmissionColor", particleColor * glowIntensity);
            }
        }
        
        // Recrear prefab con nuevas configuraciones
        if (particlePrefab != null)
        {
            DestroyImmediate(particlePrefab);
            LoadParticlePrefab();
        }
    }
    
    // ========================================
    // MÉTODOS INTERNOS
    // ========================================
    
    Vector3[] AdjustPathHeight(Vector3[] originalPoints)
    {
        Vector3[] adjustedPoints = new Vector3[originalPoints.Length];
        
        for (int i = 0; i < originalPoints.Length; i++)
        {
            adjustedPoints[i] = originalPoints[i];
            adjustedPoints[i].y += pathOffset;
        }
        
        return adjustedPoints;
    }
    
    IEnumerator CreateTrailCoroutine()
    {
        if (pathSegments == null || pathSegments.Count < 2)
        {
            yield break;
        }
        
        int particleIndex = 0;
        int consecutiveFailures = 0;
        
        // Bucle principal - Solo se detiene si se cancela manualmente
        while (isTrailActive)
        {
            bool particleCreated = false;
            
            // Crear hasta el número máximo de partículas configurado
            for (int i = 0; i < particleCount && isTrailActive; i++)
            {
                if (pathSegments.Count > 0)
                {
                    int startIndex = particleIndex % pathSegments.Count;
                    bool success = CreateSingleParticle(pathSegments, startIndex);
                    
                    if (success)
                    {
                        particleCreated = true;
                        consecutiveFailures = 0;
                    }
                    else
                    {
                        consecutiveFailures++;
                    }
                    
                    particleIndex++;
                }
                
                yield return new WaitForSeconds(loopDelay);
            }
            
            if (!particleCreated)
            {
                consecutiveFailures++;
                if (consecutiveFailures > 10)
                {
                    break;
                }
            }
            
            // Pausa entre ciclos
            yield return new WaitForSeconds(0.2f);
            
            // Limpiar partículas viejas periódicamente
            CleanupOldParticles();
        }
    }
    
    List<Vector3> GeneratePathSegments(Vector3[] points, float spacing)
    {
        List<Vector3> segments = new List<Vector3>();
        
        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector3 startPoint = points[i];
            Vector3 endPoint = points[i + 1];
            float distance = Vector3.Distance(startPoint, endPoint);
            
            int segmentCount = Mathf.Max(1, Mathf.RoundToInt(distance / spacing));
            
            for (int j = 0; j <= segmentCount; j++)
            {
                float t = (float)j / segmentCount;
                Vector3 segmentPoint = Vector3.Lerp(startPoint, endPoint, t);
                segments.Add(segmentPoint);
            }
        }
        
        return segments;
    }
    
    bool CreateSingleParticle(List<Vector3> pathSegments, int startIndex)
    {
        if (particlePrefab == null)
        {
            return false;
        }
        
        try
        {
            GameObject particle = Instantiate(particlePrefab);
            particle.SetActive(true);
            particle.transform.position = pathSegments[startIndex];
            
            // Verificar escala de la partícula
            particle.transform.localScale = Vector3.one * particleSize;

            particle.layer = LayerMask.NameToLayer("Effects");
            
            // Configurar el controlador de la partícula
            ParticleController controller = particle.GetComponent<ParticleController>();
            if (controller != null)
            {
                controller.Initialize(pathSegments, startIndex, particleSpeed, fadeOutTime, 
                                    useTrail, destinationPoint, stopAtDestination, false, arrivalMode);
                activeParticles.Add(particle);
                
                return true;
            }
            else
            {
                Destroy(particle);
                return false;
            }
        }
        catch (System.Exception)
        {
            return false;
        }
    }
    
    void CleanupOldParticles()
    {
        // Remover referencias nulas
        activeParticles.RemoveAll(particle => particle == null);
        
        // Limitar número de partículas para rendimiento
        int maxParticles = particleCount * 2;
        while (activeParticles.Count > maxParticles)
        {
            if (activeParticles[0] != null)
            {
                Destroy(activeParticles[0]);
            }
            activeParticles.RemoveAt(0);
        }
    }
    
    IEnumerator FadeOutAllParticles()
    {
        List<GameObject> particlesToFade = new List<GameObject>(activeParticles);
        
        // Iniciar fade out en todas las partículas
        foreach (GameObject particle in particlesToFade)
        {
            if (particle != null)
            {
                ParticleController controller = particle.GetComponent<ParticleController>();
                if (controller != null)
                {
                    controller.StartFadeOut();
                }
            }
        }
        
        // Esperar a que termine el fade out
        yield return new WaitForSeconds(fadeOutTime + 0.5f);
        
        // Limpiar cualquier partícula restante
        foreach (GameObject particle in particlesToFade)
        {
            if (particle != null)
            {
                Destroy(particle);
            }
        }
        
        activeParticles.Clear();
    }
    
    // MÉTODO ACTUALIZADO: Para notificar que una partícula llegó al destino
    public void OnParticleReachedDestination(GameObject particle)
    {
        particlesReachedDestination++;
        
        // Comportamiento según el modo configurado
        if (arrivalMode == ParticleArrivalMode.Individual)
        {
            // MODO INDIVIDUAL: Solo la partícula que llegó se desvanece
            // La partícula se desvanecerá por sí misma, solo removerla de la lista
            if (activeParticles.Contains(particle))
            {
                activeParticles.Remove(particle);
            }
            
            // Notificar llegada pero sin detener el sistema
            if (NavigationManager.Instance != null && particlesReachedDestination == 1)
            {
                NavigationManager.Instance.OnRouteCompleted();
            }
        }
        else
        {
            // MODO TODAS A LA VEZ: Comportamiento original
            if (routeCompleted) return; // Ya se procesó
            
            routeCompleted = true;
            
            // Detener la creación de nuevas partículas
            isTrailActive = false;
            
            if (trailCoroutine != null)
            {
                StopCoroutine(trailCoroutine);
                trailCoroutine = null;
            }
            
            // Notificar al NavigationManager que se completó la ruta
            if (NavigationManager.Instance != null)
            {
                NavigationManager.Instance.OnRouteCompleted();
            }
            
            // Iniciar fade out de todas las partículas
            StartCoroutine(FadeOutAllParticles());
        }
    }
    
    // ========================================
    // MÉTODOS DE DEBUG (SOLO GIZMOS)
    // ========================================
    
    void OnDrawGizmos()
    {
        if (pathPoints != null && pathPoints.Length > 1)
        {
            Gizmos.color = particleColor;
            
            for (int i = 0; i < pathPoints.Length - 1; i++)
            {
                Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
            }
            
            // Dibujar puntos de la ruta
            foreach (Vector3 point in pathPoints)
            {
                Gizmos.DrawWireSphere(point, particleSize);
            }
            
            // Dibujar punto de destino final
            if (stopAtDestination)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(destinationPoint, 0.3f);
            }
        }
    }
    
    // ========================================
    // INFORMACIÓN PÚBLICA
    // ========================================
    
    public bool IsTrailActive => isTrailActive;
    public int ActiveParticleCount => activeParticles.Count;
    public int PathSegmentCount => pathSegments != null ? pathSegments.Count : 0;
    public Vector3 DestinationPoint => destinationPoint;
    public bool IsRouteCompleted => routeCompleted;
    public int ParticlesReachedDestination => particlesReachedDestination;
}

// *** NO HAY MÁS CÓDIGO DESPUÉS DE ESTA LÍNEA ***
// *** LA CLASE ParticleController ESTÁ EN SU PROPIO ARCHIVO ***