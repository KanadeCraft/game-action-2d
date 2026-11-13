using UnityEngine;
using UnityEngine.SceneManagement;

public class StageController : MonoBehaviour
{
    bool restarting;

    /// <summary>
    /// 外部から各種入力できるようにするメソッド 
    /// </summary>
    public void RestartStage()
    {
        // 再読み込み中に、重ねて要求されないようにする
        if (restarting)
        {
            return;
        }
        restarting = true;

        // 現在のシーンを再読み込みする
#if UNITY_EDITOR
        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(gameObject.scene.path, new LoadSceneParameters(LoadSceneMode.Single));
#else
        SceneManager.LoadScene(gameObject.scene.buildIndex);
#endif
    }
}
