using UnityEngine;

public class WanderState : StateBase
{
	float retargetTimer;
	float wanderTimer;
    Vector3 originPosition;

	void Start()
	{
		originPosition = transform.position;
	}

	public override void OnEnter()
	{
		retargetTimer = 0f;
		wanderTimer = 0f;
	}

	public override void OnUpdate()
	{
		Debug.DrawRay(transform.position,transform.forward * monster.detectDistance,Color.red,3f);
		if (Physics.Raycast(transform.position, transform.forward * monster.detectDistance, out var hit) && retargetTimer > monster.retargetDelay)
		{
			if (hit.transform.CompareTag("Player"))
			{
				monster.ChangeState(State.Chase);
			}
		}
		
		if (wanderTimer > monster.wanderDelay)
		{
			var random = monster.wanderRadius * Random.insideUnitCircle;
			monster.agent.speed = monster.walkMaxSpeed;
			monster.agent.destination = new Vector3(random.x, 0, random.y) + originPosition;
			wanderTimer = 0f;
		}
		retargetTimer += Time.deltaTime;
		wanderTimer += Time.deltaTime;
	}
	
}