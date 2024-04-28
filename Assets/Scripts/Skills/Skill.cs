using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill : MonoBehaviour
{
    [SerializeField] protected float cooldown;
    protected float cooldownTimer;

    protected Player player;

    protected virtual void Start()
    {
        player = PlayerManager.instance.player;
    }

    protected virtual void Update()
    {
        cooldownTimer -= Time.deltaTime;
    }

    public virtual bool CanUseSkill()
    {
        if(cooldownTimer < 0)
        {
            UseSkill();
            cooldownTimer = cooldown;
            return true;
        }

        Debug.Log("Skill is on cooldown");
        return false;
    }

    public virtual void UseSkill()
    {
        // do some skill spesific things
    }

    protected virtual Transform FindClosestEnemy(Transform _checkTransform)
    {
       
        // 복제 인간 주변 25거리의 적을 모음
        Collider2D[] colliders = Physics2D.OverlapCircleAll(_checkTransform.position, Mathf.Infinity);

        float closeDistance = Mathf.Infinity;

        Transform closestEnemy = null;

        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Enemy>() != null)
            {
                // 발견한 적군까지 거리를 계산
                float distanceToEnemy = Vector2.Distance(_checkTransform.position, hit.transform.position);

                //  적군의 거리      < 무한대 
                if (distanceToEnemy < closeDistance)
                {
                    // 가장 가까운적 할당
                    closeDistance = distanceToEnemy;
                    closestEnemy = hit.transform;
                }
            }
        }

        return closestEnemy;
    }
}
