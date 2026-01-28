using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ami.BroAudio;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Base Component")]
    public Rigidbody rb;
    public Collider mainCollider;


    [Header("Move")]
    public float moveSpeed;
    public float slowRunSpeed;
    public float slowRunTransition;
    public float fastRunSpeed;
    public float movePenalty;
    public float rotationSpeed;

    [Header("Animation")]
    public Animator animator;
    public ParticleSystem jumpParticle;

    [Header("SFX")]
    public SoundID jumpSFX;

    Vector3 lastPos;
    float moveTimer = 0f;
    float speed = 0f;
    float climbAbility;
    public bool isOtherAction;

    Camera mainCamera;
    
    void Awake()
    {
        mainCamera = Camera.main;
    }
    void FixedUpdate()
    {
        GroundCheck();
        UpdateMoveKeyInput();
        SlideUpdate();
        MoveUpdate();
        JumpUpdate();

        // isGround = IsGround();
        AnimationUpdate();
        lastPos = transform.position;
    }

    #region Move
    Vector2 keyDir;
    Vector3 moveDir;
    bool isSlide;
    bool isUpHill;
    Vector3 slopeDir;
    bool isRunInput;
    // ContactPoint contact;

    public void OnMove(InputAction.CallbackContext context)
    {
        keyDir = context.ReadValue<Vector2>();
        if (context.started)
            moveTimer = slowRunTransition;

        IsWalk = keyDir != Vector2.zero;
    }

    void UpdateMoveKeyInput()
    {
        if (keyDir == Vector2.zero)
        {
            moveTimer = 0f;
        }
        else
        {

            moveDir = new Vector3(keyDir.x, 0, keyDir.y).normalized;
            // Move Direction Depends on Camera
            if (mainCamera != null)
            {
                // var forward = transform.position - mainCamera.transform.position;
                var forward = mainCamera.transform.forward;
                forward.y = 0;
                forward.Normalize();

                var right = Vector3.Cross(Vector3.up,forward);
                right.Normalize();

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
            if (isSlide) spd*= climbAbility;
        }
        else
        {
            spd /= movePenalty;
        }
        if (isSlide == false) rb.MovePosition(rb.position + spd);
        moveTimer -= Time.fixedDeltaTime;
    }

    RaycastHit lastGroundHit;
    float slideLockTimer;
    bool absoluteUpHillCondition = false;
    float originFriction = 0.5f;
    void SlideUpdate()
    {
        climbAbility = GetSlopDrag();
        mainCollider.sharedMaterial.dynamicFriction = climbAbility;

        // check slide
        if (isSlide == false)
        {
            if (isUpHill && (lastPos.y - transform.position.y) > 0.01f && slopeDir.y > 0f && isJump == false) 
            {
                isSlide = true;
                // Debug.Log($"{isUpHill} {slopeDir} {moveDir} {lastGroundHit.normal}");
                // Debug.DrawLine(transform.position, transform.position + slopeDir, Color.red, 2f);
                // Debug.DrawLine(transform.position, transform.position + moveDir, Color.green, 2f);
                // Debug.DrawLine(transform.position, transform.position + lastGroundHit.normal, Color.blue, 2f);
                mainCollider.sharedMaterial.dynamicFriction = 0f;
                rb.AddForce(Vector3.down);
                slideLockTimer = 0f;
            }
        }
        else
        {
            if ((lastGroundHit.normal.y > 0.9f && absoluteUpHillCondition == false) || Vector3.SqrMagnitude(transform.position - lastPos) < 0.01f && slideLockTimer > 1f) 
            {
                isSlide = false;
                mainCollider.sharedMaterial.dynamicFriction = originFriction;
            }
            slideLockTimer += Time.fixedDeltaTime;
        }
    }

    float GetSlopDrag()
    {
        if (Physics.Raycast(transform.position, -transform.up, out lastGroundHit, 10f) == false) return 1f;
        var map = lastGroundHit.collider.GetComponent<MapInfoController>();
        if (map == null) return 1f;

        if (map.TryGetSlopeMaxAngle(lastGroundHit,out var maxAngle) == false) return 1f;
        // check hill type
        var angle = Vector3.Angle(Vector3.up,lastGroundHit.normal);
        // Debug.Log(angle);
        absoluteUpHillCondition = angle > maxAngle;
        // Debug.Log($"{angle}... {maxAngle}");
        if (absoluteUpHillCondition)
        {
            isUpHill = true;
            mainCollider.sharedMaterial.dynamicFriction = 0f;
        }
        else isUpHill = angle > 0f;
        
        slopeDir = Vector3.ProjectOnPlane(moveDir,lastGroundHit.normal);
        // Debug.Log(slopeDir.y > 0f);
        var isMoveDrag = isUpHill && slopeDir.y > 0f;
        return isMoveDrag ? Mathf.SmoothStep(1f,0f, angle / maxAngle) : 1f;
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
    public float groundCheckThreshold = 0.1f;
    public float slopeJumpThreshold = 0.5f;
    bool isGround = true;
    bool isWall;
    bool isJump;
    bool hasJumpStarted;
    float jumpButtonDownTime = 0f;
    float curJumpVelocity = 0f;
    int curJumpType = 0;
    float curJumpChainTime = 0f;
    public void OnJump(InputAction.CallbackContext callback)
    {
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
                hasJumpStarted = false;
            }
            jumpButtonDownTime += Time.fixedDeltaTime;
        }

        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        
        if (rb.linearVelocity.y < 0f && isGround) 
        { 
            if (isJump) jumpParticle.Play();
            isJump = false;
        }

        curJumpChainTime -= Time.fixedDeltaTime;    
    }

    void JumpAction()
    {
        if (!CanJump()) return;

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

        BroAudio.Play(jumpSFX).SetVelocity(curJumpType);
    }
    bool CanJump()
    {
        var slopeCondition = isUpHill? slopeDir.y > 0f && (transform.position.y - lastPos.y < slopeJumpThreshold) && IsWalk : false;
        return !isJump && !isOtherAction && isGround && !isSlide && !slopeCondition && !isWall;  
    }

    HashSet<Collider> groundCols = new ();
    HashSet<Collider> wallCols =  new ();

    void GroundCheck()
    {
        isGround = groundCols.Count > 0 || Vector3.SqrMagnitude(transform.position - lastGroundHit.point) < 0.1f;
        isWall = wallCols.Count > 0;
    }

	void OnCollisionStay(Collision collision)
	{
        if (collision.collider.CompareTag("Ground") == false) return;
        var type = GetContactCollisionType(collision);
        if (type < 0) return;

        // groud
        if ((type & 1<<2) > 0) 
        {
            groundCols.Add(collision.collider);
        }
        else groundCols.Remove(collision.collider);
        // wall
        if ((type & 1<<1) > 0) wallCols.Add(collision.collider);
        else wallCols.Remove(collision.collider);
	}

	void OnCollisionExit(Collision collision)
	{
        if (collision.collider.CompareTag("Ground") == false) return;
		groundCols.Remove(collision.collider);
        wallCols.Remove(collision.collider);
	}

    int GetContactCollisionType(Collision collision)
    {
        var bitflags = 0;
        foreach(var contact in collision.contacts)
        {
            // (012) 0: ground 1: wall 2: celling
            if (contact.normal.y > groundCheckThreshold) bitflags |= 1<<2;
            else if (contact.normal.y > -groundCheckThreshold) bitflags |= 1<<1;
            else bitflags |= 1<<0;
        }
        return bitflags;
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
