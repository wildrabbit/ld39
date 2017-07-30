using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public const float kDefaultActivityWeight = 10f;
    public const float kDistanceThreshold = 0.05f;
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

    FacingDirection ResolveFacingDirection(Vector2 direction)
    {
        // Horizontal vs Vertical
        if (Mathf.Abs(direction.y) >= Mathf.Abs(direction.x))
        {
            // Vertical!
            return (direction.y > 0) ? FacingDirection.Up : FacingDirection.Down;
        }
        else
        {
            return (direction.x > 0) ? FacingDirection.Right : FacingDirection.Left;
        }
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

                SetDirection(ResolveFacingDirection(direction));

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
                        characterManagerRef.roomManager.SwitchLights(currentRoom, true, true);
                    }
                    transform.position = next.transform.position;
                    if (willReset)
                    {
                        pathNextIdx = 0;
                        temp = null;
                    }
                    else
                    {
                        // STOPPED!
                        if (pathNextIdx == path.Count - 1)
                        {
                            currentActivity = CharacterActivity.Idle;
                            pathNextIdx = -1;
                            path.Clear();
                            SetDirection((currentNode.facing != FacingDirection.None) ? currentNode.facing : FacingDirection.Down, currentNode.forcedFlipY, currentNode.forcedZRotation);
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

        SetDirection(currentNode.facing == FacingDirection.None ? FacingDirection.Down : currentNode.facing, currentNode.forcedFlipY, currentNode.forcedZRotation);
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
        transform.parent = characterRoot;
    }

    public void SetDirection(FacingDirection dir, bool forcedFlipY = false, float forcedRotation = 0)
    {
        // Reset flip!
        Vector3 rot = transform.eulerAngles;
        rot.z = forcedRotation;
        transform.eulerAngles = rot;
        Vector3 localScale = transform.localScale;
        localScale.y = Mathf.Abs(localScale.y) * ((forcedFlipY) ? -1 : 1);
        localScale.x = Mathf.Abs(localScale.x);
        switch(dir)
        {
            case FacingDirection.Down:
                {
                    rendererRef.sprite = defaults.animConfig.frontAnim[0];
                    // TODO: Proper anim setup!
                    break;
                }

            case FacingDirection.Up:
                {
                    rendererRef.sprite = defaults.animConfig.backAnim[0];
                    break;
                }

            case FacingDirection.Left:
                {
                    rendererRef.sprite = defaults.animConfig.sideAnim[0];
                    if (!defaults.animConfig.IsSideFacingLeft)
                    {
                        localScale.x = -localScale.x;
                    }
                    break;
                }

            case FacingDirection.Right:
                {
                    rendererRef.sprite = defaults.animConfig.sideAnim[0];
                    if (defaults.animConfig.IsSideFacingLeft)
                    {
                        localScale.x = -localScale.x;
                    }
                    break;
                }
        }
        transform.localScale = localScale;
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
        rendererRef.color = selected ? new Color(0.7f,1,1): Color.white;
    }
}
