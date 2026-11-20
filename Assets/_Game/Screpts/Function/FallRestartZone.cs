using UnityEngine;

public class FallRestartZone : MonoBehaviour
{
    [SerializeField] StageController stageController;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            stageController.RestartStage();
        }
    }
}
