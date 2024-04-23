
public class PlayerIdleState : PlayerGroundedState
{
    public PlayerIdleState(Player _player, PlayerStateMachine _stateMachine, string _animBoolNae) : base(_player, _stateMachine, _animBoolNae)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.SetZeroVelocity();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (xInput == player.facingDir && player.IsWallDetected())
            return;

        // ������ ������ 0.15f�� ������ �����ϼ� ����
        if (xInput != 0 && !player.isBusy)
            player.stateMachine.ChangeState(player.moveState);
    }
}
