using UnityEngine;
using NPBehave;
using UnityMovementAI;

public class Behaviour_Tree_Script : MonoBehaviour
{

    public Transform target;
    private Root tree;                  // The agents' behaviour tree
    private Blackboard blackboard;      // The agents' behaviour blackboard
    public int behaviourNumber = 0;     // Used to select an AI behaviour in the Unity Inspector

    SteeringBasics steeringBasics;
    Flee flee;
    Wander2 wander;

    // Start is called before the first frame update
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
        switch (behaviourNumber)
        {
            case 1:
               return Seek();
            case 2:
                return Wander();
            case 3:
                return Flee();
        }
        return null;
    }

    private void UpdatePerception()
    {
        Vector3 targetPos = target.position;
        Vector3 localPos = this.transform.InverseTransformPoint(targetPos);
        Vector3 heading = localPos.normalized;
        blackboard["targetDistance"] = localPos.magnitude;
        blackboard["targetInFront"] = heading.z > 0;
        blackboard["targetOnRight"] = heading.x > 0;
        blackboard["targetOffCentre"] = Mathf.Abs(heading.x);
    }

    private Root Flee()
    {
        return new Root(
        new Service(0.5f, UpdatePerception,
            new Selector(
               new BlackboardCondition("targetDistance", Operator.IS_SMALLER, flee.panicDist, Stops.IMMEDIATE_RESTART,
               nodeFlee()
               ))));
    }

    private Node nodeFlee()
    {
        return new Action(() => fleeFunction());
    }

    private void fleeFunction()
    {
        Vector3 accel = flee.GetSteering(target.position);
        steeringBasics.Steer(accel);
        steeringBasics.LookWhereYoureGoing();

    }

    private Root Seek()
    {
        return new Root(
        new Service(0.5f, UpdatePerception,
            new Selector(
               new BlackboardCondition("targetDistance", Operator.IS_SMALLER, 20.0f, Stops.IMMEDIATE_RESTART,
               nodeSeek()
               ))));
    }
    private void seekFunction()
    {
        Vector3 accel = steeringBasics.Seek(target.position);
        steeringBasics.Steer(accel);
        steeringBasics.LookWhereYoureGoing();
    }

    private Node nodeSeek()
    {
        return new Action(() => seekFunction());
    }


    private Root Wander()
    {
        return new Root(
        new Service(0.5f, UpdatePerception,
            new Selector(
               new BlackboardCondition("targetDistance", Operator.IS_GREATER, 20.0f, Stops.IMMEDIATE_RESTART,
               nodeWander()),
               new BlackboardCondition("targetDistance", Operator.IS_SMALLER, 20.0f, Stops.IMMEDIATE_RESTART,
               nodeSeek())
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