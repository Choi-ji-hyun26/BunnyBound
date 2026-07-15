using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum SoundType { Jump, AttackQ, AttackW, Shield, Damaged, Die, Item, Interact, Finish, WeaponUpgrade, ButtonClick, FeverHit, Transform }

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioMixer bgmMixer;

    [SerializeField] private AudioMixer sfxMixer;
    [SerializeField] private AudioClip[] audioClips;

    [SerializeField] private int sfxPoolSize = 8;
    private AudioSource[] sfxSources;

    public float bgmVolume = 0.8f;
    public float sfxVolume = 0.15f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSFXPool();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void InitializeSFXPool()
    {
        sfxSources = new AudioSource[sfxPoolSize];
        AudioMixerGroup sfxGroup = sfxMixer.FindMatchingGroups("Master")[0];

        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = sfxGroup;
            sfxSources[i] = source;
        }
    }

    public void SetBgmVolume(float value)
    {
        bgmVolume = value;
        bgmMixer.SetFloat("BGMVolume", Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f);
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = value;
        sfxMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f);
    }

    public void PlaySound(SoundType type)
    {
        int index = (int)type;

        if (index < 0 || index >= audioClips.Length) return;

        // LINQ FirstOrDefault 대신 for 루프 — 이터레이터 GC 할당 제거
        AudioSource available = null;
        for (int i = 0; i < sfxSources.Length; i++)
        {
            if (!sfxSources[i].isPlaying)
            {
                available = sfxSources[i];
                break;
            }
        }

        if (available != null)
        {
            available.clip = audioClips[index];
            available.Play();
        }
        else
        {
            // 모든 소스 사용 중이면 첫 번째 강제 재생
            sfxSources[0].Stop();
            sfxSources[0].clip = audioClips[index];
            sfxSources[0].Play();
        }
    }
}
