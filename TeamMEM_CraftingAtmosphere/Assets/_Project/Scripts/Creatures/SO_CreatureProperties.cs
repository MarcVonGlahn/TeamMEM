using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Corruption
{
    Yes,
    No
}

[CreateAssetMenu(fileName = "CreatureProperties_", menuName = "Custom/Creature Properties")]
public class SO_CreatureProperties : ScriptableObject
{
    public string CreatureName = "Example";
    public float Height = 1.5f;
    public WalkingStyle WalkingStyle = WalkingStyle.Biped;
    public Corruption Corruption = Corruption.No;
}
