using UnityEngine;

public class PlayerMotor2D : MonoBehaviour
{
    // Inspectorで設定する参照と調整値。既存のフィールド名を維持
    [SerializeField] GravityController gravityController;

    // 設定一覧
    [Header("移動関連")]
    [Header("移動速度"), SerializeField] float moveSpeed = 5f;
    [Header("走る速度"), SerializeField] float runSpeed = 8f;
    [Header("しゃがみ速度"), SerializeField] float squatSpeed = 2f;

    [Header("ジャンプ関連")]
    [Header("ジャンプ力"), SerializeField] float jumpForce = 5f;
    [Header("ジャンプの猶予時間"), SerializeField] float maxJumpBufferTime = 0.1f;
    [Header("コヨーテタイム"), SerializeField] float maxCoyoteTime = 0.08f;
    [Header("離したときに残す上向き速度の割合")]
    [Range(0f, 1f), SerializeField] float jumpCutRatio = 0.5f;

    [Header("しゃがみ・頭上関連")]
    [Header("しゃがみ時のColliderの高さ"), SerializeField] float squatColliderHeight = 0.46f;
    [Header("立ち上がりを妨げるレイヤー"), SerializeField] LayerMask standObstacleMask;
    // `headVisualPadding` は、今の「見える頭」がColliderより上にはみ出している0.02を補う値。通常は `(1 - 0.96) / 2 = 0.02`、しゃがみも `(0.5 - 0.46) / 2 = 0.02` なので共通にできる。
    [Header("Colliderの頭から見える頭までの余白"), SerializeField] float headVisualPadding = 0.02f;

    const float ceilingSkin = 0.001f;                   // 検出距離を使い切らず、少し手前で止める余白
    const float minimumJumpSpace = 0.005f;              // これ以下の空きならジャンプを開始しない基準
    const float headBoxThickness = 0.002f;              // 頭の位置に置く検査用の薄い四角の厚み
    const float headSideInset = 0f;                     // 検査幅を狭めない。今回、端から天井の下へ入る検査で幅の必要性を確認した

    // 自身のコンポーネントと、通常姿勢のCollider形状
    Rigidbody2D rb2D;
    BoxCollider2D playerCollider;
    Vector2 originalColliderSize;
    Vector2 originalColliderOffset;

    // Controllerから受け取る入力。
    float setMoveInput;
    bool setRunInput;
    bool setSquatInput;
    bool setJumpHeld;

    // 接地・姿勢と、更新をまたいで保持する値
    bool isGrounded;
    bool isSquatting;
    bool ignoreGroundOnce;  // 次の物理更新を保護するフラグ
    bool canCutJump;        // このジャンプの上昇を、まだ一度だけ弱められるか

    Vector2 groundNormal;
    Vector2 previousGravity;

    float jumpBufferRemaining;
    float coyoteRemaining;

    // 頭上検査の条件と結果を再利用する
    ContactFilter2D standFilter;
    Collider2D[] standHits = new Collider2D[8];
    RaycastHit2D[] ceilingHits = new RaycastHit2D[8];   //検査結果を入れる8件分の配列

    // それぞれの行動中のフラグを外部から取得できるようにするプロパティ
    public bool IsGrounded
    {
        get
        {
            return isGrounded && !ignoreGroundOnce;
        }
    }

    public Vector2 Velocity
    {
        get
        {
            return rb2D.linearVelocity;
        }
    }

    public bool IsSquatting
    {
        get
        {
            return isSquatting;
        }
    }

    // 箱を押す判定に使う、入力によって移動しようとしている速度。
    // 実際の速度と違い、落下やジャンプの速度は含めない。
    // プロパティ
    public Vector2 RequestedMoveVelocity
    {
        get
        {
            Vector2 gravity = gravityController.GravityVector;
            Vector2 right = new Vector2(-gravity.y, gravity.x);
            return right * setMoveInput * GetMoveSpeed();
        }
    }

    void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        // 初期化処理
        previousGravity = gravityController.GravityVector;
        isGrounded = false;

        playerCollider = GetComponent<BoxCollider2D>();
        originalColliderSize = playerCollider.size;
        originalColliderOffset = playerCollider.offset;

        standFilter = new ContactFilter2D();
        standFilter.SetLayerMask(standObstacleMask);
        standFilter.useTriggers = false;
    }

    /// <summary>
    /// 外部から各種入力できるようにするメソッド 
    /// </summary>
    public void SetMoveInput(float value)
    {
        setMoveInput = value;
    }

    public void SetRunInput(bool value)
    {
        setRunInput = value;
    }

    public void SetSquatInput(bool value)
    {
        setSquatInput = value;
    }

    public void SetJumpInput(bool value)
    {
        // ジャンプバッファ時間を設定
        if (value)
        {
            jumpBufferRemaining = maxJumpBufferTime;
        }
    }

    public void SetJumpHeld(bool value)
    {
        setJumpHeld = value;
    }

    public void SetIsGrounded(bool value)
    {
        isGrounded = value;
    }

    public void SetGroundNormal(Vector2 value)
    {
        groundNormal = value;
    }

    // 1回の物理更新で行う処理
    public void SimulateMovement()
    {
        IgnoreGroundAfterJump();
        UpdateSquatState();

        Vector2 velocity = rb2D.linearVelocity;      // 現在の速度を取得
        Vector2 gravity = gravityController.GravityVector;  // 現在の重力の方向を取得


        velocity = ApplyGravityChange(velocity, gravity);
        velocity = CalculateMovementVelocity(velocity);
        velocity = ProcessJumpInput(velocity);
        UpdateCoyoteTime();

        // 可変ジャンプ適用のため
        velocity = ApplyVariableJump(velocity);
        // ジャンプまで含めて作った速度を、頭上の空きに合わせて制限する。
        velocity = LimitCeilingVelocity(velocity);

        //Rigidbody2Dでの移動処理を行う
        rb2D.linearVelocity = velocity;
    }

    void IgnoreGroundAfterJump()
    {
        if (ignoreGroundOnce)
        {
            isGrounded = false;
            ignoreGroundOnce = false;
        }
    }

    Vector2 ApplyGravityChange(Vector2 velocity, Vector2 gravity)
    {
        if (gravity == -previousGravity)
        {
            canCutJump = false;
            // 重力が180度変化した場合の処理
            // 重力方向の速度を打ち消す
            float speedAlongGravity = Vector2.Dot(velocity, gravity);

            if (speedAlongGravity < 0f)
            {
                velocity -= gravity * speedAlongGravity; // 重力方向の速度を打ち消す
            }
        }

        previousGravity = gravity;

        return velocity;
    }

    Vector2 CalculateMovementVelocity(Vector2 velocity)
    {
        // 接地状態の確認と処理
        if (isGrounded)
        {
            canCutJump = false;
            // 接地中の処理
            // 接地中は法線の方向を床と平行にする(右向き)
            Vector2 groundRight = new Vector2(groundNormal.y, -groundNormal.x);
            Debug.DrawRay(rb2D.position, groundRight * 0.5f, Color.green, Time.fixedDeltaTime);

            // その方向・移動入力・移動速度から、currentVelocity全体を決める
            velocity = groundRight * setMoveInput * GetMoveSpeed();

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
                    velocity.x = -setMoveInput * GetMoveSpeed();
                    break;
                case { x: 0, y: -1 }:
                    // 下の処理
                    velocity.x = setMoveInput * GetMoveSpeed();
                    break;
                case { x: -1, y: 0 }:
                    // 左の処理
                    velocity.y = -setMoveInput * GetMoveSpeed();
                    break;
                case { x: 1, y: 0 }:
                    // 右の処理
                    velocity.y = setMoveInput * GetMoveSpeed();
                    break;
                default:
                    // その他の処理
                    break;
            }

            // 接地中以外での重力の影響を受ける方向の速度を計算
            Vector2 gravityDeltaVelocity = gravityController.GravityVector * gravityController.GravityAcceleration * Time.fixedDeltaTime;
            velocity += gravityDeltaVelocity;
        }

        return velocity;
    }

    /// <summary>
    /// 現在の移動速度を取得する
    /// </summary>
    float GetMoveSpeed()
    {
        if (isSquatting)
        {
            return squatSpeed;
        }

        if (setRunInput)
        {
            return runSpeed;
        }

        return moveSpeed;
    }

    Vector2 ProcessJumpInput(Vector2 velocity)
    {

        //未処理のジャンプ入力があれば、接地状態を確認してジャンプ処理を行う
        if (jumpBufferRemaining > 0f)
        {
            if (isGrounded || coyoteRemaining > 0f)
            {
                // 頭上に最低限の空きがある場合だけ、ジャンプを開始する。
                if (GetCeilingDistance(velocity, minimumJumpSpace) > minimumJumpSpace)
                {
                    velocity = CalculateJumpVelocity(velocity, isGrounded);
                    canCutJump = true;
                    ignoreGroundOnce = true;
                    coyoteRemaining = 0f;
                }

                // 天井で拒否した場合も、このジャンプ予約は消費する。
                jumpBufferRemaining = 0f;
            }
            else
            {
                jumpBufferRemaining = Mathf.Max(0f, jumpBufferRemaining - Time.fixedDeltaTime);
            }
        }

        return velocity;
    }

    void UpdateCoyoteTime()
    {
        //　ジャンプ予約処理が終わった後
        if (!isGrounded && coyoteRemaining > 0f)
        {
            coyoteRemaining = Mathf.Max(0f, coyoteRemaining - Time.fixedDeltaTime);
        }
    }

    Vector2 CalculateJumpVelocity(Vector2 velocity, bool groundedAtTakeoff)
    {
        // ジャンプ処理の実装
        // ここでは単純に重力の逆方向に速度を加える
        Vector2 up = -gravityController.GravityVector;

        // コヨーテジャンプでは落下速度を取り除く
        if (!groundedAtTakeoff)
        {
            float upSpeed = Vector2.Dot(velocity, up);

            if (upSpeed < 0f)
            {
                velocity -= up * upSpeed;
            }
        }

        // 今までのImpulseと同じ速度変化にする
        velocity += up * (jumpForce / rb2D.mass);

        return velocity;
    }

    Vector2 ApplyVariableJump(Vector2 velocity)
    {
        if (!canCutJump)
        {
            return velocity;
        }

        Vector2 up = -gravityController.GravityVector.normalized;
        float upSpeed = Vector2.Dot(velocity, up);

        // 頂点や下降に入ったら、このジャンプの可変制御は終了
        if (upSpeed <= 0f)
        {
            canCutJump = false;
            return velocity;
        }

        // 押している間は、通常の上昇を続ける
        if (setJumpHeld)
        {
            return velocity;
        }

        // 離したときに、一度だけ上向き速度を弱める
        canCutJump = false;

        float remainingUpSpeed = upSpeed * jumpCutRatio;
        return velocity + up * (remainingUpSpeed - upSpeed);
    }

    // しゃがみ状態の更新
    void UpdateSquatState()
    {
        bool nextSquatState = isGrounded && setSquatInput;

        // しゃがみを解除したくても、頭上が塞がっていれば維持する
        if (isSquatting && !nextSquatState)
        {
            if (!CanStandUp())
            {
                nextSquatState = true;
            }
        }

        // 状態が変わっていなければ、Colliderの変更も不要
        if (isSquatting == nextSquatState && gravityController.GravityVector == previousGravity)
        {
            return;
        }

        isSquatting = nextSquatState;
        ApplySquatCollider();
    }

    void ApplySquatCollider()
    {
        Vector2 colliderSize = originalColliderSize;
        Vector2 colliderOffset = originalColliderOffset;

        if (isSquatting)
        {
            // Player自身の座標で見た重力方向の取得
            Vector2 localGravity = transform.InverseTransformDirection(gravityController.GravityVector.normalized);
            bool vertical = Mathf.Abs(localGravity.y) > 0.5f;
            float originalHeight;

            if (vertical)
            {
                // 重力が上下の場合
                originalHeight = originalColliderSize.y;
                colliderSize.y = squatColliderHeight;
            }
            else
            {
                // 重力が左右の場合
                originalHeight = originalColliderSize.x;
                colliderSize.x = squatColliderHeight;
            }

            float heightDifference = originalHeight - squatColliderHeight;
            colliderOffset += localGravity * (heightDifference / 2f);
        }

        playerCollider.size = colliderSize;
        playerCollider.offset = colliderOffset;
    }

    bool CanStandUp()
    {
        // しゃがみ状態から立ち上がれるかどうかを判定する
        Vector2 localGravity = transform.InverseTransformDirection(gravityController.GravityVector.normalized);
        bool vertical = Mathf.Abs(localGravity.y) > 0.5f;
        float originalHeight;
        Vector2 checkSize = originalColliderSize;

        if (vertical)
        {
            originalHeight = originalColliderSize.y;
            checkSize.y = originalHeight - squatColliderHeight;
        }
        else
        {
            originalHeight = originalColliderSize.x;
            checkSize.x = originalHeight - squatColliderHeight;
        }

        if (originalHeight <= squatColliderHeight) return true;

        Vector2 localCheckCenter = originalColliderOffset - localGravity * (squatColliderHeight / 2f);
        Vector2 checkCenter = transform.TransformPoint(localCheckCenter);
        Vector3 worldScale = transform.lossyScale;
        checkSize.x *= Mathf.Abs(worldScale.x);
        checkSize.y *= Mathf.Abs(worldScale.y);
        checkSize.x = Mathf.Max(0.001f, checkSize.x - 0.02f);
        checkSize.y = Mathf.Max(0.001f, checkSize.y - 0.02f);

        int hitCount = Physics2D.OverlapBox(checkCenter, checkSize, transform.eulerAngles.z, standFilter, standHits);
        return hitCount == 0;
    }

    // 現在位置と横移動先の頭上を調べて、使える距離を返す
    float GetCeilingDistance(Vector2 velocity, float checkDistance)
    {
        Vector2 up = -gravityController.GravityVector.normalized;

        // Playerの囲む範囲の取得
        Bounds bounds = playerCollider.bounds;
        // Playerの重力方向の計算（上下か左右）
        bool vertical = Mathf.Abs(up.y) > 0.5f;
        // 上下だった場合半分の高さYと横幅X、左右だった場合半分の高さXと横幅Y
        float halfHeight = vertical ? bounds.extents.y : bounds.extents.x;
        float headWidth = (vertical ? bounds.size.x : bounds.size.y) - headSideInset * 2f;
        // 上下だった場合横幅(0.001からX)*高さ(headBoxThickness)、左右だった場合横幅(headBoxThickness)*高さ(0.001からY)
        Vector2 checkSize = vertical
            ? new Vector2(Mathf.Max(0.001f, headWidth), headBoxThickness)
            : new Vector2(headBoxThickness, Mathf.Max(0.001f, headWidth));
        // 頭上の位置の取得（プレイヤーの中心+コライダーの頭までの距離+はみ出ている分の距離-検査用の四角の高さの半分）
        Vector2 headPosition = (Vector2)bounds.center + up * (halfHeight + headVisualPadding - headBoxThickness * 0.5f);
        // 上下の速度方向を取り除く、横方向の移動距離
        Vector2 sideStep = (velocity - up * Vector2.Dot(velocity, up)) * Time.fixedDeltaTime;
        float closestDistance = float.PositiveInfinity;

        // 現在位置と、今回の横移動先の両方で頭上を調べる。
        for (int sample = 0; sample < 2; sample++)
        {
            Vector2 origin = headPosition + sideStep * sample;
            // 現在位置または横移動先から、頭上の障害物を検査する。
            int count = Physics2D.BoxCast(origin, checkSize, 0f, up, standFilter, ceilingHits, checkDistance + ceilingSkin);
            for (int i = 0; i < count; i++)
            {
                RaycastHit2D hit = ceilingHits[i];
                // 自分と横壁や床だった場合次へ
                if (hit.collider.attachedRigidbody == rb2D) continue;
                if (Vector2.Dot(hit.normal, up) > -0.1f) continue;

                // 開始時に重なっていると、横壁でも上向きCastの逆向き法線が返る。
                // 実際のCollider間の位置関係も使い、横壁を天井と誤認しない。
                if (hit.distance <= 0f)
                {
                    // Playerの中心位置
                    Vector2 playerCenter = (Vector2)bounds.center;

                    // その中心に一番近い、相手Collider上の点
                    Vector2 closestPoint =
                        hit.collider.ClosestPoint(playerCenter);

                    // Playerの中心から、相手へ向かう方向
                    Vector2 towardObstacle = closestPoint - playerCenter;

                    // 上方向を妨げない相手なら、次の候補へ
                    if (Vector2.Dot(towardObstacle, up) <= 0f) continue;
                }

                //最小の距離を返す
                closestDistance = Mathf.Min(closestDistance, Mathf.Max(0f, hit.distance - ceilingSkin));
            }
        }

        return closestDistance;
    }

    Vector2 LimitCeilingVelocity(Vector2 velocity)
    {
        Vector2 up = -gravityController.GravityVector.normalized;
        float upSpeed = Vector2.Dot(velocity, up);
        //上昇していなければそのまま返す
        if (upSpeed <= 0f) return velocity;

        float allowedDistance = GetCeilingDistance(velocity, upSpeed * Time.fixedDeltaTime);
        float allowedSpeed = Mathf.Min(upSpeed, allowedDistance / Time.fixedDeltaTime);
        return velocity + up * (allowedSpeed - upSpeed);
    }
}
