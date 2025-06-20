using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestor centralizado de todos los waypoints en el gemelo digital.
/// Mantiene registro de todas las salas disponibles y provee métodos de búsqueda
/// optimizados para el sistema de navegación.
/// 
/// Implementa Singleton con persistencia entre escenas para acceso global
/// desde NavigationUI, PathfindingSystem y otros componentes del sistema.
/// </summary>
public class WaypointManager : MonoBehaviour
{
    #region Singleton Pattern
    /// <summary>
    /// Instancia singleton para acceso global desde cualquier sistema
    /// </summary>
    public static WaypointManager Instance { get; private set; }
    #endregion
    
    #region Waypoint Registry
    [Header("Sistema de Waypoints")]
    [Tooltip("Lista maestra de todos los waypoints disponibles en el edificio")]
    public List<Waypoint> allWaypoints = new List<Waypoint>();
    #endregion
    
    #region Unity Lifecycle
    void Awake()
    {
        InitializeSingleton();
    }

    void Start()
    {
        ValidateWaypoints();
        LogSystemStatus();
    }
    #endregion
    
    #region Singleton Initialization
    /// <summary>
    /// Inicializa el patrón Singleton con persistencia entre escenas.
    /// Asegura que solo exista una instancia del WaypointManager.
    /// </summary>
    void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("WaypointManager creado y marcado como DontDestroyOnLoad");
        }
        else if (Instance != this)
        {
            Debug.Log("WaypointManager duplicado encontrado - destruyendo esta instancia");
            Destroy(gameObject);
        }
    }
    #endregion
    
    #region Waypoint Registration
    /// <summary>
    /// Registra un waypoint en el sistema central.
    /// Llamado automáticamente por cada Waypoint al inicializarse.
    /// </summary>
    /// <param name="waypoint">Waypoint a registrar</param>
    public void RegisterWaypoint(Waypoint waypoint)
    {
        if (waypoint == null)
        {
            Debug.LogWarning("WaypointManager: Intento de registrar waypoint null");
            return;
        }

        if (!allWaypoints.Contains(waypoint))
        {
            allWaypoints.Add(waypoint);
            Debug.Log($"Waypoint registrado: {waypoint.GetDisplayName()}");
        }
        else
        {
            Debug.LogWarning($"Waypoint {waypoint.GetDisplayName()} ya estaba registrado");
        }
    }
    
    /// <summary>
    /// Remueve un waypoint del sistema central.
    /// Útil cuando se destruyen waypoints dinámicamente.
    /// </summary>
    /// <param name="waypoint">Waypoint a remover</param>
    public void UnregisterWaypoint(Waypoint waypoint)
    {
        if (waypoint == null) return;

        if (allWaypoints.Contains(waypoint))
        {
            allWaypoints.Remove(waypoint);
            Debug.Log($"Waypoint removido: {waypoint.GetDisplayName()}");
        }
        else
        {
            Debug.LogWarning($"Intento de remover waypoint no registrado: {waypoint.GetDisplayName()}");
        }
    }
    #endregion
    
    #region Search Methods
    /// <summary>
    /// Busca un waypoint por nombre de sala (case-insensitive).
    /// </summary>
    /// <param name="roomName">Nombre de la sala a buscar</param>
    /// <returns>Waypoint encontrado o null si no existe</returns>
    public Waypoint FindWaypointByName(string roomName)
    {
        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogWarning("WaypointManager: Búsqueda con nombre vacío");
            return null;
        }

        return allWaypoints.Find(w => w != null && 
                                 w.roomName.Equals(roomName, System.StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// Busca un waypoint por código de sala (case-insensitive).
    /// Útil para navegación directa usando códigos como "301", "Lab-A", "CEE".
    /// </summary>
    /// <param name="roomCode">Código de la sala a buscar</param>
    /// <returns>Waypoint encontrado o null si no existe</returns>
    public Waypoint FindWaypointByCode(string roomCode)
    {
        if (string.IsNullOrEmpty(roomCode))
        {
            Debug.LogWarning("WaypointManager: Búsqueda con código vacío");
            return null;
        }

        return allWaypoints.Find(w => w != null && 
                                 w.roomCode.Equals(roomCode, System.StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// Obtiene todos los waypoints de un tipo específico de sala.
    /// Incluye soporte completo para CEE (Centro de Estudiantes).
    /// </summary>
    /// <param name="roomType">Tipo de sala a filtrar</param>
    /// <returns>Lista de waypoints del tipo especificado que estén activos</returns>
    public List<Waypoint> GetWaypointsByType(RoomType roomType)
    {
        return allWaypoints.FindAll(w => w != null && w.roomType == roomType && w.isActive);
    }

    /// <summary>
    /// Busca waypoints por texto parcial en nombre o código.
    /// Útil para sistemas de búsqueda con autocompletado.
    /// </summary>
    /// <param name="searchText">Texto a buscar</param>
    /// <returns>Lista de waypoints que coinciden con la búsqueda</returns>
    public List<Waypoint> SearchWaypoints(string searchText)
    {
        if (string.IsNullOrEmpty(searchText))
        {
            return GetActiveWaypoints(); // Retornar todos si no hay texto
        }

        string searchLower = searchText.ToLower();
        return allWaypoints.FindAll(w => w != null && w.isActive &&
                                   (w.roomName.ToLower().Contains(searchLower) ||
                                    w.roomCode.ToLower().Contains(searchLower) ||
                                    w.description.ToLower().Contains(searchLower)));
    }

    /// <summary>
    /// Obtiene todos los waypoints activos, ordenados alfabéticamente por código.
    /// </summary>
    /// <returns>Lista ordenada de waypoints activos</returns>
    public List<Waypoint> GetActiveWaypoints()
    {
        var activeWaypoints = allWaypoints.FindAll(w => w != null && w.isActive);
        activeWaypoints.Sort((a, b) => string.Compare(a.roomCode, b.roomCode, System.StringComparison.OrdinalIgnoreCase));
        return activeWaypoints;
    }

    /// <summary>
    /// Encuentra el waypoint más cercano a una posición específica.
    /// </summary>
    /// <param name="position">Posición de referencia</param>
    /// <param name="maxDistance">Distancia máxima de búsqueda (opcional)</param>
    /// <returns>Waypoint más cercano o null si no hay ninguno en rango</returns>
    public Waypoint FindNearestWaypoint(Vector3 position, float maxDistance = float.MaxValue)
    {
        Waypoint nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (var waypoint in allWaypoints)
        {
            if (waypoint == null || !waypoint.isActive || waypoint.transform == null) continue;

            float distance = Vector3.Distance(position, waypoint.transform.position);
            if (distance < nearestDistance && distance <= maxDistance)
            {
                nearestDistance = distance;
                nearest = waypoint;
            }
        }

        return nearest;
    }
    #endregion

    #region Statistics and Information
    /// <summary>
    /// Obtiene estadísticas detalladas del sistema de waypoints.
    /// </summary>
    /// <returns>String con información estadística</returns>
    public string GetSystemStatistics()
    {
        int totalWaypoints = allWaypoints.Count;
        int activeWaypoints = allWaypoints.FindAll(w => w != null && w.isActive).Count;
        int inactiveWaypoints = totalWaypoints - activeWaypoints;

        var typeStats = new Dictionary<RoomType, int>();
        foreach (RoomType roomType in System.Enum.GetValues(typeof(RoomType)))
        {
            typeStats[roomType] = GetWaypointsByType(roomType).Count;
        }

        string stats = $"📊 ESTADÍSTICAS DEL SISTEMA DE WAYPOINTS:\n";
        stats += $"   Total: {totalWaypoints}\n";
        stats += $"   Activos: {activeWaypoints}\n";
        stats += $"   Inactivos: {inactiveWaypoints}\n\n";
        stats += $"📍 POR TIPO DE SALA:\n";

        foreach (var kvp in typeStats)
        {
            if (kvp.Value > 0)
            {
                string displayName = kvp.Key == RoomType.CEE ? "Centro de Estudiantes" : kvp.Key.ToString();
                stats += $"   {displayName}: {kvp.Value}\n";
            }
        }

        return stats;
    }

    /// <summary>
    /// Valida todos los waypoints registrados y reporta problemas.
    /// </summary>
    /// <returns>Número de waypoints con problemas</returns>
    public int ValidateAllWaypoints()
    {
        int problemCount = 0;
        var invalidWaypoints = new List<Waypoint>();

        foreach (var waypoint in allWaypoints)
        {
            if (waypoint == null)
            {
                invalidWaypoints.Add(waypoint);
                problemCount++;
                continue;
            }

            if (!waypoint.IsValid())
            {
                problemCount++;
            }
        }

        // Limpiar referencias null
        foreach (var invalid in invalidWaypoints)
        {
            allWaypoints.Remove(invalid);
        }

        if (problemCount > 0)
        {
            Debug.LogWarning($"⚠️ WaypointManager: {problemCount} waypoints tienen problemas de configuración");
        }

        return problemCount;
    }
    #endregion

    #region Utility Methods
    /// <summary>
    /// Encuentra waypoints que contengan un texto específico en su descripción.
    /// </summary>
    /// <param name="descriptionText">Texto a buscar en la descripción</param>
    /// <returns>Lista de waypoints que coinciden</returns>
    public List<Waypoint> FindWaypointsByDescription(string descriptionText)
    {
        if (string.IsNullOrEmpty(descriptionText)) return new List<Waypoint>();

        string searchLower = descriptionText.ToLower();
        return allWaypoints.FindAll(w => w != null && w.isActive &&
                                   w.description.ToLower().Contains(searchLower));
    }

    /// <summary>
    /// Obtiene waypoints dentro de un radio específico desde una posición.
    /// </summary>
    /// <param name="center">Posición central</param>
    /// <param name="radius">Radio de búsqueda</param>
    /// <returns>Lista de waypoints dentro del radio</returns>
    public List<Waypoint> GetWaypointsInRadius(Vector3 center, float radius)
    {
        var waypointsInRadius = new List<Waypoint>();

        foreach (var waypoint in allWaypoints)
        {
            if (waypoint == null || !waypoint.isActive || waypoint.transform == null) continue;

            float distance = Vector3.Distance(center, waypoint.transform.position);
            if (distance <= radius)
            {
                waypointsInRadius.Add(waypoint);
            }
        }

        return waypointsInRadius;
    }

    /// <summary>
    /// Verifica si existe un waypoint con el código especificado.
    /// </summary>
    /// <param name="roomCode">Código a verificar</param>
    /// <returns>True si existe</returns>
    public bool WaypointCodeExists(string roomCode)
    {
        return FindWaypointByCode(roomCode) != null;
    }

    /// <summary>
    /// Obtiene una lista de todos los códigos de sala únicos.
    /// </summary>
    /// <returns>Array de códigos únicos</returns>
    public string[] GetAllRoomCodes()
    {
        var codes = new System.Collections.Generic.HashSet<string>();
        
        foreach (var waypoint in allWaypoints)
        {
            if (waypoint != null && !string.IsNullOrEmpty(waypoint.roomCode))
            {
                codes.Add(waypoint.roomCode);
            }
        }

        var codeArray = new string[codes.Count];
        codes.CopyTo(codeArray);
        System.Array.Sort(codeArray);
        return codeArray;
    }
    #endregion

    #region System Validation and Logging
    /// <summary>
    /// Valida el estado general del sistema al inicializar.
    /// </summary>
    void ValidateWaypoints()
    {
        int problems = ValidateAllWaypoints();
        
        if (problems == 0)
        {
            Debug.Log("✅ WaypointManager: Todos los waypoints son válidos");
        }
    }

    /// <summary>
    /// Registra el estado del sistema en la consola.
    /// </summary>
    void LogSystemStatus()
    {
        Debug.Log(GetSystemStatistics());
        
        // Log adicional para waypoints CEE si existen
        var ceeWaypoints = GetWaypointsByType(RoomType.CEE);
        if (ceeWaypoints.Count > 0)
        {
            Debug.Log($"🏛️ Encontrados {ceeWaypoints.Count} waypoints del Centro de Estudiantes (CEE)");
        }
    }
    #endregion

    #region Context Menu Utilities
    /// <summary>
    /// Utilidad de editor para mostrar estadísticas completas.
    /// </summary>
    [ContextMenu("📊 Mostrar Estadísticas")]
    void ShowStatistics()
    {
        Debug.Log(GetSystemStatistics());
    }

    /// <summary>
    /// Utilidad para validar todos los waypoints manualmente.
    /// </summary>
    [ContextMenu("🔍 Validar Todos los Waypoints")]
    void ManualValidateAll()
    {
        int problems = ValidateAllWaypoints();
        if (problems == 0)
        {
            Debug.Log("✅ Validación completada: Todos los waypoints son válidos");
        }
        else
        {
            Debug.LogWarning($"⚠️ Validación completada: {problems} waypoints con problemas");
        }
    }

    /// <summary>
    /// Utilidad para buscar waypoints duplicados por código.
    /// </summary>
    [ContextMenu("🔎 Buscar Códigos Duplicados")]
    void FindDuplicateCodes()
    {
        var codeCount = new Dictionary<string, int>();
        
        foreach (var waypoint in allWaypoints)
        {
            if (waypoint != null && !string.IsNullOrEmpty(waypoint.roomCode))
            {
                if (codeCount.ContainsKey(waypoint.roomCode))
                {
                    codeCount[waypoint.roomCode]++;
                }
                else
                {
                    codeCount[waypoint.roomCode] = 1;
                }
            }
        }

        bool foundDuplicates = false;
        foreach (var kvp in codeCount)
        {
            if (kvp.Value > 1)
            {
                Debug.LogWarning($"⚠️ Código duplicado encontrado: '{kvp.Key}' ({kvp.Value} waypoints)");
                foundDuplicates = true;
            }
        }

        if (!foundDuplicates)
        {
            Debug.Log("✅ No se encontraron códigos duplicados");
        }
    }

    /// <summary>
    /// Utilidad para listar todos los waypoints CEE.
    /// </summary>
    [ContextMenu("🏛️ Listar Waypoints CEE")]
    void ListCEEWaypoints()
    {
        var ceeWaypoints = GetWaypointsByType(RoomType.CEE);
        
        if (ceeWaypoints.Count == 0)
        {
            Debug.Log("No se encontraron waypoints del Centro de Estudiantes (CEE)");
            return;
        }

        Debug.Log($"🏛️ WAYPOINTS DEL CENTRO DE ESTUDIANTES ({ceeWaypoints.Count}):");
        foreach (var waypoint in ceeWaypoints)
        {
            Debug.Log($"   📍 {waypoint.GetDisplayName()} - {waypoint.description}");
        }
    }
    #endregion
}