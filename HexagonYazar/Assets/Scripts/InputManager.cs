using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    Vector2 startPosition = new Vector2();
    private void Update() 
    {
        if(!GetComponent<GridManager>().rotating && !GetComponent<GridManager>().isMoving)  // if tiles are moving or rotating dont get input
        {
            if(Input.GetMouseButtonDown(0))
            {
                startPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }

            if(Input.GetMouseButtonUp(0))
            {
                Vector2 touchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if(Vector2.Distance(startPosition, touchPosition) < 0.2f)                                           //Click
                {
                    RaycastHit2D hit = Physics2D.Raycast(touchPosition, Vector2.zero);
                    if (hit.collider != null)
                    {
                        GetComponent<TileManager>().selectThree(hit.collider.gameObject, touchPosition);
                    }
                }
                else                                                                                                //drag    
                {
                    StartCoroutine(GetComponent<TileManager>().rotateSelectedTiles(startPosition, touchPosition));
                }
            }
        }
    }


}
