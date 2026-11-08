using UnityEngine;

public class GroundDetector2D : MonoBehaviour
{
    // 重力の方向を取得するためにGravityControllerを参照
    [SerializeField] GravityController gravityController;

    // 接地判定のための当たり判定
    BoxCollider2D playerCollider;

    // 接地判定のためのレイヤーマスク
    [SerializeField] LayerMask groundLayerMask;


    // 接地判定の結果を保持するための配列(仮で8)
    // 本体のBoxが触れている面を取得して、その中から床を選ぶために、複数の接触点を取得する必要がある
    ContactPoint2D[] groundHits = new ContactPoint2D[8];

    // 接地判定の条件を設定するための変数
    ContactFilter2D contactFilter;

    // 床の傾きを指定するための変数(どの傾きまで床として認識するのかの判定用)
    [Header("床の角度の設定"), SerializeField]
    float maxGroundAngle = 45f;

    // 床の角度の保存用
    float groundAngle = 0f;

    //接地状態のフラグ
    bool isGrounded = false;

    // 床の法線を保持する変数
    Vector2 groundNormal = Vector2.zero;

    void Awake()
    {
        playerCollider = GetComponent<BoxCollider2D>();

        // 接地判定の条件 
        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(groundLayerMask);
        contactFilter.useTriggers = false;

    }

    public void CheckGround()
    {
        groundAngle = 0f;
        isGrounded = false;
        groundNormal = Vector2.zero;

        // 【GetContactsの実行】接地判定を行うためプレイヤーの当たり判定を基準に、検知条件、結果を格納する配列での判定
        // 接触候補を取得し、法線の条件を満たす面を床とする

        int hitCount = playerCollider.GetContacts(contactFilter, groundHits);

        for (int i = 0; i < hitCount; i++)
        {
            // ヒットした相手の情報を取得
            ContactPoint2D hit = groundHits[i];

            // ヒットした相手の法線と重力方向の角度を計算し角度を出す
            groundAngle = Vector2.Angle(hit.normal, -gravityController.GravityVector);

            // ヒットした相手の名前と、足元の面の法線をログに出力
            Debug.Log($"何かに衝突しました: {hit.collider.name} / 足元の面の法線: {hit.normal}");
            Debug.Log($"床の角度: {groundAngle}");
            Debug.DrawRay(hit.point, hit.normal * 0.5f, Color.red, Time.fixedDeltaTime);

            if (groundAngle <= maxGroundAngle)
            {
                isGrounded = true;
                groundNormal = hit.normal;
                break;
            }
        }
        Debug.Log($"接地判定: {isGrounded}");
    }

    /// <summary>
    /// 地面判定や法線を外部から参照できるようにするプロパティ
    /// </summary>
    public bool IsGrounded
    {
        get { return isGrounded; }
    }

    public Vector2 GroundNormal
    {
        get { return groundNormal; }
    }
}
