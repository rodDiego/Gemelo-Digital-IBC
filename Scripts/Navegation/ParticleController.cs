using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    private List<Vector3> pathPoints;
    private int currentIndex;
    private float speed;
    private float fadeTime;
    private bool isFading = false;
    private bool useTrailEffect = false;
    private bool isInitialized = false;
    
    // Variables para control de llegada
    private Vector3 destinationPoint;
    private bool stopAtDestination;
    private bool hasReachedDestination = false;
    
    // Variables para control
    private bool enableDebug;
    private ParticleArrivalMode arrivalMode;
    private int totalPathPoints;
    private int targetPointIndex;
    
    private MeshRenderer meshRenderer;
    private LineRenderer lineRenderer;
    private Material materialInstance;
    private Color originalColor;
    private List<Vector3> trailPositions = new List<Vector3>();
    private int maxTrailLength = 8;
    
    void Start()
    {
        SetupComponents();
    }
    
    void SetupComponents()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        lineRenderer = GetComponent<LineRenderer>();
        
        // Crear instancia del material para evitar modificar el original
        if (meshRenderer != null && meshRenderer.material != null)
        {
            materialInstance = new Material(meshRenderer.material);
            meshRenderer.material = materialInstance;
            originalColor = materialInstance.color;
        }
    }
    
    // MÉTODO INITIALIZE SIN DEBUG
    public void Initialize(List<Vector3> points, int startIndex, float moveSpeed, float fadeOutTime, 
                          bool useTrail, Vector3 destination, bool stopAtDest, bool debug, ParticleArrivalMode mode)
    {
        if (points == null || points.Count == 0)
        {
            return;
        }
        
        pathPoints = points;
        currentIndex = startIndex % points.Count;
        speed = moveSpeed;
        fadeTime = fadeOutTime;
        useTrailEffect = useTrail;
        isInitialized = true;
        
        // Asignaciones
        destinationPoint = destination;
        stopAtDestination = stopAtDest;
        hasReachedDestination = false;
        enableDebug = debug;
        arrivalMode = mode;
        
        // Calcular índice objetivo (último punto de la ruta)
        totalPathPoints = points.Count;
        targetPointIndex = totalPathPoints - 1;
        
        // Posicionar en el punto inicial
        if (currentIndex < pathPoints.Count)
        {
            transform.position = pathPoints[currentIndex];
        }
        
        if (useTrailEffect && lineRenderer != null)
        {
            trailPositions.Add(transform.position);
            UpdateTrailRenderer();
        }
        
        StartCoroutine(MoveAlongPath());
    }
    
    // Sobrecarga simplificada para compatibilidad
    public void Initialize(List<Vector3> points, int startIndex, float moveSpeed, float fadeOutTime, bool useTrail)
    {
        Vector3 defaultDestination = points != null && points.Count > 0 ? points[points.Count - 1] : Vector3.zero;
        Initialize(points, startIndex, moveSpeed, fadeOutTime, useTrail, defaultDestination, true, false, ParticleArrivalMode.Individual);
    }
    
    IEnumerator MoveAlongPath()
    {
        if (!isInitialized || pathPoints == null || pathPoints.Count == 0)
        {
            yield break;
        }
        
        while (!isFading && isInitialized && !hasReachedDestination)
        {
            // Verificación: Si llegamos al último punto de la ruta
            if (currentIndex == targetPointIndex)
            {
                hasReachedDestination = true;
                
                // Notificar al ParticleTrail que completamos la ruta
                if (ParticleTrail.Instance != null)
                {
                    ParticleTrail.Instance.OnParticleReachedDestination(gameObject);
                }
                
                // Comportamiento según modo
                if (arrivalMode == ParticleArrivalMode.Individual)
                {
                    // Modo individual: esta partícula se desvanece inmediatamente
                    StartFadeOut();
                }
                
                yield break;
            }
            
            // Calcular siguiente índice (SIN BUCLE - detener en el último punto)
            int nextIndex = currentIndex + 1;
            
            // Si el siguiente índice excede el final, quedarse en el último punto
            if (nextIndex >= totalPathPoints)
            {
                nextIndex = targetPointIndex;
            }
            
            Vector3 startPos = pathPoints[currentIndex];
            Vector3 targetPos = pathPoints[nextIndex];
            
            float journeyLength = Vector3.Distance(startPos, targetPos);
            float journeyTime = journeyLength / speed;
            float elapsedTime = 0;
            
            // Mover hacia el siguiente punto
            while (elapsedTime < journeyTime && !isFading && isInitialized && !hasReachedDestination)
            {
                elapsedTime += Time.deltaTime;
                float fractionOfJourney = elapsedTime / journeyTime;
                transform.position = Vector3.Lerp(startPos, targetPos, fractionOfJourney);
                
                // Actualizar trail si está habilitado
                if (useTrailEffect && lineRenderer != null)
                {
                    UpdateTrail();
                }
                
                yield return null;
            }
            
            // Avanzar al siguiente punto
            currentIndex = nextIndex;
        }
        
        // Si llegó aquí sin completar la ruta, fade out
        if (!hasReachedDestination && !isFading)
        {
            StartFadeOut();
        }
    }
    
    void UpdateTrail()
    {
        trailPositions.Add(transform.position);
        
        // Limitar la longitud del trail
        if (trailPositions.Count > maxTrailLength)
        {
            trailPositions.RemoveAt(0);
        }
        
        UpdateTrailRenderer();
    }
    
    void UpdateTrailRenderer()
    {
        if (lineRenderer != null && trailPositions.Count > 1)
        {
            lineRenderer.positionCount = trailPositions.Count;
            for (int i = 0; i < trailPositions.Count; i++)
            {
                lineRenderer.SetPosition(i, trailPositions[i]);
            }
        }
    }
    
    public void StartFadeOut()
    {
        if (!isFading)
        {
            isFading = true;
            hasReachedDestination = true; // Detener movimiento
            StartCoroutine(FadeOut());
        }
    }
    
    IEnumerator FadeOut()
    {
        float elapsedTime = 0;
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = 1 - (elapsedTime / fadeTime);
            
            // Fade del mesh renderer
            if (meshRenderer != null && materialInstance != null)
            {
                Color currentColor = materialInstance.color;
                currentColor.a = alpha * originalColor.a;
                materialInstance.color = currentColor;
                
                // También fade de emission si existe
                if (materialInstance.HasProperty("_EmissionColor"))
                {
                    Color emissionColor = materialInstance.GetColor("_EmissionColor");
                    emissionColor.a = alpha;
                    materialInstance.SetColor("_EmissionColor", emissionColor);
                }
            }
            
            // Fade del line renderer
            if (lineRenderer != null)
            {
                Color startColor = lineRenderer.startColor;
                Color endColor = lineRenderer.endColor;
                startColor.a = alpha * originalColor.a;
                endColor.a = alpha * originalColor.a * 0.3f;
                lineRenderer.startColor = startColor;
                lineRenderer.endColor = endColor;
            }
            
            yield return null;
        }
        
        Destroy(gameObject);
    }
    
    void OnDestroy()
    {
        // Limpiar material instanciado
        if (materialInstance != null)
        {
            DestroyImmediate(materialInstance);
        }
    }
    
    // Información pública
    public bool HasReachedDestination => hasReachedDestination;
    public int CurrentPathIndex => currentIndex;
    public int TargetPathIndex => targetPointIndex;
}