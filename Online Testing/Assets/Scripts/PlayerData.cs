using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public string username;
    public float R, G, B;
    public int cardback;

    public PlayerData()
    {
        username = "";
        R = 0;
        G = 0;
        B = 255;
        cardback = 0;
    }

    public Color getColor()
    {
        return new Color(R, G, B, 255);
    }

    public void setColor(Color newColor)
    {
        R = newColor.r;
        G = newColor.g; 
        B = newColor.b;
    }
}
