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
            Jump();
        }

        isGround = IsGround();
        // GravityUpdate();

        AnimationUpdate();
    }

    #region Move
    void Move()
    {
        Vector2 move = input.actions["Move"].ReadValue<Vector2>();
        var run = false;
        // Ground Check
        if (isGround == true)
            run = input.actions["Run"].IsPressed();

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

        rb.MovePosition(rb.position + moveDir);
        transform.rotation = Quaternion.LookRotation(moveDir);

        if (IsWalk == false)
        {
            moveTimer = slowRunTransition;
            IsWalk = true;    
        }
        moveTimer -= Time.fixedDeltaTime;
    }
    #endregion
    #region Jump
    private bool isGround = true;
    private bool isJumpButtonDown = false;
    private bool isJumpDone = false;
    private int objectslayerMaskOnly = -(1 << 7);
    private float jumpButtonDownTime = 0f;
    private float curJumpPower = 0f;
    private int curJumpType = 0;
    private float curJumpChainTime = 0f;
    void Jump()
    {
        var isJump = input.actions["Jump"].IsPressed();

        // Jump Checking
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
                // Jump Combo
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

                // Jump Power depends on (Jump Press Time, Jump Type)
                curJumpPower = (jumpMinPower + jumpButtonDownTime / jumpButtonDownMaxTime * (1 - jumpMinPower)) * jumpPower + (jumpChainPower * curJumpType);
                rb.AddForce(Vector3.up * curJumpPower,ForceMode.Impulse);

                isJumpDone = true;
            }
            jumpButtonDownTime += Time.fixedDeltaTime;
            isJumpButtonDown = isJump;
        }

        curJumpChainTime += Time.fixedDeltaTime;
    }

    bool IsGround()
    {
        var cols = Physics.OverlapBox(transform.position, boxSize, transform.rotation, objectslayerMaskOnly);
        return cols.Length > 0;
    }
    #endregion

    #region Animation
    private bool IsWalk;
    private RunType isRun;

    void AnimationUpdate()
    {
        if (animator == null) return;

        animator.SetBool("Walk", IsWalk);
        animator.SetInteger("RunType", (int)isRun);
        animator.SetFloat("Velocity", speed);

        animator.SetInteger("JumpType", curJumpType);
        animator.SetFloat("JumpVelocity",curJumpPower);
        animator.SetBool("IsGround", isGround);

        animator.SetBool("OtherAction", isOtherAction);
    }
    #endregion
}
