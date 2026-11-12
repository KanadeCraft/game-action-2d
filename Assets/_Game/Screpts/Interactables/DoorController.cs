using UnityEngine;

public class DoorController : MonoBehaviour
{
    // WallSwitchInteractable‚ÌQÆ‚ğæ“¾‚·‚é‚½‚ß‚Ì•Ï”
    [SerializeField] WallSwitchInteractable wallSwitchInteractable;

    void Update()
    {
        if (wallSwitchInteractable.IsActivated)
        {
            this.gameObject.SetActive(false);
        }
    }
}
