using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace GameEngine
{
    public class GameWorld
    {
        public World world;

        public GameWorld()
        {
            world = new World(new Microsoft.Xna.Framework.Vector2());
        }

        public void Update(int delta)
        {
            world.Step(1f / delta);
        }

        public T Create<T>(Vector2 pos) where T : Actor, new()
        {
            T actor = new T();
            actor.Body = BodyFactory.CreateRectangle(world, 1, 1, 1, actor);
            actor.Body.BodyType = BodyType.Dynamic;
            actor.Body.Position = pos;
            return actor;
        }

        public void Destroy(Actor actor)
        {
            world.RemoveBody(actor.Body);
            actor = null;
        }

        public void Raycast(Vector2 start, Vector2 dir, float distance, Action<Actor> callback)
        {
            world.RayCast((f, p, n, fr) =>
            {
                if (f.Body.UserData is Actor actor)
                {
                    callback(actor);
                }
                return fr;
            }, start, start + dir * distance);
        }
    }
}
