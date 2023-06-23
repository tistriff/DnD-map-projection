using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;

public class UI_InputForm : MonoBehaviour
{
    [SerializeField] private TMP_InputField playerNameField;
    [SerializeField] private TMP_InputField lobbyIdField;
    [SerializeField] private TMP_Text errorMsg;
    [SerializeField] private MainMenuController menuController;

    private void Start  ()
    {
        errorMsg.gameObject.SetActive(false);
    }

    public void submit()
    {
        string playerName = playerNameField.text;
        string lobbyId = lobbyIdField.text;
        if (!string.IsNullOrEmpty(lobbyId))
        {
            menuController.JoinGameSession(playerName, lobbyId, errorMsg);
        }
        else
        {
            errorMsg.gameObject.SetActive(true);
            errorMsg.text = "Lobby-ID benötigt!";
        }
    }

    public bool isEmpty(string inputString)
    {
        return string.IsNullOrEmpty(inputString);
    }
}
