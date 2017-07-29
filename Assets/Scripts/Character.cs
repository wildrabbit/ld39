using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterActivity
{
    Sleep,
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
    Breakdown
}

public enum Needs
{
    Food,
    Drink,
    Toilet,
    Sleep,
    Temperature,
    Hygiene,
    Entertainment,
    Social
}

public enum Skills
{
    Mechanic,
    Cooking,
    Healing,
    Electricity,
    Counselor,
    Entertainer // ?
}

public class Character : MonoBehaviour
{
    public string charName;
    public Color bodyTint;

    public float body;
    public float mind;

    public CharacterActivity activity;
    public Dictionary<Needs, float> needs;
    public Dictionary<Skills, float> skills;

    // Target room, target item @ room

    // TODO: Priorities, likes/dislikes

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
