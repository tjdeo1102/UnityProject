using Ami.BroAudio;
using DG.Tweening;
using UnityEngine;

public class BombChaseState : StateBase
{
	public float dieTimer = 5f;
	public float resetTimer = 5f;
    bool isStartRoutine = false;
	Sequence walkSFXSequence;
	Sequence startSequence;
	Sequence dieSequence;
	public override void OnEnter()
	{
		if (monster.agent.enabled)
		{
			monster.agent.destination = monster.target.transform.position;
			monster.agent.speed = monster.runMaxSpeed;	
		}

		walkSFXSequence = DOTween.Sequence();
		walkSFXSequence.AppendCallback(() => {BroAudio.Play(monster.chaseSound,transform);
												BroAudio.Play(monster.walkSound,transform);})
		.AppendInterval(1f / monster.runMaxSpeed)
		.SetLoops(-1, LoopType.Restart)
        .SetLink(monster.gameObject);
		walkSFXSequence.Play();

		monster?.chaseParticle.Play();

        //Start Routine
		startSequence = monster.transform.DOJump(transform.position,monster.jumpPower,1,1f)
			.SetEase(Ease.OutQuad)
			.OnStart(() => 
					{
						BroAudio.Play(monster.jumpSound);
					})
			.SetLink(monster.gameObject)
			.OnComplete(() => 
				{
					monster?.jumpParticle.Play();
				})
			.OnKill(() => isStartRoutine = true);

		//End Routine
		dieSequence = DOTween.Sequence();
		dieSequence.AppendInterval(dieTimer)
			.SetLink(monster.gameObject)
			.OnComplete(() => 
			{
				monster.ChangeState(State.None);
				monster.interaction.Die(DiePattern.Boom);
			});
	}

	public override void OnUpdate()
	{
		// exit jump routine to set local position when grabbed 
		if (monster.interaction.isGrabbed && startSequence.IsActive() && startSequence.IsPlaying())
		{
			startSequence.Kill();
			monster.transform.localPosition = monster.interaction.grabbedLocalPosition;
		}

        if (isStartRoutine == false || monster.agent.enabled == false) return;
        monster.agent.destination = monster.target.transform.position;
	}

	public override void OnExit()
	{
		isStartRoutine = false;
        monster.agent.speed = monster.walkMaxSpeed;
		startSequence.Kill();
        walkSFXSequence.Kill();
        dieSequence.Kill();
	}

	public void ResetBombTimer()
	{
		dieSequence.Kill();
		dieSequence = DOTween.Sequence();
		dieSequence.AppendInterval(resetTimer)
		.SetLink(monster.gameObject)
		.OnComplete(() => 
			{
				monster.ChangeState(State.None);
				monster.interaction.Die(DiePattern.Boom);
			});
	}
	
}