using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserManager : MonoBehaviour
{
    public static UserManager Instance;

    private User _currentUser;

    [SerializeField]
    private Logger _logger;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        } else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void createCurrentUser(string role)
    {
        _logger.Log(role, this);
        _currentUser = new User(role);
    }

    public User getCurrentUser()
    {
        return _currentUser;
    }

}
