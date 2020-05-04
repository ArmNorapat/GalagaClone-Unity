using System;
using UnityEngine;

namespace Gulaga.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour, IDamageable
    {
        [SerializeField] private GameObject explosionPrefab = null;

        [Header("Movement")]
        [SerializeField] private Rigidbody2D rb2d = null;
        [SerializeField] private float moveSpeed = 0;

        /// <summary>
        /// Limit player position on X Axis.
        /// </summary>
        [SerializeField] private Vector2 clampXMinMax = Vector3.zero;

        [Header("Gun")]
        [SerializeField] private KeyCode shootKey = KeyCode.A;
        [SerializeField] private GameObject bulletPref = null;
        /// <summary>
        /// Bullet spawn position and rotation.
        /// </summary>
        [SerializeField] private Transform muzzleTrans = null;
        /// <summary>
        /// Prevent gun firing speed.
        /// </summary>
        [SerializeField][Range(0, 1)] private float shootCooldown = 0;

        public event Action<Transform> OnPlayerDie;

        private float shootTimer;

        private void Start()
        {
            OnPlayerDie += DieEffect;
        }

        private void Update()
        {
            if (Input.GetKeyDown(shootKey) && shootTimer <= 0)
            {
                ShootBullet();

                shootTimer = shootCooldown;
            }
            else
            {
                shootTimer -= Time.deltaTime;
            }

            ClampPosition();
        }

        private void FixedUpdate()
        {
            Movement();
        }

        private void Movement()
        {
            var moveHorizontal = Input.GetAxisRaw("Horizontal");

            if (rb2d != null)
            {
                rb2d.velocity = Vector3.right * moveHorizontal * moveSpeed;
            }
        }

        private void ClampPosition()
        {
            var xPos = transform.position.x;
            var yPos = transform.position.y;

            var xPosClamp = Mathf.Clamp(xPos, clampXMinMax.x, clampXMinMax.y);
            var truePos = new Vector2(xPosClamp, yPos);

            transform.position = truePos;
        }

        private void ShootBullet()
        {
            Instantiate(bulletPref, muzzleTrans.position, muzzleTrans.rotation);
        }

        public void TakeDamage(int value = 0)
        {
            OnPlayerDie(transform);
        }

        private void DieEffect(Transform diePos)
        {
            var newObj = Instantiate(explosionPrefab, diePos.position, diePos.rotation);
            Destroy(newObj, GameManager.effectDestroyTime);
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Enemy"))
            {
                TakeDamage();
            }
        }
    }
}