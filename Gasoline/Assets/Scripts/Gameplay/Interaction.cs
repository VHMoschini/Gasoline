using UnityEngine;
using UnityEngine.Events;

public class Interaction : MonoBehaviour
{
    #region [ EVENTS ]

    static public UnityEvent<bool, string> isInteractable = new UnityEvent<bool, string>();

    #endregion

    #region [ VARIABLES ]

    internal UnityEvent interacted = new UnityEvent();

    [SerializeField] private string interactionPrompt;

    private bool interactable = false;

    #endregion

    #region [ MESSAGES ]
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && interactable)
        {
            interacted.Invoke();

            isInteractable.Invoke(false, interactionPrompt);
            interactable = false;
        }
    }

    #endregion

    #region [ TRIGGERS ]

    private void OnTriggerEnter2D(Collider2D collision)
    {
        isInteractable.Invoke(true, interactionPrompt);
        interactable = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        isInteractable.Invoke(false, interactionPrompt);
        interactable = false;
    }

    #endregion
}
