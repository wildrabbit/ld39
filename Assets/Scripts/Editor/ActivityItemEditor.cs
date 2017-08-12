using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ActivityItemEditor : EditorWindow
{
    public ActivityDataList dataList;

    private int viewIdx = 1;

    [MenuItem("Window/TnL/Activity Config Editor %#e")]
    static void Init()
    {
        EditorWindow.GetWindow<ActivityItemEditor>();
    }

    private void OnEnable()
    {
        if (EditorPrefs.HasKey("ObjectPath"))
        {
            string objectPath = EditorPrefs.GetString("ObjectPath");
            dataList = AssetDatabase.LoadAssetAtPath<ActivityDataList>(objectPath);            
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Activity Config Editor", EditorStyles.boldLabel);
        if (dataList != null)
        {
            if (GUILayout.Button("Show activities"))
            {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = dataList;
            }
        }

        if (GUILayout.Button("Open activity list"))
        {
            OpenActivityList();
        }

        if (GUILayout.Button("New activity list"))
        {
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = dataList;
        }

        GUILayout.EndHorizontal();

        if (dataList == null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            if (GUILayout.Button("Create new activity list", GUILayout.ExpandWidth(false)))
            {
                CreateNewActivityList();
            }
            if (GUILayout.Button("Open existing activity list", GUILayout.ExpandWidth(false)))
            {
                OpenActivityList();
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(20);

        if (dataList != null) 
        {
            GUILayout.BeginHorizontal ();
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Prev", GUILayout.ExpandWidth(false))) 
            {
                if (viewIdx> 1)
                    viewIdx--;
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Next", GUILayout.ExpandWidth(false))) 
            {
                if (viewIdx < dataList.activityList.Count) 
                {
                    viewIdx++;
                }
            }
            
            GUILayout.Space(60);
            
            if (GUILayout.Button("Add Item", GUILayout.ExpandWidth(false))) 
            {
                AddActivityConfig();
            }
            if (GUILayout.Button("Delete Item", GUILayout.ExpandWidth(false))) 
            {
                DeleteActivityConfig(viewIdx - 1);
            }
            
            GUILayout.EndHorizontal ();
            if (dataList.activityList == null)
                Debug.Log("wtf");
            if (dataList.activityList.Count > 0) 
            {
                GUILayout.BeginHorizontal ();
                viewIdx = Mathf.Clamp (EditorGUILayout.IntField ("Current Activity", viewIdx, GUILayout.ExpandWidth(false)), 1, dataList.activityList.Count);
                //Mathf.Clamp (viewIndex, 1, inventoryItemList.itemList.Count);
                EditorGUILayout.LabelField ("of   " +  dataList.activityList.Count.ToString() + "  activities", "", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal ();

                ActivityConfig current = dataList.activityList[viewIdx - 1];
                current.charActivity = (CharacterActivity)EditorGUILayout.EnumPopup("Activity type", current.charActivity);
                current.activityDelay = EditorGUILayout.FloatField("Activity delay (mins)", current.activityDelay);
                current.speechEntry = (SpeechEntry)EditorGUILayout.EnumPopup("Speech entry type", current.speechEntry);
                current.oneShot = EditorGUILayout.Toggle("One shot?", current.oneShot);

                GUILayout.Space(10);
                current.primaryNeed = (Needs)EditorGUILayout.EnumPopup("Primary need", current.primaryNeed);
                current.primaryRecoveryRate = EditorGUILayout.FloatField("Primary recovery rate", current.primaryRecoveryRate);
                GUILayout.Space(10);

                // TODO: Editor lists for secondaries and resources
            }
            else 
            {
                GUILayout.Label ("This Inventory List is Empty.");
            }
        }
        if (GUI.changed) 
        {
            EditorUtility.SetDirty(dataList);
        }
    }

    void CreateNewActivityList()
    {
        // No overwrite protection!
        viewIdx = 1;
        dataList = CreateActivityList.Create();
        if (dataList != null)
        {
            dataList.activityList = new List<ActivityConfig>();
            dataList.sideActivities = new List<SideActivityConfig>();
            string relPath = AssetDatabase.GetAssetPath(dataList);
            EditorPrefs.SetString("ObjectPath", relPath);
        }
    }

    void OpenActivityList()
    {
        string absPath = EditorUtility.OpenFilePanel("Select Activity List", "", "");
        if (absPath.StartsWith(Application.dataPath))
        {
            string relPath = absPath.Substring(Application.dataPath.Length - "Assets".Length);
            dataList = AssetDatabase.LoadAssetAtPath< ActivityDataList>(relPath);
            if (dataList.activityList == null)
                dataList.activityList = new List<ActivityConfig>();
            if (dataList)
            {
                EditorPrefs.SetString("ObjectPath", relPath);
            }
        }
    }

    void AddActivityConfig()
    {
        ActivityConfig newActivity = new ActivityConfig();
        newActivity.charActivity = CharacterActivity.Idle;
        dataList.activityList.Add(newActivity);
        viewIdx = dataList.activityList.Count;
    }

    void DeleteActivityConfig(int index)
    {
        dataList.activityList.RemoveAt(index);       
    }
}
