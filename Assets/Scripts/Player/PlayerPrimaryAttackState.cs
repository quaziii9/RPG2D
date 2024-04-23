using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerPrimaryAttackState : PlayerState
{

    public int comboCounter { get; private set;} 

    private float lastTimeAttacked;
    private float comboWindow = 2; // 콤보 리셋 시간


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
        // player.anim.speed= 3; 공격 속도 조절

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
        // player.anim.speed= 1; 공격이 끝나면 공격 속도 초기화

        comboCounter++;
        lastTimeAttacked = Time.time; // 프로젝트 시작후 경과한 시간 
    }

    public override void Update()
    {
        base.Update();

        if (stateTimer < 0) // enter에서 statetimer을 0.1f 로 설정 , statetimer가 0보다 작으면이니 잠깐동안은 플레이어의 속도가 지속, 좀더 자연스러움
            player.SetZeroVelocity();

        if (triggerCalled)
            stateMachine.ChangeState(player.idleState);
    }
}
