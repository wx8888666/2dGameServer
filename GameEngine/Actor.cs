using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    public interface IActor
    {
        void Start();
        void Update();
        void OnDestory();
    }

    public abstract class Actor : IActor
    {
        public Body Body {  get; set; }

        public void ApplyLinearImpulse(Vector2 dir,float f)
        {
            Body.ApplyLinearImpulse(dir * f);
        }

        public bool IsColliding()
        {
            return Body.ContactList != null && Body.ContactList.Contact.IsTouching();
        }

        public abstract void OnDestory();

        public abstract void Start();

        public abstract void Update();
    }
}
