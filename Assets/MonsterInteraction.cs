using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterInteraction : MonoBehaviour
{
    [Header("Base")]
    [SerializeField] MonsterMovement movement;
    [Range(0f,1f)]
    [SerializeField] float damageAngle;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("PlayerAttackArea"))
        {
            Die();
        }
        else if(collision.transform.TryGetComponent<PlayerInteraction>(out var player))
        {
            // 특정 각도 높이에서 부딪힌 경우 (밟은 판정)
            var contact = collision.contacts[0];
            Vector3 nomal = contact.normal;
            if (nomal.y < -damageAngle)
            {
                Die();
            } else
            {
                movement?.SuccessAttack();
                player.TakeDamage(transform.position, 1);
            }
        }
    }

    void Die()
    {
        // 연기 이펙트 생성 및 현재 오브젝트 파괴
        Destroy(gameObject);
    }
}
