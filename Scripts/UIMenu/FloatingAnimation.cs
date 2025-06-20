using UnityEngine;

public class FloatingAnimation : MonoBehaviour
{
    [Header("Float Settings")]
    public float floatSpeed = 1f;
    public float floatHeight = 20f;
    public float rotationSpeed = 30f;
    
    private Vector3 startPosition;
    private float randomOffset;
    
    void Start()
    {
        // Guardar posición inicial
        startPosition = transform.localPosition;
        
        // Offset aleatorio para que no se muevan todos igual
        randomOffset = Random.Range(0f, 2f * Mathf.PI);
    }
    
    void Update()
    {
        // Movimiento flotante vertical
        float newY = startPosition.y + Mathf.Sin((Time.time * floatSpeed) + randomOffset) * floatHeight;
        transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);
        
        // Rotación lenta
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}