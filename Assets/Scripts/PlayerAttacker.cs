using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttacker : MonoBehaviour
{
    [Header("Base Component")]
    [SerializeField] CharacterController controller;
    [SerializeField] PlayerMove movement;
    [SerializeField] PlayerInput input;

    [Header("Attack")]
    [SerializeField] GameObject[] attackAreaObjects;
    [SerializeField] float attackPlayTime;

    [Header("Animation")]
    [SerializeField] Animator animator;

    void Update()
    {
        if (input != null && controller != null)
        {
            Attack();
        }

        AnimationUpdate();
    }
    #region GeneralAttack ���� ���
    private float curAttackPlayTime = 0f;
    private float curAttackAreaActiveTime = 0f;
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
    #region Animation ���� ���
    bool isAttack;

    void AnimationUpdate()
    {
        if (animator == null || controller == null) return;

        // �Ϲ� ��ġ ����
        animator.SetBool("Attack",isAttack);
    }
    #endregion
}
