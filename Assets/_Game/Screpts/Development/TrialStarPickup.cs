using UnityEngine;

// TestStage05の任意収集物。接触を一度だけ数え、広がって消える。
public class TrialStarPickup : MonoBehaviour
{
    [SerializeField] GravityCollectionTrial trial;

    public bool IsCollected { get; private set; }

    SpriteRenderer[] visuals;
    Color[] originalColors;
    Vector3 originalScale;
    float effectTime;

    void Awake()
    {
        visuals = GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[visuals.Length];
        for (int i = 0; i < visuals.Length; i++) originalColors[i] = visuals[i].color;
        originalScale = transform.localScale;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsCollected || !other.CompareTag("Player")) return;

        IsCollected = true;
        GetComponent<Collider2D>().enabled = false;
        trial.CollectStar();
    }

    void Update()
    {
        if (!IsCollected) return;

        effectTime += Time.deltaTime;
        float progress = Mathf.Clamp01(effectTime / 0.22f);
        transform.localScale = originalScale * Mathf.Lerp(1f, 2.2f, progress);
        for (int i = 0; i < visuals.Length; i++)
        {
            Color color = originalColors[i];
            color.a *= 1f - progress;
            visuals[i].color = color;
        }
        if (progress >= 1f) gameObject.SetActive(false);
    }
}
