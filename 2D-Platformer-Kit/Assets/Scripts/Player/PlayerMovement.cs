using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script copied while following tutorial (in README)
// Was attempting to adapt it into the PlayerController state machine
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public PlayerMoveStatsSO moveStats;
    [SerializeField] private Collider2D bodyCollider;
    [SerializeField] private Collider2D feetCollider;
    private Rigidbody2D rb;

    // movement
    public float horizontalVelocity {get; private set;}
    private bool isFacingRight;

    // collision check
    private RaycastHit2D groundHit;
    private RaycastHit2D headHit;
    private RaycastHit2D wallHit;
    private RaycastHit2D lastWallHit;
    private bool isGrounded;
    private bool bumpedHead;
    private bool isTouchingWall;

    // jump
    public float verticalVelocity {get; private set;}
    private bool isJumping;
    private bool isFastFalling;
    private bool isFalling;
    private float fastFallTime;
    private float fastFallReleaseSpeed;
    private int numJumpsUsed;

    // apex vars
    private float apexPoint;
    private float timePastApexThreshold;
    private bool isPastApexThreshold;

    // jump buffer
    private float jumpBufferTimer;
    private bool jumpReleasedDuringBuffer;

    // coyote time
    private float coyoteTimer;

    // wall slide
    private bool isWallSliding;
    private bool isWallSlideFalling;

    // wall jump
    private bool useWallJumpMoveStats;
    private bool isWallJumping;
    private float wallJumpTime;
    private bool isWallJumpFastFalling;
    private bool isWallJumpFalling;
    private float wallJumpFastFallTime;
    private float wallJumpFastFallReleaseSpeed;

    private float wallJumpPostBufferTimer;
    private float wallJumpApexPoint;
    private float timePastWallJumpApexThreshold;
    private bool isPastWallJumpApexThreshold;


    private void Awake()
    {
        isFacingRight = true;

        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        CountTimers();
        JumpChecks();
        LandCheck();
        WallJumpCheck();

        WallSlideCheck();
    }

    private void FixedUpdate()
    {
        CollisionChecks();
        Jump();
        Fall();
        WallSlide();
        WallJump();

        if (isGrounded)
        {
            Move(moveStats.groundAcceleration, moveStats.groundDeceleration, InputManager.Instance.moveDirection);
        }
        else
        {
            // wall jumping
            if (useWallJumpMoveStats)
            {
                Move(moveStats.wallJumpMoveAcceleration, moveStats.wallJumpMoveDeceleration, InputManager.Instance.moveDirection);
            }
            // airborne
            else
            {
                Move(moveStats.airAcceleration, moveStats.airDeceleration, InputManager.Instance.moveDirection);
            }
        }

        ApplyVelocity();
    }

    private void ApplyVelocity()
    {
        // CLAMP FALL SPEED
        verticalVelocity = Mathf.Clamp(verticalVelocity, -moveStats.maxFallSpeed, 50f);

        rb.velocity = new Vector2(horizontalVelocity, verticalVelocity);
    }

    #region Movement

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (Mathf.Abs(moveInput.x) != 0f)
        {
            TurnCheck(moveInput);

            float targetVelocity = 0f;
            if (InputManager.Instance.runIsHeld)
            {
                targetVelocity = moveInput.x * moveStats.maxRunSpeed;
            }
            else
            {
                targetVelocity = moveInput.x * moveStats.maxWalkSpeed;
            }

            horizontalVelocity = Mathf.Lerp(horizontalVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }

        else if (Mathf.Abs(moveInput.x) == 0f)
        {
            horizontalVelocity = Mathf.Lerp(horizontalVelocity, 0f, deceleration * Time.fixedDeltaTime);
        }
    }

    private void TurnCheck(Vector2 moveInput)
    {
        if (isFacingRight && moveInput.x < 0)
        {
            Turn(false);
        }

        else if (!isFacingRight && moveInput.x > 0)
        {
            Turn(true);
        }
    }

    private void Turn(bool turnRight)
    {
        if (turnRight)
        {
            isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
        else
        {
            isFacingRight = false;
            transform.Rotate(0f, -180f, 0f);
        }
    }

    #endregion

    #region Land/Fall

    private void LandCheck()
    {
        // LANDED
        if ((isJumping || isFalling || isWallJumpFalling || isWallJumping || isWallSlideFalling || isWallSliding) && isGrounded && verticalVelocity <= 0f)
        {
            ResetJumpValues();
            StopWallSlide();
            ResetWallJumpValues();
            
            numJumpsUsed = 0;

            verticalVelocity = Physics2D.gravity.y;
        }
    }

    private void Fall()
    {
        // NORMAL GRAVITY WHILE FALLING
        if (!isGrounded && !isJumping && !isWallSliding && !isWallJumping)
        {
            if (!isFalling)
            {
                isFalling = true;
            }

            verticalVelocity += moveStats.gravity * Time.fixedDeltaTime;
        }
    }

    #endregion

    #region Jump

    private void ResetJumpValues()
    {
        isJumping = false;
        isFalling = false;
        isFastFalling = false;
        fastFallTime = 0f;
        isPastApexThreshold = false;
    }
    
    private void JumpChecks()
    {
        // JUMP BUTTON PRESSED
        if (InputManager.Instance.jumpWasPressed)
        {
            if (isWallSlideFalling && wallJumpPostBufferTimer >= 0f)
            {
                return;
            }

            else if (isWallSliding || (isTouchingWall && !isGrounded))
            {
                return;
            }

            jumpBufferTimer = moveStats.jumpBufferTime;
            jumpReleasedDuringBuffer = false;
        }

        // JUMP BUTTON RELEASED
        if (InputManager.Instance.jumpWasReleased)
        {
            if (jumpBufferTimer > 0f)
            {
                jumpReleasedDuringBuffer = true;
            }

            if (isJumping && verticalVelocity > 0f)
            {
                if (isPastApexThreshold)
                {
                    isPastApexThreshold = false;
                    isFastFalling = true;
                    fastFallTime = moveStats.timeForUpwardsCancel;
                    verticalVelocity = 0f;
                }
                else
                {
                    isFastFalling = true;
                    fastFallReleaseSpeed = verticalVelocity;
                }
            }
        }

        // INITIATE JUMP WITH BUFFERING & COYOTE TIME
        if (jumpBufferTimer > 0f && !isJumping && (isGrounded || coyoteTimer > 0f))
        {
            InitiateJump(1);

            if (jumpReleasedDuringBuffer)
            {
                isFastFalling = true;
                fastFallReleaseSpeed = verticalVelocity;
            }
        }

        // DOUBLE JUMP
        else if (jumpBufferTimer > 0f && (isJumping || isWallJumping || isWallSlideFalling) && !isTouchingWall && numJumpsUsed < moveStats.numJumpsAllowed)
        {
            isFastFalling = false;
            InitiateJump(1);
        }

        // AIR JUMP AFTER COYOTE TIME LAPSED
        else if (jumpBufferTimer > 0f && isFalling && !isWallSlideFalling && numJumpsUsed < moveStats.numJumpsAllowed - 1)
        {
            InitiateJump(2);
            isFastFalling = false;
        }
    }

    private void InitiateJump(int aNumJumpsUsed)
    {
        if (!isJumping)
        {
            isJumping = true;
        }

        ResetWallJumpValues();

        jumpBufferTimer = 0f;
        numJumpsUsed += aNumJumpsUsed;
        verticalVelocity = moveStats.initialJumpVelocity;
    }

    private void Jump()
    {
        // APPLY GRAVITY WHLIE JUMPING
        if (isJumping)
        {
            // CHECK FOR HEAD BUMP
            if (bumpedHead)
            {
                isFastFalling = true;
            }

            // GRAVITY ON ASCENDING
            if (verticalVelocity >= 0f)
            {
                // APEX CONTROLS
                apexPoint = Mathf.InverseLerp(moveStats.initialJumpVelocity, 0f, verticalVelocity);

                if (apexPoint > moveStats.apexThreshold)
                {
                    if (!isPastApexThreshold)
                    {
                        isPastApexThreshold = true;
                        timePastApexThreshold = 0f;
                    }

                    if (isPastApexThreshold)
                    {
                        timePastApexThreshold += Time.fixedDeltaTime;
                        if (timePastApexThreshold < moveStats.apexHangTime)
                        {
                            verticalVelocity = 0f;
                        }
                        else
                        {
                            verticalVelocity = -0.01f;
                        }
                    }
                }

                // GRAVITY ON ASCENDING BUT NOT PAST APEX THRESHOLD
                else if (!isFastFalling)
                {
                    verticalVelocity += moveStats.gravity * Time.fixedDeltaTime;
                    if (isPastApexThreshold)
                    {
                        isPastApexThreshold = false;
                    }
                }
            }

            // GRAVITY ON DESCENDING
            else if (!isFastFalling)
            {
                verticalVelocity += moveStats.gravity * moveStats.gravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }

            else if (verticalVelocity < 0f)
            {
                if (!isFalling)
                {
                    isFalling = true;
                }
            }
        }

        // JUMP CUT
        if (isFastFalling)
        {
            if (fastFallTime >= moveStats.timeForUpwardsCancel)
            {
                verticalVelocity += moveStats.gravity * moveStats.gravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (fastFallTime < moveStats.timeForUpwardsCancel)
            {
                verticalVelocity = Mathf.Lerp(fastFallReleaseSpeed, 0f, (fastFallTime / moveStats.timeForUpwardsCancel));
            }

            fastFallTime += Time.fixedDeltaTime;
        }
    }

    private void DrawJumpArc(float moveSpeed, Color gizmoColor) // does not account for max fall speed
    {
        Vector2 startPosition = new Vector2(feetCollider.bounds.center.x, feetCollider.bounds.min.y);
        Vector2 previousPosition = startPosition;
        float speed = 0f;
        if (moveStats.drawRight)
        {
            speed = moveSpeed;
        }
        else
        {
            speed = -moveSpeed;
        }
        Vector2 velocity = new Vector2(speed, moveStats.initialJumpVelocity);

        Gizmos.color = gizmoColor;

        float timeStep = 2 * moveStats.timeTillJumpApex / moveStats.arcResolution; // time step for simulation
        //float totalTime = (2 * moveStats.timeTillJumpApex) + moveStats.apexHangTime; // total time of arc including hang time

        for (int i = 0; i < moveStats.visualizationSteps; i++)
        {
            float simulationTime = i * timeStep;
            Vector2 displacement;
            Vector2 drawPoint;

            if (simulationTime < moveStats.timeTillJumpApex) // Ascending
            {
                displacement = velocity * simulationTime + 0.5f * new Vector2(0, moveStats.gravity) * simulationTime * simulationTime;
            }
            else if (simulationTime < moveStats.timeTillJumpApex + moveStats.apexHangTime) // apex hang time
            {
                float apexTime = simulationTime - moveStats.timeTillJumpApex;
                displacement = velocity * moveStats.timeTillJumpApex + 0.5f * new Vector2(0, moveStats.gravity) * moveStats.timeTillJumpApex * moveStats.timeTillJumpApex;
                displacement += new Vector2(speed, 0) * apexTime; // no vertical movement during hang time
            }
            else // descending
            {
                float descendTime = simulationTime - (moveStats.timeTillJumpApex + moveStats.apexHangTime);
                displacement = velocity * moveStats.timeTillJumpApex + 0.5f * new Vector2(0, moveStats.gravity) * moveStats.timeTillJumpApex * moveStats.timeTillJumpApex;
                displacement += new Vector2(speed, 0) * moveStats.apexHangTime; // horizontal movement during hang time
                displacement += new Vector2(speed, 0) * descendTime + 0.5f * new Vector2(0, moveStats.gravity * descendTime * descendTime);
            }

            drawPoint = startPosition + displacement;

            if (moveStats.stopOnCollision)
            {
                RaycastHit2D hit = Physics2D.Raycast(previousPosition, drawPoint - previousPosition, Vector2.Distance(previousPosition, drawPoint), moveStats.groundLayer);
                if (hit.collider != null)
                {
                    // if hit detected, stop drawing the arc there
                    Gizmos.DrawLine(previousPosition, hit.point);
                    break;
                }
            }

            Gizmos.DrawLine(previousPosition, drawPoint);
            previousPosition = drawPoint;
        }
    }

    #endregion

    #region Wall Slide

    private void WallSlideCheck()
    {
        if (isTouchingWall && !isGrounded)
        {
            if (verticalVelocity < 0f && !isWallSliding)
            {
                ResetJumpValues();
                ResetWallJumpValues();

                isWallSlideFalling = false;
                isWallSliding = true;

                if (moveStats.resetJumpOnWallSlide)
                {
                    numJumpsUsed = 0;
                }
            }
        }

        else if (isWallSliding && !isTouchingWall && !isGrounded && !isWallSlideFalling)
        {
            isWallSlideFalling = true;
            StopWallSlide();
        }

        else
        {
            StopWallSlide();
        }
    }

    private void StopWallSlide()
    {
        if (isWallSliding)
        {
            numJumpsUsed++;
            isWallSliding = false;
        }
    }

    private void WallSlide()
    {
        if (isWallSliding)
        {
            verticalVelocity = Mathf.Lerp(verticalVelocity, -moveStats.wallSlideSpeed, moveStats.wallSlideDecelerationSpeed * Time.fixedDeltaTime);
        }
    }

    #endregion

    #region Wall Jump

    private void WallJumpCheck()
    {
        if (ShouldApplyPostWallJumpBuffer())
        {
            wallJumpPostBufferTimer = moveStats.wallJumpPostBufferTime;
        }

        // wall jump fast falling
        if (InputManager.Instance.jumpWasReleased && !isWallSliding && !isTouchingWall && isWallJumping)
        {
            if (verticalVelocity > 0f)
            {
                if (isPastWallJumpApexThreshold)
                {
                    isPastWallJumpApexThreshold = false;
                    isWallJumpFastFalling = true;
                    wallJumpFastFallTime = moveStats.timeForUpwardsCancel;

                    verticalVelocity = 0f;
                }
                else
                {
                    isWallJumpFastFalling = true;
                    wallJumpFastFallReleaseSpeed = verticalVelocity;
                }
            }
        }

        // actual jump with post wall jump buffer time
        if (InputManager.Instance.jumpWasPressed && wallJumpPostBufferTimer > 0f)
        {
            InitiateWallJump();
        }
    }

    private void InitiateWallJump()
    {
        if (!isWallJumping)
        {
            isWallJumping = true;
            useWallJumpMoveStats = true;
        }

        StopWallSlide();
        ResetJumpValues();
        wallJumpTime = 0f;

        verticalVelocity = moveStats.initialWallJumpVelocity;

        int dirMultiplier = 0;
        Vector2 hitPoint = lastWallHit.collider.ClosestPoint(bodyCollider.bounds.center);

        if (hitPoint.x > transform.position.x)
        {
            dirMultiplier = -1;
        }
        else
        {
            dirMultiplier = 1;
        }

        horizontalVelocity = Mathf.Abs(moveStats.wallJumpDirection.x) * dirMultiplier;
    }

    private void WallJump()
    {
        // APPLY WALL JUMP GRAVITY
        if (isWallJumping)
        {
            // TIME TO TAKE OVER MOVEMENT CONTROLS WHILE JUMPING
            wallJumpTime += Time.fixedDeltaTime;
            if (wallJumpTime >= moveStats.timeTillJumpApex)
            {
                useWallJumpMoveStats = false;
            }

            // HIT HEAD
            if (bumpedHead)
            {
                isWallJumpFastFalling = true;
                useWallJumpMoveStats = false;
            }

            // GRAVITY IN ASCENDING
            {
                if (verticalVelocity >= 0f)
                {
                    // APEX CONTROLS
                    wallJumpApexPoint = Mathf.InverseLerp(moveStats.wallJumpDirection.y, 0f, verticalVelocity);

                    if (wallJumpApexPoint > moveStats.apexThreshold)
                    {
                        if (!isPastWallJumpApexThreshold)
                        {
                            isPastWallJumpApexThreshold = true;
                            timePastWallJumpApexThreshold = 0f;
                        }

                        if (isPastWallJumpApexThreshold)
                        {
                            timePastWallJumpApexThreshold += Time.fixedDeltaTime;
                            if (timePastWallJumpApexThreshold < moveStats.apexHangTime)
                            {
                                verticalVelocity = 0f;
                            }
                            else
                            {
                                verticalVelocity = -0.01f;
                            }
                        }
                    }

                    // GRAVITY IN ASCENDING BUT NOT PAST APEX THRESHOLD
                    else if (!isWallJumpFastFalling)
                    {
                        verticalVelocity += moveStats.wallJumpGravity * Time.fixedDeltaTime;

                        if (isPastWallJumpApexThreshold)
                        {
                            isPastWallJumpApexThreshold = false;
                        }
                    }
                }

                // GRAVITY ON DESCENDING
                else if (!isWallJumpFastFalling)
                {
                    verticalVelocity += moveStats.wallJumpGravity * Time.fixedDeltaTime;
                }

                else if (verticalVelocity < 0f)
                {
                    if (!isWallJumpFalling)
                    {
                        isWallJumpFalling = true;
                    }
                }
            }

            //HANDLE WALL JUMP CUT TIME
            if (isWallJumpFastFalling)
            {
                if (wallJumpFastFallTime >= moveStats.timeForUpwardsCancel)
                {
                    verticalVelocity += moveStats.wallJumpGravity * moveStats.wallJumpGravityOnReleaseMultiplier * Time.fixedDeltaTime;
                }
                else if (wallJumpFastFallTime < moveStats.timeForUpwardsCancel)
                {
                    verticalVelocity = Mathf.Lerp(wallJumpFastFallReleaseSpeed, 0f, (wallJumpFastFallTime / moveStats.timeForUpwardsCancel));
                }

                wallJumpFastFallTime += Time.fixedDeltaTime;
            }
        }
    }

    private bool ShouldApplyPostWallJumpBuffer()
    {
        if (!isGrounded && (isTouchingWall || isWallSliding))
        {
            return true;
        }
        return false;
    }

    private void ResetWallJumpValues()
    {
        isWallSlideFalling = false;
        useWallJumpMoveStats = false;
        isWallJumping = false;
        isWallJumpFastFalling = false;
        isWallJumpFalling = false;
        isPastWallJumpApexThreshold = false;

        wallJumpFastFallTime = 0f;
        wallJumpTime = 0f;
    }

    #endregion

    #region Collision Checks

    private void IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(feetCollider.bounds.center.x, feetCollider.bounds.min.y);
        Vector2 boxCastSize = new Vector2(feetCollider.bounds.size.x, moveStats.groundDetectionRayLength);

        groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, moveStats.groundDetectionRayLength, moveStats.groundLayer);
        if (groundHit.collider != null)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        #region DebugVisualization
        if (moveStats.debugShowIsGroundedBox)
        {
            Color rayColor;
            if (isGrounded)
            {
                rayColor = Color.green;
            }
            else
            {
                rayColor = Color.red;
            }

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * moveStats.groundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * moveStats.groundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - moveStats.groundDetectionRayLength), Vector2.right * boxCastSize.x, rayColor);
        }
        #endregion
    }

    private void BumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(feetCollider.bounds.center.x, bodyCollider.bounds.max.y);
        Vector2 boxCastSize = new Vector2(feetCollider.bounds.size.x * moveStats.headWidth, moveStats.headDetectionRayLength);

        headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, moveStats.headDetectionRayLength, moveStats.groundLayer);
        if (headHit.collider != null)
        {
            bumpedHead = true;
        }
        else
        {
            bumpedHead = false;
        }

        #region Debug Visualization
        if (moveStats.debugShowHeadBumpBox)
        {
            Color rayColor;
            if (bumpedHead)
            {
                rayColor = Color.green;
            }
            else
            {
                rayColor = Color.red;
            }

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * moveStats.headWidth, boxCastOrigin.y), Vector2.up * moveStats.headDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + (boxCastSize.x / 2) * moveStats.headWidth, boxCastOrigin.y), Vector2.up * moveStats.headDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * moveStats.headWidth, boxCastOrigin.y + moveStats.headDetectionRayLength), Vector2.right * boxCastSize.x * moveStats.headWidth, rayColor);
        }
        #endregion
    }

    private void IsTouchingWall()
    {
        float originEndPoint = 0f;
        if (isFacingRight)
        {
            originEndPoint = bodyCollider.bounds.max.x;
        }
        else
        {
            originEndPoint = bodyCollider.bounds.min.x;
        }

        float adjustedHeight = bodyCollider.bounds.size.y * moveStats.wallDetectionRayHeightMultiplier;

        Vector2 boxCastOrigin = new Vector2(originEndPoint, bodyCollider.bounds.center.y);
        Vector2 boxCastSize = new Vector2(moveStats.wallDetectionRayLength, adjustedHeight);

        wallHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, transform.right, moveStats.wallDetectionRayLength, moveStats.groundLayer);
        if (wallHit.collider != null)
        {
            lastWallHit = wallHit;
            isTouchingWall = true;
        }
        else
        {
            isTouchingWall = false;
        }

        #region Debug Visualization

        if (moveStats.debugShowWallHitbox)
        {
            Color rayColor;
            if (isTouchingWall)
            {
                rayColor = Color.green;
            }
            else
            {
                rayColor = Color.red;
            }

            Vector2 boxBottomLeft = new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - boxCastSize.y / 2);
            Vector2 boxBottomRight = new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y - boxCastSize.y / 2);
            Vector2 boxTopLeft = new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y + boxCastSize.y / 2);
            Vector2 boxTopRight = new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y + boxCastSize.y / 2);

            Debug.DrawLine(boxBottomLeft, boxBottomRight, rayColor);
            Debug.DrawLine(boxBottomRight, boxTopRight, rayColor);
            Debug.DrawLine(boxTopRight, boxTopLeft, rayColor);
            Debug.DrawLine(boxTopLeft, boxBottomLeft, rayColor);
        }

        #endregion
    }

    private void CollisionChecks()
    {
        IsGrounded();
        BumpedHead();
        IsTouchingWall();
    }

    #endregion

    #region Timers

    private void CountTimers()
    {
        // jump buffer
        jumpBufferTimer -= Time.deltaTime;

        // jump coyote time
        if (!isGrounded)
        {
            coyoteTimer -= Time.deltaTime;
        }
        else
        {
            coyoteTimer = moveStats.jumpCoyoteTime;
        }

        // wall jump buffer timer
        if (!ShouldApplyPostWallJumpBuffer())
        {
            wallJumpPostBufferTimer -= Time.deltaTime;
        }
    }

    #endregion

    #if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (moveStats.showWalkJumpArc)
        {
            DrawJumpArc(moveStats.maxWalkSpeed, Color.white);
        }

        if (moveStats.showRunJumpArc)
        {
            DrawJumpArc(moveStats.maxRunSpeed, Color.red);
        }
    }

    #endif
}