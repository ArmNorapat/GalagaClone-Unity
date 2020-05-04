using UnityEngine;

namespace Gulaga.BaseClass
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class BaseBulletController : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rb2d = null;
        [SerializeField] private float moveSpeed = 0;
        [SerializeField] private int damage = 0;
        [SerializeField] private string[] damageableTags = null;

        /// <summary>
        /// Border for clear gameObject;
        /// </summary>
        protected const float yBorder = 8;
        protected const float rotateOffset = -90f;
        protected Quaternion trueRot;
        /// <summary>
        /// Prevent multi-hit by bullet because unity destroy obj slower than OnTrigger.
        /// </summary>
        protected bool isHitAnything = false;

        private void Start()
        {
            OnStart();
        }

        private void Update()
        {
            if (Mathf.Abs(transform.position.y) > yBorder)
            {
                Destroy(gameObject);
            }
        }

        protected abstract void OnStart();

        /// <summary>
        /// Set bullet movement.
        /// </summary>
        /// <param name="dir">Direction of movement.</param>
        public void SetMovement(Vector3 dir)
        {
            if (rb2d != null)
            {
                rb2d.velocity = dir * moveSpeed;
            }
        }

        public void SetRotToDir(Vector3 dir)
        {
            var rad = Mathf.Atan2(dir.y, dir.x);
            var deg = rad * Mathf.Rad2Deg + rotateOffset;

            trueRot = Quaternion.Euler(0, 0, deg);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isHitAnything)
            {
                return;
            }

            foreach (string dt in damageableTags)
            {
                if (other.gameObject.CompareTag(dt))
                {
                    var damageObj = other.GetComponent<IDamageable>();

                    if (damageObj != null)
                    {
                        damageObj.TakeDamage(damage);
                    }

                    isHitAnything = true;
                    Destroy(gameObject);

                    break;
                }
            }
        }
    }
}