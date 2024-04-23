using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : PlayerGroundedState
{
    PlayerMoveState moveState;
    public PlayerMoveState(Player _player, PlayerStateMachine _stateMachine, string _animBoolNae) : base(_player, _stateMachine, _animBoolNae)
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

    public override void Update()
    {
        base.Update();

        player.SetVelocity(xInput * player.moveSpeed, rb.velocity.y);

        if (xInput ==0 || player.IsWallDetected())
            player.stateMachine.ChangeState(player.idleState);
    }
}
