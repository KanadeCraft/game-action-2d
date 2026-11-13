using UnityEngine;

// TestStage06の仮コース専用。開始地点、収集数、ゴール表示をまとめる。
// Playerの移動や重力のルールは変更しない。
[DefaultExecutionOrder(-200)]
public class CoursePrototypeController : MonoBehaviour
{
    [SerializeField] Transform courseRoot;
    [SerializeField] Transform startPoint;
    [SerializeField] StageController stageController;
    [SerializeField] AudioClip pickupSound;

    Rigidbody2D playerBody;
    AudioSource audioSource;
    int coins;
    int specials;
    int totalCoins;
    int totalSpecials;
    bool finished;
    float feedbackRemaining;
    Font font;
    GUIStyle titleStyle;
    GUIStyle textStyle;
    GUIStyle buttonStyle;

    public int CollectedCoins { get { return coins; } }
    public int CollectedSpecials { get { return specials; } }
    public bool IsFinished { get { return finished; } }

    void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerBody = player.GetComponent<Rigidbody2D>();

        // 06が有効なときだけ移す。保存済みPlayerの元の配置はそのまま残す。
        player.transform.position = startPoint.position;
        playerBody.position = startPoint.position;
        playerBody.linearVelocity = Vector2.zero;
        playerBody.angularVelocity = 0f;
        Physics2D.SyncTransforms();

        CourseCollectible[] items = courseRoot.GetComponentsInChildren<CourseCollectible>();
        foreach (CourseCollectible item in items)
        {
            if (item.IsSpecial) totalSpecials++;
            else totalCoins++;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0.15f;
    }

    void Update()
    {
        feedbackRemaining = Mathf.Max(0f, feedbackRemaining - Time.deltaTime);
    }

    public void Collect(bool special)
    {
        if (special) specials++;
        else coins++;

        feedbackRemaining = 0.25f;
        audioSource.pitch = special ? 1.4f : 1f + (coins % 4) * 0.06f;
        audioSource.PlayOneShot(pickupSound);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) finished = true;
    }

    string AreaName()
    {
        float x = courseRoot.InverseTransformPoint(playerBody.position).x;
        if (x < 20f) return "01　庭から外壁へ";
        if (x < 50f) return "02　屋根の道";
        if (x < 80f) return "03　回廊を抜ける";
        return "04　吹き抜けの先へ";
    }

    void OnGUI()
    {
        if (titleStyle == null)
        {
            font = Font.CreateDynamicFontFromOSFont(new[] { "Yu Gothic UI", "Meiryo", "Arial" }, 18);
            titleStyle = new GUIStyle(GUI.skin.label) { font = font, fontSize = 20, fontStyle = FontStyle.Bold };
            textStyle = new GUIStyle(GUI.skin.label) { font = font, fontSize = 14, wordWrap = true };
            buttonStyle = new GUIStyle(GUI.skin.button) { font = font, fontSize = 15 };
        }

        float width = Mathf.Min(450f, Screen.width - 24f);
        GUI.Box(new Rect(12f, 12f, width, 151f), GUIContent.none);
        titleStyle.normal.textColor = finished ? new Color(0.5f, 1f, 0.7f)
            : feedbackRemaining > 0f ? new Color(1f, 0.85f, 0.4f) : Color.white;
        GUI.Label(new Rect(25f, 22f, width - 26f, 30f), finished ? "ゴール！" : AreaName(), titleStyle);
        GUI.Label(new Rect(25f, 56f, width - 26f, 28f),
            "小さい収集物 " + coins + " / " + totalCoins + "　特別な収集物 " + specials + " / " + totalSpecials, textStyle);
        GUI.Label(new Rect(25f, 85f, width - 26f, 26f),
            finished ? "収集物を逃してもクリア。別の道も試せるよ。" : "A/D 移動　Space ジャンプ　矢印 重力　R やり直し", textStyle);
        if (GUI.Button(new Rect(25f, 119f, width - 26f, 29f), "最初からやり直す", buttonStyle))
            stageController.RestartStage();
    }

    void OnDestroy()
    {
        if (font != null) Destroy(font);
    }
}
