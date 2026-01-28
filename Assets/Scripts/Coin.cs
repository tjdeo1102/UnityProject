using System.Collections;
using System.Collections.Generic;
using Ami.BroAudio;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] int coinReward;
    [SerializeField] float coinRotateSpeed;
    [SerializeField] protected Star starPrefab;
    [SerializeField] SoundID getCoinSound;

    GameDataModel model;
	void Start()
	{
		model = GameDataModel.Instance;
	}

	// Update is called once per frame
	void Update()
    {
        transform.Rotate(0,Time.timeScale * coinRotateSpeed, 0);
    }

    protected virtual void GainCoin()
    {
        model.YellowCoin += coinReward;

        BroAudio.Play(getCoinSound);
        StarTrigger();
        Destroy(gameObject);
    }

    protected virtual void StarTrigger()
    {
        if (model.YellowCoin >= 100 && model.IsGetHiddenStar == false)
        {
            model.IsGetHiddenStar = true;
            Instantiate(starPrefab,transform.position + Vector3.up * 2,Quaternion.identity,null).isGameOverStar = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            GainCoin();
        }
    }
}
