using GameEngine;
using Protocol.Body;

namespace _2DSurviveGameServer._02Sys.Room.Actors
{
    public class WeaponActor : Actor
    {
        public WeaponObject WeaponObject { get; set; }=new WeaponObject();
        public override void OnDestory()
        {
           
        }

        public override void Start()
        {
           WeaponObject.id = this.Id;
        }

        public override void Update()
        {
           
        }
        public void UpdateWeapon(WeaponObject weaponObject)
        {
            this.WeaponObject = weaponObject;
        }
    }
}
