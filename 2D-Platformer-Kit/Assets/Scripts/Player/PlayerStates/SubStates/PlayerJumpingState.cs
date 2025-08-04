using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// handles jumps while ascending, then changes state to PlayerFallingState for descending gravity
// Also handles wall jumps (planned)
public class PlayerJumpingState : PlayerInAirState
{
    private bool isWallJumping = false;

    // apex vars
    private float apexPoint;
    private float timePastApexThreshold;
    private bool isPastApexThreshold;
    
    public PlayerJumpingState(PlayerController aPlayer, PlayerStateMachine aStateMachine, PlayerMoveStatsSO aMoveStats, string aBoolName) : 
    base(aPlayer, aStateMachine, aMoveStats, aBoolName)
    {
        
    }

    public override void Enter()
    {
        base.Enter();

        // reset jump timer
        player.IncrementNumJumps(1);
        player.SetVerticalVelocity(moveStats.initialJumpVelocity);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (player.collisionChecker.bumpedHead)
        {
            stateMachine.Fall(true);
        }

        if (InputManager.Instance.jumpWasReleased)
        {
            if (isPastApexThreshold)
            {
                isPastApexThreshold = false;
                stateMachine.Fall(true);
                // fastFallTime = moveStats.timeForUpwardsCancel; // copied from PlayerMovement
                // player.SetVerticalVelocity(0f);
            }
            else
            {
                stateMachine.Fall(true);
                // fastFallReleaseSpeed = player.verticalVelocity; // copied from PlayerMovement
            }
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // APEX CONTROLS
        apexPoint = Mathf.InverseLerp(moveStats.initialJumpVelocity, 0f, player.verticalVelocity);

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
                    player.SetVerticalVelocity(0f);
                }
                else
                {
                    player.SetVerticalVelocity(-0.01f);
                }
            }
        }

        // GRAVITY ON ASCENDING BUT NOT PAST APEX THRESHOLD
        else
        {
            player.SetVerticalVelocity(player.verticalVelocity + moveStats.gravity * Time.fixedDeltaTime);
            if (isPastApexThreshold)
            {
                isPastApexThreshold = false;
            }
        }
    }

    // set by playerStateMachine on Jump()
    public void SetIsWallJumping(bool aIsWallJumping)
    {
        isWallJumping = aIsWallJumping;
    }
}
