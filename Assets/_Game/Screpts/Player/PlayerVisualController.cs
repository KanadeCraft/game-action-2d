using UnityEngine;

public class PlayerVisualController : MonoBehaviour
{
    // 重力の方向を取得するためにGravityControllerを参照
    [SerializeField] GravityController gravityController;
    SpriteRenderer spriteRenderer;

    // 移動入力の値を保持する変数
    float moveInput;


    /// <summary>
    /// 外部から各種入力できるようにするメソッド 
    /// </summary>
    public void SetDirection(float value)
    {
        moveInput = value;
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        // 重力に合わせて、親オブジェクトに対する回転角度で自身を回転
        switch (gravityController.GravityVector)
        {
            case { x: 0, y: 1 }:
                // 上の処理
                transform.localEulerAngles = new Vector3(0, 0, 180);
                break;
            case { x: 0, y: -1 }:
                // 下の処理
                transform.localEulerAngles = new Vector3(0, 0, 0);
                break;
            case { x: -1, y: 0 }:
                // 左の処理
                transform.localEulerAngles = new Vector3(0, 0, -90);
                break;
            case { x: 1, y: 0 }:
                // 右の処理
                transform.localEulerAngles = new Vector3(0, 0, 90);
                break;
            default:
                // その他の処理
                break;
        }

        // 移動入力に応じてスプライトの向きを変更
        if (moveInput > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (moveInput < 0)
        {
            spriteRenderer.flipX = true;
        }
    }
}
