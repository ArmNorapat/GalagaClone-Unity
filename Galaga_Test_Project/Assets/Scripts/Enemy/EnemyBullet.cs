using Gulaga.BaseClass;

namespace Gulaga.Enemy
{
    public class EnemyBullet : BaseBulletController
    {
        protected override void OnStart()
        {
            SetMovement(transform.up);
        }
    }
}