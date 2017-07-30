using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomTouchMapping
{
    public string roomName;
    public Collider2D collider;
}

public class RoomTouchManager : MonoBehaviour
{
    [Header("Manager")]
    public RoomManager roomManager;

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
            if (hitInfo.collider != null)
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
