using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using Locomotion_States;
using GraphHelper;
using JetBrains.Annotations;
using UnityEngine.Experimental.GlobalIllumination;
using System.Reflection;
using System;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Rendering;
//-------------------------------------------------------------------
/*
    Base Class for player and enemies
    Handle animations 
*/
//--------------------------------------S~----------------------------

public struct
StateData<T> where T : class
{
    public AnimatorParameters animParams;
    public T dataRef;

    public StateData(T dataRefernce, AnimatorParameters animatorParams)
    {
        this.dataRef = dataRefernce;
        this.animParams = animatorParams;
        Debug.Log($"The data ref is {dataRef} of type {typeof(T)}");
    }
}


public class Controller : MonoBehaviour
{

    public InputActionReference move;
    public InputActionReference sprint;
    public InputActionReference jump;
    public GameObject mesh; //? Question to get the forward vector of where the player is facing should I use a component or another gameobject

    public Animator anim;
    //? Question What other stuff will the animator access, I want to put it in it's own class: Access Input, current direction, and ...

    private Vector3 inputVectorRef;

    public CinemachineCamera cam;

    public AnimatorParameters animatorParameters;
    private Rigidbody rb;
    

    private Vector3 lastLookDirection;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    Vector2 input;
    bool isWalking;
    bool isRunning;

    private StateData<Controller> controllerData;
    private StateMachine stateMachine;

    private Node isGrounded;
    private Node isInAir;

    private Graph groundedGraph;
    private PlayerMovement _playerMovement;



    void Awake()
    {
        //ToDo: animator comparison if the value of AnimatorParameters scriptable obj does not match the values retrieved from GetCompnent or editor assignement then re assign the value
        move.action.performed += context =>
        {
            //Debug.Log($"{context.action} performed, value as object {context.ReadValueAsObject()}");
            input = context.ReadValue<Vector2>();
            isWalking = input.x != 0 || input.y != 0;
        };
        move.action.canceled += context =>
        {
            input = Vector2.zero;
            isWalking = false;
        };
        sprint.action.performed += context => isRunning = context.ReadValueAsButton();

        controllerData = new StateData<Controller>(this, animatorParameters);
        _playerMovement = GetComponent<PlayerMovement>();
    }


        // void HandleMovement()
    // {
    //     //ToDo: Do this on awake,   Maybe animation priority?
    //     bool walkingFound = animatorParameters.Parameters.TryGetValue("isWalking", out int walkingHash);
    //     bool runningFound = animatorParameters.Parameters.TryGetValue("isRunning", out int runningHash);


    //     if (walkingFound || runningFound)
    //     {
    //         bool isAnimInWalkingState = anim.GetBool(walkingHash);
    //         bool isAnimInRunningState = anim.GetBool(runningHash);

    //         //!Is walking and is running is the input bool values;


    //         //Start Walking if already not walking
    //         if (isWalking == true && isAnimInWalkingState == false)
    //             anim.SetBool(walkingHash, true);
    //         //Stop Walking
    //         if (isWalking == false && isAnimInWalkingState == true)
    //             anim.SetBool(walkingHash, false);

    //         if ((isWalking && isRunning) && !isAnimInRunningState)
    //             anim.SetBool(runningHash, true);

    //         if ((!isWalking && !isRunning) && isAnimInRunningState)
    //             anim.SetBool(runningHash, false);
    //         if ((isWalking && !isRunning) && isAnimInRunningState)
    //             anim.SetBool(runningHash, false);
    //     }
    //     else
    //     {
    //         Debug.LogWarning($"Parameter  not found in " + animatorParameters.name);
    //         //Todo Handle graceful exception
    //     }
    // }


    void Start()
    {

        isGrounded = new Node(null, "isGrounded", new IsGrounded());    
        Node WalkNode = new Node(isGrounded, "Walk", new Walk());
        Node RunNode = new Node(isGrounded, "Run", new Run());
        //! Input -> Game State -> Animation 
        groundedGraph = GraphBuilder.Create(isGrounded)
                    .AddChild(isGrounded, WalkNode, "Walk")
                        //! GetInputVector is a Data Provider Function
                        .AddTransition(isGrounded, WalkNode, (param) => param != Vector3.zero, () => GetInputVector())
                        .DescendToChild(isGrounded, "Walk")
                            .AddTransition(WalkNode, isGrounded, (param) => param == Vector3.zero, () => GetInputVector())
                        .Generate();

        groundedGraph.root = isGrounded;



    }
    // Update is called once per frame
    void FixedUpdate()
    {

        _playerMovement.HandleMove(
            _playerMovement.GetInputVector(move.action.ReadValue<Vector2>())
        );
        //HandleMovement();
        //Rotation Handling 

        //Todo: State Machine Class
        groundedGraph.Update();
        //Movement Handling 
        Vector3 res = DrawCameraRay();
    }

    Vector3 DrawCameraRay()
    {
        Vector3 camPos = cam.transform.position;
        Vector3 playerPos = this.transform.position;

        Vector3 playerCurrentPos = playerPos - camPos;
        float playerCurrentPosMag = playerCurrentPos.magnitude;
        RaycastHit hitInfo;
        if (Physics.Raycast(camPos, Vector3.down, out hitInfo, Mathf.Infinity))
        {
            Vector3 camToFloor = (hitInfo.point - cam.transform.position);

            Debug.DrawRay(camPos, camToFloor, Color.red);
            Debug.DrawRay(camPos, playerCurrentPos, Color.green);

            //  Debug.Log(hitInfo.collider.tag);
            // Debug.Log($"The hit the point: {hitInfo.point}");
            if (hitInfo.collider.CompareTag("Ground"))
            {
                // Debug.Log("The player is on the ground");
                Vector3 floorToPlayer = (playerPos - hitInfo.point);

                Debug.DrawRay(hitInfo.point, floorToPlayer, Color.cadetBlue);
                return floorToPlayer;
            }
        }
        return Vector3.zero;
    }


    Vector3 GetInputVector()
    {
        // 1) Flatten camera forward/right to XZ plane
        Transform camT = Camera.main.transform;
        Vector3 camForward = camT.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = camT.right;
        camRight.y = 0f;
        camRight.Normalize();

        // 2) Read raw 2D input (x = A/D or left stick X, y = W/S or left stick Y)
        Vector2 input2D = move.action.ReadValue<Vector2>();

        // 3) Build world‑space move vector
        Vector3 worldMove = camForward * input2D.y + camRight * input2D.x;

        return worldMove;
    }

}


public interface IMovement
{
    Vector3 LastLookDirection { get; set; }

    void RotateTowards(Vector3 direction);
    void Move_TrasformTranslate(Vector3 moveVec, float magnitude);
    void RigidBody_Move( Vector3 moveVec, Rigidbody rb, float forceValue);
}


public interface IControllable
{

}


        // //Todo: State Machine Class
        // groundedGraph.Update();

        // //Movement Handling 
        // Vector3 res = DrawCameraRay();
        // if (inputVector != Vector3.zero && inputVector != lastLookDirection)
        // {
        //     // Save the last look direction
        //     lastLookDirection = inputVector;

        //     // Calculate angle in degrees using Atan2
        //     float angle = Mathf.Atan2(inputVector.x, inputVector.z) * Mathf.Rad2Deg;
        //     // Apply rotation around the Y axis
        //     transform.rotation = Quaternion.Euler(0, angle, 0);
        //     //Debug.Log(transform.rotation);
        // }


public class MovmementBase : MonoBehaviour, IMovement
{
    public enum MovementType {RootMotion, RigidBodyMotion, TransformTranslate}
    //TODO Scriptable Object for properties like movment Speed, jump height
    [SerializeField] protected MovementType MoveType = MovementType.RootMotion;
    public Vector3 LastLookDirection { get; set; } //! the last move direction will be the last look direction
    

    public virtual void RotateTowards(Vector3 rotationVec)
    {

        Debug.LogWarning("Root Motion Move");
        if(rotationVec != LastLookDirection)
        {
            LastLookDirection = rotationVec;
            float angle = Mathf.Atan2(rotationVec.x, rotationVec.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }
    }
    public virtual void Move_TrasformTranslate(Vector3 moveVec, float magnitude)
    {
        transform.Translate(moveVec * Time.deltaTime);


    }
    public virtual void RigidBody_Move(Vector3 moveVec, Rigidbody rb, float force)
    {
        rb.AddForce(moveVec * force, ForceMode.Force);
    }
    public virtual void HandleMove(Vector3 moveVec)
    {
        //Debug.Log($"Input Vector {moveVec}");
        moveVec.Normalize();

        if(moveVec == Vector3.zero) return;

        switch(MoveType)
        {
            case MovementType.RootMotion:
                RotateTowards(moveVec);
                break;
            case MovementType.RigidBodyMotion:
                RotateTowards(moveVec);
                RigidBody_Move(moveVec, GetComponent<Rigidbody>(), 10f);
                break;
            case MovementType.TransformTranslate:
                RotateTowards(moveVec);
                Move_TrasformTranslate(moveVec, 10f);
                break;
        }
    }
}
