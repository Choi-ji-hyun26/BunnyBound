using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System.Linq;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioMixer bgmMixer;

    [SerializeField] private AudioMixer sfxMixer;
    [SerializeField] private AudioClip[] audioClips;

    [SerializeField] private int sfxPoolSize = 5; // 풀 크기
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
        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = sfxMixer.FindMatchingGroups("Master")[0];
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

    public void PlaySound(string type)
    {
        int index = type switch
        {
            "JUMP" => 0,
            "ATTACK" => 1,
            "DAMAGED" => 2,
            "DIE" => 3,
            "ITEM" => 4,
            "FINISH" => 5,
            _ => -1
        };

        if (index < 0 || index >= audioClips.Length) return;

        // 사용 가능한 AudioSource 찾기
        AudioSource source = sfxSources.FirstOrDefault(s => !s.isPlaying);
        if (source != null)
        {
            source.clip = audioClips[index];
            source.Play();
        }
        else
        {
            // 모든 AudioSource 사용 중이면 임의로 첫 번째 AudioSource 재생
            sfxSources[0].Stop();
            sfxSources[0].clip = audioClips[index];
            sfxSources[0].Play();
        }
    }
}
