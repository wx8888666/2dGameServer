using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.Body
{
    [Serializable]
    public struct NetVector2
    {
        public float x;
        public float y;

        public static NetVector2 Zero { get { return new NetVector2(0, 0); } }
        public NetVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        #region NetVector 和 NetVector
        public static NetVector2 operator +(NetVector2 v1, NetVector2 v2)
        {
            return new NetVector2(v1.x + v2.x, v1.y + v2.y);
        }

        public static NetVector2 operator -(NetVector2 v1, NetVector2 v2)
        {
            return new NetVector2(v1.x - v2.x, v1.y - v2.y);
        }

        public static NetVector2 operator *(NetVector2 v1, NetVector2 v2)
        {
            return new NetVector2(v1.x * v2.x, v1.y * v2.y);
        }

        public static NetVector2 operator /(NetVector2 v1, NetVector2 v2)
        {
            return new NetVector2(v1.x / v2.x, v1.y / v2.y);
        }

        public static bool operator ==(NetVector2 v1, NetVector2 v2)
        {
            return v1.Equals(v2);
        }

        public static bool operator !=(NetVector2 v1, NetVector2 v2)
        {
            return !(v1 == v2);
        }
        #endregion

        #region NetVerctor 和 Float
        public static NetVector2 operator +(NetVector2 v1, float f)
        {
            return new NetVector2(v1.x + f, v1.y + f);
        }

        public static NetVector2 operator -(NetVector2 v1, float f)
        {
            return new NetVector2(v1.x - f, v1.y - f);
        }

        public static NetVector2 operator *(NetVector2 v1, float f)
        {
            return new NetVector2(v1.x * f, v1.y * f);
        }

        public static NetVector2 operator /(NetVector2 v1, float f)
        {
            return new NetVector2(v1.x / f, v1.y / f);
        }
        #endregion

        public NetVector2 Normalized()
        {
            float magnitude = (float)Math.Sqrt(x * x + y * y);
            if (magnitude > 1E-05f)
            {
                return this / magnitude;
            }
            else
            {
                return NetVector2.Zero;
            }
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


        public override string ToString()
        {
            return $"({x},{y})";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is NetVector2))
            {
                return false;
            }

            NetVector2 v2 = (NetVector2)obj;
            return Equals(v2);
        }

        public override int GetHashCode()
        {
            int hashCode = x.GetHashCode();
            return hashCode + y.GetHashCode();
        }

        public bool Equals(NetVector2 other)
        {
            return x == other.x && y == other.y;
        }
    }
}
