using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User
{
    public enum Role
    {
        Dungeonmaster,
        Player
    }

    private Role _userRole;

    public User(string role)
    {
        _userRole = (Role)System.Enum.Parse(typeof(Role), role);
    }

    public Role getRole()
    {
        return _userRole;
    }
}
