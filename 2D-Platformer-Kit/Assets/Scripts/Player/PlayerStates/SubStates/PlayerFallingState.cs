using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Active when player is descending
public class PlayerFallingState : PlayerInAirState
{
    bool isFastFalling = false;

    public PlayerFallingState(PlayerController aPlayer, PlayerStateMachine aStateMachine, PlayerMoveStatsSO aMoveStats, string aBoolName) : 
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
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        player.SetVerticalVelocity(player.verticalVelocity + moveStats.gravity * Time.fixedDeltaTime);
    }

    // set by playerStateMachine on Fall()
    public void SetIsFastFalling(bool aIsFastFalling)
    {
        isFastFalling = aIsFastFalling;
    }
}
