using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInAirState : PlayerState
{
    public PlayerInAirState(PlayerController aPlayer, PlayerStateMachine aStateMachine, PlayerMoveStatsSO aMoveStats, string aBoolName) : 
    base(aPlayer, aStateMachine, aMoveStats, aBoolName)
    {
        
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (player.collisionChecker.isGrounded)
        {
            stateMachine.ChangeState(stateMachine.idleState);
        }
        if (player.collisionChecker.isTouchingWall)
        {
            // stateMachine.ChangeState(stateMachine.wallSlideState);
        }

        if (ShouldJump())
        {
            stateMachine.ChangeState(stateMachine.jumpingState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        player.Move(moveStats.airAcceleration, moveStats.airDeceleration, InputManager.Instance.moveDirection);
    }
}