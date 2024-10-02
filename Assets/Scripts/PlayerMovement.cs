using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class PlayerMovement : MonoBehaviour
{
    [Header("Base Component")]
    [SerializeField] CharacterController controller;
    [SerializeField] PlayerInput input;


    [Header("Move")]
    [SerializeField] float moveSpeed;
    [SerializeField] float moveMaxVelocity;
    [SerializeField] float rotateSpeed;
    [SerializeField] float runSpeed;

    [Header("Jump")]
    [SerializeField] float jumpPower;
    [Range(0,1)]
    [SerializeField] float jumpMinPower;
    [SerializeField] float gravityScale;

    [SerializeField] float jumpButtonDownMaxTime;
    [SerializeField] float jumpChainTime;
    [SerializeField] float jumpChainPower;

    [Header("Ground Check")]
    [SerializeField] Vector3 boxSize;

    [Header("Animation")]
    [SerializeField] Animator animator;

    public bool isOtherAction;


    private Camera mainCamera;
    private void Awake()
    {
        mainCamera = Camera.main;
    }
    void FixedUpdate()
    {
        if (input != null && controller != null && isOtherAction==false)
        {
            Move();
            Jump();
        }
        isGround = IsGround();
        GravityUpdate();

        AnimationUpdate();
    }

    #region 이동 관련 기능
    void Move()
    {
        Vector2 move = input.actions["Move"].ReadValue<Vector2>();
        // 이미 달리는 도중에 점프하는 경우에 대해서는 기존 속력 보전한 점프
        // 점프 중에 달리는 경우는 뛰기 전 속력으로 유지
        if (isGround == true)
            isRun = input.actions["Run"].IsPressed();

        if (move == Vector2.zero)
        {
            IsWalk = false;
            return;
        }
        
        Vector3 dir = new Vector3(move.x, 0, move.y).normalized;
        // 카메라가 바라보는 방향이 앞이 되도록 설정
        if (mainCamera != null)
        {
            dir = (mainCamera.transform.forward * dir.z + mainCamera.transform.right * dir.x).normalized;
        }
        var rotate = dir + transform.position;
        transform.LookAt(new Vector3(rotate.x,transform.position.y, rotate.z));

        var velocity = dir * moveSpeed * Time.fixedDeltaTime;

        if (isRun) velocity *= runSpeed;

        controller.Move(velocity);
        IsWalk = true;
    }
    #endregion
    #region 점프관련 기능
    private bool isGround = true;
    // 이전에 이미 버튼을 누르고 있었는지 확인하는 변수
    private bool isJumpButtonDown = false;
    private bool isJumpDone = false;
    private int objectslayerMaskOnly = -(1 << 7);
    private float jumpButtonDownTime = 0f;
    private float curJumpVelocity = 0f;
    private int curJumpType = 0;
    private float curJumpChainTime = 0f;
    void Jump()
    {
        var isJump = input.actions["Jump"].IsPressed();

        // 입력을 누를 때부터 시작
        // 최대 홀드 시간까지 누른 정도만큼 점프 누른 시간을 저장
        // 최대 홀드 시간이 지나거나, 버튼을 뗄 경우에만 정해진 파워로 점프

        if (isJump && isGround && isJumpButtonDown == false)
        {
            jumpButtonDownTime = 0f;
            isJumpButtonDown = true;
            isJumpDone = false;
        }
        if (isJumpButtonDown)
        {
            if ((jumpButtonDownTime > jumpButtonDownMaxTime || isJump == false) && isJumpDone == false)
            {
                // 점프가 된 이후 부터, 점프 연계가 가능한 시간 측정
                if (curJumpChainTime < jumpChainTime)
                {
                    curJumpType++;
                    if (curJumpType > 3)
                    {
                        curJumpType = 1;
                    }
                    curJumpChainTime = 0f;
                }
                else
                {
                    curJumpType = 1;
                }
                curJumpChainTime = 0f;

                // 최소 속도 보장 + 점프 연계 파워
                float power = (jumpMinPower + jumpButtonDownTime / jumpButtonDownMaxTime * (1 - jumpMinPower)) * jumpPower + (jumpChainPower * curJumpType);
                // 초기 속도 설정
                curJumpVelocity = power;

                isJumpDone = true;
            }
            //점프키를 뗀 경우에만 다시 jump키를 누를수 있는 상태가 됨
            jumpButtonDownTime += Time.fixedDeltaTime;
            isJumpButtonDown = isJump;
        }

        curJumpChainTime += Time.fixedDeltaTime;
    }

    void GravityUpdate()
    {
        // 점프력에 따른 점프
        // 중력 고려
        // 떨어지는 속력을 가지지만 땅인 경우, 떨어지지 않음
        if (isGround && curJumpVelocity < 0f)
        {
            // 살짝 뜨는 효과 방지
            curJumpVelocity = -1f;
        }
        else
        {
            curJumpVelocity -= gravityScale * Time.fixedDeltaTime;
        }
        controller?.Move(Vector3.up * curJumpVelocity * Time.fixedDeltaTime);
    }

    bool IsGround()
    {
        // 현재 바닥과 충돌중인지 확인
        var cols = Physics.OverlapBox(transform.position, boxSize, transform.rotation, objectslayerMaskOnly);
        return cols.Length > 0;
    }
    #endregion

    #region 애니메이션 관련 기능
    private bool IsWalk;
    private bool isRun;

    void AnimationUpdate()
    {
        if (animator == null || controller == null) return;

        // 이동 업데이트
        animator.SetBool("Walk", IsWalk);
        animator.SetBool("Run", isRun);

        // 점프 업데이트
        animator.SetInteger("JumpType", curJumpType);
        animator.SetFloat("JumpVelocity",curJumpVelocity);
        animator.SetBool("IsGround", isGround);

        // Other Ataction
        animator.SetBool("OtherAction", isOtherAction);
    }
    #endregion
}
