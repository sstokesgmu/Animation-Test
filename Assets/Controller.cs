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
using UnityEditor.Rendering.LookDev;
using System.IO.Pipes;
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

    public Collider col;
    public LayerMask groundLayer;

    public CinemachineCamera cam;

    public AnimatorParameters animatorParameters;
    public Rigidbody rb;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created


    bool hasMovementInput;
    bool hasRunInput;
    bool hasJumpInput;


    //private StateMachine stateMachine;

    private Node isGrounded;
    private Node isInAir;

    private Graph groundedGraph;
    private PlayerMovement _playerMovement;
    private float lastFrameY; // You'd need this one reference



    void Awake()
    {
        //ToDo: animator comparison if the value of AnimatorParameters scriptable obj does not match the values retrieved from GetCompnent or editor assignement then re assign the value
        move.action.performed += context =>
        {
            //Debug.Log($"{context.action} performed, value as object {context.ReadValueAsObject()}");
            Vector2 input = context.ReadValue<Vector2>();
            hasMovementInput = input.x != 0 || input.y != 0;
        };
        move.action.canceled += context =>
        {
            Vector2 input = Vector2.zero;
            hasMovementInput = false;
        };
        sprint.action.performed += context => hasRunInput = context.ReadValueAsButton();
        sprint.action.canceled += context => hasRunInput = false;

        jump.action.performed += context => hasJumpInput = context.ReadValueAsButton();
        jump.action.canceled += context => hasJumpInput = false;

        // controllerData = new StateData<Controller>(this, animatorParameters);
        _playerMovement = GetComponent<PlayerMovement>();



        // //! Do we need this 
        bool isGroundedPramaFound = animatorParameters.Parameters.TryGetValue("isGrounded", out int groundedHash);
        bool isWalkingAnimParamFound = animatorParameters.Parameters.TryGetValue("isWalking", out int walkingHash);
        bool isRunningAnimParamFound = animatorParameters.Parameters.TryGetValue("isRunning", out int runningHash);
        bool isJumpingAnimParamFound = animatorParameters.Parameters.TryGetValue("TriggerJump", out int jumpHash);

        // if(!isWalkingAnimParamFound)
        // {
        //     Debug.LogWarning("The animation parameter for walking was not found");
        //     //TODO: Do something in this case
        // }

        // if(!isRunningAnumParamFound) 
        // {
        //     Debug.LogError("The animation parameter for running was not found");
        //     //TODO: Do something in this case
        // }



        isGrounded = new Node(null, "isGrounded", new IsGrounded());

        //Idles

        Node idleGrounded = new Node(isGrounded, "Idle-Grounded", new Idle(anim, groundedHash));
        Node idleInAir = new Node(isGrounded, "Idle-InAir", new Air_Idle(anim, groundedHash));

        //? What is the minimal state for a basic character to be in 
        Node WalkNode = new Node(isGrounded, "Walk", new Walk(anim,walkingHash));
        Node RunNode = new Node(isGrounded, "Run", new Run(anim, runningHash));
        Node JumpNode = new Node(idleGrounded, "Jump-Grounded", new Jump(anim, jumpHash));


        //! Input -> Game State -> Animation 
        groundedGraph = GraphBuilder.Create(isGrounded)
                    .AddChild(isGrounded, idleGrounded, "Idle-Grounded")
                    .AddChild(isGrounded, idleInAir, "Idle - Air")
                    .AddTransition(isGrounded, idleGrounded, () => _playerMovement.IsGrounded(col, groundLayer) == true)
                    .AddTransition(idleGrounded, isGrounded, () => _playerMovement.IsGrounded(col, groundLayer) == false)
                        .AddChild(idleGrounded, WalkNode, "Walk")
                            .AddTransition(idleGrounded, WalkNode, () => hasMovementInput)
                            .AddTransition(WalkNode, idleGrounded, () => !hasMovementInput)
                            .AddTransition(WalkNode, RunNode, () => hasRunInput)
                        .AddChild(idleGrounded, RunNode, "Run")
                            .AddTransition(idleGrounded, RunNode, () => hasMovementInput && hasRunInput)
                            .AddTransition(RunNode, WalkNode, () => !hasRunInput)
                            .AddTransition(RunNode, idleGrounded, () => !hasRunInput && !hasMovementInput)
                        .AddChild(idleGrounded, JumpNode, "Jump")
                            .AddTransition(idleGrounded, JumpNode, () => hasJumpInput)
                            .AddTransition(WalkNode,JumpNode, () => hasJumpInput)
                            .AddTransition(RunNode,JumpNode, () => hasJumpInput)
                            .AddTransition(JumpNode,idleInAir, () => true)
                    .AddTransition(idleInAir, isGrounded, () => _playerMovement.IsGrounded(col, groundLayer) == true)
                    .AddTransition(isGrounded, idleInAir, () => _playerMovement.IsGrounded(col, groundLayer) == false)
                            .Generate();
        groundedGraph.root = isGrounded;
    }
    void Start()
    {

        
    


    }
    // Update is called once per frame
    void FixedUpdate()
    {
        
        // if(_playerMovement.IsGrounded(col, groundLayer))

        //     Debug.Log("Player is graounded");
        // else
        //     Debug.Log("Player is In the air");
        // float currentY = transform.position.y;
        // float verticalVelocity = (currentY - lastFrameY) / Time.fixedDeltaTime;
    
        // if (verticalVelocity > 0.1f)
        //     Debug.Log("Jumping");
        // else if (verticalVelocity < -0.1f)
        //     Debug.Log("Falling");
        
        // lastFrameY = currentY; // Store for next frame
        _playerMovement.HandleMove(
            _playerMovement.GetInputVector(move.action.ReadValue<Vector2>()), anim.velocity.magnitude
        );
        //HandleMovement();
        //Rotation Handling 

        //Todo: State Machine Class
        
        // Debug current state and jump input
        if (hasJumpInput)
        {
            Debug.Log($"Jump input detected! Current state: {groundedGraph.currentNode?.name}");
            Debug.Log($"IsGrounded check: {_playerMovement.IsGrounded(col, groundLayer)}");
        }
        
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

