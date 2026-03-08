
using UnityEngine;

public class StateBase : MonoBehaviour
{
	[Header("Base")]
	protected MonsterMovement monster;
	public State stateType;
	void Awake()
	{
		monster = GetComponent<MonsterMovement>();
	}
	public virtual void OnEnter() {}
	public virtual void OnUpdate() {}
	public virtual void OnExit() {}
}