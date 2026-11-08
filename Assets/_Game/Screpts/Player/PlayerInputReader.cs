using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    float moveValue; // 入力値を格納する変数
    bool jumpValue; // ジャンプの入力値を格納する変数
    Vector2 gravityValue; // 重力の入力方向を格納する変数

    // Moveのアクション操作が行われた場合の処理
    void OnMove(InputValue value)
    {
        moveValue = value.Get<float>(); // Moveの入力値を取得している
    }

    // Jumpのアクション操作が行われた場合の処理
    void OnJump(InputValue value)
    {
        jumpValue = value.isPressed; // Jumpボタンの押下状態を取得している
    }

    // GravityDirectionのアクション操作が行われた場合の処理
    void OnGravityDirection(InputValue value)
    {
        gravityValue = value.Get<Vector2>(); // GravityDirectionの入力値を取得している
    }

    // 入力値を外部から取得できるようにするプロパティ
    public float MoveInput
    {
        get
        {
            return moveValue;
        }
    }

    public bool JumpInput
    {
        get
        {
            return jumpValue;
        }
    }

    public Vector2 GravityInput
    {
        get
        {
            return gravityValue;
        }
    }
}
