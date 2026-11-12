using UnityEngine;

public class GoalGateInteractable : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    // WallSwitchInteractable‚ÌQÆ‚ğæ“¾‚·‚é‚½‚ß‚Ì•Ï”
    [SerializeField] WallSwitchInteractable wallSwitchInteractable;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && wallSwitchInteractable.IsActivated)
        {
            spriteRenderer.color = Color.green;
        }
    }
}
