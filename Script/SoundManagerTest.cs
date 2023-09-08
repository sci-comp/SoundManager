using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SoundManagerTest : MonoBehaviour
{
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private TMP_Text sfxBusText;

    [SerializeField] Button btnBubbles = null;
    [SerializeField] Button btnVelcro = null;

    private void Start()
    {
        btnBubbles.onClick.AddListener(PlayBubbles);
        btnVelcro.onClick.AddListener(PlayVelcro);
    }

    private void Update()
    {
        UpdateBusInfo(SoundBUS.SFX, sfxBusText);
    }

    private void PlayBubbles()
    {
        SoundManager.Instance.PlaySound("sfx_bubbles");
    }

    private void PlayVelcro()
    {
        SoundManager.Instance.PlaySound("sfx_velcro");
    }

    private void UpdateBusInfo(SoundBUS bus, TMP_Text textComponent)
    {
        SoundBusInfo info = soundManager.GetBUSInfoFromList(bus);
        textComponent.text = $"Bus: {bus}\n" +
                             $"Active Voices: {info.activeSources.Count}/{info.voiceLimit}\n" +
                             $"Volume: {info.individualVolume}";
    }
}
