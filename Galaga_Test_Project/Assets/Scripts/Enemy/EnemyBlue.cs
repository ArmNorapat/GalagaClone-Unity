using Gulaga.BaseClass;
using UnityEngine;

namespace Gulaga.Enemy
{
    public class EnemyBlue : BaseEnemyController
    {
        /// <summary>
        /// Define rotate per second value when this unit is in assault state.
        /// </summary>
        [SerializeField] private float[] assaultRots = null;

        private const float ySpeedLimit = -0.4f;
        private float randAssaultRot;

        protected override void Launch()
        {
            int index = Random.Range(0, assaultRots.Length);
            if (assaultRots.Length > 0)
            {
                randAssaultRot = assaultRots[index];
            }

            LookAtDir(playerDir); // To player pos.
        }

        protected override void InvadePlayer()
        {
            MoveForward(moveSpeed);
        }

        protected override void Assault()
        {
            ignoreSmoothRot = true;

            if (transform.up.y < ySpeedLimit) //Force unit to don't go up.
            {
                transform.Rotate(0, 0, randAssaultRot * Time.deltaTime);
            }

            MoveForward(moveSpeed);
        }
    }
}