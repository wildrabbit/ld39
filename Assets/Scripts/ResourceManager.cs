using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public List<ResourceData> resourceConfig = new List<ResourceData>();

    Dictionary<string, int> resources = new Dictionary<string, int>();

	// Use this for initialization
	void Start ()
    {
        resources.Clear();
        for (int i = 0; i < resourceConfig.Count; ++i)
        {
            ResourceData config = resourceConfig[i];
            resources[config.name] = config.initialAmount;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void UseResource(string resource)
    {
        // Notify
    }    
}
