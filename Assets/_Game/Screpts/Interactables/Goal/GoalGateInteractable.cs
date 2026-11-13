using UnityEngine;

public class GoalGateInteractable : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            spriteRenderer.color = Color.green;
        }
    }
}
