using UnityEngine;
using DG.Tweening;
using Ami.BroAudio;
using System.Linq;

public enum DiePattern
{
    Press, Punch, Boom
}

public class MonsterInteraction : MonoBehaviour, IDamageable
{

    [Header("Base")]
    [SerializeField] MonsterMovement movement;
    [Range(0f,1f)]
    [SerializeField] float damageAngle;
    [SerializeField] DiePattern chainAttackDiePattern;

    public bool canStomped = true;
    public bool canThrow = false;
    public bool hasTouchDamage = true;
    public bool isGrabbed;
    public bool isThrowing;
    public bool isDie;
    public Vector3 grabbedLocalPosition;
    Rigidbody rb;
    Collider col;
	void Awake()
	{
		rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
	}
	private void OnCollisionEnter(Collision collision)
    {
        if (isDie) return;

        var player = collision.collider.GetComponentInParent<PlayerCombat>();
        bool isAttackArea = collision.collider.CompareTag("PlayerAttackArea");
        bool isPlayer = collision.collider.CompareTag("Player");

        // Attack Branch
        if (isAttackArea && player != null)
        {
            HandleAttacked(player);
            return;
        }

        // Throwing Object Contact
        if (!isPlayer && isThrowing)
        {
            Die(DiePattern.Boom);
            return;
        }

        // Player Contact
        if (isPlayer && player != null)
        {
            HandlePlayerContact(player, collision);
        }
    }

    private void HandleAttacked(PlayerCombat player)
    {
        if (canThrow)
        {
            OnThrow(player, (player.transform.forward + Vector3.up * 0.5f).normalized, 10f);
            return;
        }

        if (player.grabbedMonster != null) return;

        Die(DiePattern.Punch, player.transform.forward);
    }

    private void HandlePlayerContact(PlayerCombat player, Collision collision)
    {
        Vector3 normal = collision.contacts[0].normal;
        var contactPoint = collision.contacts[0].point;
        if (canStomped && normal.y < -damageAngle && transform.position.y < contactPoint.y)
        {
            Die(DiePattern.Press);
            return;
        }

        if (hasTouchDamage)
        {
            movement?.ChangeState(State.Wander);
            player.OnDamage(transform.position, 1);
        }
    }

    public void OnThrow(PlayerCombat player,Vector3 throwDirection, float throwPower)
    {
        if (canThrow == false) return;
        if (isGrabbed == false)
        {
            movement.agent.enabled = false;
            col.enabled = false;
            rb.isKinematic = true;
            transform.SetParent(player.transform);
            transform.position = player.grabPoint.position;
            grabbedLocalPosition = transform.localPosition;
            player.grabbedMonster = this;
            isGrabbed = true;
            if (movement.activationDic[movement.currentState] is BombChaseState bombChase)
            {
                bombChase.ResetBombTimer();
            }
        }
        else
        {
            transform.SetParent(null);
            col.enabled = true;
            movement.agent.enabled = false;
            rb.isKinematic = false;
            isThrowing = true;
            rb.AddForce(throwDirection.normalized * throwPower, ForceMode.Impulse);
            player.grabbedMonster = null;
        } 
    }

    public void Die(DiePattern pattern, Vector3 attackDirection = default)
    {
        if (isDie) return;
        isDie = true;
        movement.ChangeState(State.None);
        switch (pattern)
        {
            case DiePattern.Press:
                GetComponentInChildren<Collider>().enabled = false;
                var seq = DOTween.Sequence();
                seq.Append(transform.GetChild(0).DOScaleY(0.02f, 0.2f))
                    .AppendInterval(2f)
                    .OnComplete(() => 
                    {
                        BroAudio.Play(movement.dieSound);
                        Destroy(gameObject);
                    });
                    seq.Play();
                break;
            case DiePattern.Punch:
                col.material.bounciness = 0.5f;
                rb.isKinematic = false;
                rb.linearVelocity = Vector3.zero; 
                rb.AddForce((attackDirection + Vector3.up * 0.5f).normalized * 10f, ForceMode.VelocityChange);
                seq = DOTween.Sequence();
                seq.AppendInterval(2f)
                    .OnComplete(() => 
                    {
                        BroAudio.Play(movement.dieSound);
                        Destroy(gameObject);
                    });
                seq.Play();
                break;
            case DiePattern.Boom:
                BroAudio.Play(movement.dieSound);
                Physics.OverlapSphere(transform.position, 3f).ToList().ForEach(col =>
                {
                    var damageable = col.GetComponentInParent<IDamageable>();
                    if (damageable != null)
                    {
                        damageable.OnDamage(transform.position, 1);
                    }
                });
                Destroy(gameObject);
                break;
            default:
                break;
        }
        movement.enabled = false;
        enabled = false;
    }

	void OnDestroy()
	{
		GameManager.Instance.GenerateYellowCoin(transform.position + Vector3.up * 2f);
        Instantiate(movement.deathParticle,transform.position,Quaternion.identity,null); 
	}

	public void OnDamage(Vector3 otherPosition, int damage)
	{
        Die(chainAttackDiePattern, (otherPosition - transform.position).normalized);
	}
}