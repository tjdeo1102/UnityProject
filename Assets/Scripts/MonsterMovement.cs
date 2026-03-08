using System.Collections.Generic;
using Ami.BroAudio;
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
    public MonsterInteraction interaction;
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
    public SoundID chaseSound;
    public SoundID jumpSound;
    public SoundID dieSound;

    [Header("VFX")]
    public ParticleSystem jumpParticle;
    public GameObject deathParticle;
    public ParticleSystem chaseParticle;
    
    [Header("State")]
    public Dictionary<State,StateBase> activationDic;
    public State currentState;

	void Awake()
	{
		anim = GetComponentInChildren<Animator>();
        interaction = GetComponent<MonsterInteraction>();
        agent = GetComponent<NavMeshAgent>();
        currentState = State.None;
        // agent.updateRotation = false;
	}


	void Start()
    {
        activationDic = new () {{State.None, null}};
        foreach (var state in GetComponents<StateBase>())
        {
            activationDic.Add(state.stateType, state);
        }

        target = GameManager.Instance.Player.gameObject;
        ChangeState(State.Wander);
    }

	void Update()
	{
        anim.SetFloat("Speed", agent.velocity.magnitude);
        activationDic[currentState]?.OnUpdate();
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
