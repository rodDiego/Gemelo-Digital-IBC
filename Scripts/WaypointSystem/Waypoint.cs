using UnityEngine;

/// <summary>
/// Enum que define los tipos de salas disponibles en el gemelo digital universitario.
/// Incluye soporte completo para CEE (Centro de Estudiantes) agregado en el proyecto.
/// </summary>
[System.Serializable]
public enum RoomType
{
    Aula,
    Laboratorio,
    Oficina,
    Sala_Reuniones,
    Biblioteca,
    Auditorio,
    CEE,  // Centro de Estudiantes - Agregado para el proyecto universitario
    Otro
}

/// <summary>
/// Representa un punto de navegación en el gemelo digital del edificio Isabel Brown Caces.
/// Cada waypoint define una sala o área específica con información completa para navegación.
/// 
/// Se registra automáticamente en WaypointManager al iniciarse y provee información
/// visual en Scene View para facilitar el diseño del sistema de navegación.
/// </summary>
public class Waypoint : MonoBehaviour
{
    #region Room Information
    [Header("Información de la Sala")]
    [Tooltip("Nombre descriptivo de la sala")]
    public string roomName = "Sala Sin Nombre";
    
    [Tooltip("Código identificador de la sala (ej: '301', 'Lab-A', 'CEE')")]
    public string roomCode = "000";
    
    [Tooltip("Tipo de sala para clasificación y visualización")]
    public RoomType roomType = RoomType.Aula;
   
    [TextArea(3, 5)]
    [Tooltip("Descripción detallada de la sala y su función")]
    public string description = "Descripción de la sala";
    #endregion
   
    #region Configuration
    [Header("Configuración")]
    [Tooltip("Indica si este waypoint está activo y disponible para navegación")]
    public bool isActive = true;
    
    [Tooltip("Color para visualización en Scene View")]
    public Color waypointColor = Color.blue;
    #endregion
   
    #region Navigation Connections
    [Header("Conexiones")]
    [Tooltip("Waypoints conectados directamente (para algoritmos de pathfinding avanzados)")]
    public Waypoint[] connectedWaypoints;
    #endregion
   
    #region Unity Lifecycle
    void Start()
    {
        RegisterWaypoint();
    }
    #endregion
    
    #region Registration System
    /// <summary>
    /// Registra automáticamente este waypoint en el WaypointManager global.
    /// Se ejecuta al iniciar para asegurar que todos los waypoints estén disponibles.
    /// </summary>
    void RegisterWaypoint()
    {
        if (WaypointManager.Instance != null)
        {
            WaypointManager.Instance.RegisterWaypoint(this);
        }
        else
        {
            // Si no hay WaypointManager, intentar encontrarlo en la escena
            WaypointManager manager = FindObjectOfType<WaypointManager>();
            if (manager != null)
            {
                manager.RegisterWaypoint(this);
            }
            else
            {
                Debug.LogWarning($"No se encontró WaypointManager para registrar {GetDisplayName()}");
            }
        }
    }
    #endregion
   
    #region Debug Visualization
    /// <summary>
    /// Dibuja gizmos en Scene View para visualización y diseño del sistema.
    /// Muestra el waypoint como esfera y las conexiones como líneas.
    /// </summary>
    void OnDrawGizmos()
    {
        if (isActive)
        {
            // Dibujar waypoint principal
            Gizmos.color = waypointColor;
            Gizmos.DrawWireSphere(transform.position, 1f);
           
            // Dibujar conexiones a otros waypoints
            if (connectedWaypoints != null)
            {
                Gizmos.color = Color.yellow;
                foreach (var waypoint in connectedWaypoints)
                {
                    if (waypoint != null && waypoint.transform != null)
                    {
                        Gizmos.DrawLine(transform.position, waypoint.transform.position);
                    }
                }
            }
            
            // Dibujar etiqueta con información en Scene View
            DrawSceneLabel();
        }
    }
    
    /// <summary>
    /// Dibuja etiqueta informativa en Scene View para identificación rápida
    /// </summary>
    void DrawSceneLabel()
    {
        #if UNITY_EDITOR
        Vector3 labelPosition = transform.position + Vector3.up * 1.5f;
        UnityEditor.Handles.Label(labelPosition, GetDisplayName());
        #endif
    }
    #endregion
   
    #region Public API
    /// <summary>
    /// Obtiene el nombre completo para mostrar en UI y logging.
    /// Combina código y nombre para identificación clara.
    /// </summary>
    /// <returns>String formateado como "CÓDIGO - NOMBRE"</returns>
    public string GetDisplayName()
    {
        return $"{roomCode} - {roomName}";
    }
    
    /// <summary>
    /// Obtiene información completa del waypoint para debugging o UI avanzada
    /// </summary>
    /// <returns>String con toda la información del waypoint</returns>
    public string GetFullInfo()
    {
        string typeDisplay = roomType == RoomType.CEE ? "Centro de Estudiantes" : roomType.ToString();
        return $"{GetDisplayName()}\nTipo: {typeDisplay}\nDescripción: {description}\nActivo: {isActive}";
    }
    
    /// <summary>
    /// Verifica si este waypoint está conectado directamente a otro
    /// </summary>
    /// <param name="otherWaypoint">Waypoint a verificar</param>
    /// <returns>True si están conectados</returns>
    public bool IsConnectedTo(Waypoint otherWaypoint)
    {
        if (connectedWaypoints == null || otherWaypoint == null) return false;
        
        foreach (var waypoint in connectedWaypoints)
        {
            if (waypoint == otherWaypoint) return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Agrega una conexión bidireccional entre este waypoint y otro
    /// </summary>
    /// <param name="otherWaypoint">Waypoint a conectar</param>
    public void AddConnection(Waypoint otherWaypoint)
    {
        if (otherWaypoint == null || otherWaypoint == this) return;
        
        // Agregar conexión en este waypoint
        if (!IsConnectedTo(otherWaypoint))
        {
            System.Array.Resize(ref connectedWaypoints, connectedWaypoints.Length + 1);
            connectedWaypoints[connectedWaypoints.Length - 1] = otherWaypoint;
        }
        
        // Agregar conexión en el otro waypoint (bidireccional)
        if (!otherWaypoint.IsConnectedTo(this))
        {
            otherWaypoint.AddConnection(this);
        }
    }
    
    /// <summary>
    /// Remueve la conexión con otro waypoint
    /// </summary>
    /// <param name="otherWaypoint">Waypoint a desconectar</param>
    public void RemoveConnection(Waypoint otherWaypoint)
    {
        if (connectedWaypoints == null || otherWaypoint == null) return;
        
        var newConnections = new System.Collections.Generic.List<Waypoint>();
        foreach (var waypoint in connectedWaypoints)
        {
            if (waypoint != otherWaypoint)
            {
                newConnections.Add(waypoint);
            }
        }
        
        connectedWaypoints = newConnections.ToArray();
    }
    
    /// <summary>
    /// Obtiene la distancia directa a otro waypoint
    /// </summary>
    /// <param name="otherWaypoint">Waypoint destino</param>
    /// <returns>Distancia en metros o -1 si el waypoint es inválido</returns>
    public float GetDistanceTo(Waypoint otherWaypoint)
    {
        if (otherWaypoint == null || otherWaypoint.transform == null) return -1f;
        
        return Vector3.Distance(transform.position, otherWaypoint.transform.position);
    }
    
    /// <summary>
    /// Valida que el waypoint esté configurado correctamente
    /// </summary>
    /// <returns>True si la configuración es válida</returns>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(roomName) || string.IsNullOrEmpty(roomCode))
        {
            Debug.LogWarning($"Waypoint en {transform.position} tiene información incompleta");
            return false;
        }
        
        if (transform.position == Vector3.zero)
        {
            Debug.LogWarning($"Waypoint {GetDisplayName()} está en posición (0,0,0)");
            return false;
        }
        
        return true;
    }
    #endregion
    
    #region Context Menu Utilities
    /// <summary>
    /// Utilidad de editor para validar waypoint desde el menú contextual
    /// </summary>
    [ContextMenu("Validar Waypoint")]
    void ValidateWaypoint()
    {
        if (IsValid())
        {
            Debug.Log($"✅ Waypoint {GetDisplayName()} es válido");
        }
        else
        {
            Debug.LogError($"❌ Waypoint {GetDisplayName()} tiene problemas de configuración");
        }
    }
    
    /// <summary>
    /// Utilidad para mostrar información completa en consola
    /// </summary>
    [ContextMenu("Mostrar Información Completa")]
    void ShowFullInfo()
    {
        Debug.Log($"📍 INFORMACIÓN COMPLETA:\n{GetFullInfo()}");
        
        if (connectedWaypoints != null && connectedWaypoints.Length > 0)
        {
            Debug.Log($"🔗 CONEXIONES ({connectedWaypoints.Length}):");
            foreach (var waypoint in connectedWaypoints)
            {
                if (waypoint != null)
                {
                    Debug.Log($"   → {waypoint.GetDisplayName()}");
                }
            }
        }
        else
        {
            Debug.Log("🔗 Sin conexiones directas");
        }
    }
    
    /// <summary>
    /// Utilidad para registrar manualmente en WaypointManager
    /// </summary>
    [ContextMenu("Registrar en WaypointManager")]
    void ForceRegister()
    {
        RegisterWaypoint();
        Debug.Log($"🔄 Waypoint {GetDisplayName()} registrado manualmente");
    }
    #endregion
}