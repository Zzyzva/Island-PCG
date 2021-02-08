using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class UIScript : MonoBehaviour
{
    public Text display;
    public GameObject selected;

public void setText(GameObject t)
    {
        if ( selected != null)
        {
            selected.GetComponent<TileScript>().deselect();
        }
        selected = t;
        display.text = t.GetComponent<TileScript>().tileID.ToString();
    }
}
