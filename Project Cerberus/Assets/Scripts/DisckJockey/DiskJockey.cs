/*
 * The Disk Jockey is in control of setting the music. His handle is "One Eyed Jack." DJ is initialized via the
 * "RuntimeInitializeOnLoadMethod" method and set to never unload so that it is always available no matter which scene.
 * you play.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiskJockey : MonoBehaviour
{
    private static DiskJockey _i;
    private static AudioSource _currentAudio;

    [RuntimeInitializeOnLoadMethod]
    static void OnRuntimeMethodLoad()
    {
        var go = new GameObject("DiskJockey");
        _i = go.AddComponent<DiskJockey>();
    }
    private void Awake()
    {
        _currentAudio = gameObject.AddComponent<AudioSource>();
        _currentAudio.loop = true;
        DontDestroyOnLoad(gameObject);
    }

    public static void PlayTrack(AudioClip clip)
    {
        if (clip == _currentAudio.clip)
        {
            return;
        }
        _i.StartCoroutine(PlayTrackRoutine(clip, 1f));
    }

    private static IEnumerator PlayTrackRoutine(AudioClip clip, float fadeInOutTime)
    {
        // Split fadeInOutTime.
        fadeInOutTime /= 2f;
        // Fade Out.
        var start = _currentAudio.volume;
        var timePassed = 0f;
        while (timePassed < fadeInOutTime)
        {
            timePassed += Time.deltaTime;
            _currentAudio.volume = Mathf.Lerp(start, 0, timePassed / fadeInOutTime);
            yield return new WaitForFixedUpdate();
        }

        _currentAudio.clip = clip;
        _currentAudio.Play();
        // Fade In.
        timePassed = 0f;
        while (timePassed < fadeInOutTime)
        {
            timePassed += Time.deltaTime;
            _currentAudio.volume = Mathf.Lerp(0, start, timePassed / fadeInOutTime);
            yield return new WaitForFixedUpdate();
        }
    }
}