using System;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class ResourceData
{
    public string name;
    public List<NeedEffect> needsAffected = new List<NeedEffect>();

    public int lightMinutes = 0;
    public bool healsBerserk = false;

    public int initialAmount = 0;
    public bool requiresCharacter = false;
}