using Gulaga.BaseClass;
using UnityEngine;

namespace Gulaga.Player
{
    public class PlayerBullet : BaseBulletController
    {
        protected override void OnStart()
        {
            SetMovement(Vector3.up);
        }
    }
}