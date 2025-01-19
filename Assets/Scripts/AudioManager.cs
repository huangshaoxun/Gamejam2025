using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance; // 单例实例

    [Header("Audio Sources")]
    public AudioSource bgmSource; // 背景音乐音源

    [Header("Audio Clips")]
    public List<AudioClip> bgmClips; // 背景音乐列表
    public List<AudioClip> sfxClips; // 音效列表

    private Dictionary<string, AudioClip> bgmDictionary;
    private Dictionary<string, AudioClip> sfxDictionary;

    // 存储动态创建的音效AudioSource
    private Dictionary<string, AudioSource> sfxSources;

    private void Awake()
    {
        // 确保单例
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

    // 播放背景音乐
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
            Debug.LogWarning($"背景音乐 {clipName} 未找到！");
        }
    }

    // 停止背景音乐
    public void StopBGM()
    {
        bgmSource.Stop();
    }

    // 播放音效（默认不循环，支持随机音量）
    public void PlaySFX(string clipName, bool loop = false, float minVolume = 1f, float maxVolume = 1f)
    {
        if (sfxDictionary.TryGetValue(clipName, out AudioClip clip))
        {
            // 检查是否已有对应的AudioSource
            if (!sfxSources.TryGetValue(clipName, out AudioSource source))
            {
                // 如果没有，动态创建一个AudioSource
                source = gameObject.AddComponent<AudioSource>();
                source.clip = clip;
                sfxSources[clipName] = source;
            }

            source.volume = Random.Range(minVolume, maxVolume); // 设置随机音量
            source.loop = loop; // 设置是否循环
            source.Play();
        }
        else
        {
            Debug.LogWarning($"音效 {clipName} 未找到！");
        }
    }

    // 停止指定音效
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
            Debug.LogWarning($"音效 {clipName} 未找到或未播放！");
        }
    }

    // 设置背景音乐音量
    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = Mathf.Clamp01(volume);
    }

    // 设置音效音量
    public void SetSFXVolume(string clipName, float volume)
    {
        if (sfxSources.TryGetValue(clipName, out AudioSource source))
        {
            source.volume = Mathf.Clamp01(volume);
        }
        else
        {
            Debug.LogWarning($"音效 {clipName} 未找到！");
        }
    }
}