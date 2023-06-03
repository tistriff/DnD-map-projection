using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
    public static ScenesManager Instance;
    //[SerializeField]
    //private List<Scene> scenes;

    public void Awake()
    {
        Instance = this;
    }

    public enum Scene
    {
        Lobby,
        BlankAR
    }

    public void LoadScene(Scene scene)
    {
        SceneManager.LoadScene(scene.ToString());
    }

    public void StartGame()
    {
        SceneManager.LoadScene(Scene.BlankAR.ToString());
    }

    public void LoadLobby()
    {
        SceneManager.LoadScene(Scene.Lobby.ToString());
    }
}
