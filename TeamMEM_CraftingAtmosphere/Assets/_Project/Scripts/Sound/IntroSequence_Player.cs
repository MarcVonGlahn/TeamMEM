using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroSequence_Player : MonoBehaviour
{
    [SerializeField] List<AudioClip> introClips;

    private AudioSource _audioSource;


    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }


    public void PlayIntro(int introNumber)
    {
        _audioSource.clip = introClips[introNumber - 1];

        _audioSource.Play();
    }

    public void LockingPlayerMovement()
    {
        PlayerController ctrl = FindObjectOfType<PlayerController>();

        if (ctrl != null)
        {
            ctrl.LockMovement = true;
        }
    }


    public void UnlockingPlayerMovement()
    {
        PlayerController ctrl = FindObjectOfType<PlayerController>();

        if (ctrl != null)
        {
            ctrl.LockMovement = false;
        }
    }
}
