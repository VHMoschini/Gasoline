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
    
    [Tooltip("Trava o joystick em 8 direções (4 cardeais + 4 diagonais)")]
    public bool snap8Directions = true;
    
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
        
        // Se snap8Directions está ativo, trava nas 8 direções (cardeais + diagonais)
        if (snap8Directions && direction.magnitude > 0)
        {
            direction = SnapTo8Directions(direction);
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
    
    /// <summary>
    /// Trava a direção em uma das 8 direções (4 cardeais + 4 diagonais)
    /// baseado em qual está mais próximo
    /// </summary>
    private Vector2 SnapTo8Directions(Vector2 direction)
    {
        // Calcula o ângulo em graus
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Normaliza o ângulo para 0-360
        if (angle < 0) angle += 360f;
        
        // Determina qual das 8 direções está mais próxima
        // Cada direção tem 45° de alcance (360° / 8 = 45°)
        
        if (angle >= 337.5f || angle < 22.5f)
        {
            // Direita (0°)
            return Vector2.right;
        }
        else if (angle >= 22.5f && angle < 67.5f)
        {
            // Diagonal Direita-Cima (45°)
            return new Vector2(1f, 1f).normalized;
        }
        else if (angle >= 67.5f && angle < 112.5f)
        {
            // Cima (90°)
            return Vector2.up;
        }
        else if (angle >= 112.5f && angle < 157.5f)
        {
            // Diagonal Esquerda-Cima (135°)
            return new Vector2(-1f, 1f).normalized;
        }
        else if (angle >= 157.5f && angle < 202.5f)
        {
            // Esquerda (180°)
            return Vector2.left;
        }
        else if (angle >= 202.5f && angle < 247.5f)
        {
            // Diagonal Esquerda-Baixo (225°)
            return new Vector2(-1f, -1f).normalized;
        }
        else if (angle >= 247.5f && angle < 292.5f)
        {
            // Baixo (270°)
            return Vector2.down;
        }
        else
        {
            // Diagonal Direita-Baixo (315°)
            return new Vector2(1f, -1f).normalized;
        }
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
