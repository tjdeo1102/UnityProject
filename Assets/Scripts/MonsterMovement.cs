using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public enum State
{
    None,Wander,Chase
}

public class MonsterMovement : MonoBehaviour
{

    [Header("Base")]
    public GameObject target;
    public Animator anim;
    public float walkMaxSpeed;
    public float runMaxSpeed;
    public float wanderRadius;


    [Header("Tracking")]
    public NavMeshAgent agent;
    public float repathDelay;
    public float wanderDelay;
    public float retargetDelay;
    public float detectReleaseTime;
    public float detectDistance;
    public float jumpPower;

    Dictionary<State,StateBase> activationDic;
    State currentState = State.None;

	void Awake()
	{
		anim = GetComponentInChildren<Animator>();
        // agent.updateRotation = false;
	}


	void Start()
    {
        activationDic = new ()
        {
            {State.None, null},
            {State.Wander, transform.AddComponent<WanderState>()},
            {State.Chase, transform.AddComponent<ChaseState>()}
        };
        target = GameManager.Instance.Player.gameObject;
        ChangeState(State.Wander);
    }

	void Update()
	{
        anim.SetFloat("Speed", agent.velocity.magnitude);
        // if (agent.hasPath)
        // {
        //     Vector3 direction = agent.desiredVelocity.normalized;

        //     if (direction != Vector3.zero)
        //     {
        //         Quaternion targetRot = Quaternion.LookRotation(direction);
        //         transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 2f); 
        //     }
        // }
        activationDic[currentState]?.OnUpdate();
	}

    public void SuccessAttack()
    {
        ChangeState(State.Wander);
    }

    public void ChangeState(State state)
    {
        if (state != currentState)
        {
            activationDic[currentState]?.OnExit();
            currentState = state;
            activationDic[currentState]?.OnEnter();
        }
    }

	void OnDisable()
	{
		agent.enabled = false;
	}
}
