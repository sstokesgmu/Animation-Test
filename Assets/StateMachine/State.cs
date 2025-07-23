/*
    State(abstract)
    ├── GroundedState   -> if the grounded state can attack share that functionality to children, unless the child has constraints
    │   ├── IdleState
    │   └── WalkingState
    └── AirborneState
        ├── JumpingState
        └── FallingState
*/

using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Video;

public abstract class State
{
    protected StateMachine machine;
    protected State parent;
    protected List<State> children = new();

    //Constructor 
    public State(StateMachine machine, State parent = null)
    {
        this.machine = machine; this.parent = parent;

        //TODO: if this instance of the state has a parent add it as a child of the parent 
    }

    public State GetParent() => parent;

    public void AddChild(State child)
    {
        if (!children.Contains(child))
            children.Add(child);
    }

    public virtual void Enter()
    {
        Debug.Log($"Enter {GetType().Name}");
    }

    public abstract void Execute();


    public virtual void Exit()
    {

    }

    public virtual bool CanTransitionTo(State target)
    {
        //Check target against a list of constraints
        return true;
    }
}

public class StateMachine
{
    public State CurrentState { get; private set; }

    public void ChangeState(State newState)
    {
        if (CurrentState != null && !CurrentState.CanTransitionTo(newState))
        {
            Debug.LogWarning($"Invalid transition form {CurrentState.GetType().Name} to {newState}");
            return;
        }
        //ToDo: Exit to the common ascestor 

        CurrentState = newState;
        CurrentState.Enter();
    }

    public void Update()
    {
        CurrentState?.Execute();
    }

    private void ExitToCommonAnscestor(State from, State to)
    {
        var fromLineage = GetLineage(from);
        var toLineage = GetLineage(to);

        int i = 0;
        while (i < fromLineage.Count && i < toLineage.Count && fromLineage[i] == toLineage[i])
            i++;

        for (int j = fromLineage.Count - 1; j >= i; j--)
            fromLineage[j].Exit();
    }

    private List<State> GetLineage(State state)
    {
        var lineage = new List<State>();
        while (state != null)
        {
            lineage.Insert(0, state);
            state = state.GetParent();
        }
        return lineage;
    }

}

public class GroundedState : State
{
    public GroundedState(StateMachine machine) : base(machine) { }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Player is on the ground");
    }

    public override void Execute()
    {
        Debug.Log("Grounded  base logic");
    }
}

public class WalkingState : State
{

    public WalkingState(StateMachine machine, State parent) : base(machine, parent) { }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Player starts walking");
    }

    public override void Execute()
    {
        Debug.Log("Player is walking...");
    }

    public override void Exit()
    {
        Debug.Log("Player stops walking");
        base.Exit();
    }
}

public class PlayerStateMachine : MonoBehaviour
{
    private StateMachine machine;

    private void Start()
    {
        machine = new StateMachine();

        State grounded = new GroundedState(machine);
        State walking = new WalkingState(machine, grounded);

        machine.ChangeState(walking);
    }

    private void Update()
    {
        machine.Update();
    }
}