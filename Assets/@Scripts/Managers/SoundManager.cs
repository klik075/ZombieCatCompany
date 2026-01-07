using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    private AudioSource[] _audioSources = new AudioSource[(int)Define.ESound.MaxCount];
    private Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();

    private Transform _soundRoot;
    public Transform SoundRoot { get { return Utils.GetRootTransform(ref _soundRoot, "@SoundRoot"); } }

    void Awake()
    {
        string[] soundTypeNames = System.Enum.GetNames(typeof(Define.ESound));
        for (int i = 0; i < soundTypeNames.Length - 1; i++)
        {
            GameObject go = new GameObject { name = soundTypeNames[i] };
            _audioSources[i] = go.AddComponent<AudioSource>();
            go.transform.SetParent(SoundRoot);
        }

        _audioSources[(int)Define.ESound.Bgm].loop = true;
    }

    private AudioClip GetAudioClip(string key)
    {
        if (_audioClips.ContainsKey(key) == false)
        {
            AudioClip audioClip = ResourceManager.Instance.Get<AudioClip>(key);
            _audioClips.Add(key, audioClip);
        }

        return _audioClips[key];
    }

    public void Play2D(Define.ESound type, string key, float pitch = 1.0f)
    {
        AudioClip audioClip = GetAudioClip(key);
        Play2D(type, audioClip, pitch);
    }

    public void Play2D(Define.ESound type, AudioClip audioClip, float pitch = 1.0f)
    {
        AudioSource audioSource = _audioSources[(int)type];

        if (type == Define.ESound.Bgm)
        {
            if (audioSource.isPlaying)
                audioSource.Stop();

            audioSource.pitch = pitch;
            audioSource.clip = audioClip;
            audioSource.Play();
        }
        else
        {
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(audioClip);
        }
    }

    public void Play3D(string key, GameObject soundObject, float minDistance = 1.0f, float maxDistance = 20.0f, float pitch = 1.0f)
    {
        AudioClip audioClip = GetAudioClip(key);
        Play3D(audioClip, soundObject, minDistance, maxDistance, pitch);
    }

    public void Play3D(AudioClip audioClip, GameObject soundObject, float minDistance = 1.0f, float maxDistance = 20.0f, float pitch = 1.0f)
    {
        AudioSource audioSource = soundObject.GetOrAddComponent<AudioSource>();

        audioSource.clip = audioClip;
        audioSource.spatialBlend = 1.0f;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.pitch = pitch;
        audioSource.Play();
    }

    public void Stop(Define.ESound type)
    {
        AudioSource audioSource = _audioSources[(int)type];
        audioSource.Stop();
    }

    public void Clear()
    {
        foreach (AudioSource audioSource in _audioSources)
            audioSource.Stop();

        _audioClips.Clear();
    }

    public void SetVolume(Define.ESound type, float volume)
    {
        if (_audioSources[(int)type] != null)
            _audioSources[(int)type].volume = volume;
    }

    public float GetVolume(Define.ESound type)
    {
        if (_audioSources[(int)type] != null)
            return _audioSources[(int)type].volume;
        return 1f;
    }
}
