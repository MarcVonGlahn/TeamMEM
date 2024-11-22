using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeastiaryControl : MonoBehaviour
{
    [SerializeField] List<BeastiaryEntry> bestiaryEntries;

    [SerializeField] float animDuration = 1.0f;
    [SerializeField] AnimationCurve showCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] AnimationCurve hideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    bool _isShowing = false;
    bool _areAllDiscoverd = false;

    Vector3 _showPosition;
    Vector3 _hidePosition;

    private void Awake()
    {
        InitBestiaryEntries();

        // Set Beastiary To Hide On Start
        _showPosition = transform.position;
        _hidePosition = new Vector3(
            transform.position.x,
            transform.position.y + Screen.height,
            transform.position.z);

        transform.position = _hidePosition;
    }


    private void InitBestiaryEntries()
    {
        foreach(var entry in bestiaryEntries)
        {
            entry.InitEntry();
        }
    }



    public void OnBeastiaryActionPerformed()
    {
        if(_isShowing)
        {
            // Hide Beastiary
            transform.DOMove(_hidePosition, animDuration).SetEase(hideCurve);
            _isShowing = false;
        }
        else
        {
            // Show Beastiary
            transform.DOMove(_showPosition, animDuration).SetEase(showCurve);
            _isShowing = true;
        }
    }



    public void OnUpdateBeastiaryEntry(CreaturePropertiesHelper scannedCreature)
    {
        var entry = bestiaryEntries.Find(x => x.GetCreatureID() == scannedCreature.moveset_so.CreatureID);

        if (entry == null)
            return;

        entry.UpdateEntry(scannedCreature.moveset_so);


        // Check if all Entries are fully discovered
        bool areAllDiscovered = true;
        foreach(var e in bestiaryEntries)
        {
            if(e.AreBothVersionsDiscovered)
                continue;

            areAllDiscovered = false;
            break;
        }

        if(areAllDiscovered)
        {
            // Open the big hole in the field
        }
    }
}
