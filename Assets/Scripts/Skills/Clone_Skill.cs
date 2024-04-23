using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clone_Skill : Skill
{
    public Queue<GameObject> clone_Queue = new Queue<GameObject>();
    [Header("Clone info")]
    [SerializeField] private GameObject clonePrefab;
    [SerializeField] private float cloneDuration;
    [Space]
    [SerializeField] private bool canAttack;

    [SerializeField] private bool createCloneOnDashStart;
    [SerializeField] private bool createCloneOnDashOver;
    [SerializeField] private bool canCreateCloneOnCounterAttack;

    [Header("Clone can duplicate")]
    [SerializeField] private bool canDuplicateClone;
    [SerializeField] private float chanceToDuplicate;

    [Header("Crystal instead of clone")]
    public bool crystalInsteadOfClone;

    public void CreateClone(Transform _clonePosition, Vector3 _offset)
    {

        if(crystalInsteadOfClone) 
        {
            SkillManager.instance.crystal.CreateCrystal();
            return;
        }

        GameObject newClone = Instantiate(clonePrefab);

        newClone.GetComponent<Clone_Skill_Controller>().SetUpClone(_clonePosition, cloneDuration, canAttack, _offset, FindClosestEnemy(newClone.transform), canDuplicateClone, chanceToDuplicate, player);

        #region clone only one
        /*
        if (clone_Queue.Count > 0)
        {
            // �̹� ������ ������Ʈ�� �����ͼ� Ȱ��ȭ
            newClone = clone_Queue.Dequeue();
            clone_Queue.Enqueue(newClone);
            newClone.SetActive(true);
            newClone.GetComponent<Clone_Skill_Controller>().SetUpClone(_clonePosition, cloneDuration, canAttack);
        }
        else
        {
            // ���ο� ������Ʈ�� ����
            newClone = Instantiate(clonePrefab);
            clone_Queue.Enqueue(newClone);
            newClone.GetComponent<Clone_Skill_Controller>().SetUpClone(_clonePosition, cloneDuration, canAttack);
        }
        */
        #endregion
        // newClone.GetComponent<Clone_Skill_Controller>().SetUpClone(_clonePosition, cloneDuration, canAttack);
    }

    public void CreateCloneOnDashStart()
    {
        if (createCloneOnDashStart)
            CreateClone(player.transform, Vector3.zero);
    }

    public void CreateCloneOnDashOver()
    {
        if (createCloneOnDashOver)
            CreateClone(player.transform, Vector3.zero);
    }

    public void CreateCloneOnCounterAttack(Transform _enemyTransform)
    {
        if (canCreateCloneOnCounterAttack)
            StartCoroutine(CreateCloneWithDelay(_enemyTransform, new Vector3(1.2f * player.facingDir, 0)));
    }

    private IEnumerator CreateCloneWithDelay(Transform _transform, Vector3 _offset)
    {
        yield return new WaitForSeconds(.4f);
            CreateClone(_transform, _offset);
    }
}
