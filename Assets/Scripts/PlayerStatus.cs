using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    public void TakeDamage(int damage)
    {
        GameDataModel.Instance.Power-= damage;
    }
}
