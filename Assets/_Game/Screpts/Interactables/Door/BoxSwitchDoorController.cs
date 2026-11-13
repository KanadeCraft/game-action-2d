using UnityEngine;

public class BoxSwitchDoorController : MonoBehaviour
{
    // ‘Î‰‚µ‚Ä‚¢‚éSwitch‚ÆBoxSwitchInteractable‚ÌQÆ‚ğæ“¾‚·‚é‚½‚ß‚Ì•Ï”
    [SerializeField] GameObject boxSwitch;
    BoxSwitchInteractable boxSwitchInteractable;

    Transform[] doorChildren;
    int doorChildIndex;

    private void Awake()
    {
        boxSwitchInteractable = boxSwitch.GetComponent<BoxSwitchInteractable>();

        doorChildren = new Transform[this.transform.childCount];
        doorChildIndex = 0;

        foreach (Transform child in this.transform)
        {
            doorChildren[doorChildIndex++] = child;
        }
    }

    void Update()
    {
        if (boxSwitchInteractable.IsActivated)
        {
            for (int i = 0; i < doorChildren.Length; i++)
            {
                doorChildren[i].gameObject.SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < doorChildren.Length; i++)
            {
                doorChildren[i].gameObject.SetActive(true);
            }
        }
    }
}
