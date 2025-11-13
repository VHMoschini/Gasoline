using UnityEngine;

/// <summary>
/// Gerenciador de Input que unifica controles de PC (WASD/Setas) e Mobile (Joystick Virtual)
/// </summary>
public class InputManager : MonoBehaviour
{
    private static InputManager instance;
    public static InputManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<InputManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("InputManager");
                    instance = go.AddComponent<InputManager>();
                }
            }
            return instance;
        }
    }
    
    [Header("Mobile Controls")]
    public VirtualJoystick virtualJoystick;
    
    [Header("Settings")]
    public bool isMobile = false;
    
    [Tooltip("Força o modo mobile no Editor Unity para testar o joystick")]
    public bool forceMobileInEditor = false;
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        
        // Detecta automaticamente se é mobile
        DetectPlatform();
    }
    
    private void Start()
    {
        // O JoystickArea deve sempre estar ativo para receber eventos de toque
        // O próprio VirtualJoystick controla a visibilidade do background
        if (virtualJoystick != null)
        {
            virtualJoystick.gameObject.SetActive(true);
            if (showDebugLogs)
                Debug.Log($"[InputManager] VirtualJoystick configurado: {virtualJoystick.name}");
        }
        else
        {
            if (showDebugLogs)
                Debug.LogWarning("[InputManager] VirtualJoystick não está configurado!");
        }
        
        if (showDebugLogs)
            Debug.Log($"[InputManager] Modo detectado - isMobile: {isMobile}");
    }
    
    private void DetectPlatform()
    {
        #if UNITY_ANDROID || UNITY_IOS
            isMobile = true;
        #elif UNITY_EDITOR
            // No editor, permite forçar o modo mobile para testar
            isMobile = forceMobileInEditor;
        #else
            isMobile = false;
        #endif
        
        // Também pode detectar por touch support
        if (Input.touchSupported && !Application.isEditor)
        {
            isMobile = true;
        }
    }
    
    /// <summary>
    /// Retorna o input horizontal (-1 a 1)
    /// </summary>
    public float GetHorizontal()
    {
        float value = 0f;
        
        if (isMobile && virtualJoystick != null)
        {
            value = virtualJoystick.Horizontal;
        }
        else
        {
            value = Input.GetAxis("Horizontal");
        }
        
        // Log apenas quando há input diferente de zero
        if (showDebugLogs && Mathf.Abs(value) > 0.01f)
        {
            Debug.Log($"[InputManager] GetHorizontal: {value:F2} | isMobile: {isMobile} | VirtualJoystick: {(virtualJoystick != null ? virtualJoystick.Horizontal.ToString("F2") : "null")}");
        }
        
        return value;
    }
    
    /// <summary>
    /// Retorna o input vertical (-1 a 1)
    /// </summary>
    public float GetVertical()
    {
        float value = 0f;
        
        if (isMobile && virtualJoystick != null)
        {
            value = virtualJoystick.Vertical;
        }
        else
        {
            value = Input.GetAxis("Vertical");
        }
        
        // Log apenas quando há input diferente de zero
        if (showDebugLogs && Mathf.Abs(value) > 0.01f)
        {
            Debug.Log($"[InputManager] GetVertical: {value:F2} | isMobile: {isMobile} | VirtualJoystick: {(virtualJoystick != null ? virtualJoystick.Vertical.ToString("F2") : "null")}");
        }
        
        return value;
    }
    
    /// <summary>
    /// Retorna o vetor de movimento (x = horizontal, y = vertical)
    /// </summary>
    public Vector2 GetMovementVector()
    {
        return new Vector2(GetHorizontal(), GetVertical());
    }
    
    /// <summary>
    /// Retorna o input do botão de pulo/freio
    /// </summary>
    public float GetJump()
    {
        return Input.GetAxis("Jump");
    }
    
    /// <summary>
    /// Alterna entre modo mobile e PC (útil para testes)
    /// </summary>
    public void ToggleMobileMode()
    {
        isMobile = !isMobile;
        // Não precisa desativar o joystick, ele controla sua própria visibilidade
    }
}
