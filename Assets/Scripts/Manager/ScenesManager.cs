using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

// Manager class handle the scene loading
public class ScenesManager : NetworkBehaviour
{
    public static ScenesManager Instance;

    public enum Scene
    {
        MainMenu,
        Lobby,
        GameScene
    }

    // Checks the current scene at the start of the game and changes it to the mainmenu scene 
    public void Awake()
    {
        Instance = this;
        if (!SceneManager.GetActiveScene().name.Equals(Scene.MainMenu.ToString())
            && LobbyManager.Instance.GetCurrentLobby() == null)
            Exit();

    }
    
    // Sets the screen orientation according to the active scene
    private void Start()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (scene.Equals("Lobby") || scene.Equals("MainMenu"))
            Screen.orientation = ScreenOrientation.Portrait;
        else
            Screen.orientation = ScreenOrientation.LandscapeLeft;
    }

    // exits to the mainmenu scene
    public void Exit()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        SceneManager.LoadScene(Scene.MainMenu.ToString());
    }

    // loads the lobby scene
    public void LoadLobby()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        SceneManager.LoadScene(Scene.Lobby.ToString());
    }

    // Loads the game scene through the NetworkManager
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
