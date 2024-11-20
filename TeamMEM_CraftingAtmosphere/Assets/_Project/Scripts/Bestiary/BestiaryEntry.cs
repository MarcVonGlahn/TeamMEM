using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class BestiaryEntry : MonoBehaviour
{
    [SerializeField] string creatureID = "1";
    [Space]
    [SerializeField] List<TextMeshProUGUI> textsToDisplay;
    [SerializeField] Image creatureImage;
    [SerializeField] Image nonCorruptedImage;
    [SerializeField] Image corruptedImage;

    public void InitEntry()
    {
        foreach (var t in textsToDisplay)
        {
            t.gameObject.SetActive(false);
        }
    }
}
