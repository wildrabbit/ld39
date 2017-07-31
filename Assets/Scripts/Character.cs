using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public enum StatAlertType
    {
        None,
        Warning,
        Critical,
        Zero
    }
    public const float kWarningRatio = 0.4f;
    public const float kCriticalRatio = 0.15f;

    public const float kHealthWarningSickProbability = 0.001f;
    public const float kHealthCriticalSickProbability = 0.005f;
    public const float kMoodWarningSickProbability = 0.001f;
    public const float kMoodCriticalSickProbability = 0.005f;

    public const float kDefaultActivityWeight = 10f;
    public const float kDistanceThreshold = 0.05f;

    NodeManager nodeManagerRef = null;
    CharacterManager characterManagerRef = null;
    TimeManager timeManagerRef = null;

    CharacterConfig defaults;
    float speed;

    public string charName;

    CharacterActivity currentActivity = CharacterActivity.Idle;
    CharacterStatus currentStatus = CharacterStatus.None;

    Dictionary<Needs, float> needs = new Dictionary<Needs, float>();
    Dictionary<Skills, float> skills = new Dictionary<Skills, float>();
    Dictionary<CharacterActivity, float> preferences = new Dictionary<CharacterActivity, float>();

    Node currentNode = null;
    Node temp = null;
    List<Node> path = new List<Node>();
    int pathNextIdx = -1;
    
    public string currentRoom = "";

    public float health = 0; // Calculated field
    public float mood = 0;

    SpriteRenderer rendererRef;
    Collider2D colliderRef;

    public bool StatsUpdating
    {
        get { return (currentStatus & CharacterStatus.Dead) == CharacterStatus.None; }
    }

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
        // TODO: Check game end.
        if (StatsUpdating)
        {
            UpdateNeeds();
        }

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
                            characterManagerRef.CharacterArrivedToNode(this, currentNode);
                        }
                        else pathNextIdx++;
                    }
                }
                break;
            }
            default:break;
        }

        // Finally, determine mood health and events.
        // Both are weighted averages.
        EvaluateStats();
	}

    void EvaluateStats()
    {
        int numNeeds = (int)Needs.Count;

        StatAlertType[] needAlerts = new StatAlertType[numNeeds];
        for (int i = 0; i < needAlerts.Length; ++i)
        {
            needAlerts[i] = StatAlertType.None;
        }

        StatAlertType healthAlert = StatAlertType.None;
        StatAlertType moodAlert = StatAlertType.None;

        float totalMoodWeight = 0;
        float totalHealthWeight = 0;
        for (int i = 0; i < (int)numNeeds; ++i)
        {
            totalMoodWeight += defaults.moodWeight[i];
            totalHealthWeight += defaults.healthWeight[i];
        }

        mood = 0;
        health = 0;

        for (int i = 0; i < (int)numNeeds; ++i)
        {
            Needs n = (Needs)i;
            float needRatio = needs[n] / defaults.initialNeedValue;
            mood += defaults.moodWeight[i] * needRatio;
            health += defaults.healthWeight[i] * needRatio;

            if (needRatio < kWarningRatio)
            {
                needAlerts[i] = StatAlertType.Warning;
                if (needRatio < kCriticalRatio)
                {
                    needAlerts[i] = Mathf.Approximately(needRatio, 0.0f) ? StatAlertType.Zero : StatAlertType.Critical;                    
                }
            }
        }
        health /= totalHealthWeight;
        mood /= totalMoodWeight;

        if (Mathf.Approximately(health, 0))
        {
            currentStatus = CharacterStatus.Dead;
        }
        else if (Mathf.Approximately(mood, 0))
        {
            currentStatus = CharacterStatus.Breakdown;
        }
        else
        {
            EvaluateStat(health, CharacterStatus.Sick
                , kWarningRatio, kHealthWarningSickProbability
                , kCriticalRatio, kHealthCriticalSickProbability
                , ref currentStatus, ref healthAlert);

            EvaluateStat(mood, CharacterStatus.Breakdown
                , kWarningRatio, kMoodWarningSickProbability
                , kCriticalRatio, kMoodCriticalSickProbability
                , ref currentStatus, ref moodAlert);
        }

        if (healthAlert != StatAlertType.None)
        {
            Debug.LogWarningFormat("{0}!, health ratio: {1}", healthAlert, health);
        }

        if (moodAlert != StatAlertType.None)
        {
            Debug.LogWarningFormat("{0}!, mood ratio: {1}", moodAlert, mood);
        }

        List<int> relevantAlerts = new List<int>();
        for (int i = 0; i < needAlerts.Length; ++i)
        {
            if (needAlerts[i] != StatAlertType.None)
            {
                relevantAlerts.Add(i);
            }
        }

        if (relevantAlerts.Count > 0)
        {
            int idx = relevantAlerts[Random.Range(0, relevantAlerts.Count - 1)];
            Needs n = (Needs)idx;
            Debug.LogWarningFormat("Need {0}, ratio: {1}!", n, needs[n] / defaults.initialNeedValue);
        }
    }

    public void EvaluateStat(float ratio, CharacterStatus inflictedStatus
        , float warning, float statusWarningProbability
        , float critical, float statusCritical
        , ref CharacterStatus outStatus, ref StatAlertType alertType)
    {
        float statusChance = 0;
        if (ratio < warning)
        {
            statusChance = statusWarningProbability;
            alertType = StatAlertType.Warning;
            if (ratio < critical)
            {
                statusChance = statusCritical;
                alertType = (Mathf.Approximately(ratio, 0.0f) ? StatAlertType.Zero : StatAlertType.Critical);
            }
        }
        if (statusChance > 0.0f)
        {
            float roll = UnityEngine.Random.value;
            if (roll < statusChance)
            {
                outStatus |= inflictedStatus;
            }
        }
    }

    void UpdateNeeds()
    {
        int numNeeds = (int)Needs.Count;

        for (int i = 0; i < numNeeds; ++i)
        {
            Needs curNeed = (Needs)i;
            //float factor = GetNeedModifierFactor(curNeed);
            float amount = defaults.initialNeedValue * (timeManagerRef.ScaledDelta / (defaults.defaultNeedDepletionTimeHours[i] * 3600));
            amount *= GetNeedModifierFactor(curNeed);
            needs[curNeed] = Mathf.Clamp(needs[curNeed] - amount, 0.0f, defaults.initialNeedValue); // Don't raise anything yet. Maybe activities have changed this.
        }
    }

    float GetNeedModifierFactor(Needs need)
    {
        float factor = 1.0f;
        switch (need)
        {
            case Needs.Food:
                {
                    if (currentActivity == CharacterActivity.Eating)
                    {
                        factor = 0.0f;
                    }
                    break;
                }
            case Needs.Drink:
                {
                    if (currentActivity == CharacterActivity.Drinking)
                    {
                        factor = 0.0f;
                    }
                    break;
                }
            case Needs.Toilet:
                {
                    if (currentActivity == CharacterActivity.WC)
                    {
                        factor = 0.0f;
                    }
                    break;
                }
            case Needs.Sleep:
                {
                    if (currentActivity == CharacterActivity.Sleep)
                    {
                        factor = 0.0f;
                    }
                    break;
                }
            case Needs.Temperature:
                {
                    break; // Check inventory, or global temperature.
                }
            case Needs.Hygiene:
                {
                    if (currentActivity == CharacterActivity.Bath)
                    {
                        factor = 0.0f;
                    }
                    break;
                }
            case Needs.Entertainment:
                {
                    if (currentActivity == CharacterActivity.Computer || currentActivity == CharacterActivity.TV 
                        || currentActivity == CharacterActivity.Read || currentActivity == CharacterActivity.Dancing)
                    {
                        factor = 0.0f;
                    }
                    break;
                }
            default:break;
        }
        return factor;
    }

    public void StartGame()
    {
        currentNode = defaults.startNode;
        currentRoom = defaults.startNode.room;
        transform.position = defaults.startNode.transform.position;

        SetDirection(currentNode.facing == FacingDirection.None ? FacingDirection.Down : currentNode.facing, currentNode.forcedFlipY, currentNode.forcedZRotation);
    }

    public void SetDependencies(CharacterManager charManager, NodeManager nodeManager, TimeManager timeManager)
    {
        characterManagerRef = charManager;
        nodeManagerRef = nodeManager;
        timeManagerRef = timeManager;
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
            needs[curNeed] = 100;
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

        if (currentStatus != CharacterStatus.Breakdown && currentStatus != CharacterStatus.Dead)
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
            if (!string.IsNullOrEmpty(currentNode.furnitureKey))
            {
                characterManagerRef.CharacterLeftNode(this, currentNode);
            }
        }
    }

    public void SetSelected(bool selected)
    {
        rendererRef.sortingLayerName = selected ? characterManagerRef.layerSelected
            : (characterManagerRef.furnitureManager.IsCharacterUsingFurniture(this))
                ? characterManagerRef.layerOn
                : characterManagerRef.GetLayerForRoom(currentRoom);
        rendererRef.color = selected ? new Color(0.7f,1,1): Color.white;
    }
}
