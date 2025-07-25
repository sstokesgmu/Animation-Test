using System.Collections.Generic;
using System;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.Animations;
using StateMachineHelper;
using Unity.Mathematics;


namespace StateMachineHelper{

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
        public Dictionary<string,Node> children = new Dictionary<string,Node>();
        public List<EdgeTransition> transitionList = new List<EdgeTransition>();
        
        
        //!Rewrite the arguments
        public Node(Node parent = null, string name = null, IStatable value = null)
        {
            //So the node tells the parent it is a child of it?
            this.parent = parent;
            this.value = value;
            if (parent != null)
                parent.children[name] = this; //! Notice the Child is telling the parent "I am yor child"
        }
        public Node GetParent() => parent;
        //public Node GetChildren() => children;

        public void AddTransionToList(Node source,Node targetNode, Func<bool> condition)
        {
            EdgeTransition newTransition;
            newTransition.condition = condition;
            newTransition.targetNode = targetNode; 
            source.transitionList.Add(newTransition);
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
            return null; //TODO Break Move back up a to the parent level 
        }
    }

    public class Graph
    {
        private static readonly Func<bool> ALWAYSTRUE = () => true;
        public Node root;
        //Traversals or searches 
    }

    public interface IGraphBuilder
    {
        public Node SelectParent(Node parent);
        public IGraphBuilder AddChild(Node parent, Node Child, string childName);
        public Node AddTransition(Node source, Node target, Func<bool> condition = null);
        public IGraphBuilder SetToNextNodeInLininage(Node parent, string childName);
        public Graph Generate();

    }

    public class GraphBuilder : IGraphBuilder
    {
        private Graph graph = new Graph();
        private Node currentNode = null;
        public static IGraphBuilder Create() => new GraphBuilder(); //? What is this 

        public Node SelectParent(Node parent)
        {
            graph.root = parent;
            return parent;
        }

        public IGraphBuilder AddChild(Node parent, Node child, string childName)
        {
            parent.children.Add(childName, child);
            this.currentNode = parent;
            return this;
        }

        public IGraphBuilder SetToNextNodeInLininage(Node parent, string childName)
        {
            Dictionary<string, Node> _nodeLookup = parent.children;
            if (_nodeLookup.Count != 0)
            {
                Node childNode = null;
                if (_nodeLookup.TryGetValue(childName, out childNode))
                    this.currentNode = childNode;
                return this;
            }
            else
                return this;
            //else return null -> huge error can we do a graceful bail out        
        }

        //How are you envisioning this, where are you in the graph when you call this 




        public Node AddTransition(Node source, Node target, Func<bool> condition = null)
        {
            condition ??= () => true;
            source.AddTransionToList(source, target, condition);
            return source.parent;//!This is not right an scary 
        }
        public Graph Generate()
        {
            return graph;
        }
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

public class Walk : IStatable {
    public void Enter()
    {

    }
    public void Execute()
    {

    }
    public void Exit()
    {

    }
}


public class StateMachine : MonoBehaviour
{
    //Context State - Root States is groundend is Inair
    void Awake()
    {

        Node IsGrounded = new Node(null, "isGrounded", new IsGrounded());
        Graph groundedGraph = GraphBuilder.Create()
                    .AddChild(IsGrounded, new Node(IsGrounded, "Walk", new Walk()), "Walk")
                    .SetToNextNodeInLininage(IsGrounded, "Walk")
                    .Generate();
                    
    }
    //Tick -> how long we are in the state for 

    //Hold a ref to the paramters that the character needs 
}