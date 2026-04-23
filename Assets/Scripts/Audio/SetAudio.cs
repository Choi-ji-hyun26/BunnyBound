using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SetAudio : MonoBehaviour
{
    private enum VolumeType { BGM, SFX }
    [SerializeField] private VolumeType volumeType;

    private Slider slider;

    private void Start()
    {
        slider = GetComponent<Slider>();

        // 현재 저장된 값으로 초기화
        if (SoundManager.Instance == null) return;

        if (volumeType == VolumeType.BGM)
            slider.value = SoundManager.Instance.bgmVolume;
        else if(volumeType == VolumeType.SFX)
            slider.value = SoundManager.Instance.sfxVolume;

        slider.onValueChanged.AddListener(SetVolume);
    }

    private void SetVolume(float value)
    {
        if (SoundManager.Instance == null) return;

        if (volumeType == VolumeType.BGM)
            SoundManager.Instance.SetBgmVolume(value);
        else
            SoundManager.Instance.SetSfxVolume(value);
    }
}
