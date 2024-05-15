using Microsoft.Xna.Framework;
using Protocol.Body;

namespace _2DSurviveGameServer.Helpers
{
    public static class Ex
    {
        public static NetVector2 ToNetVector2(this Vector2 vector2)
        {
            return new NetVector2(vector2.X, vector2.Y);
        }

        public static Vector2 ToVector2(this NetVector2 netVector2)
        {
            return new Vector2(netVector2.x, netVector2.y);
        }
    }
}
