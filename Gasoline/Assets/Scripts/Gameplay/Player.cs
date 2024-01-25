using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{

    #region [ EVENTS ]

    [SerializeField] static public UnityEvent<bool> MobilityChange = new UnityEvent<bool>();

    #endregion

    #region [ VARIABLES ]

    [SerializeField] private float speed = 5f;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;

    private Vector2 movement;
    private bool canMove = true;

    #endregion

    #region [ MESSAGES ]

    private void Start()
    {
        MobilityChange.AddListener(ChangeMobility);
    }

    void Update()
    {
        if (!canMove) return;
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);

        animator.SetFloat("Speed", movement.sqrMagnitude);
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }

    #endregion

    #region [ METHODS ]

    private void ChangeMobility(bool status) => canMove = status;

    #endregion
}
