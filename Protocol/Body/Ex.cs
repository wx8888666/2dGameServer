using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.Body
{
    [Serializable]
    public class NetVector2
    {
        public float x, y;
        public NetVector2(float x,float y)
        {
            this.x = x; this.y = y;
        }

        public NetVector2()
        {
            
        }
    }
}
