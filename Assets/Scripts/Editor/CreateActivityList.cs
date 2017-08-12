using UnityEngine;
using UnityEditor;

public class CreateActivityList
{
    [MenuItem("Assets/Crete/TnL/Activity List")]
    public static ActivityDataList Create()
    {
        ActivityDataList l = ScriptableObject.CreateInstance<ActivityDataList>();

        AssetDatabase.CreateAsset(l, "Assets/Data/ActivityDataList.asset");
        AssetDatabase.SaveAssets();
        return l;
    }
}