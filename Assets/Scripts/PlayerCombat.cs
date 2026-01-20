using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Base Component")]
    [SerializeField] PlayerMovement movement;
    [SerializeField] PlayerInput input;

    [Header("Attack")]
    [SerializeField] GameObject[] attackAreaObjects;
    [SerializeField] float attackPlayTime;

    [Header("Take Damage")]
    [SerializeField] Transform[] defaultRenderers;
    [SerializeField] Transform[] damageRenderers;
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
        SetHitRenderer(true);
        
        yield return new WaitForSeconds(knockBackTime);
        SetHitRenderer(false);
        isKnock = false;
        movement.isOtherAction = false;
        damageCoroutine = null;
    }

    void SetHitRenderer(bool isActive)
    {
        foreach (var item in defaultRenderers)
        {
            item.gameObject.SetActive(!isActive);
        }
        foreach (var item in damageRenderers)
        {
            item.gameObject.SetActive(isActive);
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
            foreach (var item in attackAreaObjects)
            {
                item.SetActive(true);
            }
            movement.isOtherAction = true;
            isAttack = true;
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
