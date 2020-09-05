using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioHelper
{
    public static AudioSource PlayClip2D(AudioClip clip, float volume, float length = 0)
    {
        // create
        GameObject audioObject = new GameObject("Audio2D");
        AudioSource audioSource
            = audioObject.AddComponent<AudioSource>();

        // configure
        audioSource.clip = clip;
        audioSource.volume = volume;

        // activate
        audioSource.Play();
        Object.Destroy(audioObject, length > 0 ? length : clip.length);

        // return in case the call wants to do other things
        return audioSource;
    }
}
