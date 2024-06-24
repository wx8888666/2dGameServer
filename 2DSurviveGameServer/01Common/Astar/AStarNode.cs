using Microsoft.Xna.Framework;

namespace _2DSurviveGameServer._01Common.Astar
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
}
