using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class ActivityConfig
{
    [System.Serializable]
    public class NeedEntry
    {
        public Needs need;
        public float recovery;
    }

    [System.Serializable]
    public class ResourceEntry
    {
        public string resource;
        public int amount;
    }

    public CharacterActivity charActivity;
    public float activityDelay;

    public bool oneShot; // vs Over time.
    public Needs primaryNeed;
    public float primaryRecoveryRate;

    public SpeechEntry speechEntry;

    public List<NeedEntry> secondaryNeeds;

    public List<ResourceEntry> resourcesSpent;
    public List<ResourceEntry> resourcesGenerated;

}

[System.Serializable]
public class SideActivityConfig // TODO: Review if it makes sense to combine both
{
    public SideActivity sideActivity;
    public float activityDelay;
    public bool oneShot;
    public Needs primaryNeed;
    public float primaryRecoveryRate;
}

public class ActivityDataList : ScriptableObject
{
    public List<ActivityConfig> activityList;

    public List<SideActivityConfig> sideActivities;
}
