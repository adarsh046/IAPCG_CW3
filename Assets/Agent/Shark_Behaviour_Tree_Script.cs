// ECS7016P - Interactive Agents and Procedural Generation
// Adarsh Gupta - 220570653
// References:
// 1) Meniku/NPBehave: Event Driven Behavior Trees for Unity 3D, GitHub. Available at: https://github.com/meniku/NPBehave
// 2) Sturdyspoon/Unity-movement-ai: A unity library for common movement AI, GitHub. Available at: https://github.com/sturdyspoon/unity-movement-ai 
// 3) Scripts of Lab 1: Behaviour Trees and Lab 5: Steering Behaviours

using UnityEngine;
using NPBehave;
using UnityMovementAI;

public class Shark_Behaviour_Tree_Script : MonoBehaviour
{
    public Transform diver;
    public Transform mine;
    private Root tree;                  // Agents' behaviour tree
    private Blackboard blackboard;      // Agents' behaviour blackboard

    SteeringBasics steeringBasics;
    Wander2 wander;

    void Start()
    {
        steeringBasics = GetComponent<SteeringBasics>();
        wander = GetComponent<Wander2>();
        tree = CreateBehaviourTree();
        blackboard = tree.Blackboard;
        tree.Start();
    }

    private Root CreateBehaviourTree()
    {
        return Shark();
    }

    private void UpdatePerception()
    {
        Vector3 diverPos = diver.position;
        Vector3 minePos = mine.position;
        Vector3 localToDiverPos = transform.InverseTransformPoint(diverPos);
        Vector3 localToMinePos = transform.InverseTransformPoint(minePos);
        blackboard["diverDistance"] = localToDiverPos.magnitude;
        blackboard["mineDistance"] = localToMinePos.magnitude;
    }

    private Root Shark()
    {
        return new Root(
        new Service(UpdatePerception,
            new Selector(
               new BlackboardCondition("diverDistance", Operator.IS_SMALLER, 2.0f, Stops.IMMEDIATE_RESTART,
               diverDestroy()),
               new BlackboardCondition("diverDistance", Operator.IS_SMALLER_OR_EQUAL, 20.0f, Stops.IMMEDIATE_RESTART,
               Seek(diver)),
               new BlackboardCondition("mineDistance", Operator.IS_SMALLER, 2.0f, Stops.IMMEDIATE_RESTART,
               mineDestroy()),
               new BlackboardCondition("mineDistance", Operator.IS_SMALLER_OR_EQUAL, 20.0f, Stops.IMMEDIATE_RESTART,
               Seek(mine)),
               new BlackboardCondition("mineDistance", Operator.IS_GREATER, 20.0f, Stops.IMMEDIATE_RESTART,
               Wander())
               )));
    }

    private void destroyDiver()
    {
        // Teleport the diver at a random position, so it looks like he's dead
        Vector3 newPosition = new Vector3(UnityEngine.Random.Range(10f, 55f), 0.0f, UnityEngine.Random.Range(20f, 100f));
        diver.transform.position = newPosition;
    }

    private Node diverDestroy()
    {
        return new Action(() => destroyDiver());
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

    private void destroyMine()
    {
        // Teleport shark at the random position, so it looks like it's dead
        Vector3 newPosition = new Vector3(UnityEngine.Random.Range(10f, 55f), 0.0f, UnityEngine.Random.Range(20f, 100f));
        transform.position = newPosition;


        // Teleport mine at some random position, so it looks like mine has exploded and changed position
        Vector3 newMinePosition = new Vector3(UnityEngine.Random.Range(10f, 55f), -0.2f, UnityEngine.Random.Range(20f, 100f));
        mine.transform.position = newMinePosition;
    }

    private Node mineDestroy()
    {
        return new Action(() => destroyMine());
    }

    private void wanderFunction()
    {
        // From Wander2Unit.cs
        Vector3 accel = wander.GetSteering();
        steeringBasics.Steer(accel);
        steeringBasics.LookWhereYoureGoing();
    }

    private Node Wander()
    {
        return new Action(() => wanderFunction());
    }

}