using System.Collections.Generic;
using Ami.BroAudio;
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

    [Header("SFX")]
    public SoundID walkSound;
    public SoundID jumpSound;
    public SoundID dieSound;

    [Header("VFX")]
    public ParticleSystem jumpParticle;
    public GameObject goombaDeathParticle;
    
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
        ChangeState(State.None);
		agent.enabled = false;
	}
}
