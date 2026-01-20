using DG.Tweening;
using UnityEngine;

public class ChaseState : StateBase
{
	float releaseTimer;
	float repathTimer;

    bool isStartRoutine = false;
	public override void OnEnter()
	{
		releaseTimer = 0f;
		repathTimer = 0f;
		monster.agent.speed = monster.runMaxSpeed;

        //Start Routine
        monster.transform.DOJump(transform.position,monster.jumpPower,1,1f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => 
                isStartRoutine = true);
	}

	public override void OnUpdate()
	{
        if (isStartRoutine == false) return;

        if (Physics.Raycast(transform.position, transform.forward * monster.detectDistance, out var hit))
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
	}
	
}