using System.Threading.Tasks;
using TMPro;
using UnityEngine;

// Controller class to handle the mainmenu logic
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject _joinErrorMsg;
    [SerializeField] private GameObject _errorMsg;

    private void Start()
    {
        _errorMsg.gameObject.SetActive(false);
    }

    // Is called at pressing the "Spiel erstellen"-Button
    // Prepares the error message and
    // calls the LobbyManager to create a new Lobby
    public async void CreateGameSession(string role)
    {
        _errorMsg.SetActive(false);
        bool success = await LobbyManager.Instance.CreateLobby();

        if(!success)
        {
            SetError(_errorMsg, "Verbindung fehlgeschlagen!");
        } else
        {
            ScenesManager.Instance.LoadLobby();
        }
    }

    // Prepares the error message and
    // calls the LobbyManager to create a new Lobby
    public async void JoinGameSession(string playerName, string lobbyId)
    {
        _errorMsg.SetActive(false);
        bool success = await LobbyManager.Instance.JoinLobbyByCode(lobbyId, playerName);

        if (!success)
        {
            SetError(_errorMsg, "Keine Lobby gefunden!");
        } else
        {
            ScenesManager.Instance.LoadLobby();
        }
    }

    // Is called at pressing the "Beitreten"-Button of the join formular
    // Prepares the join error massage,
    // recives and checks the input texts of the join formular fields
    // and sends them to JoinGameSession
    public void SubmitJoin(GameObject form)
    {
        _joinErrorMsg.SetActive(false);
        string playerName = form.transform.Find("Input_Spielername").transform.GetComponent<TMP_InputField>().text;
        string lobbyId = form.transform.Find("Input_LobbyID").transform.GetComponent<TMP_InputField>().text;
        if (!string.IsNullOrEmpty(lobbyId))
        {
            JoinGameSession(playerName, lobbyId);
            form.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            SetError(_joinErrorMsg, "Lobby-ID benötigt!");
        }
    }

    // Places and activates the given error message
    public void SetError(GameObject errorMsg, string msg)
    {
        errorMsg.SetActive(true);
        errorMsg.GetComponent<TMP_Text>().text = msg;
    }
}
