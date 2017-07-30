using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeManager : MonoBehaviour
{
    class VisitedInfo
    {
        public string refNode;
        public int distance;
        public Node parent;

        public VisitedInfo(string _refNode = "", int _distance = System.Int32.MaxValue, Node _parent = null)
        {
            refNode = _refNode;
            distance = _distance;
            parent = _parent;
        }

        public override string ToString()
        {
            return string.Format("{0} => Parent: {1}, Dist to source: {2}", refNode, distance, parent.name);
        }
    }
    Dictionary<string, Node> graph = new Dictionary<string, Node>();
    

	// Use this for initialization
	void Start ()
    {
        BuildGraph();
        List<Node> testPath = new List<Node>();
        FindPath("ND (6)", "ND (26)", ref testPath);
        Debug.LogFormat("Path len: {0}", testPath.Count);
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
        path.Clear();

        if (!graph.ContainsKey(n1) || !graph.ContainsKey(n2))
        {
            Debug.LogWarningFormat("Find path: Nodes not found");
            return;
        }

        if (n1 == n2)
        {
            path.Add(graph[n1]);
            return;
        }

        //------ We probably won't need A* here, so let's go for a simple Dijkstra
        int numVertices = graph.Count;

        Dictionary<string, VisitedInfo> info = new Dictionary<string, VisitedInfo>();
        PriorityQueue<Node> nodesQueue = new PriorityQueue<Node>();

        foreach (string key in graph.Keys)
        {
            info[key] = new VisitedInfo(key);
            nodesQueue.Enqueue(graph[key], info[key].distance);
        }
        info[n1].distance = 0;
        nodesQueue.UpdateKey(graph[n1], 0);
        
        while (nodesQueue.Count > 0)
        {
            Node current = nodesQueue.Dequeue();
            Debug.LogFormat("Testing {0}", current.name);
            VisitedInfo currentInfo = info[current.name];
            int prevDistance = currentInfo.distance;

            for (int i = 0; i < current.connections.Count; ++i)
            {
                Debug.LogFormat("Testing adjacent node: {0}", current.connections[i].name);
                Node adjacent = current.connections[i];
                VisitedInfo adjacentInfo = info[adjacent.name];
                int distance = prevDistance + 1;
                if (distance < adjacentInfo.distance)
                {
                    Debug.LogFormat("Updating adjacent data {0}: parent {1}, new distance {2}", adjacent.name, current, distance);
                    adjacentInfo.parent = current;
                    adjacentInfo.distance = distance;
                    if (nodesQueue.Count > 0)
                    {
                        Debug.LogFormat("Updating priority for Node {0} to {1}", adjacent.name, distance);
                        nodesQueue.UpdateKey(adjacent, distance);
                    }
                }
            }
            if (nodesQueue.Count > 0)
                Debug.LogFormat("New top: {0}", nodesQueue.Peek().name);
        }

        VisitedInfo destination = info[n2];
        List<Node> rList = new List<Node>();
        rList.Add(graph[n2]);
        while(destination.parent != null)
        {
            rList.Add(destination.parent);
            destination = info[destination.parent.name];
        }
        for (int i = rList.Count - 1; i >= 0; --i)
        {
            Debug.LogFormat("Path: {0}", rList[i]);
            path.Add(rList[i]);
        }
    }

    //int GetSmallestIdx(int[] distances, List<Node> pending)
    //{

    //}
}
