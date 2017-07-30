using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomTouchMapping
{
    public string roomName;
    public Collider2D collider;
}

public class TouchManager : MonoBehaviour
{
    [Header("Manager")]
    public RoomManager roomManager;
    public CharacterManager characterManager;

    [Header("Config")]
    public RoomTouchMapping[] mappings;

    public Camera camRef;

    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		if (Input.GetMouseButtonDown(0))
        {
            Ray ray = camRef.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hitInfo = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

            RoomTouchMapping touchMapping = null;
            Collider2D hitCollider = hitInfo.collider;
            if (hitCollider == null) return;
            Character chara = hitCollider.GetComponent<Character>();
            // Check for characters first:
            if (chara != null)
            {
                characterManager.ToggleCharacterSelection(chara);
            }
            else
            {
                Node node = hitCollider.GetComponent<Node>();
                if (node != null && node.furnitureKey != "")
                {
                    Character selected = characterManager.GetSelected();
                    if (selected != null)
                    {
                        selected.SetTarget(node);
                    }
                }
                else
                {
                    touchMapping = System.Array.Find(mappings, (m) => m.collider == hitInfo.collider);
                    if (touchMapping != null)
                    {
                        roomManager.ToggleRoomLights(touchMapping.roomName);
                    }
                }
            }
        
        }
	}
}
