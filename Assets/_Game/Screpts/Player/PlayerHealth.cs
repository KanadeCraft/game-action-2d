using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("最大HP"), Min(1), SerializeField]
    int maxHealth = 3;
    [Header("被弾後の無敵時間"), Min(0), SerializeField]
    float invincibleTime = 1f;

    int currentHealth;
    float invincibleRemaining;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if(invincibleRemaining > 0f)
        {
            invincibleRemaining = Mathf.Max(0f, invincibleRemaining - Time.deltaTime);
        }
    }

    public void TakeDamage(int damage)
    {
        // HPが0，又はダメージが正の数で泣けて場受け付けない
        if(currentHealth <= 0 || damage <= 0)
        {
            return;
        }

        // 無敵時間が残っていればダメージを受けない
        if(invincibleRemaining > 0f)
        {
            return;
        }

        // HPは0未満にしない
        currentHealth = Mathf.Max(0, currentHealth - damage);
        // ダメージを受けたときに無敵時間を設定する
        invincibleRemaining = invincibleTime;

        Debug.Log($"ダメージを受けた：HP {currentHealth}/{maxHealth}");
    }
}
