using UnityEngine;
using GraphHelper;
using Locomotion_States;

public class StateMachine : MonoBehaviour
{
    public int speed = 0;
    public Graph GroundedGraph;
    void Awake()
    {
        //! Important  -> create all the roots(SuperStates) you think you would need first
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Node IsGrounded = new Node(null, "isGrounded", new IsGrounded());
        Node IsInAir = new Node(null, "isInAir", new IsInAir());
        Node WalkNode = new Node(IsGrounded, "Walk", new Walk());
        Node RunNode = new Node(IsGrounded, "Run", new Run());
        Debug.Log("Creating the Graph");

        GroundedGraph = GraphBuilder.Create(IsGrounded)
                                        .AddChild(IsGrounded, WalkNode, "Walk")
                                            .AddTransition(IsGrounded, WalkNode,  (s) => s < 5, ()=>speed) // Walk if speed < 5 (using ref)
                                        .AddChild(IsGrounded, RunNode, "Run")
                                            .AddTransition(IsGrounded, RunNode,  (s) => s >= 5,  ()=>speed) // Run if speed >= 5 (using ref)
                                            .Generate();
        GroundedGraph.root = IsGrounded;
       

        Debug.Log("=== Individual Nodes ===");
        Debug.Log($"IsGrounded child count: {IsGrounded.children.Count}");
    }


    // Update is called once per frame
    void Update()
    {
        GroundedGraph.root.GetNextNode();
        speed++;
    }
}
