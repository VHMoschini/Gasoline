using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Joystick Components")]
    public RectTransform joystickBackground;
    public RectTransform joystickHandle;
    
    [Header("Settings")]
    public float handleRange = 50f;
    public float deadZone = 0.1f;
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    
    private Vector2 inputVector;
    private Vector2 joystickPosition;
    private Canvas canvas;
    private Camera cam;
    
    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        
        // Se o canvas está em Screen Space - Camera, precisamos da câmera
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            cam = canvas.worldCamera;
        }
        
        // Esconde o joystick inicialmente (aparece só quando tocar)
        SetJoystickVisibility(false);
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (showDebugLogs)
            Debug.Log($"[VirtualJoystick] OnPointerDown na posição: {eventData.position}");
        
        SetJoystickVisibility(true);
        
        // Posiciona o joystick onde o jogador tocou
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackground.parent as RectTransform,
            eventData.position,
            cam,
            out joystickPosition);
            
        joystickBackground.anchoredPosition = joystickPosition;
        OnDrag(eventData);
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 direction = Vector2.zero;
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackground,
            eventData.position,
            cam,
            out direction);
        
        // Normaliza a direção baseado no range do joystick
        direction = direction / handleRange;
        
        // Limita a magnitude para não passar de 1
        if (direction.magnitude > 1)
        {
            direction = direction.normalized;
        }
        
        // Aplica dead zone
        if (direction.magnitude < deadZone)
        {
            direction = Vector2.zero;
        }
        
        inputVector = direction;
        
        // Move o handle visualmente
        joystickHandle.anchoredPosition = direction * handleRange;
        
        // Log do input (apenas quando há movimento significativo)
        if (showDebugLogs && inputVector.magnitude > 0.01f)
        {
            Debug.Log($"[VirtualJoystick] Input Vector: ({inputVector.x:F2}, {inputVector.y:F2}) | Magnitude: {inputVector.magnitude:F2}");
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (showDebugLogs)
            Debug.Log("[VirtualJoystick] OnPointerUp - Joystick liberado");
        
        inputVector = Vector2.zero;
        joystickHandle.anchoredPosition = Vector2.zero;
        SetJoystickVisibility(false);
    }
    
    private void SetJoystickVisibility(bool visible)
    {
        joystickBackground.gameObject.SetActive(visible);
    }
    
    // Propriedades públicas para acessar os valores do input
    public float Horizontal
    {
        get { return inputVector.x; }
    }
    
    public float Vertical
    {
        get { return inputVector.y; }
    }
    
    public Vector2 Direction
    {
        get { return inputVector; }
    }
}
