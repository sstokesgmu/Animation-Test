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

        if(obj is Collider col)
        {
            checkPosition = col.bounds.center - new Vector3(0, col.bounds.extents.y, 0);
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
        return Physics.Raycast(checkPosition, Vector3.down, 0.1f, groundLayer);
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

    public override void HandleMove(Vector3 moveVec, float velocity)
    {
        // float y = transform.position.y;
        // Debug.Log($"The y is {y}");

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
        bool isRootMotionActive = this.GetComponent<Animator>().applyRootMotion = false; 
        Rigidbody rb = this.GetComponent<Rigidbody>();
        rb.AddForce(Vector3.up * 10f, ForceMode.Impulse);
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
