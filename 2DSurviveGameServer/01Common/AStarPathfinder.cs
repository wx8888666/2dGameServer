using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2DSurviveGameServer._01Common
{
    public class AStarPathfinder
    {
        private int width, height;
        private Node[,] nodes;

        public AStarPathfinder(int width, int height)
        {
            this.width = width;
            this.height = height;
            nodes = new Node[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    nodes[x, y] = new Node(new Vector2(x, y));
                }
            }
        }

        public List<Vector2> FindPath(Vector2 start, Vector2 end)
        {
            int startX = (int)start.X;
            int startY = (int)start.Y;
            int endX = (int)end.X;
            int endY = (int)end.Y;

            // 边界检查
            if (startX < 0 || startX >= width || startY < 0 || startY >= height ||
                endX < 0 || endX >= width || endY < 0 || endY >= height)
            {
                throw new IndexOutOfRangeException("Start or end position is outside the bounds of the grid.");
            }

            // A* 搜索算法实现 (伪代码)
            List<Vector2> path = new List<Vector2>();

            // 初始化开放列表和封闭列表
            List<Node> openList = new List<Node>();
            HashSet<Node> closedList = new HashSet<Node>();

            Node startNode = nodes[startX, startY];
            Node endNode = nodes[endX, endY];

            openList.Add(startNode);

            while (openList.Count > 0)
            {
                // 找到开放列表中具有最低 F 成本的节点
                Node currentNode = openList[0];
                for (int i = 1; i < openList.Count; i++)
                {
                    if (openList[i].F < currentNode.F ||
                        (openList[i].F == currentNode.F && openList[i].H < currentNode.H))
                    {
                        currentNode = openList[i];
                    }
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                if (currentNode == endNode)
                {
                    // 构建路径
                    while (currentNode != startNode)
                    {
                        path.Add(currentNode.Position);
                        currentNode = currentNode.Parent;
                    }
                    path.Reverse();
                    return path;
                }

                // 遍历当前节点的所有相邻节点
                foreach (Node neighbor in GetNeighbors(currentNode))
                {
                    if (closedList.Contains(neighbor) || !IsWalkable(neighbor.Position))
                    {
                        continue;
                    }

                    int tentativeG = currentNode.G + GetDistance(currentNode, neighbor);
                    if (tentativeG < neighbor.G || !openList.Contains(neighbor))
                    {
                        neighbor.G = tentativeG;
                        neighbor.H = GetDistance(neighbor, endNode);
                        neighbor.Parent = currentNode;

                        if (!openList.Contains(neighbor))
                        {
                            openList.Add(neighbor);
                        }
                    }
                }
            }

            return path; // 没有找到路径
        }

        private IEnumerable<Node> GetNeighbors(Node node)
        {
            List<Node> neighbors = new List<Node>();

            int x = (int)node.Position.X;
            int y = (int)node.Position.Y;

            // 添加相邻的八个方向的节点
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                    {
                        continue;
                    }

                    int nx = x + dx;
                    int ny = y + dy;

                    // 检查边界
                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                    {
                        neighbors.Add(nodes[nx, ny]);
                    }
                }
            }

            return neighbors;
        }

        private bool IsWalkable(Vector2 position)
        {
            // 这里可以添加检查是否可以走的逻辑，例如检查是否是障碍物
            return true;
        }

        private int GetDistance(Node a, Node b)
        {
            int dstX = (int)Math.Abs(a.Position.X - b.Position.X);
            int dstY = (int)Math.Abs(a.Position.Y - b.Position.Y);
            return dstX + dstY;
        }

        private class Node
        {
            public Vector2 Position { get; }
            public int G { get; set; }
            public int H { get; set; }
            public int F => G + H;
            public Node Parent { get; set; }

            public Node(Vector2 position)
            {
                Position = position;
                G = int.MaxValue;
                H = 0;
                Parent = null;
            }
        }
    }
}
