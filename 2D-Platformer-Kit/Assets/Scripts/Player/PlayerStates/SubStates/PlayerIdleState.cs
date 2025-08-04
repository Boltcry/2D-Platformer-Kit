using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Active when the player is grounded and not moving
public class PlayerIdleState : PlayerGroundedState
{
    public PlayerIdleState(PlayerController aPlayer, PlayerStateMachine aStateMachine, PlayerMoveStatsSO aMoveStats, string aBoolName) : 
    base(aPlayer, aStateMachine, aMoveStats, aBoolName)
    {
        
    }

    public override void Enter()
    {
        base.Enter();

        player.SetHorizontalVelocity(0f);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (InputManager.Instance.moveDirection.x != 0f)
        {
            stateMachine.ChangeState(stateMachine.moveState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
}
