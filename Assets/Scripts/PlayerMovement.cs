using UnityEngine;
using UnityEngine.Events;

public class MovmementBase : MonoBehaviour, IMovement, IGrounded
{
    public enum MovementType {RootMotion, RigidBodyMotion, TransformTranslate}
    //TODO Scriptable Object for properties like movment Speed, jump height
    [SerializeField] protected MovementType MoveType = MovementType.RootMotion;
    public Vector3 LastLookDirection { get; set; } //! the last move direction will be the last look direction    
    public virtual void RotateTowards(Vector3 rotationVec)
    {
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
    public virtual void HandleMove(Vector3 moveVec, float velocity = 1f)
    {
        //Debug.Log($"Input Vector {moveVec}");
        moveVec.Normalize();
        if(moveVec == Vector3.zero) return;


        Debug.DrawRay(transform.position, moveVec * velocity, Color.red);
    

        //TODO: Enter the turn State 
        HasSharpTurn();

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

    public virtual bool HasSharpTurn()
    {
        // bool isSharpTurn = moveVec == -LastLookDirection;
        // if(isSharpTurn)
        //     Debug.Log("The player took a hard turn");
        // return isSharpTurn;
        return false; 
    }

    public virtual bool IsGrounded<T>(T obj, LayerMask groundLayer) where T: UnityEngine.Object
    {
        
        Vector3 checkPosition;
        float checkDistance = 0.3f; // Increased to handle animation lift 

        if(obj is Collider col)
        {
            // Start check from slightly above the bottom of the collider
            checkPosition = col.bounds.center - new Vector3(0, col.bounds.extents.y - 0.1f, 0);
            Debug.DrawRay(checkPosition, Vector3.down * checkDistance, Color.green, 1.0f);
        }
        else if(obj is GameObject gobj)
        {
            checkPosition = gobj.transform.position; 
        }
        else if(obj is Transform transform)
        {
            checkPosition = transform.position;
        }
        else
        {
            //TODO: Create a better method was called with the wrong type, that's a different type of false
            Debug.LogWarning($"Unsuproted Type: {typeof(T)}"); //? Compile Time Info
            return false;
        }
        return Physics.Raycast(checkPosition, Vector3.down, checkDistance, groundLayer);
    }
}


public interface IGrounded 
{
    
    //Checks where the feet of the player character is
    public bool IsGrounded<T>(T obj,LayerMask groundLayer) where T:Object;
}

public class PlayerMovement : MovmementBase
{
    [SerializeField] private Camera _cam;
    private Vector2 input;
    private Vector2 lastInput;

    private Vector3 moveVec;

    [SerializeField] private Animator anim;

    public override void HandleMove(Vector3 moveVec, float velocity)
    {
        // float y = transform.position.y;
        // Debug.Log($"The y is {y}");

        this.moveVec = moveVec;
        base.HandleMove(moveVec, velocity);

    }

    public override bool HasSharpTurn()
    {
        bool isSharpTurn = Vector2.Dot(input.normalized, lastInput.normalized) == -1f;
        if(isSharpTurn)
            Debug.Log("Sharp turn detected");
        lastInput = input;
        return isSharpTurn;
    }

    public void Jump()
    {
        StartCoroutine(JumpCoroutine());
    }

    private System.Collections.IEnumerator JumpCoroutine()
    {
        Debug.Log("=== JUMP STARTED ===");
        Debug.Log($"moveVec: {moveVec}, magnitude: {moveVec.magnitude}");
        Debug.Log($"input: {input}, magnitude: {input.magnitude}");
        
        // Disable root motion for physics-based jump
        anim.applyRootMotion = false;
        
        Rigidbody rb = this.GetComponent<Rigidbody>();
        Debug.Log($"Rigidbody mass: {rb.mass}, linearDamping: {rb.linearDamping}, useGravity: {rb.useGravity}");
        
        // Simple hardcoded diagonal jump
        float jumpHeight = 15f; // Upward force
        float forwardForce = 10f; // Forward force
        
        Vector3 jumpVector;
        
        // If player is moving, jump diagonally forward
        if (moveVec.magnitude > 0.1f)
        {
            // Use the actual movement direction for jump, but ensure strong upward force
            jumpVector = new Vector3(
                moveVec.normalized.x * forwardForce,  // Forward in movement direction
                jumpHeight,                           // Strong upward force
                moveVec.normalized.z * forwardForce   // Forward in movement direction
            );
            Debug.Log($"Diagonal jump applied: horizontal={forwardForce}, vertical={jumpHeight}");
        }
        else
        {
            // If not moving, jump straight up with extra force
            float stationaryJumpHeight = jumpHeight * 1.2f; // 20% more force for stationary jump
            jumpVector = Vector3.up * stationaryJumpHeight;
            Debug.Log($"Stationary jump applied with force: {stationaryJumpHeight}");
        }
        
        Debug.DrawRay(new Vector3(transform.position.x, transform.position.y + 2, transform.position.z), jumpVector, Color.blue, 2.0f);
        Debug.Log($"Applying force: {jumpVector} with ForceMode.Impulse");
        rb.AddForce(jumpVector, ForceMode.Impulse);
        Debug.Log($"Rigidbody velocity after force: {rb.linearVelocity}");
        
        // Wait a short moment to ensure we're airborne
        yield return new WaitForSeconds(0.1f);
        
        // Wait until we're grounded again
        LayerMask groundLayer = LayerMask.GetMask("Ground"); // Adjust layer name as needed
        while (!IsGrounded(GetComponent<Collider>(), groundLayer))
        {
            yield return new WaitForFixedUpdate();
        }
        
        // Small delay to ensure stable landing
        yield return new WaitForSeconds(0.1f);
        
        // Re-enable root motion once we've landed
        anim.applyRootMotion = true;
        Debug.Log("Jump completed - root motion re-enabled");
    }


    public Vector3 GetInputVector(Vector2 input)
    {
        this.input = input;
        //1: Flatten camera forward/right to the XZ plane
        Transform camT = _cam.transform;
        Vector3 camForward = camT.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = camT.right;
        camRight.y = 0;
        camRight.Normalize();
        //2: Build world-space move vector
        return camForward * input.y + camRight * input.x;
    }



}
