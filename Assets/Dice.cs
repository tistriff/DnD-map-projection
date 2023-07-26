using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{
    [SerializeField] private int _max = 0;

    public int GetMax()
    {
        return _max;
    }
}
