using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Item : MonoBehaviour
{
	public enum ItemType
	{
		YellowCoin, RedCoin, BlueCoin, Star, HiddenStar
	}

	public ItemType type;



	[Header("HiddenStar")]
	[SerializeField] GameObject hiddenstarPrefab;
	[SerializeField] float respawnHeight;

	[Header("RedCoinStar")]
    [SerializeField] GameObject starPrefab;
    [SerializeField] Vector3 respawnPosition;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            ItemTrigger(type);
        }
    }

	void ItemTrigger(ItemType curType)
	{
		switch (curType)
		{
			case ItemType.YellowCoin:
                YellowCoinTrigger();
                break;
			case ItemType.RedCoin:
                RedCoinTrigger();
                break;
			case ItemType.BlueCoin:
                BlueCoinTrigger();
                break;
			case ItemType.Star:
				StarTrigger();
				break;
            case ItemType.HiddenStar:
				HiddenStarTrigger();
                break;
            default:
				break;
		}
	}

	void YellowCoinTrigger()
	{
		GameDataModel.Instance.YellowCoin++;
        // 추후, 이펙터 추가 (사운드 및 파티클)
        CoinChecker();

        Destroy(gameObject);
	}

	void RedCoinTrigger()
	{
        GameDataModel.Instance.RedCoin++;
        // 레드코인은 일반코인 * 2와 같음
        GameDataModel.Instance.YellowCoin+=2;
        CoinChecker();


        // 추후, 이펙터 추가 (사운드 및 파티클)

        Destroy(gameObject);
    }

    void BlueCoinTrigger()
    {
        // 블루코인은 일반 코인 * 5 와 같음
        GameDataModel.Instance.YellowCoin += 5;
        CoinChecker();

        // 추후, 이펙터 추가 (사운드 및 파티클)
        Destroy(gameObject);
    }

    void CoinChecker()
    {
        // 레드코인 체크
        if (GameDataModel.Instance.RedCoin == 8 && starPrefab != null)
        {
            Instantiate(starPrefab, respawnPosition, Quaternion.identity);
            // 스타가 생성되었음을 알려줄 장치 필요
        }

        // 일반코인 체크
        if (GameDataModel.Instance.YellowCoin >= 100 && hiddenstarPrefab != null && GameDataModel.Instance.hasHiddenStar == false)
        {
            Instantiate(hiddenstarPrefab, transform.position + Vector3.up * respawnHeight, Quaternion.identity);
            GameDataModel.Instance.hasHiddenStar = true;
        }
    }


    void StarTrigger()
    {
        GameDataModel.Instance.Star++;


        Destroy(gameObject);

		// 게임 종료 or 씬 전환
    }

	void HiddenStarTrigger()
	{
        GameDataModel.Instance.Star++;
        // 추후, 이펙터 추가 (사운드 및 파티클)

        Destroy(gameObject);
    }
}
