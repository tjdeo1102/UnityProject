using DG.Tweening;
using UnityEngine;
using Ami.BroAudio;

public class BombWanderState : StateBase
{
	float retargetTimer;
	float wanderTimer;
    Vector3 originPosition;
	Sequence seq;
	void Start()
	{
		originPosition = transform.position;
		seq = DOTween.Sequence();
		seq.AppendCallback(() => {BroAudio.Play(monster.walkSound,transform);})
		.AppendInterval(1f / monster.walkMaxSpeed)
		.SetLoops(-1, LoopType.Restart)
		.SetLink(monster.gameObject);
	}

	public override void OnEnter()
	{
		retargetTimer = 0f;
		wanderTimer = 0f;
		seq.Play();
	}

	public override void OnUpdate()
	{
		if (monster.interaction.isGrabbed)
		{
			monster.ChangeState(State.Chase);
			return;
		}
		
		if (Physics.Raycast(transform.position, transform.forward, out var hit, monster.detectDistance) && retargetTimer > monster.retargetDelay)
		{
			if (hit.transform.CompareTag("Player"))
			{
				monster.ChangeState(State.Chase);
			}
		}
		
		if (wanderTimer > monster.wanderDelay && monster.agent.enabled)
		{
			var random = monster.wanderRadius * Random.insideUnitCircle;
			monster.agent.speed = monster.walkMaxSpeed;
			monster.agent.destination = new Vector3(random.x, 0, random.y) + originPosition;
			wanderTimer = 0f;
		}
		retargetTimer += Time.deltaTime;
		wanderTimer += Time.deltaTime;
	}

	public override void OnExit()
	{
		seq.Kill();
	}
}