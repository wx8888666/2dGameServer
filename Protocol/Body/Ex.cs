using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.Body
{
    [Serializable]
    public class NetVector2
    {
        public float x, y;
        public static NetVector2 Zero { get { return new NetVector2(0, 0); } }
        public NetVector2(float x,float y)
        {
            this.x = x; this.y = y;
        }

        public NetVector2()
        {
            
        }
        public static NetVector2 MoveTowards(NetVector2 current, NetVector2 target, float maxDistanceDelta)
        {
            float num = target.x - current.x;
            float num2 = target.y - current.y;
            float num3 = num * num + num2 * num2;
            if (num3 == 0f || (maxDistanceDelta >= 0f && num3 <= maxDistanceDelta * maxDistanceDelta))
            {
                return target;
            }

            float num4 = (float)Math.Sqrt(num3);
            return new NetVector2(current.x + num / num4 * maxDistanceDelta, current.y + num2 / num4 * maxDistanceDelta);
        }
        public static float Distance(NetVector2 value1, NetVector2 value2)
        {
            float num = value1.x - value2.x;
            float num2 = value1.y - value2.y;
            float x2 = num * num + num2 * num2;
            return (float)Math.Sqrt(x2);
        }
    }
}
