using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Base Component")]
    [SerializeField] CharacterController controller;
    [SerializeField] PlayerMovement movement;
    [SerializeField] PlayerInput input;
    [SerializeField] PlayerStatus status;

    [Header("Attack")]
    [SerializeField] GameObject[] attackAreaObjects;
    [SerializeField] float attackPlayTime;

    [Header("Take Damage")]
    [SerializeField] float knockBackPower;
    [SerializeField] float ImmunityTime;
    [SerializeField] float maxKnockBackTime;
    [SerializeField] float fallSpeedLimit;
    [SerializeField] float fallCoolTime;
    [SerializeField] int fallDamage;

    [Header("Animation")]
    [SerializeField] Animator animator;

    void Update()
    {
        if (input != null && controller != null && isDamaged == false)
        {
            Attack();
        }
        if (curFallCoolTime < fallCoolTime)
        {
            curFallCoolTime += Time.deltaTime;
        }
        AnimationUpdate();
    }

    #region 피격 및 넉백 관련 기능
    private bool isImmunity;
    private bool isDamaged;
    private float curKnockBackTime;
    private Coroutine damageCoroutine;
    public void TakeDamage(Vector3 otherPosition, int damage)
    {
        // 맞은 위치로 부터 자신의 위치사이의 거리를 이용해 방향계산
        // 얻은 방향에 미리 설정된 KnockBack Power만큼 밀려남
        // 이때, 기존의 Move나 Jump와 같은 동작이 있는 PlayerMove는 잠금

        // 절대 판정이므로, 공격중이던 점프중이건, 모든 PlayerMove 및 Attack을 막고 해당 넉백이 동작해야됨
        // 따라서, 해당 함수실행시, 넉백 효과만 업데이트

        if (movement != null && status != null && isImmunity == false)
        {
            if (damageCoroutine != null) StopCoroutine(damageCoroutine);
            StartCoroutine(DamageRoutine(otherPosition, damage));
        }
    }

    IEnumerator DamageRoutine(Vector3 otherPosition, int damage)
    {
        isImmunity = true;
        isDamaged = true;
        if (movement != null) movement.isOtherAction = true;
        status.TakeDamage(damage);
        var dir = (transform.position - otherPosition).normalized;
        var velocity = new Vector3(dir.x, 0, dir.y) * knockBackPower;

        var knockBackRoutine = StartCoroutine(KnockBackUpdate(velocity));

        yield return new WaitForSeconds(ImmunityTime);
        StopCoroutine(knockBackRoutine);
        isImmunity = false;
    }

    IEnumerator KnockBackUpdate(Vector3 velocity)
    {
        curKnockBackTime = 0f;

        // 실제 넉백 동안 넉백 적용
        while (curKnockBackTime < maxKnockBackTime)
        {
            controller?.Move(velocity * Time.deltaTime);
            curKnockBackTime += Time.deltaTime;
            yield return null;
        }

        // 넉백 경직은 해제, 대신 무적판정은 아직 지속
        isDamaged = false;
        if (movement != null) movement.isOtherAction = false;
    }

    #endregion
    #region GeneralAttack 관련 기능
    bool isAttack;
    private float curAttackPlayTime = 0f;
    void Attack()
    {
        bool attack = input.actions["Attack"].IsPressed();
        if (attack && isAttack == false)
        {
            foreach (var item in attackAreaObjects)
            {
                item.SetActive(true);
            }
            if (movement != null) movement.isOtherAction = true;
            isAttack = true;
        }
        // Attack 플레이 시간이 지난 경우에 다른 행동 허용
        if (curAttackPlayTime > attackPlayTime)
        {
            foreach (var item in attackAreaObjects)
            {
                item.SetActive(false);
            }
            // 다시 isAttack 입력 받을 수 있는 상태로 초기화
            if (movement != null) movement.isOtherAction = false;
            isAttack = false;
            curAttackPlayTime = 0f;
        }

        if (isAttack)
        {
            curAttackPlayTime += Time.deltaTime;
        }
    }
    #endregion

    #region 낙사 관련 기능
    private float curFallCoolTime;
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // 정해진 속력 이상의 추락일때, 낙사 데미지
        if (hit.transform.CompareTag("Ground") && controller.velocity.y < -fallSpeedLimit && curFallCoolTime > fallCoolTime)
        {
            print("Fall");
            status.TakeDamage(fallDamage);
            curFallCoolTime = 0f;
        }
    }
    #endregion

    #region Animation 관련 기능
    void AnimationUpdate()
    {
        if (animator == null || controller == null || movement == null) return;

        // 피격은 절대판정이므로 다른 모든 인터렉션 동작이 false
        if (isDamaged)
        {
            // 일반 펀치 공격
            animator.SetBool("Attack", false);
        }
        else
        {
            // 일반 펀치 공격
            animator.SetBool("Attack", isAttack);
        }
        animator.SetBool("KnockBack", isDamaged);
    }
    #endregion
}
