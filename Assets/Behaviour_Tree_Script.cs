using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NPBehave;
using UnityMovementAI;

public class Behaviour_Tree_Script : MonoBehaviour
{

    public Transform target;
    private Root tree;                  // The tank's behaviour tree
    private Blackboard blackboard;      // The tank's behaviour blackboard
    public int m_Behaviour = 0;         // Used to select an AI behaviour in the Unity Inspector

    SteeringBasics steeringBasics;
    Flee flee;

    // Start is called before the first frame update
    void Start()
    {
        steeringBasics = GetComponent<SteeringBasics>();
        flee = GetComponent<Flee>();
        tree = CreateBehaviourTree();
        blackboard = tree.Blackboard;
        tree.Start();
    }

    private Root CreateBehaviourTree()
    {
        // To change a tank's behaviour:
        // - Examine the GameManager object in the inspector.
        // - Open the Tanks list, find the tank's entry.
        // - Enable the NPC checkbox.
        // - Edit the Behaviour field to an integer N corresponding to the behaviour below
        switch (m_Behaviour)
        {
            // N=1
            //case 1:
                //return SpinBehaviour(-0.05f, 1f);
           // case 2:
                //return CustomFunction();
            case 3:
                return CustomTracking();
            // Default behaviour: turn slowly
            //default:
                //return TurnSlowly();
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

    //// Register an enemy target 
    //public void AddTarget(GameObject target)
    //{
    //    m_Targets.Add(target);
    //}

    //// Get the transform for the first target
    //private Transform TargetTransform()
    //{
    //    if (m_Targets.Count > 0)
    //    {
    //        return m_Targets[0].transform;
    //    }
    //    else
    //    {
    //        return null;
    //    }
    //}

    private Root CustomTracking()
    {
        return new Root(
        new Service(0.5f, UpdatePerception,
            new Selector(
               new BlackboardCondition("targetDistance", Operator.IS_SMALLER, 250.0f, Stops.IMMEDIATE_RESTART,
               //new Sequence(
               CustomMove()
               //new Wait(3.0f))
               )
               //,
              // new BlackboardCondition("targetOffCentre", Operator.IS_SMALLER, 0.01f, Stops.IMMEDIATE_RESTART,
               //new Sequence(
               //CustomMove(0.25f),
               //RandomFire()
               ///)),
              // new BlackboardCondition("targetOnRight", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
               //CustomRightTurn()
              // ),
               //new BlackboardCondition("targetOnRight", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART,
               //CustomLeftTurn()
               //)

               )
            ));
    }

    private void Move()
    {
        Vector3 accel = flee.GetSteering(target.position);

        steeringBasics.Steer(accel);
        steeringBasics.LookWhereYoureGoing();
    }

    private Node CustomMove()
    {
        return new Action(() => Move());
    }

    private Node CustomRandomTurn()
    {
        return null;
        //new Action(() => Turn(UnityEngine.Random.Range(-1.0f, 1.0f)));
    }

    private Node CustomRightTurn()
    {
        return null;
        //new Action(() => Turn(0.25f));
    }

    private Node CustomLeftTurn()
    {
        return null;
        //new Action(() => Turn(-0.25f));
    }

    private Node StopTurning()
    {
        return null;
       // Action(() => Turn(0f));
    }

    private Node RandomFire()
    {
        return null;
        //new Action(() => Fire(UnityEngine.Random.Range(0.2f, 1.0f)));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
