using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviourSingleton<SceneLoader>
{
    private readonly string UI_SCENE_NAME = "UIScene";
    private readonly string GAME_SCENE_NAME = "GameScene";

    private bool _isGameSceneLoaded;

    protected override void Awake()
    {
        base.Awake();

#if UNITY_EDITOR
        if (!IsSceneLoaded(UI_SCENE_NAME))
        {
            SceneManager.LoadSceneAsync(UI_SCENE_NAME, LoadSceneMode.Additive);
        }
        //if (!IsSceneLoaded(GAME_SCENE_NAME)) {
        //    LoadGameScene();
        //}
#else
        SceneManager.LoadScene(UI_SCENE_NAME, LoadSceneMode.Additive);
#endif
    }

    private bool IsSceneLoaded(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == sceneName)
            {
                return true;
            }
        }
        return false;
    }

    public void LoadGameScene()
    {
        if (_isGameSceneLoaded) {
            Debug.LogError("Game scene is already loaded.");
            return;
        }

        _isGameSceneLoaded = true;
        SceneManager.LoadSceneAsync(GAME_SCENE_NAME, LoadSceneMode.Additive);
    }
}
