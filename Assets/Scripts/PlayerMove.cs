using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [Header("Base Component")]
    [SerializeField] CharacterController controller;
    [SerializeField] PlayerInput input;


    [Header("Move")]
    [SerializeField] float moveSpeed;
    [SerializeField] float moveMaxVelocity;
    [SerializeField] float runSpeed;

    [Header("Jump")]
    [SerializeField] float jumpPower;
    [Range(0,1)]
    [SerializeField] float jumpMinPower;
    [SerializeField] float jumpMaxSpeed;
    [SerializeField] float jumpDelay;
    [SerializeField] float jumpButtonDownMaxTime;
    [SerializeField] float checkFloorDistance;

    [Header("Animation")]
    [SerializeField] Animator animator;

    public bool isOtherAction;

    // Update is called once per frame
    void Update()
    {
        if (input != null && controller != null && isOtherAction==false)
        {
            Move();
            Jump();
        }

        JumpUpdate();

        AnimationUpdate();
    }

    void Move()
    {
        Vector2 move = input.actions["Move"].ReadValue<Vector2>();
        // 이미 달리는 도중에 점프하는 경우에 대해서는 기존 속력 보전한 점프
        // 점프 중에 달리는 경우는 뛰기 전 속력으로 유지
        if (canJump == true)
            isRun = input.actions["Run"].IsPressed();

        if (move == Vector2.zero)
        {
            IsWalk = false;
            return;
        }
        
        Vector3 dir = new Vector3(move.x, 0, move.y).normalized;

        transform.LookAt(transform.position + dir);
        
        if (isRun)
        {
            controller.Move(dir * moveSpeed * Time.deltaTime * runSpeed);
        }
        else
        {
            controller.Move(dir * moveSpeed * Time.deltaTime);
        }
        IsWalk = true;
    }

    #region Jump관련 기능
    private bool canJump = true;
    // 이전에 이미 버튼을 누르고 있었는지 확인하는 변수
    private bool isJumpButtonDown = false;
    private int objectslayerMaskOnly = -(1 << 7);
    private float jumpButtonDownTime = 0;
    private float curJumpDelay = 0f;
    private float curJumpVelocity = 0f;

    void Jump()
    {
        Debug.DrawRay(transform.position, -transform.up * checkFloorDistance);

        var isJump = input.actions["Jump"].IsPressed();

        // 입력을 누를 때부터 시작
        // 최대 홀드 시간까지 누른 정도만큼 점프 누른 시간을 저장
        // 최대 홀드 시간이 지나거나, 버튼을 뗄 경우에만 정해진 파워로 점프

        if (isJump && canJump)
        {
            jumpButtonDownTime += Time.deltaTime;
            isJumpButtonDown = true;
        }

        if (isJumpButtonDown && (jumpButtonDownTime > jumpButtonDownMaxTime || isJump == false))
        {
            // 최소 속도 보장
            float power = (jumpMinPower + jumpButtonDownTime / jumpButtonDownMaxTime * (1 - jumpMinPower)) * jumpPower;
            // 초기 속도 설정
            curJumpVelocity = power;
        
            // 변수 초기화
            jumpButtonDownTime = 0f;
            isJumpButtonDown = false;
            canJump = false;
        }
        // 점프 후, 딜레이 시간 적용 후, 바닥 충돌 확인
        // 현재 바닥과 충돌중인지 확인
        if (canJump == false)
        {
            if (curJumpDelay > jumpDelay)
            {
                Ray ray = new Ray(transform.position, -transform.up * checkFloorDistance);
                canJump = Physics.Raycast(ray, checkFloorDistance, objectslayerMaskOnly);
                curJumpDelay = 0f;
            }
            curJumpDelay += Time.deltaTime;
        }
    }

    void JumpUpdate()
    {
        // 점프력에 따른 점프
        // 중력 고려
        curJumpVelocity -= Physics.gravity.magnitude * Time.deltaTime;
        // 최저 최고 속력 제한
        curJumpVelocity = Mathf.Clamp(curJumpVelocity, -jumpMaxSpeed, jumpMaxSpeed);
        controller.Move(Vector3.up * curJumpVelocity * Time.deltaTime);
    }

    #endregion

    #region Animation 관련 기능
    private bool IsWalk;
    private bool isRun;

    void AnimationUpdate()
    {
        if (animator == null || controller == null) return;

        // 이동 업데이트
        animator.SetBool("Walk", IsWalk);
        animator.SetBool("Run", isRun);

        // 점프 업데이트
        animator.SetBool("Jump", !canJump);

        // Other Ataction
        animator.SetBool("OtherAction", isOtherAction);
    }
    #endregion
}
