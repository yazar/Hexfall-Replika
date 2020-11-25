using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    GridManager grid;

    [SerializeField] float tileSpeed = 0.1f;
    [SerializeField] bool isBomb = false;

    private Vector3 targetPosition;
    public Vector3 TargetPosition{ get{return targetPosition;}  set{targetPosition = value;} }

    private Color tileColor;
    public Color TileColor { get { return tileColor; } set { tileColor = value; } }

    private Vector2 tileGridPosition;
    public Vector2 TileGridPosition { get { return tileGridPosition; } set { tileGridPosition = value; } }

    private bool selected = false;
    public bool Selected { get { return selected; } set { selected = value; } }

    bool moving = false;
    bool stopped = true;
    


    private void Awake() {
        grid = FindObjectOfType<GridManager>();
    }


    private void Update()
    {
        if (!selected)
        {
            GoPosition();
        }
        DropTileDown();
        stopped = transform.position.y == targetPosition.y;
        grid.isMoving = !stopped;

    }

    // if there is an empty space under tile, go there
    private void DropTileDown()
    {
        if (tileGridPosition.x != 0)
        {
            Vector2 tileBelow = new Vector2(tileGridPosition.x - 1, tileGridPosition.y);
            if (moving && grid.tiles[tileBelow] != null && stopped && !selected)
            {
                grid.CheckMatch(tileGridPosition);
                moving = false;
            }
            if (grid.tiles[tileBelow] == null)
            {
                grid.tiles[tileGridPosition] = null;
                targetPosition = grid.realPositions[tileBelow];
                moving = true;
                grid.tiles[tileBelow] = this.gameObject;
                tileGridPosition = tileBelow;
            }
            
        }
    }

    // takes tile to the target positions
    public void GoPosition()
    {
        Vector3 currentPosition = GetComponent<Transform>().position;
        if(currentPosition.y > targetPosition.y)
        {
            GetComponent<Transform>().position += Vector3.down * tileSpeed * Time.deltaTime;
        }
        else
        {
            GetComponent<Transform>().position = targetPosition;
        }
    }
}
