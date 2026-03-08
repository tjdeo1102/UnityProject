using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NumCounterUI : MonoBehaviour
{
    public enum UIType
    {
        Coin,Star,Life,Power
    }
    [Header("Base")]
    [SerializeField] Sprite[] numImages;
    [SerializeField] Image FirstDigit;
    [SerializeField] Image SecondDigit;
    [SerializeField] Image ThirdDigit;
    [SerializeField] UIType type;


    [Header("PowerSetting")]
    [SerializeField] float hidePowerUITime;
    [SerializeField] float hideSpeed;
    [SerializeField] Vector3 hidePos;
    private Vector3 originPos;
    private Coroutine hideCoroutine;
    private WaitForSeconds delay;
    private RectTransform rectTransform;
    private GameDataModel model;

    private void OnDisable()
    {
        switch (type)
        {
            case UIType.Star:
                model.OnStarChanged -= UpdateDigit;
                break;
            case UIType.Life:
                model.OnLifeChanged -= UpdateDigit;
                break;
            case UIType.Coin:
                model.OnYellowCoinChanged -= UpdateDigit;
                break;
            case UIType.Power:
                model.OnPowerChanged -= UpdateDigit;
                break;
            default:
                break;
        }
    }
    void Start()
    {
        // 라이프 사이클 호출순서 보장이 안되므로 Start에서 구독
        model = GameDataModel.Instance;
        switch (type)
        {
            case UIType.Star:
                model.OnStarChanged += UpdateDigit;
                UpdateDigit(model.Star);
                break;
            case UIType.Life:
                model.OnLifeChanged += UpdateDigit;
                UpdateDigit(model.Life);
                break;
            case UIType.Coin:
                model.OnYellowCoinChanged += UpdateDigit;
                UpdateDigit(model.YellowCoin);
                break;
            case UIType.Power:
                model.OnPowerChanged += UpdateDigit;
                UpdateDigit(model.Power);
                delay = new WaitForSeconds(hidePowerUITime);
                if(TryGetComponent<RectTransform>(out rectTransform))
                {
                    originPos = rectTransform.anchoredPosition;
                    hideCoroutine = StartCoroutine(HideRoutine());
                }
                break;
            default:
                break;
        }
    }

    IEnumerator HideRoutine()
    {
        rectTransform.anchoredPosition = originPos;
        yield return delay;
        while (rectTransform != null) {
            rectTransform.anchoredPosition = Vector3.MoveTowards(rectTransform.anchoredPosition , hidePos, Time.deltaTime * hideSpeed);
            yield return null;
        }
    }

    void UpdateDigit(int number)
    {
        if (FirstDigit == null || SecondDigit == null || ThirdDigit == null) return;
        // 파워의 변화가 있는 경우에만 실행될 HideRoutine
        if (type == UIType.Power && rectTransform != null)
        {
            if (hideCoroutine != null) StopCoroutine(hideCoroutine);
            hideCoroutine = StartCoroutine(HideRoutine());
        }

        if (number > 99)
        {
            var first = (number / 100) % 10;
            FirstDigit.sprite = numImages[first];
            FirstDigit.gameObject.SetActive(true);
        }
        else
        {
            FirstDigit.gameObject.SetActive(false);
        }
        if (number > 9)
        {
            var second = (number / 10) % 10;
            SecondDigit.sprite = numImages[second];
            SecondDigit.gameObject.SetActive(true);
        }
        else
        {
            SecondDigit.gameObject.SetActive(false);
        }
        var third = number % 10;
        ThirdDigit.sprite = numImages[third];
    }
}
