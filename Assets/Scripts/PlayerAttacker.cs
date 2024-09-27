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
    #region GeneralAttack 관련 기능
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
    #region Animation 관련 기능
    bool isAttack;

    void AnimationUpdate()
    {
        if (animator == null || controller == null) return;

        // 일반 펀치 공격
        animator.SetBool("Attack",isAttack);
    }
    #endregion
}
