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

    #region �̵� ���� ���
    void Move()
    {
        Vector2 move = input.actions["Move"].ReadValue<Vector2>();
        // �̹� �޸��� ���߿� �����ϴ� ��쿡 ���ؼ��� ���� �ӷ� ������ ����
        // ���� �߿� �޸��� ���� �ٱ� �� �ӷ����� ����
        if (isGround == true)
            isRun = input.actions["Run"].IsPressed();

        if (move == Vector2.zero)
        {
            IsWalk = false;
            return;
        }
        
        Vector3 dir = new Vector3(move.x, 0, move.y).normalized;
        // ī�޶� �ٶ󺸴� ������ ���� �ǵ��� ����
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
    #region �������� ���
    private bool isGround = true;
    // ������ �̹� ��ư�� ������ �־����� Ȯ���ϴ� ����
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

        // �Է��� ���� ������ ����
        // �ִ� Ȧ�� �ð����� ���� ������ŭ ���� ���� �ð��� ����
        // �ִ� Ȧ�� �ð��� �����ų�, ��ư�� �� ��쿡�� ������ �Ŀ��� ����

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
                // ������ �� ���� ����, ���� ���谡 ������ �ð� ����
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

                // �ּ� �ӵ� ���� + ���� ���� �Ŀ�
                float power = (jumpMinPower + jumpButtonDownTime / jumpButtonDownMaxTime * (1 - jumpMinPower)) * jumpPower + (jumpChainPower * curJumpType);
                // �ʱ� �ӵ� ����
                curJumpVelocity = power;

                isJumpDone = true;
            }
            //����Ű�� �� ��쿡�� �ٽ� jumpŰ�� ������ �ִ� ���°� ��
            jumpButtonDownTime += Time.fixedDeltaTime;
            isJumpButtonDown = isJump;
        }

        curJumpChainTime += Time.fixedDeltaTime;
    }

    void GravityUpdate()
    {
        // �����¿� ���� ����
        // �߷� ���
        // �������� �ӷ��� �������� ���� ���, �������� ����
        if (isGround && curJumpVelocity < 0f)
        {
            // ��¦ �ߴ� ȿ�� ����
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
        // ���� �ٴڰ� �浹������ Ȯ��
        var cols = Physics.OverlapBox(transform.position, boxSize, transform.rotation, objectslayerMaskOnly);
        return cols.Length > 0;
    }
    #endregion

    #region �ִϸ��̼� ���� ���
    private bool IsWalk;
    private bool isRun;

    void AnimationUpdate()
    {
        if (animator == null || controller == null) return;

        // �̵� ������Ʈ
        animator.SetBool("Walk", IsWalk);
        animator.SetBool("Run", isRun);

        // ���� ������Ʈ
        animator.SetInteger("JumpType", curJumpType);
        animator.SetFloat("JumpVelocity",curJumpVelocity);
        animator.SetBool("IsGround", isGround);

        // Other Ataction
        animator.SetBool("OtherAction", isOtherAction);
    }
    #endregion
}
