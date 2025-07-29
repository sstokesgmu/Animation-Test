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

    public Collider col;
    public LayerMask groundLayer;

    public CinemachineCamera cam;

    public AnimatorParameters animatorParameters;
    private Rigidbody rb;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created


    bool hasMovmentInput;
    bool hasRunInput;


    //private StateMachine stateMachine;

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
            Vector2 input = context.ReadValue<Vector2>();
            hasMovmentInput = input.x != 0 || input.y != 0;
        };
        move.action.canceled += context =>
        {
            Vector2 input = Vector2.zero;
            hasMovmentInput = false;
        };
        sprint.action.performed += context => hasRunInput = context.ReadValueAsButton();
        sprint.action.canceled += context => hasRunInput = false;

       // controllerData = new StateData<Controller>(this, animatorParameters);
        _playerMovement = GetComponent<PlayerMovement>();



        isGrounded = new Node(null, "isGrounded", new IsGrounded());

        bool isWalkingAnimParamFound = animatorParameters.Parameters.TryGetValue("isWalking", out int walkingHash);
        bool isRunningAnumParamFound = animatorParameters.Parameters.TryGetValue("isRunning", out int runningHash);

        if(!isWalkingAnimParamFound)
        {
            Debug.LogWarning("The animation parameter for walking was not found");
            //TODO: Do something in this case
        }

        if(!isRunningAnumParamFound) 
        {
            Debug.LogError("The animation parameter for running was not found");
            //TODO: Do something in this case
        }

        //? What is the minimal state for a basic character to be in 
        Node WalkNode = new Node(isGrounded, "Walk", new Walk(anim,walkingHash));
        Node RunNode = new Node(isGrounded, "Run", new Run(anim, runningHash));
        //! Input -> Game State -> Animation 
        groundedGraph = GraphBuilder.Create(isGrounded)
                    .AddChild(isGrounded, WalkNode, "Walk")
                        .AddTransition(isGrounded, WalkNode, () => hasMovmentInput)
                        .AddTransition(WalkNode, isGrounded, () => !hasMovmentInput)
                        .AddTransition(WalkNode, RunNode, () => hasRunInput)
                    .AddChild(isGrounded, RunNode, "Run")
                        .AddTransition(isGrounded, RunNode, () => hasMovmentInput && hasRunInput)
                        .AddTransition(RunNode, isGrounded, () => !hasRunInput && !hasMovmentInput)
                        .AddTransition(RunNode, WalkNode, () => !hasRunInput)
                            .Generate();
        groundedGraph.root = isGrounded;
    }
    void Start()
    {

        
    


    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if(_playerMovement.IsGrounded(col, groundLayer))
            Debug.Log("Player is graounded");
        else
            Debug.Log("Player is In the air");
    
        _playerMovement.HandleMove(
            _playerMovement.GetInputVector(move.action.ReadValue<Vector2>()), anim.velocity.magnitude
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

