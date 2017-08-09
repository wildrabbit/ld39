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
    // Take this sh*t out of here
    public const float kWarningRatio = 0.3f;
    public const float kCriticalRatio = 0.1f;

    public const float kHealthWarningSickProbability = 0.05f;
    public const float kHealthCriticalSickProbability = 0.07f;
    public const float kMoodWarningBreakdownProbability = 0.05f;
    public const float kMoodCriticalBreakdownProbability = 0.08f;

    public const float kStatusCheckDelay = 3.0f;

    public const float kDefaultActivityWeight = 10f;
    public const float kDistanceThreshold = 0.05f;

    public readonly CharacterActivity[] entertActivities = new CharacterActivity[] { CharacterActivity.Computer, CharacterActivity.TV, CharacterActivity.Read, CharacterActivity.Dancing };
    public readonly float[] entertMinutes = new float[] { 30.0f, 60.0f, 120.0f, 15.0f };
    public readonly float[] entertRates = new float[] { 0.2f, 0.15f, 0.1f, 0.25f };

    // Dependencies: 
    GameplayManager gameplayManager;

    float activityElapsed = -1.0f;
    float statusCheckElapsed = -1.0f;

    [HideInInspector]
    public CharacterConfig defaults;
    float speed;

    public string charName;

    public CharacterActivity currentActivity = CharacterActivity.Idle;
    public CharacterStatus currentStatus = CharacterStatus.None;

    Dictionary<Needs, float> needs = new Dictionary<Needs, float>();
    Dictionary<Skills, float> skills = new Dictionary<Skills, float>();
    Dictionary<CharacterActivity, float> preferences = new Dictionary<CharacterActivity, float>();

    public Node currentNode = null;
    Node temp = null;
    List<Node> path = new List<Node>();
    int pathNextIdx = -1;
    
    public string currentRoom = "";

    public float health = 0; // Calculated field
    public float mood = 0;

    public GameObject selectionItem;
    public GameObject speechObject;
    public SpriteRenderer speechIcon;

    Transform speechRoot;
    Vector3 offset;


    public bool Dead
    {
        get
        {
            return currentStatus == CharacterStatus.Dead;
        }
    }

    public bool Breakdown
    {
        get
        {
            return (currentStatus & CharacterStatus.Breakdown) != CharacterStatus.None;
        }
    }

    public bool Sick
    {
        get
        {
            return (currentStatus & CharacterStatus.Sick) != CharacterStatus.None;
        }
    }

    SpriteRenderer rendererRef;
    Collider2D colliderRef;

    public bool StatsUpdating
    {
        get { return (currentStatus & CharacterStatus.Dead) == CharacterStatus.None; }
    }

    void Awake()
    {
        // Internal dependencies.
        rendererRef = GetComponent<SpriteRenderer>();
        colliderRef = GetComponent<Collider2D>();

        speechRoot = GameObject.Find("SpeechRoot").transform;
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
        if (Dead)
        {
            return;
        }

        if (selectionItem.activeInHierarchy)
        {
            selectionItem.transform.Rotate(Vector3.forward, 120 * Time.deltaTime);
        }

        if (StatsUpdating)
        {
            UpdateNeeds();
        }

        TimeManager timeManagerRef = gameplayManager.timeManager;

        float delta = Time.deltaTime; // We might also need the scaled value?
        switch(currentActivity)
        {
            case CharacterActivity.Idle:
            {
                UpdateIdle(delta);
                break;
            }
            case CharacterActivity.Moving:
            {
                UpdateMotion(delta);                
                break;
            }
            case CharacterActivity.Sleep:
            {
                ApplyActivity(CharacterActivity.Sleep, delta, timeManagerRef.ScaledDelta, 30, 0.2f, Needs.Sleep, SpeechEntry.FocusSleep);                
                break;               
            }
            case CharacterActivity.Eating:
            {
                ApplyActivity(CharacterActivity.Eating, delta, timeManagerRef.ScaledDelta, 60, 0.5f, Needs.Food, SpeechEntry.FocusEat, true);
                break;
            }
            case CharacterActivity.Drinking:
            {
                ApplyActivity(CharacterActivity.Drinking, delta, timeManagerRef.ScaledDelta, 30, 1.0f, Needs.Water, SpeechEntry.FocusDrink, true);
                break;
            }
            case CharacterActivity.Bath:
            {
                ApplyActivity(CharacterActivity.Bath, delta, timeManagerRef.ScaledDelta, 15, 0.1f, Needs.Hygiene, SpeechEntry.FocusBath);
                break;
            }
            case CharacterActivity.WC:
            {
                ApplyActivity(CharacterActivity.WC, delta, timeManagerRef.ScaledDelta, 10, 1.0f, Needs.Toilet, SpeechEntry.FocusToilet, true);
                break;
            }
            case CharacterActivity.Computer:
            case CharacterActivity.TV:
            case CharacterActivity.Read:
            case CharacterActivity.Dancing:
            {
                int actIdx = System.Array.IndexOf(entertActivities, currentActivity);
                if (actIdx > 0)
                {
                    ApplyActivity(currentActivity, delta, timeManagerRef.ScaledDelta, entertMinutes[actIdx], entertRates[actIdx], Needs.Entertainment, SpeechEntry.FocusEntertainment);
                }
                break;
            }
            default:break;
        }

        // Finally, determine mood health and events.
        // Both are weighted averages.
        EvaluateStats();
	}

    void UpdateIdle(float delta)
    {
        gameplayManager.characterManager.CharacterArrivedToNode(this, currentNode);        
    }

    void UpdateMotion(float delta)
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
                gameplayManager.roomManager.SwitchLights(currentRoom, true, true);
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
                    gameplayManager.characterManager.CharacterArrivedToNode(this, currentNode);
                }
                else pathNextIdx++;
            }
        }
    }

    void ApplyActivity(CharacterActivity act, float delta, float worldDelta, float recoverMinutes, float percent, Needs primary, SpeechEntry speech, bool singleUsage = false, Needs[] secNeeds = null, float[] secRates = null, string[] resTypes = null, int[] resAmounts = null)
    {
        activityElapsed += worldDelta;
        if (activityElapsed >= recoverMinutes * 60)
        {
            // Yay recover!
            float needRecovered = percent * defaults.initialNeedValue;
            needs[primary] = Mathf.Clamp(needs[primary] + needRecovered, 0, defaults.initialNeedValue);
            Talk(speech);

            if (secNeeds != null && secRates != null)
            {
                for (int i = 0; i < secNeeds.Length; ++i)
                {
                    float secDelta = secRates[i] * defaults.initialNeedValue;
                    needs[secNeeds[i]] = Mathf.Clamp(needs[primary] + secDelta, 0, defaults.initialNeedValue);
                }
            }

            if (resTypes != null && resAmounts != null)
            {
                // TODO: Resources!
            }

            if (singleUsage || Mathf.Approximately(needs[primary], defaults.initialNeedValue))
            {
                // Change to idle and disable furniture
                currentActivity = CharacterActivity.Idle;
                gameplayManager.characterManager.CharacterLeftNode(this, currentNode);
            }
            else
            {
                activityElapsed = 0.0f;
            }
        }
    }
    void EvaluateStats()
    {
        statusCheckElapsed += Time.deltaTime;

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
        health = Mathf.Clamp01(health / totalHealthWeight);
        if (health < 0.01f) health = 0;
        mood = Mathf.Clamp01(mood / totalMoodWeight);
        if (mood < 0.01f) mood = 0;

        if (Mathf.Approximately(health, 0))
        {
            currentStatus = CharacterStatus.Dead;
            Talk(SpeechEntry.Dead);
        }
        else if (Mathf.Approximately(mood, 0))
        {
            currentStatus = CharacterStatus.Breakdown;
        }
        else
        {
            if (statusCheckElapsed >= kStatusCheckDelay)
            {
                statusCheckElapsed = 0;
                EvaluateStat(health, CharacterStatus.Sick
                    , kWarningRatio, kHealthWarningSickProbability
                    , kCriticalRatio, kHealthCriticalSickProbability
                    , SpeechEntry.Sick
                    , ref currentStatus, ref healthAlert);

                EvaluateStat(mood, CharacterStatus.Breakdown
                    , kWarningRatio, kMoodWarningBreakdownProbability
                    , kCriticalRatio, kMoodCriticalBreakdownProbability
                    , SpeechEntry.Breakdown
                    , ref currentStatus, ref moodAlert);
            }
        }

        if (healthAlert != StatAlertType.None && healthAlert != StatAlertType.Zero)
        {
            Talk(SpeechEntry.FocusHealth);
            //Debug.LogWarningFormat("{0}!, health ratio: {1}", healthAlert, health);
        }

        if (moodAlert != StatAlertType.None)
        {
            //Debug.LogWarningFormat("{0}!, mood ratio: {1}", moodAlert, mood);
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
            SpeechEntry e = SpeechEntry.None;
            switch(n)
            {
                case Needs.Food:
                    {
                        e = SpeechEntry.FocusHungry;
                        break;
                    }
                case Needs.Water:
                    {
                        e = SpeechEntry.FocusThirsty;
                        break;
                    }
                case Needs.Toilet:
                    {
                        e = SpeechEntry.FocusNeedsToilet;
                        break;
                    }
                case Needs.Hygiene:
                    {
                        e = SpeechEntry.FocusFilthy;
                        break;
                    }
                case Needs.Sleep:
                    {
                        e = SpeechEntry.FocusSleepy;
                        break;
                    }
                case Needs.Entertainment:
                    {
                        e = SpeechEntry.FocusBored;
                        break;
                    }
                default:break;
            }
            if (e != SpeechEntry.None && Random.value < 0.000001f)
            {
                Talk(e);
            }
        }
    }

    public void EvaluateStat(float ratio, CharacterStatus inflictedStatus
        , float warning, float statusWarningProbability
        , float critical, float statusCritical
        , SpeechEntry statusSpeech
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
            if (roll < statusChance && ((outStatus & inflictedStatus) == CharacterStatus.None))
            {
                outStatus |= inflictedStatus;
                Talk(statusSpeech);
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
            float amount = defaults.initialNeedValue * (gameplayManager.timeManager.ScaledDelta / (defaults.defaultNeedDepletionTimeHours[i] * 3600));
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
            case Needs.Water:
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
        statusCheckElapsed = 0;
        currentNode = defaults.startNode;
        currentRoom = defaults.startNode.room;
        transform.position = defaults.startNode.transform.position;

        offset = speechObject.transform.localPosition;
        speechObject.transform.parent = speechRoot;

        SetDirection(currentNode.facing == FacingDirection.None ? FacingDirection.Down : currentNode.facing, currentNode.forcedFlipY, currentNode.forcedZRotation);
        EvaluateStats();
    }

    public void InitFromConfig(GameplayManager _gameplayManager, CharacterConfig cfg, Transform characterRoot, Node node, string roomName)
    {
        gameplayManager = _gameplayManager;
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

        switch (dir)
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
        CharacterManager characterManager = gameplayManager.characterManager;
        FurnitureManager furnitureManager = gameplayManager.furnitureManager;
        NodeManager nodeManager = gameplayManager.nodeManager;
        if (target == currentNode) return;

        if (characterManager.ExistsCharacterAtNode(target))
        {
            Talk(SpeechEntry.Taken);
            return;
        }

        CharacterActivityCheckResult result = CanCharacterEngageInActivity(furnitureManager.GetActivity(target.furnitureKey), null);
        if (result == CharacterActivityCheckResult.Forbidden)
        {
            Talk(SpeechEntry.Restricted);
            return;
        }
        
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
            nodeManager.FindPath(currentNode.name, target.name, ref path);
            pathNextIdx = 0;
            currentActivity = CharacterActivity.Moving;
            if (!string.IsNullOrEmpty(currentNode.furnitureKey))
            {
                characterManager.CharacterLeftNode(this, currentNode);
            }
        }
    }

    public void SetSelected(bool selected)
    {
        CharacterManager characterManager = gameplayManager.characterManager;
        rendererRef.sortingLayerName = selected ? characterManager.layerSelected
            : (gameplayManager.furnitureManager.IsCharacterUsingFurniture(this))
                ? characterManager.layerOn
                : characterManager.GetLayerForRoom(currentRoom);
        selectionItem.SetActive(selected);
    }

    public void SetCurrentActivity(CharacterActivity activity)
    {
        SetCurrentActivity(activity, null);
    }

    public CharacterActivityCheckResult CanCharacterEngageInActivity(CharacterActivity activity, ActivityContext context)
    {
        RoomManager roomManager = gameplayManager.roomManager;
        if (Dead) return CharacterActivityCheckResult.Dead; // Obviously...
        if (Breakdown && activity != CharacterActivity.Idle && activity != CharacterActivity.Moving) return CharacterActivityCheckResult.Breakdown;
        switch(activity)
        {
            case CharacterActivity.Sleep:
            {
                if (needs[Needs.Sleep] >= defaults.initialNeedValue * 0.95f)
                {
                    return CharacterActivityCheckResult.NeedSatisfied;
                }
                else if (roomManager.IsRoomLit(currentRoom))
                {
                        return CharacterActivityCheckResult.RoomIsLit;
                }
                break;
            }
            case CharacterActivity.Eating:
            {
                if (needs[Needs.Food] >= defaults.initialNeedValue * 0.95f)
                {
                    return CharacterActivityCheckResult.NeedSatisfied;
                }
                break;
            }
            case CharacterActivity.Drinking:
            {
                if (needs[Needs.Water] >= defaults.initialNeedValue * 0.95f)
                {
                        return CharacterActivityCheckResult.NeedSatisfied;
                }
                break;
            }
            case CharacterActivity.Bath:
            {
                if (needs[Needs.Hygiene] / defaults.initialNeedValue >= 0.7f)
                    return CharacterActivityCheckResult.NeedSatisfied;
                break;
            }
            case CharacterActivity.Computer:
            case CharacterActivity.TV:
            case CharacterActivity.Dancing:
            case CharacterActivity.Read:
            {
                if (Sick) return CharacterActivityCheckResult.Sick;
                break;
            }
            case CharacterActivity.WC:
            {
                if (needs[Needs.Toilet] / defaults.initialNeedValue >= 0.55f)
                {
                    return CharacterActivityCheckResult.NeedSatisfied;
                }
                break;
            }
            case CharacterActivity.Repairing:
            case CharacterActivity.Operating:
            case CharacterActivity.Cooking:
            {
                if (Sick) return CharacterActivityCheckResult.Sick;
                else if (defaults.kid)
                {
                    Talk(SpeechEntry.Restricted);
                    return CharacterActivityCheckResult.Forbidden;
                }
                break;
            }
            default: break;
        }
        return CharacterActivityCheckResult.Success;
    }

    public void SetCurrentActivity(CharacterActivity activity, ActivityContext context)
    {
        currentActivity = activity;
        activityElapsed = 0;
    }

    public void Talk(SpeechEntry speech)
    {
        if (speechObject.activeInHierarchy) return;
        StartCoroutine(ShowSpeech(gameplayManager.characterManager.GetSpeechIcon(speech)));
    }

    IEnumerator ShowSpeech(Sprite icon)
    {
        if (icon != null)
        {
            speechObject.SetActive(true);
            Vector3 off = offset;
            if (transform.localScale.y < 0)
            {
                off.y = 0.0f;
            }
            speechObject.transform.position = transform.position + off;
            speechIcon.sprite = icon;
            yield return new WaitForSeconds(0.8f);
            speechObject.SetActive(false);
        }
        yield return null;
    }
}
