using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BestiaryControl : MonoBehaviour
{
    [SerializeField] List<BestiaryEntry> bestiaryEntries;

    private void Start()
    {
        
    }


    private void InitBestiaryEntries()
    {
        foreach(var entry in bestiaryEntries)
        {
            entry.InitEntry();
        }
    }
}
