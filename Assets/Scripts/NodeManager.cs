using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeManager : MonoBehaviour
{
    Dictionary<string, Node> graph = new Dictionary<string, Node>();
    

	// Use this for initialization
	void Start ()
    {
        BuildGraph();
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void BuildGraph()
    {
        Node[] nodes = FindObjectsOfType<Node>();
        int numNodes = nodes.Length;
        for (int i = 0; i < numNodes; ++i)
        {
            graph[nodes[i].name] = nodes[i];
        }

        // Fix adjacency list, just in case:
        foreach(Node node in nodes) // should be fine for arrays, apparently
        {
            for (int i = 0; i < node.connections.Count; ++i)
            {
                Node other = node.connections[i];
                if (other == node) continue; // should never be the case
                if (!other.IsAdjacentTo(node.name))
                {
                    Debug.LogWarningFormat("Adding missing connection between {0} and {1}", node.name, other.name);
                    other.AddAdjacent(node);
                }
            }
        }        
    }

    public void FindPath(string n1, string n2, ref List<Node> path)
    {
        if (!graph.ContainsKey(n1) || !graph.ContainsKey(n2))
        {
            Debug.LogWarningFormat("Find path: Nodes not found");
        }

        path.Clear();

        if (n1 == n2)
        {
            path.Add(graph[n1]);
            return;
        }

        //------ We probably won't need A* here, so let's go for a simple Dijkstra
        int numVertices = graph.Count;
        int[] distances = new int[numVertices];
        string[] parents= new string[numVertices];
        string[] nodeKeys = new string[numVertices];
        List<Node> pending = new List<Node>();

        graph.Keys.CopyTo(nodeKeys, 0);
        for (int i = 0; i < numVertices; ++i)
        {
            if (nodeKeys[i] == n1)
            {
                distances[i] = 0;
            }
            else
            {
                distances[i] = System.Int32.MaxValue;                
            }

            pending.Add(graph[nodeKeys[i]]);
        }

        //while (pending.Count > 0)
        //{
        //    int current = GetSmallestIdx(pending);
        //    Node currentNode = pending[current];
        //    pending.RemoveAt(current);
        //}
        //for (int i = 0; i < current.connections.Count; ++i)
        //{
        //    Node adjacent = current.connections[i];
        //}
        
    }

    //int GetSmallestIdx(int[] distances, List<Node> pending)
    //{

    //}
}
