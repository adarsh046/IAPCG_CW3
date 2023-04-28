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
        new Service(0.5f, UpdatePerception,
            new Selector(
               new BlackboardCondition("sharkDistance", Operator.IS_SMALLER_OR_EQUAL, flee.panicDist, Stops.IMMEDIATE_RESTART,
               sharkFlee()),
               new BlackboardCondition("mermaidDistance", Operator.IS_SMALLER_OR_EQUAL, 20.0f, Stops.IMMEDIATE_RESTART,
               mermaidSeek()),
               new BlackboardCondition("mineDistance", Operator.IS_SMALLER_OR_EQUAL, 15.0f, Stops.IMMEDIATE_RESTART,
               mineFlee()),
               new BlackboardCondition("treasureDistance", Operator.IS_SMALLER, 2.0f, Stops.IMMEDIATE_RESTART,
               treasureDestroy()),
               new BlackboardCondition("treasureDistance", Operator.IS_SMALLER_OR_EQUAL, 20.0f, Stops.IMMEDIATE_RESTART,
               treasureSeek()),
               new BlackboardCondition("treasureDistance", Operator.IS_GREATER, 20.0f, Stops.IMMEDIATE_RESTART,
               nodeWander())
               )));
    }

    private Node sharkFlee()
    {
        return new Action(() => fleeShark());
    }

    private void fleeShark()
    {
        float distanceToShark = Vector3.Distance(transform.position, shark.position);
        if (distanceToShark <= flee.panicDist)
        {
            Vector3 accel = flee.GetSteering(shark.position);
            steeringBasics.Steer(accel);
            steeringBasics.LookWhereYoureGoing();
        }
        else
        {
            wanderFunction();
        }
    }

    private Node mineFlee()
    {
        return new Action(() => fleeMine());
    }

    private void fleeMine()
    {
        float distanceToMine = Vector3.Distance(transform.position, mine.position);
        if (distanceToMine <= flee.panicDist)
        {
            Vector3 accel = flee.GetSteering(mine.position);
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
        Vector3 newPosition = new Vector3(UnityEngine.Random.Range(10f, 55f), 0.6f, UnityEngine.Random.Range(20f, 100f));
        treasure.transform.position = newPosition;
    }

    private Node treasureDestroy()
    {
        return new Action(() => destroyTreasure());
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

    private void seekMermaid()
    {
        Vector3 accel = steeringBasics.Seek(mermaid.position);
        steeringBasics.Steer(accel);
        steeringBasics.LookWhereYoureGoing();
    }

    private Node mermaidSeek()
    {
        return new Action(() => seekMermaid());
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