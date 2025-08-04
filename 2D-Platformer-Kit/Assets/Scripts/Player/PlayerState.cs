using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Parent class for individual states in the Player State Machine
public class PlayerState
{
    protected PlayerController player;
    protected PlayerStateMachine stateMachine;
    protected PlayerMoveStatsSO moveStats;

    protected float startTime;
    private string animBoolName;

    public PlayerState(PlayerController aPlayer, PlayerStateMachine aStateMachine, PlayerMoveStatsSO aMoveStats, string aBoolName)
    {
        player = aPlayer;
        stateMachine = aStateMachine;
        moveStats = aMoveStats;
        animBoolName = aBoolName;
    }

    // called by StateMachine in ChangeState
    public virtual void Enter()
    {
        player.anim?.SetBool(animBoolName, true);
        startTime = Time.time;

        Debug.Log("entering state" +animBoolName);
    }

    // called by StateMachine in ChangeState before next state is Entered
    public virtual void Exit()
    {
        player.anim?.SetBool(animBoolName, false);
    }

    // called every Update from StateMachine via PlayerController. Checks if the state should transition to a different one
    public virtual void LogicUpdate()
    {
        //
    }

    // called every FixedUpdate from StateMachine via PlayerController. Handles actual physics calculation and sets the player's next vertical / horizontal velocity
    public virtual void PhysicsUpdate()
    {
        //
    }

    // Utility method for all states to use
    // Should maybe move to LogicUpdate in PlayerState?
    protected bool ShouldJump()
    {
        if (InputManager.Instance.jumpWasPressed)
        {
            // air jump, regular jump
            if (player.numJumpsUsed < moveStats.numJumpsAllowed)
            {
                if (player.jumpBufferTimer > 0f)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
