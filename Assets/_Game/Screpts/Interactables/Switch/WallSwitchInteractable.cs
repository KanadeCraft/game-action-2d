using UnityEngine;

public class WallSwitchInteractable : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    bool isActivated = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            spriteRenderer.color = Color.green;
            isActivated = true;
        }
    }

    // 入力値を外部から取得できるようにするプロパティ
    public bool IsActivated
    {
        get
        {
            return isActivated;
        }
    }
}
