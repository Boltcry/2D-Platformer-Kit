using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public virtual void Enter()
    {
        DoChecks();
        player.anim?.SetBool(animBoolName, true);
        startTime = Time.time;

        Debug.Log("entering state" +animBoolName);
    }

    public virtual void Exit()
    {
        player.anim?.SetBool(animBoolName, false);
    }

    public virtual void LogicUpdate()
    {
        //
    }

    public virtual void PhysicsUpdate()
    {
        DoChecks();
    }

    public virtual void DoChecks()
    {
        //
    }

    protected bool ShouldJump()
    {
        if (InputManager.Instance.jumpWasPressed)
        {
            if (player.numJumpsUsed < moveStats.numJumpsAllowed)
            {
                return true;
            }
        }
        return false;
    }
}
