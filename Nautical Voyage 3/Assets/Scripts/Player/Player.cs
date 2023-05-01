using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7.5f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float jumpTime = 0.5f;

    [Header("Turn Check")]
    [SerializeField] private GameObject lLeg;
    [SerializeField] private GameObject rLeg;

    [Header("Ground Check")]
    [SerializeField] private float extraHeight = 0.25f;
    [SerializeField] private LayerMask whatIsGround;

    [HideInInspector] public bool isFacingRight;

    [Header("References")]
    private Rigidbody2D rb;
    private Collider2D coll;
    private Animator anim;
    [SerializeField] private TrailRenderer tr;

    [Header("Camera Stuff")]
    [SerializeField] private GameObject _cameraFollowGO;

    //jump stuff
    private float moveInput;
    private bool isJumping;
    private bool isFalling;
    private float jumpTimeCounter;

    //coyote timer
    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    //jump buffer
    private float jumpBufferTime = 0.1f;
    private float jumpBufferCounter;

    //dash
    private bool canDash = true;
    private bool isDashing;
    private float dashingPower = 24f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 1f;

    //double jump
    private bool doubleJump;

    //ground detection
    private RaycastHit2D groundHit;

    //camera stuff
    private CameraFollowObject _cameraFollowObject;
    private float _fallSpeedYDampingChangeThreshond;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();

        StartDirectionCheck();



        _cameraFollowObject = _cameraFollowGO.GetComponent<CameraFollowObject>();

        _fallSpeedYDampingChangeThreshond = CameraManager.instance._fallSpeedYDampingChangeThreshold;
    }

    private void Update()
    {
        if (isDashing)
        {
            return;
        }

        Move();
        Jump();
        CoyoteTimeCheck();
        JumpBufferCheck();
        DashCheck();
        DoubleJumpCheck();
        FallCheck();
        CheckForLand();

        if (CheckForLand())
        {
            anim.SetTrigger("landed");
        }

        //if we are falling past a certain speed threshold
        if (rb.velocity.y < _fallSpeedYDampingChangeThreshond && !CameraManager.instance.IsLerpingYDamping && !CameraManager.instance.LerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpYDamping(true);
        }

        //if we are standing still or moving up
        if (rb.velocity.y >= 0f && !CameraManager.instance.IsLerpingYDamping && CameraManager.instance.LerpedFromPlayerFalling)
        {
            //reset so it can be called again
            CameraManager.instance.LerpedFromPlayerFalling = false;

            CameraManager.instance.LerpYDamping(false);
        }
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            return;
        }
    }

    #region Movement Functions

    private void Move()
    {
        moveInput = UserInput.instance.moveInput.x;

        if (moveInput > 0 || moveInput < 0)
        {
            anim.SetBool("isRunning", true);
            TurnCheck();
        }

        else
        {
            anim.SetBool("isRunning", false);
        }

        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    private void Jump()
    {
        //button was pushed this frame
        if (jumpBufferCounter >0f && coyoteTimeCounter >0f || UserInput.instance.controls.Movement.Jump.WasPressedThisFrame() && doubleJump)
        {
            isJumping = true;
            jumpTimeCounter = jumpTime;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpBufferCounter = 0f;
            doubleJump = !doubleJump;
        }

        //button is being held
        if (UserInput.instance.controls.Movement.Jump.IsPressed())
        {
            if (jumpTimeCounter > 0 && isJumping)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpTimeCounter -= Time.deltaTime;
                anim.SetTrigger("jump");
            }

            else if (jumpTimeCounter == 0)
            {
                isJumping = false;
            }

            else
            {
                isJumping = false;
            }
        }

        //button was released this frame
        if (UserInput.instance.controls.Movement.Jump.WasReleasedThisFrame())
        {
            isFalling = true;
            isJumping = false;
            coyoteTimeCounter = 0f;

            if (!isJumping && CheckForLand())
            {

            }
        }
    }

    private void CoyoteTimeCheck()
    {
        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
        }

        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    private void JumpBufferCheck()
    {
        if (UserInput.instance.controls.Movement.Jump.WasPressedThisFrame())
        {
            jumpBufferCounter = jumpBufferTime;
        }

        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    private bool CheckForLand()
    {
        if (isFalling && IsGrounded())
        {
            //player has landed
            anim.SetTrigger("landed");
            isFalling = false;
            return true;
        }

        else
        {
            return false;
        }
    }

    private void FallCheck()
    {
        if (isFalling)
        {
            anim.SetTrigger("fall");
        }
    }

    #endregion

    #region Movement Abilities

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(transform.right.x * dashingPower, 0f);
        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    private void DashCheck()
    {
        if (UserInput.instance.controls.Movement.Dash.WasPressedThisFrame() && canDash)
        {
            StartCoroutine(Dash());
            isJumping = false;
        }

        if (isDashing)
        {
            anim.SetBool("isDashing", true);
        }
        else
        {
            anim.SetBool("isDashing", false);
        }
    }
    
    private void DoubleJumpCheck()
    {
        if (IsGrounded() && !UserInput.instance.controls.Movement.Jump.WasPressedThisFrame())
        {
            doubleJump = true;
        }
    }

    #endregion

    #region Ground/Landed Check

    private bool IsGrounded()
    {
        groundHit = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0, Vector2.down, extraHeight, whatIsGround);

        if (groundHit.collider != null)
        {
            return true;
        }

        else
        {
            return false;
        }
    }

    #endregion

    #region Turn Checks

    private void StartDirectionCheck()
    {
        if (rLeg.transform.position.x > lLeg.transform.position.x)
        {
            isFacingRight = true;
        }

        else
        {
            isFacingRight = false;
        }
    }

    private void TurnCheck()
    {
        if (UserInput.instance.moveInput.x > 0 && !isFacingRight)
        {
            Turn();
        }

        else if (UserInput.instance.moveInput.x < 0 && isFacingRight)
        {
            Turn();
        }
    }

    private void Turn()
    {
        if (isFacingRight)
        {
            Vector3 rotator = new Vector3(transform.rotation.x, 180f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            isFacingRight = !isFacingRight;

            //turn the camera follow object
            _cameraFollowObject.CallReturn();
        }

        else
        {
            Vector3 rotator = new Vector3(transform.rotation.x, 0f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            isFacingRight = !isFacingRight;

            //turn the camera follow object
            _cameraFollowObject.CallReturn();
        }
    }

    #endregion
}
