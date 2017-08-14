using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpeechHelper
{
    public static void ReportEnvironmentActivityCheck(Character chara, EnvironmentActivityCheckResult result)
    {
        switch (result)
        {
            case EnvironmentActivityCheckResult.NoPower:
                {
                    chara.Talk(SpeechEntry.NoPower);
                    break;
                }
            case EnvironmentActivityCheckResult.DarknessRequired:
                {
                    chara.Talk(SpeechEntry.NoLight);
                    break;
                }
            case EnvironmentActivityCheckResult.LightRequired:
                {
                    chara.Talk(SpeechEntry.Light);
                    break;
                }
            default: break;
        }
    }

    public static void ReportCharacterActivityCheck(Character chara, CharacterActivityCheckResult result)
    {
        switch(result)
        {
            case CharacterActivityCheckResult.Forbidden:
            {
                chara.Talk(SpeechEntry.Restricted);
                break;
            }
            default:break;
        }

    }
}
