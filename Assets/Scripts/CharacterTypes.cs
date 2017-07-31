using System;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterMood: int
{
    Happy = 0,
    Content,
    Indifferent,
    Cranky,
    Angry,
    Berserk,
    Count
}

public enum Needs : int
{
    Food = 0,
    Water,
    Toilet,
    Sleep,
    Temperature,
    Hygiene,
    Entertainment,
    Social,
    Count
}

[System.Serializable]
public class NeedEffect
{
    public Needs need;
    public bool absolute;
    public float needAmount; // A percent of the max if not absolute.
}

public enum Skills : int
{
    Mechanic = 0,
    Cooking,
    Healing,
    Electrician,
    Counselor,
    Defender,
    Count
}

[Flags]
public enum CharacterStatus
{
    None = 0,
    Sick = 0x1, // Impaired skills
    Breakdown = 0x2, // Uncontrollable.
    Dead = 0x4//...
}

public enum CharacterActivity : int
{
    Sleep = 0,
    Moving,
    Idle,
    Repairing,
    Operating,
    Read,
    Dancing,
    TV,
    Computer,
    Cooking,
    Eating,
    Drinking,
    WC,
    Bath,
    Healing,
    Count
}

public enum EnvironmentActivityCheckResult
{
    Success,
    NoFood,
    NoWater,
    Light, //Darkness activities :D
    NoLight,
    NoPower,
    NoMeds
}
public enum CharacterActivityCheckResult
{
    Success,
    Dead,
    Breakdown,
    Sick,
    NeedSatisfied,
    NeedNotMet,
    SkillFailed
}

public class ActivityContext
{
    public string furnitureKey; // For example, if we're operating/repairing, WTF is it we're fixing?
}

[System.Serializable]
public class CharacterAnimationConfig
{
    public Sprite[] frontAnim;
    public Sprite[] backAnim;
    public Sprite[] sideAnim; // split left/right eventually?
    public bool IsSideFacingLeft;
    // TODO: Moar anims
}

public enum FacingDirection
{
    None,
    Up,
    Down,
    Left,
    Right
}

public enum SpeechEntry
{
    NeedFull,
    Light,
    NoLight,
    NoPower,
    Taken,
    Restricted,
    FocusEat,
    FocusHungry,
    FocusDrink,
    FocusThirsty,
    FocusToilet,
    FocusNeedsToilet,
    FocusSleep,
    FocusSleepy,
    FocusBath,
    FocusFilthy,
    FocusEntertainment,
    FocusBored,
    ScaredLightning,
    Random1,
    Random2,
    Random3,
    Random4,
}

[System.Serializable]
public class CharacterConfig
{
    [System.Serializable]
    public struct SkillPair
    {
        public Skills sk;
        public int level;
        // TODO: Support learning? 
        public bool Equals(SkillPair other)
        {
            return Equals(other, this);
        }

        public override bool Equals(object obj)
        {
            return obj is SkillPair && ((SkillPair)obj).sk == sk;
        }
    }

    [System.Serializable]
    public struct ActivityConfig
    {
        public CharacterActivity activity;
        public float weight;
        public float timeToFull;

        public bool Equals(ActivityConfig other)
        {
            return Equals(other, this);
        }

        public override bool Equals(object obj)
        {
            return obj is ActivityConfig && ((ActivityConfig)obj).activity == activity;
        }
    }

    /*
    Food = 0,
    Drink,
    Toilet,
    Sleep,
    Temperature,
    Hygiene,
    Entertainment */
    public float[] defaultNeedDepletionTimeHours = new float[(int)Needs.Count] { 6.0f, 4.0f, 3.0f, 12.0f, 16.0f, 10.0f, 8.0f, 24.0f };
    public float[] moodWeight = new float[(int)Needs.Count] { 15.0f, 15.0f, 15.0f, 20.0f, 12.0f, 8.0f, 25.0f, 10.0f };
    public float[] healthWeight = new float[(int)Needs.Count] { 25.0f, 30.0f, 8.0f, 20.0f, 16.0f, 10.0f, 8.0f, 4.0f };


    public bool kid;
    public string name;
    public Color clothesColour;
    public Node startNode;
    public Sprite sprite;
    public Sprite portrait;
    public float defaultSpeed = 2.0f;
    public float initialNeedValue = 100.0f;
    public CharacterAnimationConfig animConfig;


    // Preferences
    public List<SkillPair> skillLevels = new List<SkillPair>(); // If not found, assumed 0
    public List<ActivityConfig> preferences = new List<ActivityConfig>();// Weighting player's likelihood of favouring activities over others. 
                                                                         // By default will be 10.
}