using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreaturePropertiesHelper : MonoBehaviour
{
    public bool WasScanned = false;
    public SO_Moveset moveset_so;

    public void SetCreatureProperties(SO_Moveset moveset)
    {
        moveset_so = moveset;
    }
}
