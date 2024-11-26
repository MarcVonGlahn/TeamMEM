using UnityEngine.Audio;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioMixerGroup mixer;

    public List<Sound> sounds;
    public List<Sound> ambients;

    private AudioSource ambienceSource;

    private void Awake()
    {
        if(instance == null)
            instance = this;


        foreach(var s in sounds)
        {
            GameObject child = new GameObject("AudioSource_" + s.name);
            child.transform.parent = transform;
            s.source = child.AddComponent<AudioSource>();

            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.playOnAwake = false;

            s.source.outputAudioMixerGroup = mixer;
        }
    }


    public void Play(string name)
    {
        Sound s = sounds.Find(x => x.name == name);

        s.source.Play();
    }


    public void StartAmbience()
    {
        GameObject temp = new GameObject("Ambience_Sound");
        ambienceSource = temp.AddComponent<AudioSource>();

        ambienceSource.Play();
    }
}
