using Ami.BroAudio;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Playables;

public class Star : MonoBehaviour
{
    [Header("Star Settings")]
    public bool isGameOverStar;
    [SerializeField] Transform starModel;
    [SerializeField] CinemachineVirtualCamera cam;
    [SerializeField] Collider col;
    [SerializeField] SoundID appearStarSound;


    [Header("Other Settings")]
    [SerializeField] float camForwardDistance = 3f;
    [SerializeField] Vector3 AnimationStarOffset = new Vector3(0, 1f, 0);
    void Start()
    {
        InitStarAnimation();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            GetStar();
        }
    }

    void GetStar()
    {
        GameDataModel.Instance.Star++;
        GetStarAnimation();

        if (isGameOverStar)
            GameManager.Instance.LoadScene(-1);
    }

    void InitStarAnimation()
    {
        var seq = DOTween.Sequence();
        seq.Append(starModel.DOJump(starModel.position, 2f, 1, 3.5f).SetEase(Ease.OutQuad)).SetUpdate(true);
        seq.AppendInterval(0.5f);
        seq.OnStart(() =>
        {
            Time.timeScale = 0f;
            cam.transform.position = transform.position + transform.forward * camForwardDistance;
            cam.transform.LookAt(transform);
            cam.Priority = 20;
            col.enabled = false;
            BroAudio.Play(appearStarSound);
            var effect = Effect.Custom("Master", -80f, 0.1f);
            BroAudio.SetEffect(effect,BroAudioType.Music);
        });
        seq.OnComplete(() =>
        {
            Time.timeScale = 1f;
            cam.Priority = 0;
            col.enabled = true;
            var effect = Effect.Custom("Master", 0f, 0.1f);
            BroAudio.SetEffect(effect,BroAudioType.Music);
        });

        seq.Play();
    }

    void GetStarAnimation()
    {
        var player = GameManager.Instance.Player;
        var playerCam = GameManager.Instance.GetStarShotCamera;

        var seq = DOTween.Sequence();
        seq.Append(transform.DOScale(Vector3.one * 0.1f, 0.3f).SetEase(Ease.OutBack).SetDelay(1.5f));

        seq.AppendInterval(1.2f);
        seq.OnStart(() =>
        {
            BroAudio.Play(appearStarSound);
            transform.localScale = Vector3.zero;
            transform.position = player.transform.position + AnimationStarOffset;
            Time.timeScale = 0f;
            player.animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            player.animator.SetBool("GetStar", true);
            playerCam.transform.position = player.transform.position + player.transform.forward * camForwardDistance;
            playerCam.transform.LookAt(player.transform);
            playerCam.SetActive(true);
        });

        seq.OnComplete(() =>
        {
            Time.timeScale = 1f;
            player.animator.updateMode = AnimatorUpdateMode.Normal;
            player.animator.SetBool("GetStar", false);
            playerCam.SetActive(false);
            Destroy(gameObject);
        });

        seq.SetUpdate(true);
        seq.Play();
    }
}
