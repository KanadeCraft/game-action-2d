using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// TestStage05専用。星の数を表示し、既存のGoalへの到着と再挑戦を扱う。
// Playerの入力・移動・接地の処理には手を加えない。
public class GravityCollectionTrial : MonoBehaviour
{
    public int CollectedCount { get; private set; }
    public bool HasReachedGoal { get; private set; }

    AudioSource collectSound;
    AudioClip[] notes;
    Font hudFont;
    GUIStyle titleStyle;
    GUIStyle detailStyle;
    GUIStyle buttonStyle;
    bool restarting;
    float feedbackRemaining;

    [SerializeField] StageController stageController;

    void Awake()
    {
        collectSound = gameObject.AddComponent<AudioSource>();
        collectSound.playOnAwake = false;
        collectSound.spatialBlend = 0f;
        collectSound.volume = 0.2f;

        // 短い音を３つ作る。集めるほど音程が上がる。
        notes = new AudioClip[3];
        float[] frequencies = { 523.25f, 659.25f, 783.99f };
        for (int note = 0; note < notes.Length; note++)
        {
            const int sampleRate = 22050;
            float[] samples = new float[3308];
            for (int i = 0; i < samples.Length; i++)
            {
                float progress = (float)i / samples.Length;
                float envelope = Mathf.Min(1f, progress * 30f) * (1f - progress) * (1f - progress);
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequencies[note] * i / sampleRate) * envelope * 0.5f;
            }
            notes[note] = AudioClip.Create("Trial star " + (note + 1), samples.Length, 1, sampleRate, false);
            notes[note].SetData(samples, 0);
        }
    }

    void Update()
    {
        feedbackRemaining = Mathf.Max(0f, feedbackRemaining - Time.deltaTime);
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartTrial();
        }
#endif
    }

    public void CollectStar()
    {
        CollectedCount++;
        feedbackRemaining = 0.35f;
        collectSound.PlayOneShot(notes[Mathf.Clamp(CollectedCount - 1, 0, notes.Length - 1)]);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HasReachedGoal = true;
        }
    }

    public void RestartTrial()
    {
        stageController.RestartStage();
    }

    void OnGUI()
    {
        if (titleStyle == null)
        {
            hudFont = Font.CreateDynamicFontFromOSFont(new[] { "Yu Gothic UI", "Meiryo", "Arial" }, 18);
            titleStyle = new GUIStyle(GUI.skin.label) { font = hudFont, fontSize = 22, fontStyle = FontStyle.Bold };
            detailStyle = new GUIStyle(GUI.skin.label) { font = hudFont, fontSize = 15, wordWrap = true };
            buttonStyle = new GUIStyle(GUI.skin.button) { font = hudFont, fontSize = 16 };
        }

        float width = Mathf.Min(440f, Screen.width - 24f);
        GUI.Box(new Rect(12f, 12f, width, 142f), GUIContent.none);
        titleStyle.normal.textColor = HasReachedGoal ? new Color(0.55f, 1f, 0.72f)
            : feedbackRemaining > 0f ? new Color(1f, 0.88f, 0.35f) : Color.white;
        GUI.Label(new Rect(26f, 22f, width - 28f, 32f),
            (HasReachedGoal ? "ゴール！　" : "スター　") + CollectedCount + " / 3", titleStyle);

        string message = HasReachedGoal
            ? CollectedCount == 3 ? "３個とも取れた！ 別の飛び方も試せるよ。" : "クリア！ 次は曲がる高さを変えてみよう。"
            : "上へ落ちて、横へ。曲がる高さで取れる数が変わる。\nスターを逃してもゴールできる。";
        GUI.Label(new Rect(26f, 57f, width - 28f, 46f), message, detailStyle);
        if (GUI.Button(new Rect(26f, 110f, width - 28f, 30f), "もう一度　[R]", buttonStyle))
        {
            RestartTrial();
        }
    }

    void OnDestroy()
    {
        if (notes != null)
        {
            foreach (AudioClip note in notes) if (note != null) Destroy(note);
        }
        if (hudFont != null) Destroy(hudFont);
    }
}
