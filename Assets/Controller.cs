using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
//-------------------------------------------------------------------
/*
    Base Class for player and enemies
    Handle animations 
*/
//--------------------------------------S~----------------------------
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

    StateMachine machine;
    private Vector3 lastLookDirection;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    Vector2 input;
    bool isWalking;
    bool isRunning;



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
    }

    void Start()
    {
        move.action.Enable();
        sprint.action.Enable();
        machine = new StateMachine();
        
        MoveState state = new MoveState(10f);
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        
        HandleMovement();

        //Rotation Handling 
        Vector3 inputVector = GetInputVector().normalized;
  
        //Todo: State Machine Class

        //Movement Handling 
        Vector3 res = DrawCameraRay();
        if (inputVector != Vector3.zero && inputVector != lastLookDirection)
        {
            // Save the last look direction
            lastLookDirection = inputVector;

            // Calculate angle in degrees using Atan2
            float angle = Mathf.Atan2(inputVector.x, inputVector.z) * Mathf.Rad2Deg;
            // Apply rotation around the Y axis
            transform.rotation = Quaternion.Euler(0, angle, 0);
            //Debug.Log(transform.rotation);
        }
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

    void HandleMovement()
    {
        //ToDo: Do this on awake,   Maybe animation priority?
        bool walkingFound = animatorParameters.Parameters.TryGetValue("isWalking", out int walkingHash);
        bool runningFound = animatorParameters.Parameters.TryGetValue("isRunning", out int runningHash);


        if (walkingFound || runningFound)
        {
            bool isAnimInWalkingState = anim.GetBool(walkingHash);
            bool isAnimInRunningState = anim.GetBool(runningHash);

            //!Is walking and is running is the input bool values;


            //Start Walking if already not walking
            if (isWalking == true && isAnimInWalkingState == false)
                anim.SetBool(walkingHash, true);
            //Stop Walking
            if (isWalking == false && isAnimInWalkingState == true)
                anim.SetBool(walkingHash, false);

            if ((isWalking && isRunning) && !isAnimInRunningState)
                anim.SetBool(runningHash, true);

            if ((!isWalking && !isRunning) && isAnimInRunningState)
                anim.SetBool(runningHash, false);
            if ((isWalking && !isRunning) && isAnimInRunningState)
                anim.SetBool(runningHash, false);
        }
        else
        {
            Debug.LogWarning($"Parameter  not found in " + animatorParameters.name);
            //Todo Handle graceful exception
        }
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
