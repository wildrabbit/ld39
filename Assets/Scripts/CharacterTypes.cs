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
    Drink,
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
public enum CharacterActivity : int
{
    Sleep = 0,
    Moving,
    Idle,
    Repairing,
    Operating,
    Read,
    TV,
    Console,
    Toys,
    Cooking,
    Eating,
    WC,
    Bath, // Probably not
    Dead,
    Breakdown,
    Count
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
    public struct NeedPair
    {
        public Needs nd;
        public float initial;
        // Time to fulfill?? (Belongs more in furniture, doesn't it?)
        public bool Equals(NeedPair other)
        {
            return Equals(other, this);
        }

        public override bool Equals(object obj)
        {
            return obj is NeedPair && ((NeedPair)obj).nd == nd;
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

    public bool kid;
    public string name;
    public Color clothesColour;
    public Node startNode;
    public Sprite sprite;
    public float defaultSpeed = 2.0f;

    public CharacterAnimationConfig animConfig;


    // Preferences
    public List<SkillPair> skillLevels = new List<SkillPair>(); // If not found, assumed 0
    public List<NeedPair> startNeeds = new List<NeedPair>();
    public List<ActivityConfig> preferences = new List<ActivityConfig>();// Weighting player's likelihood of favouring activities over others. 
                                                                         // By default will be 10.
}