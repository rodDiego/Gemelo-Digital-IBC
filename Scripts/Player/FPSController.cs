using UnityEngine;

/// <summary>
/// Controlador First-Person optimizado para navegación en el gemelo digital universitario.
/// Maneja movimiento, rotación de cámara, física y validación de posición del jugador.
/// 
/// Incluye sistema de validación de posición para evitar que el jugador se atasque
/// y configuración mejorada del CharacterController para Unity 6.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    #region Inspector Configuration - Movement
    [Header("=== CONFIGURACIÓN DE MOVIMIENTO ===")]
    [Tooltip("Velocidad de movimiento del jugador")]
    public float moveSpeed = 5f;
    
    [Tooltip("Sensibilidad del mouse para rotación de cámara")]
    public float mouseSensitivity = 2f;
    #endregion
    
    #region Inspector Configuration - Physics
    [Header("=== CONFIGURACIÓN DE FÍSICA ===")]
    [Tooltip("Fuerza de gravedad aplicada")]
    public float gravity = -9.81f;
    
    [Tooltip("Usar gravedad en el controlador")]
    public bool useGravity = true;
    
    [Tooltip("Distancia para detectar el suelo")]
    public float groundCheckDistance = 0.2f;
    
    [Tooltip("Capas consideradas como suelo")]
    public LayerMask groundMask = -1;
    #endregion
    
    #region Inspector Configuration - References
    [Header("=== REFERENCIAS ===")]
    [Tooltip("Transform de la cámara del jugador")]
    public Transform playerCamera;
    #endregion
    
    #region Inspector Configuration - Advanced
    [Header("=== CONFIGURACIÓN AVANZADA ===")]
    [Tooltip("Suavizado del movimiento para transiciones más fluidas")]
    [Range(0.1f, 1.0f)]
    public float movementSmoothing = 0.8f;
    
    [Tooltip("Altura mínima permitida del jugador")]
    public float minHeight = 0.5f;
    
    [Tooltip("Altura máxima permitida del jugador")]
    public float maxHeight = 10f;
    #endregion
    
    #region Private State Variables
    /// <summary>
    /// Rotación horizontal acumulada (Yaw)
    /// </summary>
    private float rotationX = 0f;
    
    /// <summary>
    /// Rotación vertical acumulada (Pitch)
    /// </summary>
    private float rotationY = 0f;
    
    /// <summary>
    /// Referencia al CharacterController configurado
    /// </summary>
    private CharacterController controller;
    
    /// <summary>
    /// Velocidad actual del jugador (incluye gravedad)
    /// </summary>
    private Vector3 velocity;
    
    /// <summary>
    /// Indica si el jugador está tocando el suelo
    /// </summary>
    private bool isGrounded;
    
    /// <summary>
    /// Última posición válida conocida para recuperación
    /// </summary>
    private Vector3 lastValidPosition;
    
    /// <summary>
    /// Movimiento suavizado para transiciones fluidas
    /// </summary>
    private Vector3 smoothMovement;
    #endregion
    
    #region Unity Lifecycle
    void Start()
    {
        InitializeController();
        SetupCamera();
        SetupCursor();
        
        // Guardar posición inicial como válida
        lastValidPosition = transform.position;
        
        Debug.Log("✅ FPSController mejorado inicializado");
    }
    
    void Update()
    {
        // Verificar que playerCamera esté asignada
        if (playerCamera == null)
        {
            Debug.LogError("❌ Player Camera no está asignada en FPSController!");
            return;
        }
        
        HandleMouseLook();
        HandleMovement();
        HandleGravity();
        ValidatePosition();
    }
    #endregion
    
    #region Controller Setup
    /// <summary>
    /// Configura el CharacterController con parámetros optimizados para Unity 6.
    /// Reduce "rebotes" y mejora la suavidad del movimiento.
    /// </summary>
    void InitializeController()
    {
        controller = GetComponent<CharacterController>();
        
        // Configuración optimizada para navegación suave
        controller.skinWidth = 0.02f;        // Reducido para menos "rebotes"
        controller.minMoveDistance = 0.0001f; // Muy pequeño para movimiento suave
        controller.radius = 0.25f;           // Radio ligeramente más pequeño
        controller.height = 1.8f;            // Altura estándar de persona
        controller.center = new Vector3(0, 0.9f, 0); // Centrado correctamente
        
        Debug.Log($"🎮 CharacterController configurado - Radius: {controller.radius}, SkinWidth: {controller.skinWidth}");
    }
    
    /// <summary>
    /// Busca y asigna la cámara automáticamente si no está configurada
    /// </summary>
    void SetupCamera()
    {
        if (playerCamera == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
            {
                playerCamera = cam.transform;
                Debug.Log("📷 Cámara asignada automáticamente: " + playerCamera.name);
            }
            else
            {
                Debug.LogError("❌ No se encontró ninguna cámara en " + gameObject.name);
            }
        }
    }
    
    /// <summary>
    /// Configura el cursor para modo FPS (bloqueado y oculto)
    /// </summary>
    void SetupCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    #endregion
    
    #region Mouse Look System
    /// <summary>
    /// Maneja la rotación de la cámara basada en movimiento del mouse.
    /// Aplica rotación horizontal al cuerpo y vertical solo a la cámara.
    /// </summary>
    void HandleMouseLook()
    {
        // Solo procesar input si el cursor está bloqueado (modo FPS activo)
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            
            // Acumular rotaciones
            rotationX += mouseX;
            rotationY -= mouseY;
            rotationY = Mathf.Clamp(rotationY, -89f, 89f); // Evitar inversión completa
            
            // Aplicar rotaciones
            transform.rotation = Quaternion.Euler(0f, rotationX, 0f); // Cuerpo: solo horizontal
            playerCamera.localRotation = Quaternion.Euler(rotationY, 0f, 0f); // Cámara: solo vertical
        }
    }
    #endregion
    
    #region Movement System
    /// <summary>
    /// Maneja el movimiento del jugador con suavizado y separación de ejes.
    /// Procesa movimiento horizontal independientemente de la gravedad.
    /// </summary>
    void HandleMovement()
    {
        // Capturar input de movimiento
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        
        // Calcular dirección de movimiento relativa al jugador
        Vector3 targetMovement = transform.right * moveX + transform.forward * moveZ;
        targetMovement = targetMovement.normalized * moveSpeed;
        
        // Aplicar suavizado para transiciones fluidas
        smoothMovement = Vector3.Lerp(smoothMovement, targetMovement, movementSmoothing);
        
        // Separar movimiento horizontal de velocidad vertical
        Vector3 horizontalMove = new Vector3(smoothMovement.x, 0, smoothMovement.z);
        Vector3 finalMovement = horizontalMove + new Vector3(0, velocity.y, 0);
        
        // Ejecutar movimiento y detectar colisiones
        CollisionFlags collisionFlags = controller.Move(finalMovement * Time.deltaTime);
        
        // Actualizar estado de suelo basado en colisiones
        isGrounded = (collisionFlags & CollisionFlags.Below) != 0;
        
        // Actualizar última posición válida si el movimiento fue exitoso
        if (collisionFlags != CollisionFlags.Sides)
        {
            lastValidPosition = transform.position;
        }
    }
    #endregion
    
    #region Gravity and Ground Detection
    /// <summary>
    /// Maneja la gravedad y detección de suelo con verificación dual.
    /// Combina detección por colisión del CharacterController y raycast adicional.
    /// </summary>
    void HandleGravity()
    {
        if (useGravity)
        {
            // Verificación adicional con raycast para mayor precisión
            bool groundedByRaycast = Physics.Raycast(transform.position, Vector3.down, 
                                                   groundCheckDistance + controller.skinWidth, groundMask);
            
            // Combinar ambas verificaciones para mayor confiabilidad
            isGrounded = isGrounded || groundedByRaycast;
            
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Pequeña fuerza hacia abajo para mantener contacto
            }
            else
            {
                velocity.y += gravity * Time.deltaTime;
            }
            
            // Limitar velocidad de caída para evitar problemas
            velocity.y = Mathf.Max(velocity.y, -20f);
        }
        else
        {
            velocity.y = 0f;
        }
    }
    #endregion
    
    #region Position Validation System
    /// <summary>
    /// Valida que el jugador esté en una posición aceptable.
    /// Restaura la última posición válida si detecta problemas.
    /// </summary>
    void ValidatePosition()
    {
        Vector3 currentPos = transform.position;
        
        // Verificar límites de altura
        if (currentPos.y < minHeight || currentPos.y > maxHeight)
        {
            Debug.LogWarning($"⚠️ Jugador fuera de límites de altura: {currentPos.y}. Restaurando posición.");
            RestoreLastValidPosition();
            return;
        }
        
        // Actualizar última posición válida si hay movimiento significativo
        if (Vector3.Distance(currentPos, lastValidPosition) > 0.01f)
        {
            lastValidPosition = currentPos;
        }
    }
    
    /// <summary>
    /// Restaura al jugador a la última posición válida conocida
    /// </summary>
    void RestoreLastValidPosition()
    {
        transform.position = lastValidPosition;
        velocity = Vector3.zero;
        Debug.Log("🔄 Posición del jugador restaurada");
    }
    #endregion
    
    #region Public Control API
    /// <summary>
    /// Habilita o deshabilita el controlador FPS.
    /// Maneja automáticamente el estado del cursor.
    /// </summary>
    /// <param name="enabled">Estado deseado del controlador</param>
    public void SetControllerEnabled(bool enabled)
    {
        this.enabled = enabled;
        
        if (!enabled)
        {
            // Desactivado: liberar cursor y detener movimiento
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            smoothMovement = Vector3.zero;
            velocity = Vector3.zero;
        }
        else
        {
            // Activado: bloquear cursor para modo FPS
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    /// <summary>
    /// Teletransporta al jugador a una posición específica de forma segura.
    /// Desactiva temporalmente el CharacterController para evitar conflictos.
    /// </summary>
    /// <param name="position">Posición de destino</param>
    public void TeleportTo(Vector3 position)
    {
        controller.enabled = false;
        transform.position = position;
        lastValidPosition = position;
        velocity = Vector3.zero;
        controller.enabled = true;
        Debug.Log($"🚀 Jugador teletransportado a: {position}");
    }
    
    /// <summary>
    /// Modifica la velocidad de movimiento en tiempo real
    /// </summary>
    /// <param name="newSpeed">Nueva velocidad (mínimo 0.1)</param>
    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = Mathf.Max(0.1f, newSpeed);
    }
    
    /// <summary>
    /// Modifica la sensibilidad del mouse en tiempo real
    /// </summary>
    /// <param name="newSensitivity">Nueva sensibilidad (mínimo 0.1)</param>
    public void SetMouseSensitivity(float newSensitivity)
    {
        mouseSensitivity = Mathf.Max(0.1f, newSensitivity);
    }
    #endregion
    
    #region Public Properties for Debug
    /// <summary>
    /// Indica si el jugador está tocando el suelo
    /// </summary>
    public bool IsGrounded => isGrounded;
    
    /// <summary>
    /// Velocidad actual del jugador (incluye componente Y de gravedad)
    /// </summary>
    public Vector3 Velocity => velocity;
    
    /// <summary>
    /// Última posición válida registrada
    /// </summary>
    public Vector3 LastValidPosition => lastValidPosition;
    
    /// <summary>
    /// Indica si el jugador se está moviendo actualmente
    /// </summary>
    public bool IsMoving => smoothMovement.magnitude > 0.1f;
    #endregion
    
    #region Debug Visualization
    /// <summary>
    /// Dibuja gizmos de depuración cuando el objeto está seleccionado.
    /// Muestra área de detección de suelo, cápsula del CharacterController y última posición válida.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (controller != null)
        {
            // Dibujar raycast de detección de suelo
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector3 rayStart = transform.position + Vector3.up * 0.1f;
            Gizmos.DrawRay(rayStart, Vector3.down * (groundCheckDistance + controller.skinWidth));
            
            // Dibujar cápsula del CharacterController
            Gizmos.color = Color.yellow;
            Vector3 center = transform.position + controller.center;
            float capsuleHeight = controller.height;
            float capsuleRadius = controller.radius;
            
            // Esferas superior e inferior de la cápsula
            Gizmos.DrawWireSphere(center + Vector3.up * (capsuleHeight/2 - capsuleRadius), capsuleRadius);
            Gizmos.DrawWireSphere(center + Vector3.down * (capsuleHeight/2 - capsuleRadius), capsuleRadius);
            
            // Dibujar última posición válida
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(lastValidPosition, Vector3.one * 0.2f);
        }
    }
    
    /// <summary>
    /// Maneja eventos de colisión del CharacterController para depuración.
    /// Dibuja rayos de debug en el punto de colisión.
    /// </summary>
    /// <param name="hit">Información de la colisión</param>
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Debug visual de colisiones para identificar problemas
        if (hit.gameObject.layer == LayerMask.NameToLayer("Building"))
        {
            // Colisión normal con edificio (rojo)
            Debug.DrawRay(hit.point, hit.normal, Color.red, 0.1f);
        }
        else
        {
            // Colisión con otro objeto (amarillo)
            Debug.DrawRay(hit.point, hit.normal, Color.yellow, 0.1f);
        }
    }
    #endregion
}