using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine
{
    public PlayerController player {get; private set;}
    public PlayerState currentState {get; private set;}

    // STATE REFERENCES
    public PlayerIdleState idleState {get; private set;}
    public PlayerMoveState moveState {get; private set;}
    public PlayerFallingState fallinState {get; private set;}
    public PlayerJumpingState jumpingState {get; private set;}

    public void Initialize(PlayerController aPlayer)
    {
        player = aPlayer;

        idleState = new PlayerIdleState(player, this, player.moveStats, "idle");
        moveState = new PlayerMoveState(player, this, player.moveStats, "move");
        fallingState = new PlayerFallingState(player, this, player.moveStats, "falling");
        jumpingState = new PlayerJumpingState(player, this, player.moveStats, "jumping");
        
        ChangeState(idleState);
    }

    public void ChangeState(PlayerState aNewState)
    {
        if (currentState != null)
        {
            currentState.Exit();
        }
        currentState = aNewState;
        currentState.Enter();
    }

    public void Jump(bool isWallJumping)
    {
        ChangeState(jumpingState);
        jumpingState.SetIsWallJumping(isWallJumping);
    }

    public void Fall(bool isFastFalling)
    {
        ChangeState(fallingState);
        fallingState.SetIsFastFalling(isFastFalling);
    }
}