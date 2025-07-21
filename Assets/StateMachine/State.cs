using ActorMovement;
using Unity.VisualScripting;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public State state; //Current State 
    public State fallback; //Fallback reset state 
    public State[] possibleState; //TODO: I feel like this info should be given by the controller,



    public void Set(State newState, bool forceReset = false)
    {
        if (state != newState || forceReset)
        {
            //Stop current state
            state.Exit();
            state = newState;
            state.Enter();

            //Start the new state 
        }
    }

    void Update()
    {
        Debug.Log("hello");

    }

}

public abstract class State
{
    //TODO: Add a reference to the statemachine
    public State(){}

    public bool isComplete { get; protected set; }
    protected float totalTimeToComplete = 0f;

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }


    //Send to the machine
    public void Execute()
    {
        //Todo: StateMachine.NewState(this)
    }
}

public class MoveState : State, IMovable
{
    public MoveState(float moveSpeed) : base()
    {
        Debug.Log("Creating move State");
    }
    public override void Enter()
    {
        Debug.Log("Entering the move state");
    }

    public override void Update()
    {
        Debug.Log("Doing stuff in the move state");
    }

    public override void Exit()
    {
        Debug.Log("Exiting the Move State");
    }


    //Interface methods 

    //     Vector3 GetInputVector()
    // {
    // // 1) Flatten camera forward/right to XZ plane
    // Transform camT = Camera.main.transform;
    // Vector3 camForward = camT.forward;
    // camForward.y = 0f;
    // camForward.Normalize();

    // Vector3 camRight = camT.right;
    // camRight.y = 0f;
    // camRight.Normalize();

    // // 2) Read raw 2D input (x = A/D or left stick X, y = W/S or left stick Y)
    // Vector2 input2D = move.action.ReadValue<Vector2>();

    // // 3) Build world‑space move vector
    // Vector3 worldMove = camForward * input2D.y + camRight * input2D.x;

    // return worldMove;
    // }

    //TODO: Project the arugments to the basis Vector;
    public Vector3 GetMovementVector(Vector3 basis, params Vector3[] arguments)
    {
        Vector3 result = Vector3.zero;
        foreach (Vector3 vector in arguments)
        {
            Debug.Log("Vector is : " + vector);
        }
        return basis + result;
    }

}



namespace ActorMovement
{
    interface IMovable
    {
        //Take Movement Vector 
        public Vector3 GetMovementVector(Vector3 basis, params Vector3[] arguments);
        //Todo: IDK should this be a defined method or use a defined method  like flatten all the augument vectors to the same axis dimension of the basis
    }
}
