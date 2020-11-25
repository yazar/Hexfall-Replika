using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    TextMesh textMesh;          // bomb text
    TileManager tileManager;

    // Min and Max Values for bomb to countdown
    [SerializeField] int bombMaxNumber = 10;
    [SerializeField] int bombMinNumber = 4;
    int bombNumber;

    private void Awake() 
    {
        tileManager = FindObjectOfType<TileManager>();
        textMesh = GetComponentInChildren<TextMesh>();
        bombNumber = Random.Range(bombMinNumber, bombMaxNumber);
        textMesh.text = bombNumber.ToString();
    }

    // Countdown bomb on move
    public void CountDownTheBomb()
    {
        bombNumber--;
        textMesh.text = bombNumber.ToString();
        if(bombNumber <= 0)
        {
            tileManager.ExplodeAllTiles();
        }
    }

}
