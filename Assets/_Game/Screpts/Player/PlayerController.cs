using UnityEngine;

public class PlayerController : MonoBehaviour
{
    GroundDetector2D groundDetector2D;
    PlayerInputReader playerInputReader;
    PlayerMotor2D playerMotor2D;
    PlayerHealth playerHealth;
    [SerializeField] GravityController gravityController;

    [SerializeField] PlayerVisualController visualController;

    [SerializeField] StageController stageController;
    bool restartInputFlag;

    //ジャンプ入力のフラグを保持する変数
    bool jumpInputFlag;

    //重力入力のフラグを保持する変数
    bool changeGravityInputFlag;

    [Header("死亡からリスタートまでの時間"), Min(0f), SerializeField]
    float deathRestartDelay = 1f;

    bool hasStartedDeath;
    float deathRestartRemaining;

    void Awake()
    {
        groundDetector2D = GetComponent<GroundDetector2D>();
        playerInputReader = GetComponent<PlayerInputReader>();
        playerMotor2D = GetComponent<PlayerMotor2D>();
        playerHealth = GetComponent<PlayerHealth>();

        jumpInputFlag = false;
        changeGravityInputFlag = false;
    }

    void Update()
    {
        HandleRestartInput();

        if(playerHealth.IsDead)
        {
            UpdateDeath();
            return;
        }

        UpdateMovementInput();
        UpdateJumpInput();
        UpdateGravityInput();
        visualController.SetDirection(playerInputReader.MoveInput);
        visualController.SetAnimationState(playerMotor2D.IsGrounded, playerMotor2D.Velocity, playerInputReader.RunInput);
        visualController.SetInvincibleState(playerHealth.IsInvincible);
    }

    void FixedUpdate()
    {
        if (playerHealth.IsDead)
        {
            playerMotor2D.StopMovement();
            return;
        }

        // CheckGroundの呼び出し
        groundDetector2D.CheckGround();

        // GroundDetector2Dから接地状態と床の法線を取得してPlayerMotor2Dに渡す
        playerMotor2D.SetIsGrounded(groundDetector2D.IsGrounded);
        playerMotor2D.SetGroundNormal(groundDetector2D.GroundNormal);

        // PlayerMotor2DのSimulateMovementを呼び出す
        playerMotor2D.SimulateMovement();

        // Motorが決めた、実際のしゃがみ状態を渡す
        visualController.SetSquatState(playerMotor2D.IsSquatting);
    }

    void HandleRestartInput()
    {
        bool currentRestartInput = playerInputReader.ResetInput;

        // 今回は押されていて、前回は押されていなかった
        if (currentRestartInput && !restartInputFlag)
        {
            stageController.RestartStage();
        }

        // 次のUpdateで比較するために保存
        restartInputFlag = currentRestartInput;
    }

    void UpdateMovementInput()
    {
        // PlayerInputReaderから入力値を取得してPlayerMotor2Dに渡す
        playerMotor2D.SetMoveInput(playerInputReader.MoveInput);
        playerMotor2D.SetRunInput(playerInputReader.RunInput);
        playerMotor2D.SetSquatInput(playerInputReader.SquatInput);
    }

    void UpdateJumpInput()
    {
        // 今回のボタンの状態を取得
        bool currentJumpInput = playerInputReader.JumpInput;

        // 押している間はtrue、離したらfalseを毎回渡す
        playerMotor2D.SetJumpHeld(currentJumpInput);

        // 今回は押されていて、前回は押されていなかった場合
        if (currentJumpInput && !jumpInputFlag)
        {
            playerMotor2D.SetJumpInput(true);
        }

        // 次回の比較に使うため、今回の状態を保存
        jumpInputFlag = currentJumpInput;
    }

    void UpdateGravityInput()
    {
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
    }

    void UpdateDeath()
    {
        // 死亡した最初の１回だけ行う
        if (!hasStartedDeath)
        {
            hasStartedDeath = true;
            deathRestartRemaining = deathRestartDelay;

            playerMotor2D.StopMovement();

            visualController.SetInvincibleState(false);
            visualController.SetDirection(0f);
            visualController.SetAnimationState(playerMotor2D.IsGrounded, Vector2.zero, false);

            Debug.Log("HPが0になった：死亡");
        }

        deathRestartRemaining = Mathf.Max(0f, deathRestartRemaining - Time.deltaTime);

        if (deathRestartRemaining <= 0f)
        {
            stageController.RestartStage();
        }
    }
}
