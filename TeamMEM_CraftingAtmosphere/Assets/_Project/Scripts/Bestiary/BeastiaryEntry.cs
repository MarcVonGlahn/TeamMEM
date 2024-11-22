using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class BeastiaryEntry : MonoBehaviour
{
    [SerializeField] string creatureID = "1";
    [Space]
    [SerializeField] List<TextMeshProUGUI> textsToDisplay;
    [SerializeField] Image creatureImage;
    [SerializeField] Image nonCorruptedImage;
    [SerializeField] Image corruptedImage;

    bool _wasDiscovered = false;
    bool _corruptedDiscovered = false;
    bool _nonCorruptedDiscovered = false;

    public bool AreBothVersionsDiscovered {
        get
        { 
            return _corruptedDiscovered && _nonCorruptedDiscovered;
        }
    }

    public void InitEntry()
    {
        foreach (var t in textsToDisplay)
        {
            t.gameObject.SetActive(false);
        }

        creatureImage.color = Color.black;

        corruptedImage.gameObject.SetActive(false);
        nonCorruptedImage.gameObject.SetActive(false);
    }


    public string GetCreatureID() { return creatureID; }


    public void UpdateEntry(SO_Moveset moveset)
    {
        // Initially activate a bunch of stuff when first discovered
        if (!_wasDiscovered)
        {
            _wasDiscovered = true;

            foreach (var t in textsToDisplay)
            {
                t.gameObject.SetActive(true);
            }

            creatureImage.color = Color.white;

            corruptedImage.gameObject.SetActive(true);
            nonCorruptedImage.gameObject.SetActive(true);

            switch (moveset.Corruption)
            {
                case Corruption.Yes:
                    nonCorruptedImage.DOFade(0.2f, 0);
                    _corruptedDiscovered = true;
                    break;
                case Corruption.No:
                    corruptedImage.DOFade(0.2f, 0);
                    _nonCorruptedDiscovered = true;
                    break;
            }
        }
        else
        {
            switch (moveset.Corruption)
            {
                case Corruption.Yes:
                    corruptedImage.DOFade(1f, 0);
                    _corruptedDiscovered = true;
                    break;
                case Corruption.No:
                    nonCorruptedImage.DOFade(1f, 0);
                    _corruptedDiscovered = true;
                    break;
            }
        }
    }
}
