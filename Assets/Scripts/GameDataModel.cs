using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameDataModel : MonoBehaviour
{
    public static GameDataModel Instance;
    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Header("Status")]
    [SerializeField] private int power;

    public int Power 
    { 
        get { return power; } 
        set 
        {
            if (value < 0)
            {
                power = 0;
                // �Ŀ��� ������, ������ ����
                Life--;
            }
            else if (value > 8)
            {
                power = 8;
            }    
            else power = value;
            OnPowerChanged?.Invoke(power); 
        } 
    }

    [SerializeField] private int life;
    public int Life
    {
        get { return life; }
        set
        {
            if (value < 0) life = 0;
            else life = value;
            OnLifeChanged?.Invoke(life);
        }
    }

    [SerializeField] private int yellowCoin;

    public int YellowCoin
    {
        get { return yellowCoin; }
        set
        {
            if (value < 0) yellowCoin = 0;
            else
            {
                var dif = value - yellowCoin;
                if (dif > 0)
                {
                    //���� ���͸�ŭ �Ŀ� ȸ��
                    Power += dif;
                }
                yellowCoin = value;
            }
            OnYellowCoinChanged?.Invoke(yellowCoin);
        }
    }

    public bool hasHiddenStar;

    [SerializeField] private int redCoin;

    public int RedCoin
    {
        get { return redCoin; }
        set
        {
            if (value < 0) redCoin = 0;
            else redCoin = value;
            OnRedCoinChanged?.Invoke(redCoin);
        }
    }

    [SerializeField] private int star;

    public int Star
    {
        get { return star; }
        set
        {
            if (value < 0) star = 0;
            else
            {
                if (star < value)
                {
                    // �ϴ��� ��Ÿ ���о��� �� ����
                    GameManager.Instance.LoadScene(-1);
                }
                star = value;
            }
            OnStarChanged?.Invoke(star);
        }
    }
    public UnityAction<int> OnPowerChanged;

    public UnityAction<int> OnLifeChanged;

    public UnityAction<int> OnYellowCoinChanged;

    public UnityAction<int> OnRedCoinChanged;

    public UnityAction<int> OnStarChanged;
}
