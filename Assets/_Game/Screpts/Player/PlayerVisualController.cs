using UnityEngine;

public class PlayerVisualController : MonoBehaviour
{
    // 重力の方向を取得するためにGravityControllerを参照
    [SerializeField] GravityController gravityController;

    // 自分の情報を保持する変数
    [Header("見える本体の高さ（Scaleが1のとき）"), SerializeField]
    float visualBodyHeight = 1f;

    Vector3 originalScale;
    Vector3 originalPosition;
    float originalHeight;
    SpriteRenderer spriteRenderer;
    Animator animator;

    // 移動入力の値を保持する変数
    float moveInput;

    // しゃがみ状態と高さの割合を保持する変数
    bool isSquatting;
    [Header("しゃがみ時の見た目の高さの割合"), Range(0.1f, 1f), SerializeField]
    float squatHeightRatio = 0.5f;


    /// <summary>
    /// 外部から各種入力できるようにするメソッド 
    /// </summary>
    public void SetDirection(float value)
    {
        moveInput = value;
    }

    public void SetSquatState(bool value)
    {
        isSquatting = value;
    }

    public void SetAnimationState(bool grounded, Vector2 velocity, bool runHeld)
    {
        Vector2 up = -gravityController.GravityVector.normalized;
        Vector2 right = new Vector2(up.y, -up.x);

        float horizontalSpeed = Mathf.Abs(Vector2.Dot(velocity, right));
        float upSpeed = Vector2.Dot(velocity, up);

        animator.SetBool("Grounded", grounded);
        animator.SetFloat("Speed", horizontalSpeed);
        animator.SetFloat("UpSpeed", upSpeed);
        animator.SetBool("Squatting", isSquatting);
        animator.SetBool("RunHeld", runHeld);
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        originalScale = transform.localScale;
        originalPosition = transform.localPosition;

        // 透明な余白を除いた、本体の高さを使う
        originalHeight = visualBodyHeight * originalScale.y;
    }

    private void LateUpdate()
    {
        UpdateGravityRotation();
        UpdateFacingDirection();
        UpdateSquatVisual();
    }

    void UpdateGravityRotation()
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
    }

    void UpdateFacingDirection()
    {
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

    void UpdateSquatVisual()
    {
        // 通常は元の高さ
        float heightRatio = 1f;

        if (isSquatting)
        {
            heightRatio = squatHeightRatio;
        }

        // 元のScaleを基準に、高さだけ変える
        Vector3 visualScale = originalScale;
        visualScale.y = originalScale.y * heightRatio;
        transform.localScale = visualScale;

        // 減った高さの半分だけ中心を下げる
        float heightDifference = originalHeight * (1f - heightRatio);

        Vector3 visualPosition = originalPosition;
        Vector3 localDown = transform.localRotation * Vector3.down;
        visualPosition += localDown * (heightDifference / 2f);
        transform.localPosition = visualPosition;
    }
}
