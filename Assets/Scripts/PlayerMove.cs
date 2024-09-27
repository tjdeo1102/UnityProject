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
        // �̹� �޸��� ���߿� �����ϴ� ��쿡 ���ؼ��� ���� �ӷ� ������ ����
        // ���� �߿� �޸��� ���� �ٱ� �� �ӷ����� ����
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

    #region Jump���� ���
    private bool canJump = true;
    // ������ �̹� ��ư�� ������ �־����� Ȯ���ϴ� ����
    private bool isJumpButtonDown = false;
    private int objectslayerMaskOnly = -(1 << 7);
    private float jumpButtonDownTime = 0;
    private float curJumpDelay = 0f;
    private float curJumpVelocity = 0f;

    void Jump()
    {
        Debug.DrawRay(transform.position, -transform.up * checkFloorDistance);

        var isJump = input.actions["Jump"].IsPressed();

        // �Է��� ���� ������ ����
        // �ִ� Ȧ�� �ð����� ���� ������ŭ ���� ���� �ð��� ����
        // �ִ� Ȧ�� �ð��� �����ų�, ��ư�� �� ��쿡�� ������ �Ŀ��� ����

        if (isJump && canJump)
        {
            jumpButtonDownTime += Time.deltaTime;
            isJumpButtonDown = true;
        }

        if (isJumpButtonDown && (jumpButtonDownTime > jumpButtonDownMaxTime || isJump == false))
        {
            // �ּ� �ӵ� ����
            float power = (jumpMinPower + jumpButtonDownTime / jumpButtonDownMaxTime * (1 - jumpMinPower)) * jumpPower;
            // �ʱ� �ӵ� ����
            curJumpVelocity = power;
        
            // ���� �ʱ�ȭ
            jumpButtonDownTime = 0f;
            isJumpButtonDown = false;
            canJump = false;
        }
        // ���� ��, ������ �ð� ���� ��, �ٴ� �浹 Ȯ��
        // ���� �ٴڰ� �浹������ Ȯ��
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
        // �����¿� ���� ����
        // �߷� ���
        curJumpVelocity -= Physics.gravity.magnitude * Time.deltaTime;
        // ���� �ְ� �ӷ� ����
        curJumpVelocity = Mathf.Clamp(curJumpVelocity, -jumpMaxSpeed, jumpMaxSpeed);
        controller.Move(Vector3.up * curJumpVelocity * Time.deltaTime);
    }

    #endregion

    #region Animation ���� ���
    private bool IsWalk;
    private bool isRun;

    void AnimationUpdate()
    {
        if (animator == null || controller == null) return;

        // �̵� ������Ʈ
        animator.SetBool("Walk", IsWalk);
        animator.SetBool("Run", isRun);

        // ���� ������Ʈ
        animator.SetBool("Jump", !canJump);

        // Other Ataction
        animator.SetBool("OtherAction", isOtherAction);
    }
    #endregion
}
