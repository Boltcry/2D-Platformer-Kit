using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : PlayerGroundedState
{
    private float idleThreshold = 0.1f;     // if horizontal velocity is lower than this value change to idle state
    // TODO: speed float for animator

    public PlayerMoveState(PlayerController aPlayer, PlayerStateMachine aStateMachine, PlayerMoveStatsSO aMoveStats, string aBoolName) : 
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

        if (Mathf.Abs(player.currentVelocity.x) < idleThreshold)
        {
            stateMachine.ChangeState(stateMachine.idleState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        player.Move(moveStats.groundAcceleration, moveStats.groundDeceleration, InputManager.Instance.moveDirection);
        // anim.SetFloat() for variable move speed
    }
}