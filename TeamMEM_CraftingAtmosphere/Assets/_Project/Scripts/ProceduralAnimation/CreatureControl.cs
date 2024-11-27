using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureControl : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] ProceduralAnimationController procAnimController;
    [SerializeField] FollowPath followPathControl;
    [SerializeField] CreaturePropertiesHelper creaturePropertiesHelper;
    [Header("Overall Creature Attributes")]
    [SerializeField] bool startMovingOnStart = true;
    [SerializeField] SO_Moveset moveset;

    [Header("Sounds")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] Vector2 randomPlayRange = new Vector2(9, 16);

    bool _isMakingSounds = true;

    public bool IsMakingSounds { get => _isMakingSounds; set => _isMakingSounds = value; }

    private void Awake()
    {
        if (startMovingOnStart)
        {
            followPathControl.SetShouldMove(true);
            followPathControl.SetWalkingSpeed(moveset.WalkingSpeed);
        }

        creaturePropertiesHelper.SetCreatureProperties(moveset);
    }


    private void Start()
    {
        if (_isMakingSounds)
        {
            StartCoroutine(PlayingCreatureSounds());
        }
    }


    public SO_Moveset GetMoveset()
    {
        return moveset;
    }


    public IEnumerator PlayingCreatureSounds()
    {
        if (audioSource.clip == null)
            yield break;

        while (_isMakingSounds)
        {
            float randomWait = Random.Range(randomPlayRange.x, randomPlayRange.y);
            yield return new WaitForSeconds(randomWait);

            audioSource.Play();
        }
    }
}
