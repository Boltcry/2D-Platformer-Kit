using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// WIP implementation of a Player State Machine, handles general variables that should be accessible by all player states
public class PlayerController : Entity
{
    public PlayerStateMachine stateMachine {get; private set;}

    // REFERENCES
    public Animator anim {get; private set;}
    public Rigidbody2D rb {get; private set;}
    public CollisionChecker collisionChecker {get; private set;}

    public Vector2 currentVelocity {get; private set;}

    [SerializeField]
    public PlayerMoveStatsSO moveStats;

    // MOVEMENT
    public float horizontalVelocity {get; private set;}   // updated by playerStates through SetHorizontalVelocity() & SetVerticalVelocity()
    public float verticalVelocity {get; private set;}

    // JUMP
    public int numJumpsUsed {get; private set;}
    // jump buffer
    public float jumpBufferTimer {get; private set;}
    public bool jumpReleasedDuringBuffer {get; private set;}
    
    private void Awake()
    {
        stateMachine = new PlayerStateMachine();
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        collisionChecker = GetComponent<CollisionChecker>();
        
        stateMachine.Initialize(this);
    }

    private void Update()
    {
        currentVelocity = rb.velocity;
        stateMachine.currentState?.LogicUpdate();
    }

    private void FixedUpdate()
    {
        TurnCheck(InputManager.Instance.moveDirection);
        stateMachine.currentState?.PhysicsUpdate();

        ApplyVelocity();
    }

    // called at the end of FixedUpdate()
    private void ApplyVelocity()
    {
        // CLAMP FALL SPEED
        verticalVelocity = Mathf.Clamp(verticalVelocity, -moveStats.maxFallSpeed, 50f);

        rb.velocity = new Vector2(horizontalVelocity, verticalVelocity);
        currentVelocity = rb.velocity;
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

    // called by PlayerStates
    public void SetHorizontalVelocity(float aVelocity)
    {
        horizontalVelocity = aVelocity;
    }

    public void SetVerticalVelocity(float aVelocity)
    {
        verticalVelocity = aVelocity;
    }

    public void IncrementNumJumps(int aNumJumps)
    {
        numJumpsUsed += aNumJumps;
    }

    // UTILITY METHODS

    //  states need to call Move every FixedUpdate, just with different acceleration / deceleration values
    public void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (Mathf.Abs(moveInput.x) != 0f)
        {
            float targetVelocity = 0f;
            if (InputManager.Instance.runIsHeld)
            {
                targetVelocity = moveInput.x * moveStats.maxRunSpeed;
            }
            else
            {
                targetVelocity = moveInput.x * moveStats.maxWalkSpeed;
            }

            SetHorizontalVelocity(Mathf.Lerp(horizontalVelocity, targetVelocity, acceleration * Time.fixedDeltaTime));
        }

        else if (Mathf.Abs(moveInput.x) == 0f)
        {
            SetHorizontalVelocity(Mathf.Lerp(horizontalVelocity, 0f, deceleration * Time.fixedDeltaTime));
        }
    }
}
