using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuxFurniture : MonoBehaviour
{
    public List<Furniture> enabledDependentFurnitures;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetDependentItemEnabled(Furniture furniture, bool value)
    {
        if (value)
        {
            if (!enabledDependentFurnitures.Contains(furniture))
            {
                enabledDependentFurnitures.Add(furniture);
            }
            if (!gameObject.activeInHierarchy)
                gameObject.SetActive(true);
        }
        else
        {
            if (enabledDependentFurnitures.Contains(furniture))
            {
                enabledDependentFurnitures.Remove(furniture);
            }
            if (gameObject.activeInHierarchy && enabledDependentFurnitures.Count == 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
