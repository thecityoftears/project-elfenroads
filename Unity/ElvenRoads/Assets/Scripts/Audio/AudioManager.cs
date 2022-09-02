using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// based on https://www.youtube.com/watch?v=HhFKtiRd0qI

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    Sound[] sounds;

    public static AudioManager instance;
    
    void Start() {
        foreach(Sound s in sounds) {
            GameObject _go = new GameObject("Sound: " + s.name);
            _go.transform.parent = gameObject.transform;
            s.SetSource(_go.AddComponent<AudioSource>());
        }
        instance = this;
    }

    public static void PlaySound(string name) {
        if(instance == null)
            return;

        foreach(Sound s in instance.sounds) {
            if(s.name == name) {
                s.Play();
                return;
            }
        }
    }
}
[System.Serializable]
public class  Sound {
    public string name;
    public AudioClip clip;
    [Range(0.0f, 1.0f)]
    public float volume = 0.7f;
    [Range(0.5f, 1.5f)]
    public float pitch = 1.0f;

    private AudioSource source;
    public bool loop = false;

    public void SetSource(AudioSource _source) {
        source = _source;
        source.clip = clip;
    }

    public void Play() {
        source.volume = volume;
        source.pitch = pitch;
        source.loop = loop;
        source.Play();
    }
}
