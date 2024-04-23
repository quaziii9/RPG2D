using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerPrimaryAttackState : PlayerState
{

    public int comboCounter { get; private set;} 

    private float lastTimeAttacked;
    private float comboWindow = 2; // �޺� ���� �ð�


    public PlayerPrimaryAttackState(Player _player, PlayerStateMachine _stateMachine, string _animBoolNae) : base(_player, _stateMachine, _animBoolNae)
    {
    }

    public override void Enter()
    {
        base.Enter();
        xInput = 0;

        if (comboCounter > 2 || Time.time >= lastTimeAttacked + comboWindow)
            comboCounter = 0;

        player.anim.SetInteger("ComboCounter", comboCounter);
        // player.anim.speed= 3; ���� �ӵ� ����

        float attackDir = player.facingDir;

        if (xInput != 0)
            attackDir = xInput;

        player.SetVelocity(player.attackMovement[comboCounter].x * attackDir, player.attackMovement[comboCounter].y);

        stateTimer = .1f;   
    }

    public override void Exit()
    {
        base.Exit();

        player.StartCoroutine("BusyFor", .15f);
        // player.anim.speed= 1; ������ ������ ���� �ӵ� �ʱ�ȭ

        comboCounter++;
        lastTimeAttacked = Time.time; // ������Ʈ ������ ����� �ð� 
    }

    public override void Update()
    {
        base.Update();

        if (stateTimer < 0) // enter���� statetimer�� 0.1f �� ���� , statetimer�� 0���� �������̴� ��񵿾��� �÷��̾��� �ӵ��� ����, ���� �ڿ�������
            player.SetZeroVelocity();

        if (triggerCalled)
            stateMachine.ChangeState(player.idleState);
    }
}
