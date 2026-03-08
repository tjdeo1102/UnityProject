using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IDamageable
{
    public void OnDamage(Vector3 otherPosition, int damage);
}

public class PlayerCombat : MonoBehaviour, IDamageable
{
    [Header("Base Component")]
    [SerializeField] PlayerMovement movement;
    [SerializeField] PlayerInput input;

    [Header("Attack")]
    [SerializeField] GameObject[] attackAreaObjects;
    [SerializeField] float attackPlayTime;
    public Transform grabPoint;
    public MonsterInteraction grabbedMonster;

    [Header("Take Damage")]
    [SerializeField] PlayerHitEffect[] hitEffects;
    [SerializeField] float knockBackPower;
    [SerializeField] float knockBackTime;
    [SerializeField] float fallSpeedLimit;
    [SerializeField] int fallDamage;

    [Header("Animation")]
    [SerializeField] Animator animator;

    void Update()
    {
        AttackUpdate();
        AnimationUpdate();
    }

    #region Hit
    private bool isKnock;
    private Coroutine damageCoroutine;
    public void OnDamage(Vector3 otherPosition, int damage)
    {
        if (movement != null && isKnock == false)
        {
            if (damageCoroutine == null)
                damageCoroutine = StartCoroutine(DamageRoutine(otherPosition, damage));
        }
    }

    IEnumerator DamageRoutine(Vector3 otherPosition, int damage)
    {
        isKnock = true;
        isAttack = false;

        movement.isOtherAction = true;
        GameDataModel.Instance.Power-= damage;

        var dir = (transform.position - otherPosition).normalized;
        var knockVelocity = new Vector3(dir.x, 0, dir.y) * knockBackPower;

        movement.rb.AddForce(knockVelocity,ForceMode.Impulse);
        PlayHitEffect();
        yield return new WaitForSeconds(knockBackTime);
        
        isKnock = false;
        movement.isOtherAction = false;
        damageCoroutine = null;
    }

    void PlayHitEffect()
    {
        foreach (var item in hitEffects)
        {
            item.PlayHitEffect();
        }
    }

    #endregion
    #region GeneralAttack
    bool isAttack;
    private float curAttackPlayTime = 0f;
	public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed && isAttack == false)
        {
            movement.isOtherAction = true;
            isAttack = true;
            if (grabbedMonster != null)
            {
                grabbedMonster.OnThrow(this, (transform.forward + Vector3.up * 0.5f).normalized, 10f);
                return;
            }

            foreach (var item in attackAreaObjects)
            {
                item.SetActive(true);
            }
        }
    }

    void AttackUpdate()
    {
        if (isAttack)
        {
            if (curAttackPlayTime > attackPlayTime)
            {
                foreach (var item in attackAreaObjects)
                {
                    item.SetActive(false);
                }
                if (movement != null) movement.isOtherAction = false;
                isAttack = false;
                curAttackPlayTime = 0f;
            }
            curAttackPlayTime += Time.deltaTime;
        }
    }
    #endregion

    #region Fall
	void OnCollisionEnter(Collision collision)
	{
		if (movement.rb.linearVelocity.y < -fallSpeedLimit)
        {
            GameDataModel.Instance.Power -= fallDamage;
        }
	}
	#endregion

	#region Animation 
	void AnimationUpdate()
    {
        animator.SetBool("IsAttack", isAttack);
        animator.SetBool("IsKnock", isKnock);
    }
	#endregion
}
