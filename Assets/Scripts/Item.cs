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
        // ����, ������ �߰� (���� �� ��ƼŬ)
        CoinChecker();

        Destroy(gameObject);
	}

	void RedCoinTrigger()
	{
        GameDataModel.Instance.RedCoin++;
        // ���������� �Ϲ����� * 2�� ����
        GameDataModel.Instance.YellowCoin+=2;
        CoinChecker();


        // ����, ������ �߰� (���� �� ��ƼŬ)

        Destroy(gameObject);
    }

    void BlueCoinTrigger()
    {
        // ��������� �Ϲ� ���� * 5 �� ����
        GameDataModel.Instance.YellowCoin += 5;
        CoinChecker();

        // ����, ������ �߰� (���� �� ��ƼŬ)
        Destroy(gameObject);
    }

    void CoinChecker()
    {
        // �������� üũ
        if (GameDataModel.Instance.RedCoin == 8 && starPrefab != null)
        {
            Instantiate(starPrefab, respawnPosition, Quaternion.identity);
            // ��Ÿ�� �����Ǿ����� �˷��� ��ġ �ʿ�
        }

        // �Ϲ����� üũ
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

		// ���� ���� or �� ��ȯ
    }

	void HiddenStarTrigger()
	{
        GameDataModel.Instance.Star++;
        // ����, ������ �߰� (���� �� ��ƼŬ)

        Destroy(gameObject);
    }
}
