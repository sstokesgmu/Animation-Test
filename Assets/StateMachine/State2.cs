using System.Collections.Generic;
using System;
using UnityEngine;
using NUnit.Framework;

public struct EdgeTransition 
{
    public Func<bool> condition;
    public Node targetNode;
}
public class Node
{
    //If parent is null then 
    public Node parent = null;
    public IStatable value = null; //Make this type unspecific 
    public List<Node> children = new List<Node>();
    public List<EdgeTransition> transitionList = new List<EdgeTransition>();
    
    public Node(Node parent = null, IStatable value = null)
    {
        //So the node tells the parent it is a child of it?
        this.parent = parent;
        this.value = value;
        if(parent != null)
            parent.children.Add(this);
    }
    public Node GetParent() => parent;
    public List<Node> GetChildren() => children;

    public void AddTransionToList(Func<bool> condition, Node targetNode)
    {
        EdgeTransition newTransition;
        newTransition.condition = condition;
        newTransition.targetNode = targetNode; 
        transitionList.Add(newTransition);
    }
    //If the value is not a primative 


    //Look through conditions and see if the conditions in the transition list return true
    public Node GetNextNode()
    {
        foreach (EdgeTransition possibleTrans in transitionList)
        {
            if (possibleTrans.condition())
            {
                //Go to the next transition seen that to the graph/StateMachine 
                return possibleTrans.targetNode;

            }
        }
        return null;
    }
}

public class Graph
{
    public Node root;
    public Graph(Node root)
    {
        this.root = root;
    }
}


public interface IStatable
{
    void Enter();
    void Execute();
    void Exit();
}

public class IsGrounded: IStatable
{
    public void Enter()
    {
        Debug.Log("IsGrounded: Entering grounded state");
    }
    
    public void Execute()
    {
        Debug.Log("IsGrounded: Executing grounded state logic");
    }
    
    public void Exit()
    {
        Debug.Log("IsGrounded: Exiting grounded state");
    }
}

public class IsInAir: IStatable
{
    public void Enter()
    {
        Debug.Log("IsInAir: Entering in-air state");
    }
    
    public void Execute()
    {
        Debug.Log("IsInAir: Executing in-air state logic");
    }
    
    public void Exit()
    {
        Debug.Log("IsInAir: Exiting in-air state");
    }
}



public class StateMachine: MonoBehaviour
{
    //Context State - Root States is groundend is Inair
    void Awake()
    {
        Node IsGrounded = new Node(null, new IsGrounded());
        IsGrounded.AddTransionToList((int arg) =>
        {
            if (arg < 0)
                return true;
            else
                return false;
        }, new Node(null, new IsInAir()));

    }
    //Tick -> how long we are in the state for 

    //Hold a ref to the paramters that the character needs 
}