using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using static Dm.net.buffer.ByteArrayBuffer;

namespace _2DSurviveGameServer._01Common
{
    //public class AStarMap
    //{
    //    private bool[,] walkableTiles;

    //    public AStarMap(int width, int height)
    //    {
    //        walkableTiles = new bool[width, height];
    //        for (int i = 0; i < width; i++)
    //        {
    //            for (int j = 0; j < height; j++)
    //            {
    //                walkableTiles[i, j] = true; // 默认为可行走
    //            }
    //        }
    //    }

    //    public bool IsWalkable(int x, int y)
    //    {
    //        return walkableTiles[x, y];
    //    }

    //    public void SetWalkable(int x, int y, bool isWalkable)
    //    {
    //        walkableTiles[x, y] = isWalkable;
    //    }

    //    public int Width => walkableTiles.GetLength(0);
    //    public int Height => walkableTiles.GetLength(1);
    //}
    //public class AStarPathfinder
    //{
    //    private AStarMap map;

    //    public AStarPathfinder(AStarMap map)
    //    {
    //        this.map = map;
    //    }

    //    public List<Vector2> FindPath(Vector2 start, Vector2 end)
    //    {
    //        // 确保起点和终点在地图内
    //        int startX = (int)(start.X + map.Width / 2);
    //        int startY = (int)(start.Y + map.Height / 2);
    //        int endX = (int)(end.X + map.Width / 2);
    //        int endY = (int)(end.Y + map.Height / 2);

    //        this.Log("Starting pathfinding from ({0},{1}) to ({2},{3})", startX, startY, endX, endY);

    //        if (startX < 0 || startX >= map.Width || startY < 0 || startY >= map.Height ||
    //            endX < 0 || endX >= map.Width || endY < 0 || endY >= map.Height)
    //        {
    //            this.Log("Start or End is out of map bounds.");
    //            return new List<Vector2>();
    //        }

    //        Node startNode = new Node(new Vector2(startX, startY)) { GCost = 0, HCost = Heuristic(new Vector2(startX, startY), new Vector2(endX, endY)) };
    //        Node endNode = new Node(new Vector2(endX, endY));

    //        List<Node> openList = new List<Node> { startNode };
    //        HashSet<Node> closedList = new HashSet<Node>();

    //        while (openList.Count > 0)
    //        {
    //            Node currentNode = openList.OrderBy(n => n.FCost).First();
    //            openList.Remove(currentNode);
    //            closedList.Add(currentNode);

    //            this.Log("Current Node: ({0},{1}), FCost: {2}", currentNode.Position.X, currentNode.Position.Y, currentNode.FCost);

    //            if (currentNode.Position.Equals(endNode.Position))
    //            {
    //                this.Log("Path Found");
    //                return RetracePath(startNode, currentNode);
    //            }

    //            foreach (Node neighbor in GetNeighbors(currentNode))
    //            {
    //                int neighborX = (int)(neighbor.Position.X + map.Width / 2);
    //                int neighborY = (int)(neighbor.Position.Y + map.Height / 2);

    //                this.Log("Checking if neighbor ({0},{1}) is walkable.", neighborX, neighborY);

    //                if (!map.IsWalkable(neighborX, neighborY) || closedList.Contains(neighbor))
    //                {
    //                    this.Log("Neighbor ({0},{1}) is not walkable or already in closed list", neighborX, neighborY);
    //                    continue;
    //                }

    //                float newMovementCostToNeighbor = currentNode.GCost + Vector2.Distance(currentNode.Position, neighbor.Position);
    //                if (newMovementCostToNeighbor < neighbor.GCost || !openList.Contains(neighbor))
    //                {
    //                    neighbor.GCost = newMovementCostToNeighbor;
    //                    neighbor.HCost = Heuristic(neighbor.Position, endNode.Position);
    //                    neighbor.Parent = currentNode;

    //                    if (!openList.Contains(neighbor))
    //                    {
    //                        this.Log("Adding Neighbor ({0},{1}) to open list with FCost: {2}", neighbor.Position.X, neighbor.Position.Y, neighbor.FCost);
    //                        openList.Add(neighbor);
    //                    }
    //                }
    //            }
    //        }

    //        this.Log("Path not found");
    //        return new List<Vector2>();
    //    }



    //    private float Heuristic(Vector2 a, Vector2 b)
    //    {
    //        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    //    }

    //    private List<Vector2> RetracePath(Node startNode, Node endNode)
    //    {
    //        List<Vector2> path = new List<Vector2>();
    //        Node currentNode = endNode;

    //        while (currentNode != startNode)
    //        {
    //            path.Add(currentNode.Position);
    //            currentNode = currentNode.Parent;
    //        }
    //        path.Add(startNode.Position); // 添加起点
    //        path.Reverse();
    //        return path;
    //    }

    //    private List<Node> GetNeighbors(Node node)
    //    {
    //        List<Node> neighbors = new List<Node>();
    //        Vector2[] neighborOffsets = new Vector2[]
    //        {
    //        new Vector2(0, 1),
    //        new Vector2(1, 0),
    //        new Vector2(0, -1),
    //        new Vector2(-1, 0),
    //        new Vector2(1, 1),
    //        new Vector2(1, -1),
    //        new Vector2(-1, 1),
    //        new Vector2(-1, -1)
    //        };

    //        foreach (Vector2 offset in neighborOffsets)
    //        {
    //            Vector2 neighborPosition = node.Position + offset;
    //            if (neighborPosition.X >= -map.Width / 2 && neighborPosition.X < map.Width / 2 &&
    //                neighborPosition.Y >= -map.Height / 2 && neighborPosition.Y < map.Height / 2)
    //            {
    //                neighbors.Add(new Node(neighborPosition));
    //            }
    //        }

    //        return neighbors;
    //    }
    //}
    //public class Node
    //{
    //    public Vector2 Position { get; }
    //    public float GCost { get; set; } // 从起点到这个节点的移动代价
    //    public float HCost { get; set; } // 从这个节点到终点的估计代价（启发式函数）
    //    public float FCost => GCost + HCost; // 总代价
    //    public Node Parent { get; set; } // 父节点，用于重建路径

    //    public Node(Vector2 position)
    //    {
    //        Position = position;
    //    }
    //    public override bool Equals(object obj)
    //    {
    //        if (obj == null || !(obj is Node)) return false;
    //        return Position.Equals(((Node)obj).Position);
    //    }

    //    public override int GetHashCode()
    //    {
    //        return Position.GetHashCode();
    //    }
    //}

}
