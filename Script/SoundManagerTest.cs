using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SoundManagerTest : MonoBehaviour
{
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private TMP_Text ambientBusText;
    [SerializeField] private TMP_Text environmentBusText;
    [SerializeField] private TMP_Text sfxBusText;
    [SerializeField] private TMP_Text uiBusText;
    [SerializeField] private TMP_Text voiceBusText;

    [SerializeField] Button btnBubbles = null;
    [SerializeField] Button btnVelcro = null;

    private void Start()
    {
        btnVelcro.onClick.AddListener(PlayVelcro);
    }

    private void Update()
    {
        UpdateBusInfo(SoundBUS.Ambient, ambientBusText);
        UpdateBusInfo(SoundBUS.Environment, environmentBusText);
        UpdateBusInfo(SoundBUS.SFX, sfxBusText);
        UpdateBusInfo(SoundBUS.UI, uiBusText);
        UpdateBusInfo(SoundBUS.Voice, voiceBusText);
    }

    private void PlayVelcro()
    {
        Debug.Log("Playing");
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
