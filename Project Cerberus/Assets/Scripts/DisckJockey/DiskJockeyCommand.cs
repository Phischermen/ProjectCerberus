/*
 * DiskJockeyCommand is used to easily change music at the start of a scene.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiskJockeyCommand : MonoBehaviour
{
    public AudioClip clip;

    private void Start()
    {
        DiskJockey.PlayTrack(clip);
    }
}