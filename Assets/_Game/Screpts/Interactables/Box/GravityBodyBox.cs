using UnityEngine;

public class GravityBodyBox : MonoBehaviour
{
    [SerializeField] GravityController gravityController;

    [Header("箱が止まれる床の最大角度"), Range(0f, 89f), SerializeField]
    float maxGroundAngle = 45f;

    Rigidbody2D rb2D;
    Vector2 previousGravity;
    ContactPoint2D[] contacts = new ContactPoint2D[16];
    bool isGrounded;
    Vector2 groundNormal;

    void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        previousGravity = gravityController.GravityVector;
    }

    void FixedUpdate()
    {
        Vector2 gravity = gravityController.GravityVector;
        Vector2 currentVelocity = rb2D.linearVelocity;

        // 重力が変化した場合、速度をリセットする
        if (gravity != previousGravity)
        {
            currentVelocity = Vector2.zero;
        }
        previousGravity = gravity;

        currentVelocity += gravity * gravityController.GravityAcceleration * Time.fixedDeltaTime;

        int contactCount = rb2D.GetContacts(contacts);
        CheckGround(contactCount, gravity);

        if (isGrounded)
        {
            Vector2 groundRight = new Vector2(groundNormal.y, -groundNormal.x);
            float pushSpeed = GetPushSpeed(contactCount, groundRight);

            // 足場に沿う速度だけを押し入力で決める。押していなければ0。
            // 足場に垂直な速度は残し、落下まで止めないようにする。
            float normalSpeed = Vector2.Dot(currentVelocity, groundNormal);
            currentVelocity = groundRight * pushSpeed + groundNormal * normalSpeed;
        }

        rb2D.linearVelocity = currentVelocity;
    }

    void CheckGround(int contactCount, Vector2 gravity)
    {
        isGrounded = false;
        groundNormal = Vector2.zero;
        float bestAngle = maxGroundAngle;

        for (int i = 0; i < contactCount; i++)
        {
            float angle = Vector2.Angle(contacts[i].normal, -gravity);
            if (angle <= bestAngle)
            {
                isGrounded = true;
                groundNormal = contacts[i].normal;
                bestAngle = angle;
            }
        }
    }

    float GetPushSpeed(int contactCount, Vector2 groundRight)
    {
        float pushSpeed = 0f;

        for (int i = 0; i < contactCount; i++)
        {
            ContactPoint2D contact = contacts[i];
            Collider2D other = contact.collider;
            Collider2D boxCollider = contact.otherCollider;
            if (other.attachedRigidbody == rb2D)
            {
                boxCollider = other;
                other = contact.otherCollider;
            }

            Rigidbody2D otherBody = other.attachedRigidbody;
            if (otherBody == null)
            {
                continue;
            }

            PlayerMotor2D playerMotor = otherBody.GetComponent<PlayerMotor2D>();
            if (playerMotor == null)
            {
                continue;
            }

            // 古い接触情報が残っていても、今は離れていれば押していない。
            // ただし、接触しているかどうかの判定は、接触点の距離で行う。
            ColliderDistance2D distance = other.Distance(boxCollider);
            if (!distance.isValid || distance.distance > Physics2D.defaultContactOffset * 2f) //測定できなかった、または許容する距離より離れているか
            {
                continue;
            }

            Vector2 requestedVelocity = playerMotor.RequestedMoveVelocity;

            // 接触していても、箱から離れる方向の入力なら押していない。
            if (Vector2.Dot(requestedVelocity, contact.normal) <= 0f)
            {
                continue;
            }

            float requestedSpeed = Vector2.Dot(requestedVelocity, groundRight);
            if (Mathf.Abs(requestedSpeed) > Mathf.Abs(pushSpeed))
            {
                pushSpeed = requestedSpeed;
            }
        }

        return pushSpeed;
    }
}
