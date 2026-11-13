using UnityEngine;

// 仮コースの収集物。小さいものと、寄り道用の大きいものを区別する。
public class CourseCollectible : MonoBehaviour
{
    [SerializeField] CoursePrototypeController course;
    [SerializeField] bool special;
    bool collected;

    public bool IsSpecial
    {
        get { return special; }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player")) return;

        collected = true;
        course.Collect(special);
        gameObject.SetActive(false);
    }
}
