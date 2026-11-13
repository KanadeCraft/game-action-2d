using UnityEngine;

public class BoxSwitchInteractable : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    Color defaultColor;
    bool isActivated;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultColor = spriteRenderer.color;
        isActivated = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Box")
        {
            spriteRenderer.color = Color.green;
            isActivated = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Box")
        {
            spriteRenderer.color = Color.yellow;
            isActivated = false;
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
