using UnityEngine;

public class RedCoin : Coin
{
    [Header("Red Coin Settings")]
    [SerializeField] private GameObject starSpawnObject;

    protected override void GainCoin()
    {
        GameDataModel.Instance.RedCoin++;
        base.GainCoin();
    }

    protected override void StarTrigger()
    {
        base.StarTrigger();

        if (GameDataModel.Instance.RedCoin == 8)
        {
            Debug.Log("Spawn Red Coin Star");
            var obj = Instantiate(starPrefab, starSpawnObject.transform.position + Vector3.up * 2, Quaternion.identity,null);
            Debug.Log(obj.name);
        }
    }
}
