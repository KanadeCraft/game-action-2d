using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("最大HP"), Min(1), SerializeField]
    int maxHealth = 3;

    int currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        // HPが0，又はダメージが正の数で泣けて場受け付けない
        if(currentHealth <= 0 || damage <= 0)
        {
            return;
        }

        // HPは0未満にしない
        currentHealth = Mathf.Max(0, currentHealth - damage);

        Debug.Log($"ダメージを受けた：HP {currentHealth}/{maxHealth}");
    }
}
