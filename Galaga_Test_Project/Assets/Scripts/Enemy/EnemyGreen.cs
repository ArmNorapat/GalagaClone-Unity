using Gulaga.BaseClass;
using UnityEngine;

namespace Gulaga.Enemy
{
    public class EnemyGreen : BaseEnemyController
    {
        [SerializeField] private GameObject beamObj = null;
        /// <summary>
        /// Get it's animation state.
        /// </summary>
        [SerializeField] private Animator beamAnim = null;

        private const float beamNormalizeStopTime = 0.9f;
        private bool isBeamReady = true;

        protected override void Launch()
        {
            isBeamReady = true;
        }

        protected override void InvadePlayer()
        {
            MoveForward(moveSpeed);
            FixPos();
        }

        protected override void Assault()
        {
            if (!isBeamReady || CurrentInvadeTyrant != this)
            {
                MoveForward(moveSpeed);
                return;
            }

            beamObj.SetActive(true); //Shoot beam

            if (beamAnim != null)
            {
                var animation = beamAnim.GetCurrentAnimatorStateInfo(0);

                if (animation.normalizedTime > beamNormalizeStopTime)
                {
                    MoveForward(moveSpeed);
                    beamObj.SetActive(false); //Stop beam
                    isBeamReady = false;
                }
            }

            FixPos();
        }

        /// <summary>
        /// Unit x position will always stand in x position of waitingPos.
        /// </summary>
        protected void FixPos()
        {
            var fixXPos = new Vector2(waitingTrans.position.x, transform.position.y);
            transform.position = fixXPos;
        }
    }
}