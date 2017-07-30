using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public const float kDefaultActivityWeight = 10f;
    public const float kDistanceThreshold = 0.3f;
    NodeManager nodeManagerRef = null;
    CharacterManager characterManagerRef = null;

    CharacterConfig defaults;
    float speed;

    public string charName;

    CharacterActivity currentActivity = CharacterActivity.Idle;
    Dictionary<Needs, float> needs = new Dictionary<Needs, float>();
    Dictionary<Skills, float> skills = new Dictionary<Skills, float>();
    Dictionary<CharacterActivity, float> preferences = new Dictionary<CharacterActivity, float>();

    Node currentNode = null;
    Node temp = null;
    List<Node> path = new List<Node>();
    int pathNextIdx = -1;
    
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
	void Update ()
    {
        float delta = Time.deltaTime; // We might also need the scaled value?
        switch(currentActivity)
        {
            case CharacterActivity.Moving:
            {
                bool willReset = temp != null;
                Node next = !willReset ? path[pathNextIdx] : temp;

                Vector2 direction = (next.transform.position - transform.position).normalized;
                    transform.Translate(direction * speed * delta);

                float distanceToNextNode = Vector2.Distance(transform.position, next.transform.position);
                if (distanceToNextNode < kDistanceThreshold)
                {
                    currentNode = next;
                    int prevRoomIdx = System.Array.FindIndex(next.rooms, x => x == currentRoom);
                    if (prevRoomIdx >= 0)
                    {
                        int nextRoomIdx = (prevRoomIdx + 1) % 2;
                        Debug.LogFormat("{2} moves from {0} to {1}", next.rooms[prevRoomIdx], next.rooms[nextRoomIdx], name);
                        currentRoom = next.rooms[nextRoomIdx];
                    }
                    transform.position = next.transform.position;
                    if (willReset)
                    {
                        pathNextIdx = 0;
                    }
                    else
                    {
                        if (pathNextIdx == path.Count - 1)
                        {
                            currentActivity = CharacterActivity.Idle;
                        }
                        else pathNextIdx++;
                    }
                }
                break;
            }
            default:break;
        }		
	}

    public void StartGame()
    {
        currentNode = defaults.startNode;
        currentRoom = defaults.startNode.room;
        transform.position = defaults.startNode.transform.position;
        // Reset other values
    }

    public void SetDependencies(CharacterManager charManager, NodeManager nodeManager)
    {
        characterManagerRef = charManager;
        nodeManagerRef = nodeManager;
    }

    public void InitFromConfig(CharacterConfig cfg, Transform characterRoot, Node node, string roomName)
    {
        defaults = cfg;
        name = charName = defaults.name;

        speed = defaults.defaultSpeed;
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

    public void SetTarget(Node target)
    {
        if (target == currentNode) return;

        if (currentActivity != CharacterActivity.Breakdown && currentActivity != CharacterActivity.Dead)
        {
            bool oldPath = path.Count > 0;
            if (oldPath)
            {
                temp = path[pathNextIdx];
            }
            else
            {
                temp = null;
            }
            nodeManagerRef.FindPath(currentNode.name, target.name, ref path);
            pathNextIdx = 0;
            currentActivity = CharacterActivity.Moving;
        }
    }

    public void SetSelected(bool selected)
    {
        rendererRef.sortingLayerName = selected ? characterManagerRef.layerSelected
            : characterManagerRef.GetLayerForRoom(currentRoom);
        rendererRef.color = selected ? Color.green : Color.white;
    }
}
