using UnityEngine;

/// <summary>
/// Gerencia a UI de gameplay para mobile
/// </summary>
public class UIMobileGameplay : MonoBehaviour
{
    [Header("Mobile Controls")]
    public VirtualJoystick virtualJoystick;
    
    [Header("References")]
    public InputManager inputManager;
    
    void Start()
    {
        SetupMobileControls();
    }
    
    private void SetupMobileControls()
    {
        // Garante que o InputManager está configurado
        if (inputManager == null)
        {
            inputManager = InputManager.Instance;
        }
        
        // Conecta o joystick ao InputManager
        if (inputManager != null && virtualJoystick != null)
        {
            inputManager.virtualJoystick = virtualJoystick;
        }
        
        // O JoystickArea deve sempre estar ativo para receber eventos de toque
        // O próprio VirtualJoystick controla a visibilidade do background
        if (virtualJoystick != null)
        {
            virtualJoystick.gameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// Método público para ativar/desativar completamente os controles mobile (útil para menus)
    /// </summary>
    public void SetMobileControlsActive(bool active)
    {
        if (virtualJoystick != null)
        {
            virtualJoystick.gameObject.SetActive(active);
        }
    }
}
