using UnityEngine;

public class GravityController : MonoBehaviour
{
    /// <summary>
    /// 重力の方向を表すベクトル。デフォルトでは下方向(Vector2.down)に設定
    /// 重力の大きさはgravityAccelerationで調整可能(一時的)
    /// </summary>
    Vector2 gravityVector = Vector2.down;
    [SerializeField] float gravityAcceleration = 9.81f;

    /// <summary>
    /// それぞれを外部から参照できるようにするプロパティ
    /// </summary>
    public Vector2 GravityVector
    {
        get { return gravityVector; }
    }

    public float GravityAcceleration
    {
        get { return gravityAcceleration; }
    }
    /// <summary>
    /// 外部から重力を入力できるようにするメソッド
    /// </summary>

    // 旧バージョンの重力方向設定メソッド
    public void OldSetGravityDirection(Vector2 gravityDirection)
    {
        //押されたキーに対応した重力方向に変更する
        switch (gravityDirection)
        {
            case { x: 0, y: 1 }:
                // 上の処理
                gravityVector = gravityDirection;
                break;
            case { x: 0, y: -1 }:
                // 下の処理
                gravityVector = gravityDirection;
                break;
            case { x: -1, y: 0 }:
                // 左の処理
                gravityVector = gravityDirection;
                break;
            case { x: 1, y: 0 }:
                // 右の処理
                gravityVector = gravityDirection;
                break;
            default:
                // その他の処理
                break;
        }
    }

    // 新バージョンの重力方向設定メソッド
    public void SetGravityDirection(Vector2 gravityDirection)
    {
        // 右矢印で右方向へ、左矢印で左方向へ、上矢印で重力を180°変更する
        switch (gravityDirection)
        {
            case { x: 0, y: 1 }:
                // 上の処理
                gravityVector = -gravityVector;
                break;
            case { x: -1, y: 0 }:
                // 左の処理
                gravityVector = new Vector2(gravityVector.y, -gravityVector.x);
                break;
            case { x: 1, y: 0 }:
                // 右の処理
                gravityVector = new Vector2(-gravityVector.y, gravityVector.x);
                break;
            default:
                // その他の処理
                break;
        }
    }
}
