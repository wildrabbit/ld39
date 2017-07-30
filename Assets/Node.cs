using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{

    public List<Node> connections;
    public string room;
    public string furnitureKey = "";
    public string[] rooms;

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
