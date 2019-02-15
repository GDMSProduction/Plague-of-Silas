using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlannerNode
{
    public PlannerNode parent;
    public SearchNode searchNode;

    public float heuristicCost;
    public float givenCost;
    public float finalCost;
}

public class SearchNode
{
    public Transform transform;

    public List<SearchNode> neighbors;

    public SearchNode(Transform _position)
    {
        transform = _position;
        neighbors = new List<SearchNode>();
    }

}

public class PriorityQueue<T>
{
    List<T> queue;
    delegate bool CompareItems(T a, T b);

    CompareItems IsGreater;

    public PriorityQueue(Func<T, T, bool> function)
    {
        queue = new List<T>();
        IsGreater = new CompareItems(function);
    }

    /// <summary>
    /// Adds new element to the queue using the Compare function.
    /// </summary>s
    public void Add(T newItem)
    {
        if (isEmpty())
        {
            queue.Add(newItem);
        }
        else
        {
            for (int i = 0; i < queue.Count; i++)
            {
                if (!IsGreater(newItem, queue[i]))
                {
                    queue.Insert(i, newItem);
                    return;
                }
            }

            queue.Add(newItem);
        }
        
    }

    /// <summary>
    /// Gets next element in the queue and removes it.
    /// </summary>s
    public T GetHead()
    {
        if (!isEmpty())
        {
            T result = queue[0];

            return result;
        }
        else
            throw new SystemException("Queue is empty");
    }

    public void Remove(T element)
    {
        queue.Remove(element);
    }

    public bool isEmpty()
    {
        return queue.Count == 0;
    }
}



public class OwlNavigation : MonoBehaviour
{
    PriorityQueue<PlannerNode> queue;

    List<SearchNode> nodes;

    public List<Transform> pathToTake;

    //List for the owl to choose 
    public List<SearchNode> mainNodes;

    [SerializeField] GameObject waypointRef;

    public List<GameObject> debugWaypoints;

    void Start()
    {
        queue = new PriorityQueue<PlannerNode>(Compare);
        nodes = new List<SearchNode>();
        mainNodes = new List<SearchNode>();

        //Get nodes 
        foreach (Transform child in transform)
        {
            if (child.CompareTag("PatrolPoint"))
            {
                SearchNode newNode = new SearchNode(child);

                NodeScript nodeData = child.GetComponent<NodeScript>();

                foreach(Transform t in nodeData.neighbors)
                {
                    SearchNode neighbor = new SearchNode(t);
                    newNode.neighbors.Add(neighbor);
                }

                nodes.Add(newNode);
            }
        }

        foreach (Transform child in transform.Find("MainNodes"))
        {
            if(child.CompareTag("PatrolPoint"))
            {
                SearchNode newNode = new SearchNode(child);

                NodeScript nodeData = child.GetComponent<NodeScript>();

                foreach (Transform t in nodeData.neighbors)
                {
                    SearchNode neighbor = new SearchNode(t);
                    newNode.neighbors.Add(neighbor);
                }

                nodes.Add(newNode);
                mainNodes.Add(newNode);
            }
        }
    }


    public bool Compare(PlannerNode a, PlannerNode b)
    {
        return a.finalCost > b.finalCost;
    }
   
    /// <summary>
    /// If return = false, then there's no valid path to desiredPosition
    /// </summary>
    public bool BeginSearch(Transform currentPosition, SearchNode desiredNode)
    {
        pathToTake.Clear();

        int i = 0;
        for (; i < nodes.Count; i++)
        {
            if (nodes[i].transform == currentPosition)
                break;
        }

        GetPathTo(nodes[i], desiredNode);

        return pathToTake.Count > 0;
    }


    /// <summary>
    /// Populates the pathToTake list.
    /// </summary>
    void GetPathTo(SearchNode currentNode, SearchNode desiredNode)
    {
        List<SearchNode> result = new List<SearchNode>();

        Dictionary<Transform, PlannerNode> visited = new Dictionary<Transform, PlannerNode>();

        PlannerNode head = new PlannerNode();
        head.parent = null;
        head.searchNode = nodes[FindNodeIndex(currentNode)];
        head.givenCost = 0;
        head.heuristicCost = head.finalCost = Vector3.Distance(currentNode.transform.position, desiredNode.transform.position);

        visited.Add(head.searchNode.transform, head);
        queue.Add(head);

        while(!queue.isEmpty())
        {
            PlannerNode curNode = queue.GetHead();

            if(curNode.searchNode == desiredNode)
            {
                //Build path
                pathToTake = new List<Transform>();

                PlannerNode it = curNode;

                while(it.parent != null) 
                {
                    pathToTake.Add(it.searchNode.transform);
                    GameObject marker = Instantiate(waypointRef);
                    marker.transform.position = it.searchNode.transform.position;

                    debugWaypoints.Add(marker);
                    it = it.parent;
                }

                return;
            }

            foreach(SearchNode n in curNode.searchNode.neighbors)
            {
                float tempGivenCost = curNode.givenCost + Vector3.Distance(curNode.searchNode.transform.position, n.transform.position);

                if (visited.ContainsKey(n.transform))
                {
                    PlannerNode visitedRef = visited[n.transform];

                    if (tempGivenCost < visitedRef.givenCost)
                    {
                        queue.Remove(visitedRef);
                        visitedRef.givenCost = tempGivenCost;
                        visitedRef.finalCost = visitedRef.heuristicCost + visitedRef.givenCost;
                        visitedRef.parent = curNode;

                        queue.Add(visitedRef);
                    }
                }
                else
                {
                    PlannerNode newNode = new PlannerNode();
                    newNode.searchNode = nodes[FindNodeIndex(n)];
                    newNode.givenCost = tempGivenCost;
                    newNode.heuristicCost = Vector3.Distance(n.transform.position, desiredNode.transform.position);
                    newNode.finalCost = newNode.givenCost + newNode.heuristicCost;
                    newNode.parent = curNode;

                    visited.Add(n.transform, newNode);
                    queue.Add(newNode);
                }

            }

            queue.Remove(curNode);
        }

    }

    int FindNodeIndex(SearchNode nodeToFind)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].transform == nodeToFind.transform)
                return i;
        }

        //Error
        return -1;
    }
}
