using UnityEngine;
using DG.Tweening;

public enum DiePattern
{
    Press, Punch, Throw
}

public class MonsterInteraction : MonoBehaviour
{

    [Header("Base")]
    [SerializeField] MonsterMovement movement;
    [Range(0f,1f)]
    [SerializeField] float damageAngle;

    bool isDie;
    Rigidbody rb;
	void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}
	private void OnCollisionEnter(Collision collision)
    {
        if (isDie) return;

        if (collision.collider.CompareTag("PlayerAttackArea"))
        {
            Die(DiePattern.Punch, collision.contacts[0].point);
        }
        else if(collision.transform.TryGetComponent<PlayerCombat>(out var player))
        {
            var contact = collision.contacts[0];
            Vector3 nomal = contact.normal;
            if (nomal.y < -damageAngle)
            {
                Die(DiePattern.Press);
            } else
            {
                movement?.SuccessAttack();
                player.OnDamage(transform.position, 1);
            }
        }
    }

    void Die(DiePattern pattern, Vector3 hitPos = default)
    {
        if (isDie) return;
        Debug.Log("Die");
        isDie = true;
        var seq = DOTween.Sequence();
        switch (pattern)
        {
            case DiePattern.Press:
                GetComponentInChildren<Collider>().enabled = false;
                seq.Append(transform.GetChild(0).DOScaleY(0.01f, 0.2f))
                    .AppendInterval(2f)
                    .OnComplete(() => Destroy(gameObject));
                break;
            case DiePattern.Punch:
                rb.isKinematic = false;
                var pushDir = ((transform.position - hitPos).normalized + Vector3.up * 0.5f).normalized;
                rb.AddForce(pushDir * 10f, ForceMode.Impulse);
                seq.AppendInterval(2f)
                    .OnComplete(() => Destroy(gameObject));
                break;
            default:
                break;
        }
        seq.Play();
        movement.enabled = false;
        enabled = false;
    }
}
