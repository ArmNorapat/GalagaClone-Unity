using UnityEngine;

namespace Gulaga.BaseClass
{
    public abstract class BaseEnemyController : MonoBehaviour, IDamageable
    {
        /// <summary>
        /// Special unit with powerful weapon.
        /// </summary>
        [SerializeField] private bool isTyrant = false;
        [SerializeField] private int live = 0;
        [SerializeField] protected float moveSpeed = 0;
        /// <summary>
        /// When reach this y position ai will change aiState to assault.
        /// </summary>
        [SerializeField] float assaultPosY = 0f;
        [SerializeField] protected int score = 0;
        [SerializeField] private GameObject dieEffect = null;

        public enum AiState
        {
            EarlySpawn,
            InWaitingPosition,
            ReadyToInvade,
            Launch,
            InvadePlayer,
            Assault,
        }
        public bool IsReadyToInvade
        {
            get
            {
                switch (aiState)
                {
                    case AiState.InWaitingPosition:
                        if (isTyrant)
                        {
                            if (CurrentInvadeTyrant == null)
                            {
                                CurrentInvadeTyrant = this;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        return true;

                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Only one tyrant can invade player at the same time. 
        /// Ex. green enemy.
        /// </summary>
        public static BaseEnemyController CurrentInvadeTyrant = null;
        /// <summary>
        /// Waiting position before invade player.
        /// </summary>
        protected Transform waitingTrans { get; private set; }
        protected const float rotateSpeed = 600;
        /// <summary>
        /// Override unit's speed when do not invade or assault player.
        /// </summary>
        protected const float spawnMoveSpeed = 4f;
        protected const float rotateOffset = -90f;
        protected const float closeDistance = 0.1f;
        /// <summary>
        /// If reach border enemy will teleport back to wating pos.
        /// </summary>
        protected const float belowBorderY = -6f;
        protected const float offsetYEnemyBack = 10f;
        protected AiState aiState = AiState.EarlySpawn;
        /// <summary>
        /// Value for smooth movetoward.
        /// </summary>
        protected Quaternion smoothRot;
        /// <summary>
        /// Start rotation when invade player.
        /// </summary>
        protected Vector3 invadeRot = new Vector3(0, 0, 180);
        /// <summary>
        /// Rotation when isn't invade player.
        /// </summary>
        protected Vector3 waitingRot = new Vector3(0, 0, 0);
        protected Vector3 playerDir
        {
            get { return GameManager.Instance.PlayerPos - transform.position; }
        }
        protected Vector3 waitingDir
        {
            get { return waitingTrans.position - transform.position; }
        }
        protected bool ignoreSmoothRot = false;

        private void Update()
        {
            switch (aiState)
            {
                case AiState.EarlySpawn:
                    EarlySpawn();
                    break;

                case AiState.InWaitingPosition:
                    InWaitingPosition();
                    break;

                case AiState.ReadyToInvade:
                    ReadyToInvade();
                    break;

                case AiState.Launch:
                    if (isTyrant)
                    {
                        CurrentInvadeTyrant = this;
                    }
                    Launch();
                    aiState = AiState.InvadePlayer;
                    break;

                case AiState.InvadePlayer:
                    InvadePlayer();
                    if (transform.position.y <= assaultPosY)
                    {
                        aiState = AiState.Assault;
                    }
                    break;

                case AiState.Assault:
                    Assault();
                    if (transform.position.y < belowBorderY)
                    {
                        TeleportBack();
                    }
                    break;
            }

            if (!ignoreSmoothRot)
            {
                SmoothRotate();
            }
        }

        /// <summary>
        /// First thing when enemy spawn what should it do?
        /// Ex. Let's Ai go to waiting position and wait next order.
        /// </summary>
        protected virtual void EarlySpawn()
        {
            if (waitingDir.magnitude < closeDistance)
            {
                aiState = AiState.InWaitingPosition;
            }
            else
            {
                LookAtDir(waitingDir, false);
                MoveForward(spawnMoveSpeed);
            }
        }

        /// <summary>
        /// Idle and do nothing.
        /// </summary>
        private void InWaitingPosition()
        {
            smoothRot = Quaternion.Euler(waitingRot);
            transform.position = waitingTrans.position;
        }

        /// <summary>
        /// Command by GameManager. Set to invade rot and launch this unit.
        /// </summary>
        private void ReadyToInvade()
        {
            smoothRot = Quaternion.Euler(invadeRot);

            if (smoothRot == transform.rotation)
            {
                aiState = AiState.Launch;
            }
        }

        /// <summary>
        /// Trigger once first time when leave waiting position.
        /// </summary>
        protected abstract void Launch();

        /// <summary>
        /// Leave waiting position and do some action.
        /// </summary>
        protected abstract void InvadePlayer();

        /// <summary>
        /// Trigger when reach Y condition to perform special action.
        /// </summary>
        protected abstract void Assault();

        public void TakeDamage(int value)
        {
            live -= value;

            if (live <= 0)
            {
                GameManager.Instance.ClearEnemy(gameObject);
                GameManager.Instance.UpdateScore(score);

                if (dieEffect != null)
                {
                    var newObj = Instantiate(dieEffect, transform.position, Quaternion.identity);
                    Destroy(newObj, GameManager.effectDestroyTime);
                }

                if (isTyrant)
                {
                    if (CurrentInvadeTyrant != null && CurrentInvadeTyrant == this)
                    {
                        CurrentInvadeTyrant = null;
                    }
                }

                Destroy(gameObject);
            }
        }

        public void SetWaitingPos(Transform newTrans)
        {
            waitingTrans = newTrans;
        }

        public void SetAiState(AiState targetState)
        {
            aiState = targetState;
        }

        protected void LookAtDir(Vector3 dir, bool isSmoothRot = true)
        {
            var rad = Mathf.Atan2(dir.y, dir.x);
            var deg = rad * Mathf.Rad2Deg + rotateOffset;

            smoothRot = Quaternion.Euler(0, 0, deg);

            if (!isSmoothRot)
            {
                transform.rotation = smoothRot;
            }

            ignoreSmoothRot = false;
        }

        protected void MoveForward(float speed)
        {
            Vector3 movement = transform.up * speed * Time.deltaTime;
            transform.position += movement;
        }

        protected void SmoothRotate()
        {
            var currentRot = transform.rotation;
            transform.rotation = Quaternion.RotateTowards(currentRot, smoothRot, rotateSpeed * Time.deltaTime);
        }

        /// <summary>
        /// While attacking and reach Y min position. Unit should comback to it's waiting pos.
        /// </summary>
        protected void TeleportBack()
        {
            if (waitingTrans != null)
            {
                var comebackPos = waitingTrans.position;
                var trueComebackPos = new Vector2(comebackPos.x, comebackPos.y + offsetYEnemyBack);

                transform.position = trueComebackPos;
            }

            if (isTyrant && CurrentInvadeTyrant == this)
            {
                CurrentInvadeTyrant = null;
            }

            SetAiState(BaseEnemyController.AiState.EarlySpawn);
        }
    }
}