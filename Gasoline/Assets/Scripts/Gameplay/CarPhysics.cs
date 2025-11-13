using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CarPhysics : MonoBehaviour
{
    public enum groundCheck { rayCast, sphereCaste };
    public enum MovementMode { Velocity, AngularVelocity };
    public MovementMode movementMode;
    public groundCheck GroundCheck;
    public LayerMask drivableSurface;

    public float MaxSpeed, accelaration, turn, gravity = 7f, downforce = 5f;
    public bool AirControl = false;
    public Rigidbody rb, carBody;

    [HideInInspector]
    public RaycastHit hit;
    public AnimationCurve frictionCurve;
    public AnimationCurve turnCurve;
    public PhysicsMaterial frictionMaterial;
    [Header("Visuals")]
    public Transform BodyMesh;
    public Transform[] FrontWheels = new Transform[2];
    public Transform[] RearWheels = new Transform[2];
    [HideInInspector]
    public Vector3 carVelocity;

    [Range(0, 10)]
    public float BodyTilt;
    [Header("Audio settings")]
    public AudioSource engineSound;
    [Range(0, 1)]
    public float minPitch;
    [Range(1, 3)]
    public float MaxPitch;
    public AudioSource SkidSound;

    [HideInInspector]
    public float skidWidth;


    private float radius, horizontalInput, verticalInput;
    private Vector3 origin;
    public bool canMove = false;
    
    [Header("Debug")]
    public bool showDebugLogs = false;

    private void Start()
    {
        radius = rb.GetComponent<SphereCollider>().radius;
        if (movementMode == MovementMode.AngularVelocity)
        {
            Physics.defaultMaxAngularSpeed = 100;
        }
        
        if (showDebugLogs)
            Debug.Log($"[CarPhysics] Iniciado - canMove: {canMove}");
    }
    private void Update()
    {

        if (canMove)
        {
            // Usa InputManager para suportar tanto PC quanto Mobile
            horizontalInput = InputManager.Instance.GetHorizontal();
            verticalInput = InputManager.Instance.GetVertical();
            
            // Log apenas quando há input
            if (showDebugLogs && (Mathf.Abs(horizontalInput) > 0.01f || Mathf.Abs(verticalInput) > 0.01f))
            {
                Debug.Log($"[CarPhysics] Input recebido - Horizontal: {horizontalInput:F2}, Vertical: {verticalInput:F2}");
            }
        }
        Visuals();
        AudioManager();

    }
    public void AudioManager()
    {
        engineSound.pitch = Mathf.Lerp(minPitch, MaxPitch, Mathf.Abs(carVelocity.z) / MaxSpeed);
        if (Mathf.Abs(carVelocity.x) > 10 && grounded())
        {
            SkidSound.mute = false;
        }
        else
        {
            SkidSound.mute = true;
        }
    }

    public void ToggleMovent()
    {
        canMove = !canMove;
        if (showDebugLogs)
            Debug.Log($"[CarPhysics] ToggleMovent chamado - canMove agora é: {canMove}");
    }
    
    /// <summary>
    /// Ativa o movimento do veículo (chamado por Timeline/Signals)
    /// </summary>
    public void EnableMovement()
    {
        canMove = true;
        Debug.Log("[CarPhysics] EnableMovement chamado - Movimento ATIVADO");
    }
    
    /// <summary>
    /// Desativa o movimento do veículo
    /// </summary>
    public void DisableMovement()
    {
        canMove = false;
        Debug.Log("[CarPhysics] DisableMovement chamado - Movimento DESATIVADO");
    }
    
    /// <summary>
    /// Define o estado de movimento diretamente
    /// </summary>
    public void SetCanMove(bool value)
    {
        canMove = value;
        if (showDebugLogs)
            Debug.Log($"[CarPhysics] SetCanMove chamado com valor: {value}");
    }

    void FixedUpdate()
    {
        carVelocity = carBody.transform.InverseTransformDirection(carBody.linearVelocity);

        if (Mathf.Abs(carVelocity.x) > 0)
        {

            frictionMaterial.dynamicFriction = frictionCurve.Evaluate(Mathf.Abs(carVelocity.x / 100));
        }


        if (grounded())
        {

            float sign = Mathf.Sign(carVelocity.z);
            float TurnMultiplyer = turnCurve.Evaluate(carVelocity.magnitude / MaxSpeed);
            if (verticalInput > 0.1f || carVelocity.z > 1)
            {
                carBody.AddTorque(Vector3.up * horizontalInput * sign * turn * 100 * TurnMultiplyer);
            }
            else if (verticalInput < -0.1f || carVelocity.z < -1)
            {
                carBody.AddTorque(Vector3.up * horizontalInput * sign * turn * 100 * TurnMultiplyer);
            }


            if (InputManager.Instance.GetJump() > 0.1f)
            {
                rb.constraints = RigidbodyConstraints.FreezeRotationX;
            }
            else
            {
                rb.constraints = RigidbodyConstraints.None;
            }



            if (movementMode == MovementMode.AngularVelocity)
            {
                if (Mathf.Abs(verticalInput) > 0.1f)
                {
                    rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, carBody.transform.right * verticalInput * MaxSpeed / radius, accelaration * Time.deltaTime);
                }
            }
            else if (movementMode == MovementMode.Velocity)
            {
                if (Mathf.Abs(verticalInput) > 0.1f && InputManager.Instance.GetJump() < 0.1f)
                {
                    rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, carBody.transform.forward * verticalInput * MaxSpeed, accelaration / 10 * Time.deltaTime);
                }
            }

            rb.AddForce(-transform.up * downforce * rb.mass);

            carBody.MoveRotation(Quaternion.Slerp(carBody.rotation, Quaternion.FromToRotation(carBody.transform.up, hit.normal) * carBody.transform.rotation, 0.12f));
        }
        else
        {
            if (AirControl)
            {
                float TurnMultiplyer = turnCurve.Evaluate(carVelocity.magnitude / MaxSpeed);

                carBody.AddTorque(Vector3.up * horizontalInput * turn * 100 * TurnMultiplyer);
            }

            carBody.MoveRotation(Quaternion.Slerp(carBody.rotation, Quaternion.FromToRotation(carBody.transform.up, Vector3.up) * carBody.transform.rotation, 0.02f));
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, rb.linearVelocity + Vector3.down * gravity, Time.deltaTime * gravity);
        }

    }
    public void Visuals()
    {
        foreach (Transform FW in FrontWheels)
        {
            FW.localRotation = Quaternion.Slerp(FW.localRotation, Quaternion.Euler(FW.localRotation.eulerAngles.x,
                               30 * horizontalInput, FW.localRotation.eulerAngles.z), 0.1f);
            FW.GetChild(0).localRotation = rb.transform.localRotation;
        }
        RearWheels[0].localRotation = rb.transform.localRotation;
        RearWheels[1].localRotation = rb.transform.localRotation;

        if (carVelocity.z > 1)
        {
            BodyMesh.localRotation = Quaternion.Slerp(BodyMesh.localRotation, Quaternion.Euler(Mathf.Lerp(0, -5, carVelocity.z / MaxSpeed),
                               BodyMesh.localRotation.eulerAngles.y, BodyTilt * horizontalInput), 0.05f);
        }
        else
        {
            BodyMesh.localRotation = Quaternion.Slerp(BodyMesh.localRotation, Quaternion.Euler(0, 0, 0), 0.05f);
        }


    }

    public bool grounded()
    {
        origin = rb.position + rb.GetComponent<SphereCollider>().radius * Vector3.up;
        var direction = -transform.up;
        var maxdistance = rb.GetComponent<SphereCollider>().radius + 0.2f;

        if (GroundCheck == groundCheck.rayCast)
        {
            if (Physics.Raycast(rb.position, Vector3.down, out hit, maxdistance, drivableSurface))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        else if (GroundCheck == groundCheck.sphereCaste)
        {
            if (Physics.SphereCast(origin, radius + 0.1f, direction, out hit, maxdistance, drivableSurface))
            {
                return true;

            }
            else
            {
                return false;
            }
        }
        else { return false; }
    }



}

