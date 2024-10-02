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

    #region �ǰ� �� �˹� ���� ���
    private bool isImmunity;
    private bool isDamaged;
    private float curKnockBackTime;
    private Coroutine damageCoroutine;
    public void TakeDamage(Vector3 otherPosition, int damage)
    {
        // ���� ��ġ�� ���� �ڽ��� ��ġ������ �Ÿ��� �̿��� ������
        // ���� ���⿡ �̸� ������ KnockBack Power��ŭ �з���
        // �̶�, ������ Move�� Jump�� ���� ������ �ִ� PlayerMove�� ���

        // ���� �����̹Ƿ�, �������̴� �������̰�, ��� PlayerMove �� Attack�� ���� �ش� �˹��� �����ؾߵ�
        // ����, �ش� �Լ������, �˹� ȿ���� ������Ʈ

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

        // ���� �˹� ���� �˹� ����
        while (curKnockBackTime < maxKnockBackTime)
        {
            controller?.Move(velocity * Time.deltaTime);
            curKnockBackTime += Time.deltaTime;
            yield return null;
        }

        // �˹� ������ ����, ��� ���������� ���� ����
        isDamaged = false;
        if (movement != null) movement.isOtherAction = false;
    }

    #endregion
    #region GeneralAttack ���� ���
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
        // Attack �÷��� �ð��� ���� ��쿡 �ٸ� �ൿ ���
        if (curAttackPlayTime > attackPlayTime)
        {
            foreach (var item in attackAreaObjects)
            {
                item.SetActive(false);
            }
            // �ٽ� isAttack �Է� ���� �� �ִ� ���·� �ʱ�ȭ
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

    #region ���� ���� ���
    private float curFallCoolTime;
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // ������ �ӷ� �̻��� �߶��϶�, ���� ������
        if (hit.transform.CompareTag("Ground") && controller.velocity.y < -fallSpeedLimit && curFallCoolTime > fallCoolTime)
        {
            print("Fall");
            status.TakeDamage(fallDamage);
            curFallCoolTime = 0f;
        }
    }
    #endregion

    #region Animation ���� ���
    void AnimationUpdate()
    {
        if (animator == null || controller == null || movement == null) return;

        // �ǰ��� ���������̹Ƿ� �ٸ� ��� ���ͷ��� ������ false
        if (isDamaged)
        {
            // �Ϲ� ��ġ ����
            animator.SetBool("Attack", false);
        }
        else
        {
            // �Ϲ� ��ġ ����
            animator.SetBool("Attack", isAttack);
        }
        animator.SetBool("KnockBack", isDamaged);
    }
    #endregion
}
