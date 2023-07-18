using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : NetworkBehaviour
{
    public static ScenesManager Instance;
    //[SerializeField]
    //private List<Scene> scenes;

    public enum Scene
    {
        MainMenu,
        Lobby,
        GameScene
    }

    public void Awake()
    {
        Instance = this;
        if (!SceneManager.GetActiveScene().name.Equals(Scene.MainMenu.ToString())
            && LobbyManager.Instance.GetCurrentLobby() == null)
            Exit();

    }

    private void Start()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (scene.Equals("Lobby") || scene.Equals("MainMenu"))
            Screen.orientation = ScreenOrientation.Portrait;
        else
            Screen.orientation = ScreenOrientation.LandscapeLeft;
    }

    public void Exit()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        SceneManager.LoadScene(Scene.MainMenu.ToString());
    }

    public void LoadLobby()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        SceneManager.LoadScene(Scene.Lobby.ToString());
    }

    public void LoadGame()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        string sceneName = Scene.GameScene.ToString();
        var status = NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        if (status != SceneEventProgressStatus.Started)
        {
            Debug.LogWarning($"Failed to load {sceneName} " +
                  $"with a {nameof(SceneEventProgressStatus)}: {status}");
        }
    }
}
