using Ami.BroAudio;
using DG.Tweening;
using UnityEngine;

public class ChaseState : StateBase
{
	float releaseTimer;
	float repathTimer;

    bool isStartRoutine = false;
	Sequence seq;
	void Start()
	{
        seq = DOTween.Sequence();
		seq.AppendCallback(() => {BroAudio.Play(monster.walkSound,transform);})
		.AppendInterval(1f / monster.runMaxSpeed)
		.SetLoops(-1, LoopType.Restart)
        .SetLink(monster.gameObject);
	}

	public override void OnEnter()
	{
		releaseTimer = 0f;
		repathTimer = 0f;
		monster.agent.speed = monster.runMaxSpeed;
		seq.Play();

        //Start Routine
        monster.transform.DOJump(transform.position,monster.jumpPower,1,1f)
                .SetEase(Ease.OutQuad)
                .OnStart(() => BroAudio.Play(monster.jumpSound))
                .SetLink(monster.gameObject)
                .OnComplete(() => 
                    {
                        isStartRoutine = true;
                        monster.jumpParticle.Play();
                    });
	}

	public override void OnUpdate()
	{
        if (isStartRoutine == false) return;

        if (Physics.Raycast(transform.position, transform.forward, out var hit, monster.detectDistance))
        {
            if (hit.transform.CompareTag("Player"))
            {
                releaseTimer = 0f;
            }
            else if (releaseTimer > monster.detectReleaseTime)
            {
                monster.ChangeState(State.Wander);
            }
        }

        if (repathTimer > monster.repathDelay)
        {
            monster.agent.destination = monster.target.transform.position;
            repathTimer = 0f;
        }
		repathTimer += Time.deltaTime;
		releaseTimer += Time.deltaTime;
	}

	public override void OnExit()
	{
		isStartRoutine = false;
        monster.agent.speed = monster.walkMaxSpeed;
        seq.Kill();
	}
	
}