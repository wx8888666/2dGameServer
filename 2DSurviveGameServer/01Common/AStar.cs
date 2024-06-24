
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;

namespace _2DSurviveGameServer._01Common
{
    public enum NodeType
    {
        Walk,
        Stop,
    }

    public class AStarNode
    {
        public int x;
        public int y;

        public float f;
        public float g;
        public float h;
        public AStarNode father;

        public NodeType type;

        public Vector2 worldPos;

        public AStarNode(int x, int y, NodeType type, Vector2 worldPos)
        {
            this.x = x;
            this.y = y;
            this.type = type;
            this.worldPos = worldPos;
        }
    }

    public class AStarMap
    {
        public float m_NodeRadius;
        public int mapW;
        public int mapH;
        private AStarNode[,] nodes;
        public float m_NodeDiameter;
        private World world;

        public AStarMap(World world, int mapWidth, int mapHeight, float nodeRadius)
        {
            this.world = world;
            this.mapW = mapWidth;
            this.mapH = mapHeight;
            this.m_NodeRadius = nodeRadius;
            this.m_NodeDiameter = nodeRadius * 2;
            this.nodes = new AStarNode[mapW, mapH];

            InitializeNodes();
        }

        private void InitializeNodes()
        {
            for (int i = 0; i < mapW; i++)
            {
                for (int j = 0; j < mapH; j++)
                {
                    Vector2 worldPos = new Vector2(i * m_NodeDiameter, j * m_NodeDiameter);
                    bool canWalk = !IsBlocked(worldPos);
                    nodes[i, j] = new AStarNode(i, j, canWalk ? NodeType.Walk : NodeType.Stop, worldPos);
                }
            }
        }

        private bool IsBlocked(Vector2 position)
        {
            AABB aabb = new AABB(position - new Vector2(m_NodeRadius), position + new Vector2(m_NodeRadius));
            bool blocked = false;

            world.QueryAABB(fixture =>
            {
                blocked = true;
                return false; // Stop the query
            }, ref aabb);

            return blocked;
        }

        public AStarNode GetFromPos(Vector2 pos)
        {
            int x = (int)(pos.X / m_NodeDiameter);
            int y = (int)(pos.Y / m_NodeDiameter);
            if (x < 0 || x >= mapW || y < 0 || y >= mapH)
            {
                return null;
            }
            return nodes[x, y];
        }

        public bool GetFirstPos(Vector2 startPos, Vector2 endPos, out Vector2 target)
        {
            target = Vector2.Zero;
            var path = FindPath(startPos, endPos);
            if (path != null && path.Count > 0)
            {
                target = path[0].worldPos;
                return true;
            }
            return false;
        }

        public List<AStarNode> FindPath(Vector2 startPos, Vector2 endPos)
        {
            List<AStarNode> openList = new List<AStarNode>();
            List<AStarNode> closeList = new List<AStarNode>();

            AStarNode start = GetFromPos(startPos);
            AStarNode end = GetFromPos(endPos);
            if (start == null || end == null || start.type == NodeType.Stop || end.type == NodeType.Stop)
            {
                return null;
            }

            if (startPos == endPos || (start.x == end.x && start.y == end.y))
            {
                return new List<AStarNode> { new AStarNode(start.x, start.y, NodeType.Walk, endPos) };
            }

            start.father = null;
            start.f = 0;
            start.g = 0;
            start.h = 0;
            closeList.Add(start);

            while (true)
            {
                FindNearlyNodeToOpenList(start.x - 1, start.y - 1, 1.4f, start, end, openList, closeList);
                FindNearlyNodeToOpenList(start.x, start.y - 1, 1.4f, start, end, openList, closeList);
                FindNearlyNodeToOpenList(start.x + 1, start.y - 1, 1.4f, start, end, openList, closeList);
                FindNearlyNodeToOpenList(start.x - 1, start.y, 1.4f, start, end, openList, closeList);
                FindNearlyNodeToOpenList(start.x + 1, start.y, 1.4f, start, end, openList, closeList);
                FindNearlyNodeToOpenList(start.x - 1, start.y + 1, 1.4f, start, end, openList, closeList);
                FindNearlyNodeToOpenList(start.x, start.y + 1, 1.4f, start, end, openList, closeList);
                FindNearlyNodeToOpenList(start.x + 1, start.y + 1, 1.4f, start, end, openList, closeList);

                if (openList.Count == 0)
                {
                    return null;
                }

                openList.Sort((a, b) => a.f.CompareTo(b.f));

                closeList.Add(openList[0]);
                start = openList[0];
                openList.RemoveAt(0);

                if (start == end)
                {
                    List<AStarNode> path = new List<AStarNode> { end };
                    while (end.father != null)
                    {
                        end = end.father;
                        path.Add(end);
                    }
                    path.Reverse();
                    path[0].worldPos = startPos;
                    path[path.Count - 1].worldPos = endPos;
                    return path;
                }
            }
        }

        private void FindNearlyNodeToOpenList(int x, int y, float g, AStarNode father, AStarNode end, List<AStarNode> openList, List<AStarNode> closeList)
        {
            if (x < 0 || x >= mapW || y < 0 || y >= mapH)
            {
                return;
            }

            AStarNode node = nodes[x, y];
            if (node == null || node.type == NodeType.Stop || closeList.Contains(node) || openList.Contains(node))
            {
                return;
            }

            node.father = father;
            node.g = father.g + g;
            node.h = Math.Abs(end.x - node.x) + Math.Abs(end.y - node.y);
            node.f = node.g + node.h;

            openList.Add(node);
        }
    }
    }
