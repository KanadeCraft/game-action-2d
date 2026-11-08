using UnityEngine;

public class CameraController : MonoBehaviour
{
    //PlayerのGameObjectを取得するための変数
    GameObject player;

    //GravityControllerの重力方向を取得するための変数
    [SerializeField] GravityController gravityController;

    //現在のカメラの向きを設定するための変数
    Vector3 currentRotation;

    //現在の重力の向きを設定するための変数
    Vector2 currentGravityDirection;

    //カメラの目標回転を設定するための変数
    Vector3 targetRotation;

    //回転にかける時間を設定するための変数
    [SerializeField] float rotationSpeed = 0.25f;

    //経過時間を計測するための変数
    float elapsedTime = 0f;

    //カメラを調整するための変数
    [SerializeField] float cameraOffset = 0.5f;

    void Awake()
    {
        //PlayerのTransformを取得
        player = GameObject.FindGameObjectWithTag("Player");

        currentRotation = transform.eulerAngles;
        currentGravityDirection = gravityController.GravityVector;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void LateUpdate()
    {
        // 現在の重力方向とGravityControllerの重力方向を比較して、異なる場合に回転を変更する
        if (currentGravityDirection != gravityController.GravityVector)
        {
            currentRotation = transform.eulerAngles;

            // 重力方向に応じてカメラの回転を変更する
            switch (gravityController.GravityVector)
            {
                case { x: 0, y: 1 }:
                    // 上の処理
                    targetRotation = new Vector3(0, 0, 180);
                    break;
                case { x: 0, y: -1 }:
                    // 下の処理
                    targetRotation = new Vector3(0, 0, 0);
                    break;
                case { x: -1, y: 0 }:
                    // 左の処理
                    targetRotation = new Vector3(0, 0, -90);
                    break;
                case { x: 1, y: 0 }:
                    // 右の処理
                    targetRotation = new Vector3(0, 0, 90);
                    break;
                default:
                    // その他の処理
                    break;
            }

            //経過時間をリセット
            elapsedTime = 0f;

            // 現在の重力方向を更新
            currentGravityDirection = gravityController.GravityVector;
        }

        // 経過時間を更新
        elapsedTime += Time.deltaTime;

        // 進捗率 = 経過時間 ÷ 回転にかける時間
        float progress = elapsedTime / rotationSpeed;

        // Mathf.SmoothStepを使用して、進捗率を滑らかにする
        float progressSmooth = Mathf.SmoothStep(0f, 1f, progress);

        // カメラの回転を補間して滑らかにする
        transform.rotation = Quaternion.Slerp(Quaternion.Euler(currentRotation), Quaternion.Euler(targetRotation), progressSmooth);

        //カメラを現在の向きから見て下方向にずらす
        Vector2 cameraOffsetPosition = -transform.up * cameraOffset;

        // カメラの位置をPlayerの位置に合わせる
        transform.position = new Vector3(player.transform.position.x + cameraOffsetPosition.x, player.transform.position.y + cameraOffsetPosition.y, transform.position.z);
    }
}
