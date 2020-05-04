using Gulaga.BaseClass;
using UnityEngine;

namespace Gulaga.Enemy
{
    public class EnemyRed : BaseEnemyController
    {
        /// <summary>
        /// Offset when flying to position.
        /// </summary>
        [SerializeField] private float[] offsetXPos = null;
        /// <summary>
        /// Speed x multiplier when this unit assault.
        /// </summary>
        [SerializeField][Range(0, 5)] private float speedXMultiplier = 0;

        [Header("Gun")]
        [SerializeField] private GameObject bulletPref = null;
        [SerializeField] private Transform[] muzzles = null;
        /// <summary>
        /// Rotate gun to direction depend on unit's x vector.
        /// </summary>
        [SerializeField] private float attackRot = 0;

        private float randOffsetPosX;
        private bool isShoot;

        protected override void Launch()
        {
            isShoot = false;

            var index = Random.Range(0, offsetXPos.Length);
            if (offsetXPos.Length > 0)
            {
                randOffsetPosX = offsetXPos[index];
            }

            var xPosOffset = transform.position + randOffsetPosX * Vector3.right;
            var targetPos = new Vector3(xPosOffset.x, GameManager.Instance.PlayerPos.y);
            var targetDir = targetPos - transform.position;

            LookAtDir(targetDir);
        }

        protected override void InvadePlayer()
        {
            Movement();
        }

        protected override void Assault()
        {
            Movement(speedXMultiplier);

            if (!isShoot)
            {
                ShootBullet();
            }
        }

        /// <summary>
        /// Special movement with addition X axis speed.
        /// </summary>
        /// <param name="xSpeedMultiplier">Speed multiplier</param>
        private void Movement(float xSpeedMultiplier = 1)
        {
            var movement = transform.up * moveSpeed * Time.deltaTime;
            movement.x *= xSpeedMultiplier;

            transform.position += movement;
        }

        private void ShootBullet()
        {
            var trueMuzzleRot = attackRot;

            if (randOffsetPosX < 0) //If move left shoot right
            {
                trueMuzzleRot = attackRot;
            }
            else //If move right shoot left
            {
                trueMuzzleRot = -attackRot;
            }

            foreach (Transform muzzle in muzzles)
            {
                muzzle.localRotation = Quaternion.Euler(0, 0, trueMuzzleRot);

                if (bulletPref != null)
                {
                    var newObj = Instantiate(bulletPref, muzzle.position, muzzle.rotation);
                    var newBullet = newObj.GetComponent<BaseBulletController>();
                }
            }

            isShoot = true;
        }
    }
}