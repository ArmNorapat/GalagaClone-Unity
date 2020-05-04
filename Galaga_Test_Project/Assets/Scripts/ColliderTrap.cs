using UnityEngine;

/// <summary>
/// Special class for collider to damage something.
/// </summary>
public class ColliderTrap : MonoBehaviour
{
    [SerializeField] private string damageAbleTag = null;
    [SerializeField] private int damage = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.CompareTag(damageAbleTag))
        {
            var damageObj = other.GetComponent<IDamageable>();

            if(damageObj != null)
            {
                damageObj.TakeDamage(damage);
            }
        }
    }
}