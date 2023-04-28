using UnityEngine;
using NPBehave;
using UnityMovementAI;

public class Mermaid_Behaviour_Tree_Script : MonoBehaviour
{
    public Transform treasure;
    public Transform mine;
    public Transform diver;
    private Root tree;              // Agents' behaviour tree
    private Blackboard blackboard;  // Agents' behaviour blackboard

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
        return Mermaid();
    }

    private void UpdatePerception()
    {
        Vector3 treasurePos = treasure.position;
        Vector3 minePos = mine.position;
        Vector3 diverPos = diver.position;
        Vector3 localToTreasurePos = transform.InverseTransformPoint(treasurePos);
        Vector3 localToMinePos = transform.InverseTransformPoint(minePos);
        Vector3 localToDiverPos = transform.InverseTransformPoint(diverPos);
        blackboard["treasureDistance"] = localToTreasurePos.magnitude;
        blackboard["mineDistance"] = localToMinePos.magnitude;
        blackboard["diverDistance"] = localToDiverPos.magnitude;
    }

    private void destroyTreasure()
    {
        // Teleport the treasure at some random position, so it looks like mermaid is hiding the treasure
        Vector3 newPosition = new Vector3(UnityEngine.Random.Range(10f, 55f), 0.3f, UnityEngine.Random.Range(20f, 100f));
        treasure.transform.position = newPosition;
    }

    private Node treasureDestroy()
    {
        return new Action(() => destroyTreasure());
    }

    private void destroyDiver()
    {
        // Teleport mermaid to some random position, so it looks like mermaid was killed by the diver
        Vector3 newPosition = new Vector3(UnityEngine.Random.Range(10f, 55f), 0.3f, UnityEngine.Random.Range(20f, 100f));
        transform.position = newPosition;

    }

    private Node diverDestroy()
    {
        return new Action(() => destroyDiver());
    }


    private void seekTreasure()
    {
        Vector3 accel = steeringBasics.Seek(treasure.position);
        steeringBasics.Steer(accel);
        steeringBasics.LookWhereYoureGoing();
    }

    private Node treasureSeek()
    {
        return new Action(() => seekTreasure());
    }

    private void destroyMine()
    {
        // Teleport her at the random position, so it looks like she's dead
        Vector3 newPosition = new Vector3(UnityEngine.Random.Range(10f, 55f), 0.3f, UnityEngine.Random.Range(20f, 100f));
        transform.position = newPosition;

        // Teleport mine at some random position, so it looks like mine has exploded and changed position
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


    private Root Mermaid()
    {
        return new Root(
        new Service(0.5f, UpdatePerception,
            new Selector(
               new BlackboardCondition("diverDistance", Operator.IS_SMALLER, 2.0f, Stops.IMMEDIATE_RESTART,
               diverDestroy()),
               new BlackboardCondition("diverDistance", Operator.IS_SMALLER_OR_EQUAL, flee.panicDist, Stops.IMMEDIATE_RESTART,
               diverFlee()),
               new BlackboardCondition("treasureDistance", Operator.IS_SMALLER, 2.0f, Stops.IMMEDIATE_RESTART,
               treasureDestroy()),
               new BlackboardCondition("treasureDistance", Operator.IS_SMALLER_OR_EQUAL, 20.0f, Stops.IMMEDIATE_RESTART,
               treasureSeek()),
               new BlackboardCondition("mineDistance", Operator.IS_SMALLER, 2.0f, Stops.IMMEDIATE_RESTART,
               mineDestroy()),
               new BlackboardCondition("mineDistance", Operator.IS_SMALLER_OR_EQUAL, 20.0f, Stops.IMMEDIATE_RESTART,
               mineSeek()),
               new BlackboardCondition("mineDistance", Operator.IS_GREATER, 20.0f, Stops.IMMEDIATE_RESTART,
               nodeWander())
               )));
    }
    private Node diverFlee()
    {
        return new Action(() => fleeDiver());
    }

    private void fleeDiver()
    {
        float distanceToDiver = Vector3.Distance(transform.position, diver.position);
        if (distanceToDiver <= flee.panicDist)
        {
            Vector3 accel = flee.GetSteering(diver.position);
            steeringBasics.Steer(accel);
            steeringBasics.LookWhereYoureGoing();
        }
        else
        {
            wanderFunction();
        }
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