
using _2DSurviveGameServer._02Sys.Room.Actors;
using _2DSurviveGameServer._02Sys.Room.FSM;
using GameEngine;
using Microsoft.Extensions.Hosting;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using Protocol.Body;
using System.Diagnostics;

namespace _2DSurviveGameServer._01Common.Config
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

        List<AStarNode> openList = new List<AStarNode>();
        List<AStarNode> closeList = new List<AStarNode>();

        public AStarMap(JObject jo)
        {


            m_NodeRadius = float.Parse(jo["NodeRadius"].ToString());
            m_NodeDiameter = float.Parse(jo["NodeDiameter"].ToString());
            mapH = int.Parse(jo["MapH"].ToString());
            mapW = int.Parse(jo["MapW"].ToString());

            nodes = new AStarNode[mapW, mapH];

            JArray rowArr = JArray.Parse(jo["Nodes"].ToString());
            for (int i = 0; i < rowArr.Count; i++)
            {
                JArray columnArr = JArray.Parse(rowArr[i].ToString());
                for (int j = 0; j < columnArr.Count; j++)
                {
                    JObject obj = JObject.Parse(columnArr[j].ToString());
                    float x = float.Parse(obj["x"].ToString());
                    float y = float.Parse(obj["y"].ToString());
                    bool canWalk = bool.Parse(obj["canWalk"].ToString());
                    AStarNode node = new AStarNode(i, j, canWalk ? NodeType.Walk : NodeType.Stop, new Vector2(x, y));
                    nodes[i, j] = node;
                }
            }
        }


        public AStarNode GetFromPos(Vector2 pos)
        {
            float percentX = (pos.X + mapW / 2) / mapW;
            float percentY = (pos.Y + mapH / 2) / mapH;
            percentX = Math.Clamp(percentX, 0, 1);
            percentY = Math.Clamp(percentY, 0, 1);
            int x = (int)Math.Round((mapW - 1) * percentX);
            int y = (int)Math.Round((mapH - 1) * percentY);
            try
            {
                return nodes[x, y];
            }
            catch { return null; }
        }

        public bool GetFirstPos(Vector2 startPos, Vector2 endPos, out Vector2 target)
        {
            target = Vector2.Zero;
            var res = FindPath(startPos, endPos, out var path);
            if (res == 0)
            {
                target = path[0].worldPos;
                return true;
            }
            return false;
        }

        private int SortOpenList(AStarNode a, AStarNode b)
        {
            if (a == null || b == null)
            {
                throw new ArgumentException($"错误，a:{a} b:{b}");
            }

            // 检查并记录 a 和 b 的 f 值
            //Console.WriteLine($"Comparing nodes: a.f={a.f}, b.f={b.f}");
            if (a.f > b.f)
                return 1;
            else if (a.f == b.f)
                return 1;
            else
                return -1;
        }

        public int FindPath(Vector2 startPos, Vector2 endPos, out List<AStarNode> path)
        {
            path = new List<AStarNode>();
            AStarNode start = GetFromPos(startPos);
            AStarNode end = GetFromPos(endPos);
            if (start == null || end == null)
            {
                //this.Log($"【AStarMap】起点或者终点超出地图边界");
                return -1;
            }

            //开始或目标点为stop，不合法
            if (start.type == NodeType.Stop || end.type == NodeType.Stop)
            {
                //this.Log($"【AStarMap】结束点为stop");
                return -1;
            }

            //开始和结束点坐标相同
            if (startPos == endPos)
            {
                //this.Log($"【AStarMap】起点和终点一致，不用寻路");
                return -3;
            }

            //开始和结束点在同个格子里
            if (start.x == end.x && start.y == end.y)
            {
                //this.Log($"【AStarMap】起点和终点在同一个格子，返回终点坐标");
                //直接返回目标点
                path.Add(new AStarNode(start.x, start.y, NodeType.Walk, endPos));
                return 0;
            }

            openList.Clear();
            closeList.Clear();

            start.father = null;
            start.f = 0;
            start.g = 0;
            start.h = 0;
            closeList.Add(start);

            while (true)
            {
                // 左上 x - 1 y - 1
                FindNearlyNodeToOpenList(start.x - 1, start.y - 1, 1.4f, start, end);
                // 上 x y -1
                FindNearlyNodeToOpenList(start.x, start.y - 1, 1.4f, start, end);
                // 右上 x + 1 y - 1
                FindNearlyNodeToOpenList(start.x + 1, start.y - 1, 1.4f, start, end);
                // 左 x - 1 y
                FindNearlyNodeToOpenList(start.x - 1, start.y, 1.4f, start, end);
                // 右 x + 1 y
                FindNearlyNodeToOpenList(start.x + 1, start.y, 1.4f, start, end);
                // 左下 x - 1 y + 1
                FindNearlyNodeToOpenList(start.x - 1, start.y + 1, 1.4f, start, end);
                // 下 x y + 1
                FindNearlyNodeToOpenList(start.x, start.y + 1, 1.4f, start, end);
                // 右下 x + 1 y + 1
                FindNearlyNodeToOpenList(start.x + 1, start.y + 1, 1.4f, start, end);

                if (openList.Count == 0)
                {
                    // 没有寻找到路线（可能是卡在了死角，需要重置位置）
                    // this.Log("【AStarMap】openList == 0");
                    return -1;
                }
                openList = openList.Where(node => node != null).ToList();
                //LogOpenList();
                openList.Sort(SortOpenList);

                closeList.Add(openList[0]);
                start = openList[0];
                openList.RemoveAt(0);

                if (start == end)
                {
                    path.Add(end);
                    while (end != null && end.father != null)
                    {
                        path.Add(end.father);
                        end = end.father;
                    }
                    path.Reverse();

                    // 把第一个格子替换成当前自身位置
                    AStarNode first = path[0];
                    first.worldPos = startPos;
                    // 把最后一个格子替换成当前目标点坐标
                    AStarNode last = path[path.Count - 1];
                    last.worldPos = endPos;

                    // this.Log("【AStarMap】寻路成功，路径点数：" + path.Count);
                    return 0;
                }
            }
        }

        private void LogOpenList()
        {
            foreach (var node in openList)
            {
                Console.WriteLine($"Node: x={node.x}, y={node.y}, f={node.f}, g={node.g}, h={node.h}, type={node.type}");
            }
        }

        private void FindNearlyNodeToOpenList(int x, int y, float g, AStarNode father, AStarNode end)
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
            //Console.WriteLine($"Adding node to openList: x={node.x}, y={node.y}, f={node.f}, g={node.g}, h={node.h}");
            openList.Add(node);
            
            
        }
    }
}
