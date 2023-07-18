using TMPro;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private TMP_Text _errorMsg;

    private void Start()
    {
        _errorMsg.gameObject.SetActive(false);
    }

    public void CreateGameSession(string role)
    {
        LobbyManager.Instance.CreateLobby();
    }

    public void QuickJoinGameSession()
    {
        LobbyManager.Instance.QuickJoinLobby(SetError);
    }

    public void JoinGameSession(string playerName, string lobbyId)
    {
        
        LobbyManager.Instance.JoinLobbyByCode(lobbyId, playerName, SetError);
    }


    public void SubmitJoin(GameObject form)
    {
        string playerName = form.transform.Find("Input_Spielername").transform.GetComponent<TMP_InputField>().text;
        string lobbyId = form.transform.Find("Input_LobbyID").transform.GetComponent<TMP_InputField>().text;
        if (!string.IsNullOrEmpty(lobbyId))
        {
            JoinGameSession(playerName, lobbyId);
        }
        else
        {
            Debug.LogError("Lobby-ID benötigt!");
            SetError("Lobby-ID benötigt!");
        }
    }

    public void SetError(string msg)
    {
        _errorMsg.gameObject.SetActive(true);
        _errorMsg.text = msg;
    }
}
