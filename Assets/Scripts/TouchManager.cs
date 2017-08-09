using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomTouchMapping
{
    public string roomName;
    public Collider2D collider;
}

public class TouchManager : MonoBehaviour, IGameplaySystem
{
    [Header("Config")]
    public RoomTouchMapping[] mappings;

    public Camera camRef;
    GameplayManager gameplayManager;

    public void Initialise(GameplayManager manager)
    {
        gameplayManager = manager;
    }

    public void StartGame ()
    {}
	
	// Update is called once per frame
	public void UpdateSystem (float dt)
    {
		if (Input.GetMouseButtonDown(0))
        {
            Ray ray = camRef.ScreenPointToRay(Input.mousePosition);
            LayerMask mask = LayerMask.NameToLayer("Raindrops");
            RaycastHit2D hitInfo = Physics2D.GetRayIntersection(ray, Mathf.Infinity, ~(1 << mask));

            RoomTouchMapping touchMapping = null;
            Collider2D hitCollider = hitInfo.collider;
            if (hitCollider == null) return;
            Character chara = hitCollider.GetComponent<Character>();
            // Check for characters first:
            if (chara != null)
            {
                gameplayManager.characterManager.ToggleCharacterSelection(chara);
            }
            else
            {
                Node node = hitCollider.GetComponent<Node>();
                if (node != null && node.furnitureKey != "")
                {
                    Character selected = gameplayManager.characterManager.GetSelected();
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
                        gameplayManager.roomManager.ToggleRoomLights(touchMapping.roomName);
                    }
                }
            }
        
        }
	}

    public void PauseGame(bool value)
    {
    }

    public void GameFinished(GameResult result)
    {
    }
}
