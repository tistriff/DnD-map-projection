using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorCube : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Renderer>().material.color = Random.ColorHSV(0f, 1f, 1f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
