using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
    public static ScenesManager Instance;
    //[SerializeField]
    //private List<Scene> scenes;

    [SerializeField]
    private Logger _logger;

    public void Awake()
    {
        Instance = this;
    }

    public enum Scene
    {
        MainMenu,
        Lobby,
        GameScene
    }

    public void LoadScene(Scene scene)
    {
        SceneManager.LoadScene(scene.ToString());
    }

    public void Exit(Scene scene)
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
        SceneManager.LoadScene(Scene.GameScene.ToString());
    }
}
