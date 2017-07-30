using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public const float kDefaultActivityWeight = 10f;

    CharacterConfig defaults;

    public string charName;

    CharacterActivity currentActivity = CharacterActivity.Idle;
    Dictionary<Needs, float> needs = new Dictionary<Needs, float>();
    Dictionary<Skills, float> skills = new Dictionary<Skills, float>();
    Dictionary<CharacterActivity, float> preferences = new Dictionary<CharacterActivity, float>();

    Node currentNode;
    Node targetNode = null;

    public string currentRoom = "";

    float happiness; // Calculated field
    CharacterMood Mood; // An even more calculated field


    SpriteRenderer rendererRef;
    Collider2D colliderRef;

    void Awake()
    {
        rendererRef = GetComponent<SpriteRenderer>();
        colliderRef = GetComponent<Collider2D>();
    }
    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void StartGame()
    {
        currentNode = defaults.startNode;
        currentRoom = defaults.startNode.room;
        transform.position = defaults.startNode.transform.position;
        // Reset other values
    }

    public void InitFromConfig(CharacterConfig cfg, Transform characterRoot, Node node, string roomName)
    {
        defaults = cfg;
        name = charName = defaults.name;

        rendererRef.sprite = defaults.sprite;
        needs.Clear();
        for (int i = 0; i < (int)Needs.Count; ++i)
        {
            Needs curNeed = (Needs)i;
            CharacterConfig.NeedPair pair = cfg.startNeeds.Find(x => x.nd == curNeed);
            if (!pair.Equals(default(CharacterConfig.NeedPair)))
            {
                needs[curNeed] = pair.initial;
            }
            else
            {
                needs[curNeed] = 100;
            }
        }

        skills.Clear();
        for (int i = 0; i < (int)Skills.Count; ++i)
        {
            Skills curSkill = (Skills)i;
            CharacterConfig.SkillPair pair = cfg.skillLevels.Find(x => x.sk == curSkill);
            if (!pair.Equals(default(CharacterConfig.SkillPair)))
            {
                skills[curSkill] = pair.level;
            }
            else
            {
                skills[curSkill] = 0;
            }
        }

        preferences.Clear();
        for (int i = 0; i < (int)CharacterActivity.Count; ++i)
        {
            CharacterActivity curActivity = (CharacterActivity)i;
            CharacterConfig.ActivityConfig pair = cfg.preferences.Find(x => x.activity == curActivity);
            if (!pair.Equals(default(CharacterConfig.ActivityConfig)))
            {
                preferences[curActivity] = pair.weight;
            }
            else
            {
                preferences[curActivity] = kDefaultActivityWeight;
            }
        }


        // Location
        currentNode = node;
        currentRoom = roomName;
        transform.position = node.transform.position;
        transform.parent = characterRoot;
    }

    public void SetLayer(string layer)
    {
        rendererRef.sortingLayerName = layer; 
    }
}
