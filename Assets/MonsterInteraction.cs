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
            // Ư�� ���� ���̿��� �ε��� ��� (���� ����)
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
        // ���� ����Ʈ ���� �� ���� ������Ʈ �ı�
        Destroy(gameObject);
    }
}
