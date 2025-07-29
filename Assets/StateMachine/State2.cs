using System.Collections.Generic;
using System;
using UnityEngine;

using static StateEvents;


namespace GraphHelper{

    public struct EdgeTransition
    {
        public Func<bool> condition;
        public Node targetNode;
    }
    public class Node
    {
        //If parent is null then 
        public Node parent = null;

        public string name = "";
        public IStatable value = null; //Make this type unspecific 
        public Dictionary<string,Node> children = new Dictionary<string,Node>();
        public List<EdgeTransition> transitionList = new List<EdgeTransition>();


        //!Rewrite the arguments
        public Node(Node parent = null, string name = null, IStatable value = null)
        {
            //So the node tells the parent it is a child of it?
            this.parent = parent;
            this.value = value;
            this.name = name;
            if (parent != null)
            {
                Debug.Log($"The child has a parent called: {parent.name}");

                //TODO: Where do we want to add the child in the constructor or as a Fluent Method
                // parent.children[name] = this; //! Notice the Child is telling the parent "I am yor child"
            }
            else
            {
                Debug.Log($"The node is the root node: {this.name}");
            }
                
        }
        public Node GetParent() => this.parent;
        public string GetName() => this.name;
        //public Node GetChildren() => children;

        public void AddTransionToList(Node source, Node targetNode, Func<bool> condition)
        {
            EdgeTransition newTransition;
            newTransition.condition = condition;
            newTransition.targetNode = targetNode;
            source.transitionList.Add(newTransition);
        }
        //If the value is not a primative 


        //Look through conditions and see if the conditions in the transition list return true
        //TODO: Figure out when do keeping track of where we are on the graph
        public Node GetNextNode()
        {
            foreach (EdgeTransition possibleTrans in transitionList)
            {
                if (possibleTrans.condition())
                {
                    //Debug.Log($"Transition condition met: {possibleTrans.targetNode.name}");

                    //Go to the next transition seen that to the graph/StateMachine 
                   return possibleTrans.targetNode;

                }
            }
            return null; //TODO Break Move back up a to the parent level 
        }
    }

    public class Graph
    {
       //TODO: What does this do -> private static readonly Func<bool> ALWAYSTRUE = () => true;
        public Node root;
        public Node currentNode;
        //Traversals or searches 
        
        public void PrintGraph(Node startNode = null)
        {
            if (startNode == null) startNode = root;
            if (startNode == null) 
            {
                Debug.Log("Graph is empty - no root node set");
                return;
            }
            
            Debug.Log("=== GRAPH STRUCTURE ===");
            PrintNodeRecursive(startNode, 0);
            Debug.Log("=== END GRAPH ===");
        }

        private void PrintNodeRecursive(Node node, int depth)
        {
            if (node == null) return;

            string indent = new string(' ', depth * 2);
            Debug.Log($"{indent}Node: {node.name} (Type: {node.value?.GetType().Name})");

            // Print transitions
            if (node.transitionList.Count > 0)
            {
                Debug.Log($"{indent}  Transitions:");
                foreach (var transition in node.transitionList)
                {
                    Debug.Log($"{indent}    -> {transition.targetNode.name}");
                }
            }

            // Print children
            if (node.children.Count > 0)
            {
                Debug.Log($"{indent}  Children:");
                foreach (var child in node.children)
                {
                    PrintNodeRecursive(child.Value, depth + 2);
                }
            }

        }
        public void Update()
            {
                if (currentNode == null)
                    currentNode = root;

                

                //! Now this op seems to only work on transitions inside graphs (node to node) but not graph to graph 
                Node nextNode = currentNode?.GetNextNode();
                if (nextNode != null && nextNode != currentNode)
                {
                    (currentNode.value as IStatable)?.Exit();
                    //Debug.Log($"Transitioning from {currentNode.name} to {nextNode.name}");
                    currentNode = nextNode;
                    (currentNode.value as IStatable)?.Enter();
                }
            }
        
    }

    public interface IGraphBuilder
    {
        public IGraphBuilder AscendToParent(Node currentNode);
        public IGraphBuilder AddChild(Node parent, Node Child, string childName);
        public IGraphBuilder AddTransition(Node source, Node target, Func<bool> condition = null);
        public IGraphBuilder AddTransition<T>(Node source, Node target, Func<T, bool> condition, Func<T> parameter1);
        public IGraphBuilder AddTransition<T1, T2>(Node source, Node target, Func<T1, T2, bool> condition, Func<T1> parameter1, Func<T2> parameter2);
        public IGraphBuilder DescendToChild(Node parent, string childName);

        public Graph Generate();

    }

    public class GraphBuilder : IGraphBuilder
    {
        private Graph graph = new Graph();

        public Node root = null;
        private Node currentNode = null;
        GraphBuilder(Node root)
        {
            this.root = root;
            this.currentNode = root;
            
        }
        public static IGraphBuilder Create(Node root) => new GraphBuilder(root); //? What is this and can I setup the current node in this static method
    
        public IGraphBuilder AscendToParent(Node currentNode)
        {
            //! If the parent equals null then we are at the root of the graph 
            this.currentNode = currentNode?.parent ?? currentNode;
            return this;
        } 
        public IGraphBuilder AddChild(Node parent, Node child, string childName)
        {
            Debug.Log($"Adding the child name {childName}");
            currentNode.children.Add(childName, child);
            this.currentNode = parent;

            return this;
        }

        public IGraphBuilder DescendToChild(Node parent, string childName)
        {

            Debug.LogWarning("Hello");
            Dictionary<string, Node> _nodeLookup = parent.children;
            if (_nodeLookup.Count != 0)
            {
                Node childNode = null;
                if (_nodeLookup.TryGetValue(childName, out childNode))
                    this.currentNode = childNode;
                else //? How do I handle this do we keep going with the method chaining
                    Debug.LogWarning($"The name {childName} does not exist on the current graph");
                return this;
            }
            else
                return this;    
        }

        public IGraphBuilder AddTransition(Node source, Node target, Func<bool> condition = null)
        {
            condition ??= () => true;
            source.AddTransionToList(source, target, condition);
            return this; 
        }
        
        // Overload for functions with one parameter
        public IGraphBuilder AddTransition<T>(Node source, Node target, Func<T, bool> condition, Func<T> parameterProvider)
        {
            // Convert parameterized function to parameterless by capturing the parameter
            Func<bool> parameterlessCondition = () => condition(parameterProvider());
            source.AddTransionToList(source, target, parameterlessCondition);
            return this;
        }
        
        
        // Overload for functions with two parameters
        public IGraphBuilder AddTransition<T1, T2>(Node source, Node target, Func<T1, T2, bool> condition, Func<T1> paramProvider1, Func<T2> paramProvider2)
        {
            Func<bool> parameterlessCondition = () => condition(paramProvider1(), paramProvider2());
            source.AddTransionToList(source, target, parameterlessCondition);
            return this;
        }
        public Graph Generate()
        {
            //graph.root = root;
            return graph;
        }
    } 
}

namespace Locomotion_States
{
    public class IsGrounded : IStatable
    {
        public void Enter()
        {
            //Debug.Log("IsGrounded: Entering grounded state");
            NotifyStateChange("Grounded");
        }

        public void Execute()
        {
            Debug.Log("IsGrounded: Executing grounded state logic");
        }

        public void Exit()
        {
           // Debug.Log("IsGrounded: Exiting grounded state");
        }
    }

    public class IsInAir : IStatable
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

    public class Idle : IStatable
    {

        public void Enter()
        {
            Debug.Log("Entering the Idle State");
        }
        public void Execute()
        {
            Debug.Log("Executing inside the Idle State");
        }

        public void Exit()
        {
            Debug.Log("Exiting the Idle State");
        }
    }

    public class Walk : IStatable
    {

        private Animator animator;
        private int walkingHash;

        public Walk(Animator anim, int walkHash)
        {
            animator = anim;
            walkingHash = walkHash;
        }

        public void Enter()
        {
            NotifyStateChange("Walking");
            if (!animator.GetBool(walkingHash))
                animator.SetBool(walkingHash, true);
        }
        public void Execute()
        {

        }
        public void Exit()
        {
            if (animator.GetBool(walkingHash))
                animator.SetBool(walkingHash, false);
        }
    }

    public class Run : IStatable
    {
        private Animator animator;
        private int runningHash;
        public Run(Animator anim, int runHash)
        {
            animator = anim;
            runningHash = runHash;
        }
        public void Enter()
        {
            NotifyStateChange("Run");
            if (!animator.GetBool(runningHash))
                animator.SetBool(runningHash, true);
        }
        public void Execute()
        {
            Debug.Log("Executing the Running State");
        }
        public void Exit()
        {
           // Debug.Log("Exiting the Running State");
            if (animator.GetBool(runningHash))
                animator.SetBool(runningHash, false);
        }
    }
}


public interface IStatable
{
    
    void Enter();
    void Execute();
    void Exit();
}

// public class StateMachine : MonoBehaviour
