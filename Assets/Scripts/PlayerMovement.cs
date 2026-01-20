using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Base Component")]
    public Rigidbody rb;


    [Header("Move")]
    public float moveSpeed;
    public float slowRunSpeed;
    public float slowRunTransition;
    public float fastRunSpeed;
    public float movePenalty;
    public float rotationSpeed;
    public int slopeAngleWindowSize = 15;

    [Header("Ground Check")]
    public Vector3 boxSize;

    [Header("Animation")]
    public Animator animator;


    Vector3 lastPos;
    float moveTimer = 0f;
    float speed = 0f;
    float drag;
    public bool isOtherAction;

    Camera mainCamera;
    
    void Awake()
    {
        mainCamera = Camera.main;
    }
    void FixedUpdate()
    {
        drag = GetSlopDrag();
        SlideUpdate();
        MoveUpdate();
        JumpUpdate();

        // isGround = IsGround();
        AnimationUpdate();
        lastPos = transform.position;
    }

    #region Move
    Vector3 moveDir;
    bool isSlide;
    bool isRunInput;
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 move = context.ReadValue<Vector2>();

        if (move == Vector2.zero)
        {
            IsWalk = false;
            moveTimer = 0f;
        }
        else
        {
            moveTimer = slowRunTransition;
            IsWalk = true;

            moveDir = new Vector3(move.x, 0, move.y).normalized;
            // Move Direction Depends on Camera
            if (mainCamera != null)
            {
                // var forward = transform.position - mainCamera.transform.position;
                var forward = mainCamera.transform.forward;
                forward.y = 0;
                var right = Vector3.Cross(Vector3.up,forward);
                moveDir = (forward * moveDir.z + right * moveDir.x).normalized;
            }
        }
    }

    void MoveUpdate()
    {
        if (isOtherAction || IsWalk == false) return; 
        // move : fastMove : run
		if (isRunInput)
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

        var spd = speed * Time.fixedDeltaTime * moveDir;
        if (isGround)
        {
            var targetRot = Quaternion.LookRotation(spd);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
            spd*= drag;
        }
        else
        {
            spd /= movePenalty;
        }
        if (isSlide == false) rb.MovePosition(rb.position + spd);
        moveTimer -= Time.fixedDeltaTime;
    }

    void SlideUpdate()
    {
        // check slide
        if ((lastPos.y - transform.position.y) > 0.001f && isJump == false) 
        {
            if (drag < 0.2f) isSlide = true;
        }
        else
        {
            isSlide = false;
        }
    }

    float slopeAverageAngle = 0f;
    Queue<float> angleQueue = new Queue<float>();
    float sumAngleX = 0f;
    float sumAngleY = 0f;
    float GetSlopDrag()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1f))
        {
            var mapInfo = hit.collider.GetComponent<MapInfoController>();
            float maxSlopeAngle = mapInfo != null ? mapInfo.GetMaxSlopeAngleByHitInfo(hit) : 45f;

            // check hill type
            var slopeDir = Vector3.ProjectOnPlane(moveDir, hit.normal).normalized;
            // Debug.DrawRay(hit.point, slopeDir, Color.red, 3f);
            var isUpHill = moveDir.y < slopeDir.y;
            // Debug.DrawRay(hit.point, moveDir, Color.blue, 3f);

            float currentAngle = Vector3.Angle(Vector3.up, hit.normal);
            angleQueue.Enqueue(currentAngle);
            var rad = currentAngle * Mathf.Deg2Rad;
            sumAngleX += Mathf.Cos(rad);
            sumAngleY += Mathf.Sin(rad);

            if (angleQueue.Count > slopeAngleWindowSize)
            {
                rad = angleQueue.Dequeue() * Mathf.Deg2Rad;
                sumAngleX -= Mathf.Cos(rad);
                sumAngleY -= Mathf.Sin(rad);
            }
            slopeAverageAngle = Mathf.Atan2(sumAngleY, sumAngleX) * Mathf.Rad2Deg;
            return isUpHill ? Mathf.SmoothStep(1f,0f, slopeAverageAngle / maxSlopeAngle) : 1f;
        }
        return 1f;
    }
    
    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.started)
            isRunInput = true;
        else if (context.canceled)
            isRunInput = false;
    }
    

    #endregion
    #region Jump
    [Header("Jump")]
    public float jumpMaxPower;
    public float jumpMinPower;
    public float fallMultiplier;
    public float jumpHoldMaxTime;
    public float jumpChainTime;
    public float jumpChainPower;
    bool isGround = true;
    bool isJump = false;
    bool hasJumpStarted = false;
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
        
        if (rb.linearVelocity.y < 0f && isGround) 
        { 
            isJump = false;
        }

        curJumpChainTime -= Time.fixedDeltaTime;    
    }

    void JumpAction()
    {
        if (isJump == true) return;

        // Jump Combo
        if (curJumpChainTime > 0f)
            curJumpType = (curJumpType + 1) % 3;
        else
            curJumpType = 0;
        curJumpChainTime = jumpChainTime;

        // Jump Power depends on (Jump Press Time, Jump Type)
        curJumpVelocity = Mathf.Lerp(jumpMinPower, jumpMaxPower, jumpButtonDownTime / jumpHoldMaxTime) + jumpChainPower * curJumpType;
        rb.linearVelocity = Vector3.up * curJumpVelocity;
        isJump = true;

        hasJumpStarted = false;
    }

    HashSet<Collision> groundCols = new ();

	void OnCollisionEnter(Collision collision)
	{
        if (IsGroundCheck(collision))
        {
            groundCols.Add(collision);
        }
        isGround = groundCols.Count > 0;
	}

	void OnCollisionExit(Collision collision)
	{
		groundCols.Remove(collision);
        isGround = groundCols.Count > 0;
	}

    bool IsGroundCheck(Collision collision)
    {
        foreach(var contanct in collision.contacts)
        {
            if (contanct.normal.y > 0.1f) return true;
        }
        return false;
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
        animator.SetBool("IsJump", isJump);
        animator.SetBool("IsSlide", isSlide);

        animator.SetBool("OtherAction", isOtherAction);
    }
    #endregion
}
