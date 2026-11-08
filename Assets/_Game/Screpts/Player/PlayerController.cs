using UnityEngine;

public class PlayerController : MonoBehaviour
{
    GroundDetector2D groundDetector2D;
    PlayerInputReader playerInputReader;
    PlayerMotor2D playerMotor2D;
    [SerializeField] GravityController gravityController;

    [SerializeField] PlayerVisualController visualController;

    //ジャンプ入力のフラグを保持する変数
    bool jumpInputFlag;

    //重力入力のフラグを保持する変数
    bool changeGravityInputFlag;

    void Awake()
    {
        groundDetector2D = GetComponent<GroundDetector2D>();
        playerInputReader = GetComponent<PlayerInputReader>();
        playerMotor2D = GetComponent<PlayerMotor2D>();

        jumpInputFlag = false;
        changeGravityInputFlag = false;
    }

    void Update()
    {
        // PlayerInputReaderから入力値を取得してPlayerMotor2Dに渡す
        playerMotor2D.SetMoveInput(playerInputReader.MoveInput);

        // 今回のボタンの状態を取得
        bool currentJumpInput = playerInputReader.JumpInput;

        // 今回は押されていて、前回は押されていなかった場合
        if (currentJumpInput && !jumpInputFlag)
        {
            playerMotor2D.SetJumpInput(true);
        }

        // 次回の比較に使うため、今回の状態を保存
        jumpInputFlag = currentJumpInput;

        // 今回の重力入力の状態を取得
        bool currentGravityInput = playerInputReader.GravityInput != Vector2.zero;

        // 方向入力がある場合のみ、一度だけGravityControllerに重力向きを渡す
        if (currentGravityInput && !changeGravityInputFlag)
        {
            // PlayerInputReaderから入力値を取得してGravityControllerに重力向きを渡す
            gravityController.SetGravityDirection(playerInputReader.GravityInput);
        }

        // 次回の比較に使うため、今回の状態を保存
        changeGravityInputFlag = currentGravityInput;

        visualController.SetDirection(playerInputReader.MoveInput);
    }

    void FixedUpdate()
    {
        // CheckGroundの呼び出し
        groundDetector2D.CheckGround();

        // GroundDetector2Dから接地状態と床の法線を取得してPlayerMotor2Dに渡す
        playerMotor2D.SetIsGrounded(groundDetector2D.IsGrounded);
        playerMotor2D.SetGroundNormal(groundDetector2D.GroundNormal);

        // PlayerMotor2DのSimulateMovementを呼び出す
        playerMotor2D.SimulateMovement();
    }
}
