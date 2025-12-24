using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Base Component")]
    [SerializeField] Rigidbody rb;
    [SerializeField] PlayerInput input;


    [Header("Move")]
    [SerializeField] float moveSpeed;
    [SerializeField] float slowRunSpeed;
    [SerializeField] float slowRunTransition;
    [SerializeField] float fastRunSpeed;
    [SerializeField] float movePenalty;

    [Header("Ground Check")]
    [SerializeField] Vector3 boxSize;

    [Header("Animation")]
    [SerializeField] Animator animator;


    float moveTimer = 0f;
    float speed = 0f;
    public bool isOtherAction;

    Camera mainCamera;
    
    void Awake()
    {
        mainCamera = Camera.main;
    }
    void FixedUpdate()
    {
        if (isOtherAction==false)
        {
            Move();
        }
        JumpUpdate();

        isGround = IsGround();
        AnimationUpdate();
    }

    #region Move
    void Move()
    {
        Vector2 move = input.actions["Move"].ReadValue<Vector2>();
        var run = input.actions["Run"].IsPressed();

        if (move == Vector2.zero)
        {
            IsWalk = false;
            moveTimer = 0f;
            return;
        }
        
        Vector3 dir = new Vector3(move.x, 0, move.y).normalized;
        // Move Direction Depends on Camera
        if (mainCamera != null)
        {
            var forward = transform.position - mainCamera.transform.position;
            forward.y = 0;
            var right = Vector3.Cross(Vector3.up,forward);
            dir = (forward * dir.z + right * dir.x).normalized;
        }

		// move : fastMove : run
		if (run)
        {
            isRun = RunType.Fast;
            speed = fastRunSpeed;
        }
        else 
        {
            if(moveTimer < 0f)
            {
                isRun = RunType.Slow;
                speed = slowRunSpeed;
                moveTimer = -1f;    
            }
            else
            {
                isRun = RunType.None;
                speed = moveSpeed;
            }
        }

        var moveDir = speed * Time.fixedDeltaTime * dir;
        if (isGround)
            transform.rotation = Quaternion.LookRotation(moveDir);
        else
        {
            moveDir /= movePenalty;
        }
        rb.MovePosition(rb.position + moveDir);

        if (IsWalk == false)
        {
            moveTimer = slowRunTransition;
            IsWalk = true;    
        }
        moveTimer -= Time.fixedDeltaTime;
    }
    #endregion
    #region Jump
    [Header("Jump")]
    [SerializeField] float jumpMaxPower;
    [SerializeField] float jumpMinPower;
    [SerializeField] float fallMultiplier;
    [SerializeField] float jumpHoldMaxTime;
    [SerializeField] float jumpChainTime;
    [SerializeField] float jumpChainPower;
    bool isGround = true;
    bool hasJumpStarted = false;
    int objectslayerMaskOnly = -(1 << 7);
    float jumpButtonDownTime = 0f;
    float curJumpVelocity = 0f;
    int curJumpType = 0;
    float curJumpChainTime = 0f;
    public void OnJump(InputAction.CallbackContext callback)
    {
        if (isGround == false) return;

        // Jump Checking
        if (callback.started)
        {
            hasJumpStarted = true;
            jumpButtonDownTime = 0f;
        }
        else if (callback.canceled && hasJumpStarted)
        {
            JumpAction();
        }
    }

    void JumpUpdate()
    {
        if(hasJumpStarted)
        {
            if (jumpButtonDownTime > jumpHoldMaxTime)
            {
                JumpAction();
            }
            jumpButtonDownTime += Time.fixedDeltaTime;
        }

        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        curJumpChainTime -= Time.fixedDeltaTime;    
    }

    void JumpAction()
    {
        // Jump Combo
        if (curJumpChainTime > 0f)
            curJumpType = (curJumpType + 1) % 3;
        else
            curJumpType = 0;
        curJumpChainTime = jumpChainTime;

        // Jump Power depends on (Jump Press Time, Jump Type)
        curJumpVelocity = Mathf.Lerp(jumpMinPower, jumpMaxPower, jumpButtonDownTime / jumpHoldMaxTime) + jumpChainPower * curJumpType;
        rb.linearVelocity = Vector3.up * curJumpVelocity;
        
        hasJumpStarted = false;
    }

    bool IsGround()
    {
        var cols = Physics.OverlapBox(transform.position, boxSize, transform.rotation, objectslayerMaskOnly);
        return cols.Length > 0;
    }
    #endregion

    #region Animation
    bool IsWalk;
    RunType isRun;

    void AnimationUpdate()
    {
        if (animator == null) return;

        animator.SetBool("Walk", IsWalk);
        animator.SetInteger("RunType", (int)isRun);
        animator.SetFloat("Velocity", speed);

        animator.SetInteger("JumpType", curJumpType);
        animator.SetFloat("JumpVelocity",curJumpVelocity);
        animator.SetBool("IsGround", isGround);

        animator.SetBool("OtherAction", isOtherAction);
    }
    #endregion
}
