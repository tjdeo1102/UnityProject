using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterMovement : MonoBehaviour
{
    [Header("Base")]
    [SerializeField] GameObject target;
    [SerializeField] float walkMaxSpeed;
    [SerializeField] float runMaxSpeed;
    [SerializeField] float wanderRadius;


    [Header("Tracking")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] float repathDelay;
    [SerializeField] float wanderDelay;
    [SerializeField] float retargetDelay;
    [SerializeField] float detectReleaseTime;
    [SerializeField] float detectDistance;

    private Coroutine trackingCoroutine;
    private Coroutine wanderCoroutine;
    private Coroutine runCoroutine;
    private bool lastFindStatus = true;
    private Vector3 originPosition;


    void Start()
    {
        originPosition = transform.position;
        trackingCoroutine = StartCoroutine(TrackingRoutine());
    }
    void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * detectDistance);
    }

    IEnumerator WanderRoutine()
    {
        WaitForSeconds wanderDelay = new WaitForSeconds(this.wanderDelay);

        while (agent != null)
        {
            // ���� ��ġ �������� ��ȸ
            var random = wanderRadius * Random.insideUnitCircle;
            // ������ �ӵ��� �ȱ�
            agent.speed = walkMaxSpeed;
            agent.destination = new Vector3(random.x, 0, random.y) + originPosition;

            yield return wanderDelay;
        }
    }
    IEnumerator RunRoutine()
    {
        WaitForSeconds repathDelay = new WaitForSeconds(this.repathDelay);

        while(agent != null && target != null)
        { 
            // �ִ� �ӷ±��� �������� �߰�
            agent.speed = runMaxSpeed;
            agent.destination = target.transform.position;
            
            yield return repathDelay;
        }
    }

    public void SuccessAttack()
    {
        if (trackingCoroutine != null) StopCoroutine(trackingCoroutine);
        // ��ȸ ����
        ChangeState(false);
        trackingCoroutine = StartCoroutine(TrackingRoutine());
    }

    IEnumerator TrackingRoutine()
    {
        WaitForSeconds targetDelay = new WaitForSeconds(retargetDelay);
        WaitForSeconds releaseDelay = new WaitForSeconds(detectReleaseTime);

        // ���� ���� ����, ��Ÿ���� �ϴ� ������ �ð� �ʿ�
        yield return targetDelay;

        while (agent != null)
        {
            Ray ray = new Ray(transform.position, transform.forward);

            if (Physics.Raycast(ray, out var hit,detectDistance))
            {
                if (hit.transform.CompareTag("Player"))
                {
                    ChangeState(true);
                    yield return releaseDelay;
                }
            }
            else
            {
                ChangeState(false);
            }
            yield return null;
        }
    }

    void ChangeState(bool isFind)
    {
        if (isFind != lastFindStatus)
        {
            if (isFind == false)
            {
                if (runCoroutine != null) StopCoroutine(runCoroutine);
                wanderCoroutine = StartCoroutine(WanderRoutine());
            }
            else
            {
                if (wanderCoroutine != null) StopCoroutine(wanderCoroutine);
                runCoroutine = StartCoroutine(RunRoutine());
            }
            lastFindStatus = isFind;
        }
    }
}
