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
        new Service(0.5f, UpdatePerception,
            new Selector(
               new BlackboardCondition("diverDistance", Operator.IS_SMALLER, 2.0f, Stops.IMMEDIATE_RESTART,
               diverDestroy()),
               new BlackboardCondition("diverDistance", Operator.IS_SMALLER_OR_EQUAL, 20.0f, Stops.IMMEDIATE_RESTART,
               diverSeek()),
               //new BlackboardCondition("diverDistance", Operator.IS_GREATER, 20.0f, Stops.IMMEDIATE_RESTART,
               //nodeWander()),
               new BlackboardCondition("mineDistance", Operator.IS_SMALLER, 2.0f, Stops.IMMEDIATE_RESTART,
               mineDestroy()),
               new BlackboardCondition("mineDistance", Operator.IS_SMALLER_OR_EQUAL, 20.0f, Stops.IMMEDIATE_RESTART,
               mineSeek()),
               new BlackboardCondition("mineDistance", Operator.IS_GREATER, 20.0f, Stops.IMMEDIATE_RESTART,
               nodeWander())
               )));
    }

    private void destroyDiver()
    {
        // Get the current position of the target
        Vector3 currentPosition = diver.transform.position;

        // Teleport the target at a random position, so it looks like he's dead
        Vector3 newPosition = new Vector3(UnityEngine.Random.Range(10f, 55f), 0.3f, UnityEngine.Random.Range(20f, 100f));
        diver.transform.position = newPosition;

    }

    private Node diverDestroy()
    {
        return new Action(() => destroyDiver());
    }


    private void seekDiver()
    {
        Vector3 accel = steeringBasics.Seek(diver.position);
        steeringBasics.Steer(accel);
        steeringBasics.LookWhereYoureGoing();
    }

    private Node diverSeek()
    {
        return new Action(() => seekDiver());
    }

    private void destroyMine()
    {
        // Teleport shark at the random position, so it looks like it's dead
        Vector3 newPosition = new Vector3(UnityEngine.Random.Range(10f, 55f), 0.3f, UnityEngine.Random.Range(20f, 100f));
        transform.position = newPosition;


        // Teleport mine at some random position, so it looks like mine has changed position
        Vector3 newMinePosition = new Vector3(UnityEngine.Random.Range(10f, 55f), 0.3f, UnityEngine.Random.Range(20f, 100f));
        mine.transform.position = newMinePosition;
    }

    private Node mineDestroy()
    {
        return new Action(() => destroyMine());
    }

    private void seekMine()
    {
        Vector3 accel = steeringBasics.Seek(mine.position);
        steeringBasics.Steer(accel);
        steeringBasics.LookWhereYoureGoing();
    }

    private Node mineSeek()
    {
        return new Action(() => seekMine());
    }

    private Root Wander()
    {
        return new Root(
        new Service(0.5f, UpdatePerception,
            new Selector(
               new BlackboardCondition("targetDistance", Operator.IS_GREATER, 20.0f, Stops.IMMEDIATE_RESTART,
               nodeWander()),
               new BlackboardCondition("targetDistance", Operator.IS_SMALLER, 20.0f, Stops.IMMEDIATE_RESTART,
               diverSeek())
               )));
    }
    private void wanderFunction()
    {
        Vector3 accel = wander.GetSteering();
        steeringBasics.Steer(accel);
        steeringBasics.LookWhereYoureGoing();
    }

    private Node nodeWander()
    {
        return new Action(() => wanderFunction());
    }

}