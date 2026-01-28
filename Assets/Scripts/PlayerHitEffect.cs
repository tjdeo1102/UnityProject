using DG.Tweening;
using UnityEngine;

public class PlayerHitEffect : MonoBehaviour
{
	[SerializeField] float effectDuration = 1.5f;
	[SerializeField] Transform targetSkin;
	float timer;
    Material mat;
	SkinnedMeshRenderer skinRenderer;
	void OnEnable()
	{
		skinRenderer = GetComponent<SkinnedMeshRenderer>();
		mat = skinRenderer.material;
	}

	void Update()
	{
		timer += Time.deltaTime;
		if (timer > effectDuration)
		{
			mat.DOKill();
			skinRenderer.enabled = false;
			targetSkin.gameObject.SetActive(true);
		}
	}

	public void PlayHitEffect()
	{
		timer = 0f;
		skinRenderer.enabled = true;
		targetSkin.gameObject.SetActive(false);
		mat.DOKill();
        mat.DOFade(0,0.1f).From(0.5f).SetLoops(-1,LoopType.Yoyo);
	}

	void OnDisable()
	{
		mat.DOKill();
	}
}
