
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance; // ����ʵ��

    [Header("Audio Sources")]
    public AudioSource bgmSource; // ����������Դ

    [Header("Audio Clips")]
    public List<AudioClip> bgmClips; // ���������б�
    public List<AudioClip> sfxClips; // ��Ч�б�

    private Dictionary<string, AudioClip> bgmDictionary;
    private Dictionary<string, AudioClip> sfxDictionary;

    // �洢��̬��������ЧAudioSource
    private Dictionary<string, AudioSource> sfxSources;

    private void Awake()
    {
        // ȷ������
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // �糡������
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeDictionaries();
    }

    private void InitializeDictionaries()
    {
        bgmDictionary = new Dictionary<string, AudioClip>();
        sfxDictionary = new Dictionary<string, AudioClip>();
        sfxSources = new Dictionary<string, AudioSource>();

        foreach (var clip in bgmClips)
        {
            bgmDictionary[clip.name] = clip;
        }

        foreach (var clip in sfxClips)
        {
            sfxDictionary[clip.name] = clip;
        }
    }

    // ���ű�������
    public void PlayBGM(string clipName, bool loop = true)
    {
        if (bgmDictionary.TryGetValue(clipName, out AudioClip clip))
        {
            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning($"�������� {clipName} δ�ҵ���");
        }
    }

    // ֹͣ��������
    public void StopBGM()
    {
        bgmSource.Stop();
    }

    // ������Ч
    public void PlaySFX(string clipName)
    {
        if (sfxDictionary.TryGetValue(clipName, out AudioClip clip))
        {
            // ����Ƿ����ж�Ӧ��AudioSource
            if (!sfxSources.TryGetValue(clipName, out AudioSource source))
            {
                // ���û�У���̬����һ��AudioSource
                source = gameObject.AddComponent<AudioSource>();
                source.clip = clip;
                sfxSources[clipName] = source;
            }

            source.Play();
        }
        else
        {
            Debug.LogWarning($"��Ч {clipName} δ�ҵ���");
        }
    }

    // ָֹͣ����Ч
    public void StopSFX(string clipName)
    {
        if (sfxSources.TryGetValue(clipName, out AudioSource source))
        {
            if (source.isPlaying)
            {
                source.Stop();
            }
        }
        else
        {
            Debug.LogWarning($"��Ч {clipName} δ�ҵ���δ���ţ�");
        }
    }

    // ���ñ�����������
    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = Mathf.Clamp01(volume);
    }

    // ������Ч����
    public void SetSFXVolume(string clipName, float volume)
    {
        if (sfxSources.TryGetValue(clipName, out AudioSource source))
        {
            source.volume = Mathf.Clamp01(volume);
        }
        else
        {
            Debug.LogWarning($"��Ч {clipName} δ�ҵ���");
        }
    }
}
