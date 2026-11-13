using UnityEngine;

public class PlayerMotor2D : MonoBehaviour
{
    // 重力の方向と加速度を取得するためにGravityControllerを参照
    [SerializeField] GravityController gravityController;

    Rigidbody2D rb2D;
    float setMoveInput;
    Vector2 previousGravity;

    // それぞれの最大猶予時間とそれぞれの時間を保持する変数
    float jumpBufferRemaining;
    [Header("ジャンプの猶予時間"), SerializeField]
    float maxJumpBufferTime = 0.1f;
    float coyoteRemaining;
    [Header("コヨーテタイム"), SerializeField]
    float maxCoyoteTime = 0.08f;

    [Header("移動速度"), SerializeField] 
    float moveSpeed = 5f;
    [Header("ジャンプ力"), SerializeField] 
    float jumpForce = 5f;

    bool isGrounded;
    Vector2 groundNonal;

    // 次の物理更新を保護するフラグ
    bool ignoreGroundOnce;

    // 箱を押す判定に使う、入力によって移動しようとしている速度。
    // 実際の速度と違い、落下やジャンプの速度は含めない。
    // プロパティ
    public Vector2 RequestedMoveVelocity
    {
        get
        {
            Vector2 gravity = gravityController.GravityVector;
            Vector2 right = new Vector2(-gravity.y, gravity.x);
            return right * setMoveInput * moveSpeed;
        }
    }

    /// <summary>
    /// 外部から各種入力できるようにするメソッド 
    /// </summary>
    public void SetMoveInput(float value)
    {
        setMoveInput = value;
    }

    public void SetJumpInput(bool value)
    {
        // ジャンプバッファ時間を設定
        if (value)
        {
            jumpBufferRemaining = maxJumpBufferTime;
        }
    }

    public void SetIsGrounded(bool value)
    {
        isGrounded = value;
    }

    public void SetGroundNormal(Vector2 value)
    {
        groundNonal = value;
    }

    void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        // 初期化処理
        previousGravity = gravityController.GravityVector;
        isGrounded = false;
    }

    public void SimulateMovement()
    {
        if (ignoreGroundOnce)
        {
            isGrounded = false;
            ignoreGroundOnce = false;
        }

        // 現在の速度を取得
        Vector2 currentVelocity = rb2D.linearVelocity;
        // 現在の重力の方向を取得
        Vector2 gravity = gravityController.GravityVector;

        if (gravity == -previousGravity)
        {
            // 重力が180度変化した場合の処理
            // 重力方向の速度を打ち消す
            float speedAlongGravity = Vector2.Dot(currentVelocity, gravity);

            if(speedAlongGravity < 0f)
            {
                currentVelocity -= gravity * speedAlongGravity; // 重力方向の速度を打ち消す
            }
        }

        previousGravity = gravity;

        // 接地状態の確認と処理
        if (isGrounded)
        {
            // 接地中の処理
            // 接地中は法線の方向を床と平行にする(右向き)
            Vector2 groundRight = new Vector2(groundNonal.y, -groundNonal.x);
            Debug.DrawRay(rb2D.position, groundRight * 0.5f, Color.green, Time.fixedDeltaTime);

            // その方向・移動入力・移動速度から、currentVelocity全体を決める
            currentVelocity = groundRight * setMoveInput * moveSpeed;

            // 接地中はkoyoteTimeをリセットする
            coyoteRemaining = maxCoyoteTime;

        }
        else
        {
            // 接地中以外の処理
            // 重力に合わせて、入力で操作する速度の成分を切り替え
            switch (gravityController.GravityVector)
            {
                case { x: 0, y: 1 }:
                    // 上の処理
                    currentVelocity.x = -setMoveInput * moveSpeed;
                    break;
                case { x: 0, y: -1 }:
                    // 下の処理
                    currentVelocity.x = setMoveInput * moveSpeed;
                    break;
                case { x: -1, y: 0 }:
                    // 左の処理
                    currentVelocity.y = -setMoveInput * moveSpeed;
                    break;
                case { x: 1, y: 0 }:
                    // 右の処理
                    currentVelocity.y = setMoveInput * moveSpeed;
                    break;
                default:
                    // その他の処理
                    break;
            }

            // 接地中以外での重力の影響を受ける方向の速度を計算
            Vector2 gravityDeltaVelocity = gravityController.GravityVector * gravityController.GravityAcceleration * Time.fixedDeltaTime;
            currentVelocity += gravityDeltaVelocity;
        }

        //Rigidbody2Dでの移動処理を行う
        rb2D.linearVelocity = currentVelocity;

        //未処理のジャンプ入力があれば、接地状態を確認してジャンプ処理を行う
        if(jumpBufferRemaining > 0f)
        {
            if(isGrounded || coyoteRemaining > 0f)
            {
                Jump(isGrounded);
                jumpBufferRemaining = 0f;
                coyoteRemaining = 0f;
            }
            else
            {
                jumpBufferRemaining = Mathf.Max(0f, jumpBufferRemaining - Time.fixedDeltaTime);
            }
        }

        //　ジャンプ予約処理が終わった後
        if(!isGrounded && coyoteRemaining > 0f)
        {
            coyoteRemaining = Mathf.Max(0f, coyoteRemaining - Time.fixedDeltaTime);
        }

    }

    void Jump(bool groundedAtTakeoff)
    {
        // ジャンプ処理の実装
        // ここでは単純に重力の逆方向に速度を加える

        Vector2 up = -gravityController.GravityVector;

        if (!groundedAtTakeoff)
        {
            Vector2 velocity = rb2D.linearVelocity;
            float upSpeed = Vector2.Dot(velocity, up);

            if (upSpeed < 0f)
            {
                rb2D.linearVelocity = velocity - up * upSpeed;
            }
        }

        rb2D.AddForce(up * jumpForce, ForceMode2D.Impulse);

        // ジャンプ時のデバッグログ
        // Debug.Log($"接地={groundedAtTakeoff}, 踏み切り高さ={Vector2.Dot(rb2D.position, up):F3}, 上向き初速={Vector2.Dot(rb2D.linearVelocity, up):F3}");

        ignoreGroundOnce = true;
    }


}
