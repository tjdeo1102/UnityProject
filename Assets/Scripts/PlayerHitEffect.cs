using DG.Tweening;
using UnityEngine;

public class PlayerHitEffect : MonoBehaviour
{
    Material mat;
	void OnEnable()
	{
		mat = GetComponent<SkinnedMeshRenderer>().material;
        mat.DOKill();
        mat.DOFade(0,0.1f).From(0.5f).SetLoops(-1,LoopType.Yoyo);
	}

	void OnDisable()
	{
		mat.DOKill();
	}
}
