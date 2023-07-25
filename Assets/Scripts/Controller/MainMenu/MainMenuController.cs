using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject _joinErrorMsg;
    [SerializeField] private GameObject _errorMsg;

    private void Start()
    {
        _errorMsg.gameObject.SetActive(false);
    }

    public async void CreateGameSession(string role)
    {
        _errorMsg.SetActive(false);
        bool success = await LobbyManager.Instance.CreateLobby();

        if(success)
        {
            SetError(_errorMsg, "Verbindung fehlgeschlagen!");
        }
    }

    public async void QuickJoinGameSession()
    {
        _errorMsg.SetActive(false);
        bool success = await LobbyManager.Instance.QuickJoinLobby();

        if (success)
        {
            SetError(_errorMsg, "Verbindung fehlgeschlagen!");
        }
    }

    public async void JoinGameSession(string playerName, string lobbyId)
    {
        _errorMsg.SetActive(false);
        bool success = await LobbyManager.Instance.JoinLobbyByCode(lobbyId, playerName);

        if (success)
        {
            SetError(_errorMsg, "Keine Lobby gefunden!");
        }
    }


    public void SubmitJoin(GameObject form)
    {
        _joinErrorMsg.SetActive(false);
        string playerName = form.transform.Find("Input_Spielername").transform.GetComponent<TMP_InputField>().text;
        string lobbyId = form.transform.Find("Input_LobbyID").transform.GetComponent<TMP_InputField>().text;
        if (!string.IsNullOrEmpty(lobbyId))
        {
            JoinGameSession(playerName, lobbyId);
            form.SetActive(false);
        }
        else
        {
            SetError(_joinErrorMsg, "Lobby-ID benötigt!");
        }
    }

    public void SetError(GameObject errorMsg, string msg)
    {
        errorMsg.SetActive(true);
        errorMsg.GetComponent<TMP_Text>().text = msg;
    }
}
