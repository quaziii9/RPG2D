using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    private EntityFX fx;

    [Header("Major stats")]
    public Stat strength; // �� : 1 point increase damage by 1% and crit.power by 1%
    public Stat agility; // ��ø : 1 point increase evasion(ȸ��) by 1% and crit.chance by 1%
    public Stat intelligence; // ���� : 1 point increase magic damage by 1 and magic resistance(����) by 3
    public Stat vitality; // Ȱ�� : 1 point increase health by 3 or 5 points

    [Header("Offensive stats")]
    public Stat damage;
    public Stat critChance;
    public Stat critPower;          // default value 150%

    [Header("Defencive stats")]
    public Stat maxHealth;
    public Stat armor;
    public Stat evasion; // ȸ��
    public Stat magicResistance; // ���� ���׷�

    [Header("Magic stats")]
    public Stat fireDamage;
    public Stat iceDamage;
    public Stat lightingDamage;

    public bool isIgnited; // ��ȭ : does damage over time
    public bool isChilled; // �ð� : reduce armor by 20%
    public bool isShocked; // ���� : reduce accuracy by 20% 

    [SerializeField] private float ailmentsDuration = 4;

    private float ignitedTimer;
    private float chillTimer;
    private float shockedTimer;


    private float igniteDamageCooldown = .3f;
    private float igniteDamageTimer;
    private int ignitedDamage;
    [SerializeField] private GameObject shockStrikePrefab;
    private int shockDamage;
    public int currentHealth;

    public System.Action onHealthChanged;

    public bool isDead {  get; private set; }  

    protected virtual void Start()
    {
        critPower.SetDefaultValue(150);
        currentHealth = GetMaxHealthValue();

        fx = GetComponent<EntityFX>();

    }

    protected virtual void Update()
    {
        ignitedTimer -= Time.deltaTime;
        chillTimer -= Time.deltaTime;
        shockedTimer -= Time.deltaTime;

        igniteDamageTimer -= Time.deltaTime;

        if (ignitedTimer < 0)    // isingeted�� ���ݹް� 4�ʵ���
            isIgnited = false;

        if (chillTimer < 0)
            isChilled = false;

        if (shockedTimer < 0)
            isShocked = false;

        if(isIgnited)
            ApplyIgniteDamage();
    }

    public virtual void IncreaseStatBy(int _modifer, float _duration, Stat _statToModify)
    {
        StartCoroutine(StatModCoroutine(_modifer, _duration, _statToModify));
    }

    private IEnumerator StatModCoroutine(int _modifer, float _duration, Stat _statToModify)
    {
        _statToModify.AddModifier(_modifer);

        yield return new WaitForSeconds(_duration);

        _statToModify.RemoveModifier(_modifer);
    }


    public virtual void DoDamage(CharacterStats _targetStats)
    {

        if (TargetCanAvoidAttack(_targetStats))
            return;

        int totalDamage = damage.GetValue() + strength.GetValue();
        
        if (CanCrit()) // ũ��Ƽ�� ���� ���� üũ
        { 
            totalDamage = CalculaterCriticalDamge(totalDamage); // ũ��Ƽ�� ������ ���
           // Debug.Log("total crit damage is " + totalDamage);
        }




        totalDamage = CheckTargetArmor(_targetStats, totalDamage);
        _targetStats.TakeDamage(totalDamage);

        // if inventory current weapon has fire effect
        // then DoMagicalDamage(_targetStats);
        DoMagicalDamage(_targetStats); // remove if you don't wnat to apply magic hit on primary attack
    }

    #region Magical damage and ailments

    public virtual void DoMagicalDamage(CharacterStats _targetStats)
    {
        int _fireDamage = fireDamage.GetValue();
        int _iceDamage = iceDamage.GetValue();
        int _lightingDamage = lightingDamage.GetValue();

        int totalMagicalDamage = _fireDamage + _iceDamage + _lightingDamage + intelligence.GetValue();

        totalMagicalDamage = CheckTargetResistance(_targetStats, totalMagicalDamage);
        _targetStats.TakeDamage(totalMagicalDamage);

        if (Mathf.Max(_fireDamage, _iceDamage, _lightingDamage) <= 0)
            return;

        AttemptyToApplyAilements(_targetStats, _fireDamage, _iceDamage, _lightingDamage);

    }

    private void AttemptyToApplyAilements(CharacterStats _targetStats, int _fireDamage, int _iceDamage, int _lightingDamage)
    {
        bool canApplyIgnite = _fireDamage > _iceDamage && _fireDamage > _lightingDamage;
        bool canApplyChill = _iceDamage > _fireDamage && _iceDamage > _lightingDamage;
        bool canApplyShock = _lightingDamage > _fireDamage && _lightingDamage > _iceDamage;

        while (!canApplyIgnite && !canApplyChill && !canApplyShock)
        {
            if (Random.value < .3f && _fireDamage > 0)
            {
                canApplyIgnite = true;
                _targetStats.ApplyAliments(canApplyIgnite, canApplyChill, canApplyShock);
               // Debug.Log("fire");
                return;
            }

            if (Random.value < .5f && _iceDamage > 0)
            {
                canApplyChill = true;
                _targetStats.ApplyAliments(canApplyIgnite, canApplyChill, canApplyShock);
               // Debug.Log("ice");
                return;
            }

            if (Random.value < .5f && _lightingDamage > 0)
            {
                canApplyShock = true;
                _targetStats.ApplyAliments(canApplyIgnite, canApplyChill, canApplyShock);
               // Debug.Log("light");
                return;
            }
        }

        if (canApplyIgnite)
            _targetStats.SetupIgniteDamage(Mathf.RoundToInt(_fireDamage * .2f));

        if (canApplyShock)
            _targetStats.SetupShockStrikeDamage(Mathf.RoundToInt(_lightingDamage * .1f));


        _targetStats.ApplyAliments(canApplyIgnite, canApplyChill, canApplyShock);
    }

    public void ApplyAliments(bool _ignite, bool _chill, bool _shock)
    {
        bool canApplyIgnite = !isIgnited && !isChilled && !isShocked;
        bool canApplyChill = !isIgnited && !isChilled && !isShocked;
        bool canApplyShock = !isIgnited && !isChilled;

        if (_ignite && canApplyIgnite)
        {
            isIgnited = _ignite;
            ignitedTimer = ailmentsDuration;

            fx.IgniteFxFor(ailmentsDuration);
        }

        if (_chill && canApplyChill)
        {
            isChilled = _chill;
            chillTimer = ailmentsDuration;

            float slowPercentage = .2f;

            GetComponent<Entity>().SlowEntityBy(slowPercentage, ailmentsDuration);
            fx.ChillFxFor(ailmentsDuration);
        }

        if (_shock && canApplyShock)
        {
            if(!isShocked)
            {
                ApplyShock(_shock);
            }
            else
            {
                if (GetComponent<Player>() != null)
                    return;
                HitNearestTargetWithShockStrike();
            }

        }        
    }

    public void ApplyShock(bool _shock)
    {
        if (isShocked)
            return;

        isShocked = _shock;
        shockedTimer = ailmentsDuration;

        fx.ShockFxFor(ailmentsDuration);
    }

    private void HitNearestTargetWithShockStrike()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, Mathf.Infinity);

        float closeDistance = Mathf.Infinity;

        Transform closestEnemy = null;

        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Enemy>() != null && Vector2.Distance(transform.position, hit.transform.position) > 1)
            {
                // �߰��� �������� �Ÿ��� ���
                float distanceToEnemy = Vector2.Distance(transform.position, hit.transform.position);

                // ���� �Ÿ��� ��������� ������
                if (distanceToEnemy < closeDistance)
                {
                    // ���� ������� �Ҵ�
                    closeDistance = distanceToEnemy;
                    closestEnemy = hit.transform;
                }
            }

            if (closestEnemy == null)
                closestEnemy = transform;
        }

        if (closestEnemy != null)
        {
            GameObject newShockStrike = Instantiate(shockStrikePrefab, transform.position, Quaternion.identity);

            newShockStrike.GetComponent<ShockStrike_Controller>().Setup(shockDamage, closestEnemy.GetComponent<CharacterStats>());
        }
    }

    private void ApplyIgniteDamage()
    {
        if (igniteDamageTimer < 0)  // �ҿ� Ÿ�� ������ 0.3f ���� burn ������
        {
            // Debug.Log("take burn damage" + ignitedDamage);

            DecreaseHealthBy(ignitedDamage);

            if (currentHealth < 0 && !isDead)
                Die();

            igniteDamageTimer = igniteDamageCooldown;
        }
    }

    public void SetupIgniteDamage(int _damage) => ignitedDamage = _damage;
    public void SetupShockStrikeDamage(int _damage) => shockDamage = _damage;

    #endregion

    public virtual void TakeDamage(int _damage)
    {
        DecreaseHealthBy(_damage);

        GetComponent<Entity>().DamageImpact();
        fx.StartCoroutine("FlashFX");

        // Debug.Log(_damage);

        if (currentHealth < 0 && !isDead)
            Die();

       
    }

    public virtual void IncreaseHealthBy(int _amount)
    {
        currentHealth += _amount;

        if (currentHealth > GetMaxHealthValue())
            currentHealth = GetMaxHealthValue();


        if(onHealthChanged != null)
            onHealthChanged();
    }

    protected virtual void DecreaseHealthBy(int _damage)
    {
        currentHealth -= _damage;

        if(onHealthChanged != null )
            onHealthChanged();
    }




    protected virtual void Die()
    {
        isDead = true;
    }

    #region Stat calculation
    private int CheckTargetArmor(CharacterStats _targetStats, int totalDamage) // ����
    {
        if(_targetStats.isChilled)  // ��밡 �ð� ���¸� 
            totalDamage -= Mathf.RoundToInt(_targetStats.armor.GetValue() * .8f);   // ������ = ������ 80 % �� ����,  
        else
            totalDamage -= _targetStats.armor.GetValue();

        totalDamage = Mathf.Clamp(totalDamage, 0, int.MaxValue);
        return totalDamage;
    }   

    private int CheckTargetResistance(CharacterStats _targetStats, int totalMagicalDamage)
    {
        totalMagicalDamage -= _targetStats.magicResistance.GetValue() + (_targetStats.intelligence.GetValue() * 3);
        totalMagicalDamage = Mathf.Clamp(totalMagicalDamage, 0, int.MaxValue);
        return totalMagicalDamage;
    }

    private bool TargetCanAvoidAttack(CharacterStats _targetStats)  // ȸ��
    {
        int totalEvasion = _targetStats.evasion.GetValue() + _targetStats.agility.GetValue();

        if (isShocked)  // ���� ���� ���� ���¸�
            totalEvasion += 20; // ���� ȸ�� �ɷ� ��� 20

        if (Random.Range(0, 100) < totalEvasion)
        {
            return true;
        }
        return false;
    }

    private bool CanCrit()
    {
        int totalCriticalChance = critChance.GetValue() + agility.GetValue();

        if(Random.Range(0, 100) <= totalCriticalChance)
        {
            return true;
        }
        return false;
    }

    private int CalculaterCriticalDamge(int _damage)
    {
        float totalCritPower = (critPower.GetValue() + strength.GetValue()) * .01f; // ũ��Ƽ�� %
        //Debug.Log("total crit power % " + totalCritPower);

        float critDamage = _damage * totalCritPower;        // ���� ���� * ũ��Ƽ�� % 
        //Debug.Log("crit damage before round up " + critDamage);

        return Mathf.RoundToInt(critDamage); // �ݿø�
    }

    public int GetMaxHealthValue()
    {
        return maxHealth.GetValue() + vitality.GetValue() * 5;
    }
    #endregion

}
