using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{

    public List<Node> connections;
    public string room;
    public string furnitureKey = "";
    public string[] rooms;
    public Collider2D colliderRef;

    public FacingDirection facing = FacingDirection.None;
    public bool forcedFlipY = false;
    public float forcedZRotation = 0;

    public void Awake()
    {
        colliderRef = GetComponent<Collider2D>();
    }

    public bool IsAdjacentTo(string nodeID)
    {
        return connections.Exists(x => x.name == nodeID);
    }

    public void AddAdjacent(Node connectedNode)
    {
        connections.Add(connectedNode);
    }

    public override string ToString()
    {
        return name;
    }
}
