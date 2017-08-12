using System;
using System.Collections.Generic;
using UnityEngine;


public class ActivityManager : MonoBehaviour
{
    public ActivityDataList activityConfigList;

    Dictionary<CharacterActivity, ActivityConfig> activityTable;
    Dictionary<SideActivity, SideActivityConfig> sideActivities;

    public void BuildTable()
    {
        activityTable = new Dictionary<CharacterActivity, ActivityConfig>();
        foreach(ActivityConfig cfg in activityConfigList.activityList)
        {
            activityTable[cfg.charActivity] = cfg;
        }

        sideActivities = new Dictionary<SideActivity, SideActivityConfig>();
        foreach (SideActivityConfig cfg in activityConfigList.sideActivities)
        {
            sideActivities[cfg.sideActivity] = cfg;
        }
    }

    public bool TryGetActivity(CharacterActivity activity, out ActivityConfig cfg)
    {
        return activityTable.TryGetValue(activity, out cfg);
    }

    public bool TryGetSideActivity(SideActivity activity, out SideActivityConfig cfg)
    {
        return sideActivities.TryGetValue(activity, out cfg);
    }
}