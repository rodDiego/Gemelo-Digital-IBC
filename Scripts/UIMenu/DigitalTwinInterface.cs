using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DigitalTwinInterface : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button iniciarButton;
    public Button ajustesButton;
    public Button salirButton;
    
    void Start()
    {
        // Configurar los eventos de los botones
        iniciarButton.onClick.AddListener(OnIniciarClick);
        ajustesButton.onClick.AddListener(OnAjustesClick);
        salirButton.onClick.AddListener(OnSalirClick);
    }

    void OnIniciarClick()
    {
        Debug.Log("Iniciando sistema de gemelo digital...");
        // Cargar la escena 3D
        SceneManager.LoadScene("DigitalTwin");
    }
    
    void OnAjustesClick()
    {
        Debug.Log("Abriendo panel de configuración...");
        // Aquí iría la lógica para abrir ajustes
    }
    
    void OnSalirClick()
    {
        Debug.Log("Cerrando aplicación...");
        // Aquí iría la lógica para salir
        Application.Quit();
    }
}