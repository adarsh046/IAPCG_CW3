// ECS7016P - Interactive Agents and Procedural Generation
// Adarsh Gupta - 220570653
// References:
// 1) Meniku/NPBehave: Event Driven Behavior Trees for Unity 3D, GitHub. Available at: https://github.com/meniku/NPBehave
// 2) Sturdyspoon/Unity-movement-ai: A unity library for common movement AI, GitHub. Available at: https://github.com/sturdyspoon/unity-movement-ai 
// 3) Scripts of Lab 1: Behaviour Trees and Lab 5: Steering Behaviours

using UnityEngine;
using NPBehave;
using UnityMovementAI;

public class Diver_Behaviour_Tree_Script : MonoBehaviour
{
    public Transform treasure;
    public Transform shark;
    public Transform mine;
    public Transform mermaid;
    private Root tree;                  // Agents' behaviour tree
    private Blackboard blackboard;      // Agents' behaviour blackboard

    SteeringBasics steeringBasics;
    Flee flee;
    Wander2 wander;

    void Start()
    {
        steeringBasics = GetComponent<SteeringBasics>();
        flee = GetComponent<Flee>();
        wander = GetComponent<Wander2>();
        tree = CreateBehaviourTree();
        blackboard = tree.Blackboard;
        tree.Start();
    }

    private Root CreateBehaviourTree()
    {
        return Diver();
    }

    private void UpdatePerception()
    {
        Vector3 treasurePos = treasure.position;
        Vector3 sharkPos = shark.position;
        Vector3 minePos = mine.position;
        Vector3 mermaidPos = mermaid.position;
        Vector3 localToTreasurePos = transform.InverseTransformPoint(treasurePos);
        Vector3 localToSharkPos = transform.InverseTransformPoint(sharkPos);
        Vector3 localToMinePos = transform.InverseTransformPoint(minePos);
        Vector3 localToMermaidPos = transform.InverseTransformPoint(mermaidPos);
        blackboard["treasureDistance"] = localToTreasurePos.magnitude;
        blackboard["sharkDistance"] = localToSharkPos.magnitude;
        blackboard["mineDistance"] = localToMinePos.magnitude;
        blackboard["mermaidDistance"] = localToMermaidPos.magnitude;
    }

    private Root Diver()
    {
        return new Root(
        new Service(UpdatePerception,
            new Selector(
               new BlackboardCondition("sharkDistance", Operator.IS_SMALLER_OR_EQUAL, flee.panicDist, Stops.IMMEDIATE_RESTART,
               Flee(shark)),
               new BlackboardCondition("mermaidDistance", Operator.IS_SMALLER_OR_EQUAL, 20.0f, Stops.IMMEDIATE_RESTART,
               Seek(mermaid)),
               new BlackboardCondition("mineDistance", Operator.IS_SMALLER_OR_EQUAL, 15.0f, Stops.IMMEDIATE_RESTART,
               Flee(mine)),
               new BlackboardCondition("treasureDistance", Operator.IS_SMALLER, 2.0f, Stops.IMMEDIATE_RESTART,
               treasureDestroy()),
               new BlackboardCondition("treasureDistance", Operator.IS_SMALLER_OR_EQUAL, 20.0f, Stops.IMMEDIATE_RESTART,
               Seek(treasure)),
               new BlackboardCondition("treasureDistance", Operator.IS_GREATER, 20.0f, Stops.IMMEDIATE_RESTART,
               nodeWander())
               )));   
    }

    private Node Flee(Transform gameObject)
    {
        return new Action(() => fleeFrom(gameObject));
    }

    private void fleeFrom(Transform gameObject)
    {
        float distanceToObject = Vector3.Distance(transform.position, gameObject.position);
        if (distanceToObject <= flee.panicDist)
        {
            // From FleeUnit.cs
            Vector3 accel = flee.GetSteering(gameObject.position);
            steeringBasics.Steer(accel);
            steeringBasics.LookWhereYoureGoing();
        }
        else
        {
            wanderFunction();
        }
    }

    private void destroyTreasure()
    {
        // Teleport treasure at some random position, so it looks like diver has looted the treasure
        Vector3 newPosition = new Vector3(UnityEngine.Random.Range(10f, 55f), -0.2f, UnityEngine.Random.Range(20f, 100f));
        treasure.transform.position = newPosition;
    }

    private Node treasureDestroy()
    {
        return new Action(() => destroyTreasure());
    }

    private void seekTo(Transform gameObject)
    {
        // From SeekUnit.cs
        Vector3 accel = steeringBasics.Seek(gameObject.position);
        steeringBasics.Steer(accel);
        steeringBasics.LookWhereYoureGoing();
    }

    private Node Seek(Transform gameObject)
    {
        return new Action(() => seekTo(gameObject));
    }

    private void wanderFunction()
    {
        // From Wander2Unit.cs
        Vector3 accel = wander.GetSteering();
        steeringBasics.Steer(accel);
        steeringBasics.LookWhereYoureGoing();
    }

    private Node nodeWander()
    {
        return new Action(() => wanderFunction());
    }

}