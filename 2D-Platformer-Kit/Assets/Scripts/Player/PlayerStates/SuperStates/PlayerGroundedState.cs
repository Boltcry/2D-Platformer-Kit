using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Not actually used in the State Machine but used as a parent for Idle & Move states
public class PlayerGroundedState : PlayerState
{

    public PlayerGroundedState(PlayerController aPlayer, PlayerStateMachine aStateMachine, PlayerMoveStatsSO aMoveStats, string aBoolName) : 
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

        if (!player.collisionChecker.isGrounded)
        {
            stateMachine.Fall(false);
        }

        if (ShouldJump())
        {
            stateMachine.ChangeState(stateMachine.jumpingState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
}