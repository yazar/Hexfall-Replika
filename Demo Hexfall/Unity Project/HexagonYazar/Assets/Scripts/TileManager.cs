using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TileManager : MonoBehaviour
{
    [SerializeField] GameObject outline;
    [SerializeField] GameObject explosion;
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] GameController gameController = null;

    private GridManager grid;
    private List<Vector2> selectedThree = new List<Vector2>() {new Vector2(-1, -1),new Vector2(-1, -1) ,new Vector2(-1, -1)};


    private void Start() {
        grid = GetComponent<GridManager>();
    }

    public void selectThree(GameObject selectedTile, Vector2 touchPosition)
    {
        ResetSelectedTiles();
        selectedThree[0] = selectedTile.GetComponent<Tile>().TileGridPosition;
        Vector2 gridPosition = selectedThree[0];
        List<Vector2> neighbors = grid.GetNeighBors(gridPosition);

        Vector3 midPointOfSelected = new Vector2();
        List<float> distances = new List<float>();
        int selectedSide = 0;
        for (int i = 0; i < neighbors.Count; i++)
        {
            if (grid.tiles[neighbors[i]] == null 
                || grid.tiles[neighbors[(i + 1) % neighbors.Count]] == null 
                || grid.tiles[gridPosition] == null)    continue;

            float distance = Vector2.Distance(touchPosition, grid.tiles[neighbors[i]].transform.position);
            distance += Vector2.Distance(touchPosition, grid.tiles[neighbors[(i + 1) % neighbors.Count]].transform.position);
            distances.Add(distance);
            distances.Sort(SortByDistance);
            if (distances.IndexOf(distance) == 0)
            {
                selectedSide = i;
                selectedThree[1] = neighbors[i];
                selectedThree[2] = neighbors[(i + 1) % neighbors.Count];
                midPointOfSelected = (grid.tiles[gridPosition].transform.position +
                                      grid.tiles[neighbors[i]].transform.position +
                                      grid.tiles[neighbors[(i + 1) % neighbors.Count]].transform.position) / 3;
            }
        }
        SetSelectedTiles();
        if ((gridPosition.y + selectedSide) % 2 != 0)
        {
            outline.transform.rotation = Quaternion.Euler(0, 0, 60);
        }
        else
        {
            outline.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        outline.transform.position = new Vector2(midPointOfSelected.x, midPointOfSelected.y);
    }

    //rotates selected tiles in given direction
    public IEnumerator rotateSelectedTiles(Vector2 start, Vector2 end)
    {
        grid.rotating = true;
        SetSelectedTiles();
        Vector2 directionStart = start - new Vector2(outline.transform.position.x, outline.transform.position.y);
        Vector2 directionEnd = end - new Vector2(outline.transform.position.x, outline.transform.position.y);
        float angle = Vector2.SignedAngle(directionStart, directionEnd);

        Vector3 midPoint = (grid.tiles[selectedThree[0]].transform.position +
                            grid.tiles[selectedThree[1]].transform.position +
                            grid.tiles[selectedThree[2]].transform.position) / 3;

        for (int j = 0; j < 3; j++)
        {
            for (int i = 0; i < 120 / rotationSpeed; i++)
            {
                grid.tiles[selectedThree[0]].transform.RotateAround(midPoint, Vector3.forward, Mathf.Sign(angle) * rotationSpeed);
                grid.tiles[selectedThree[1]].transform.RotateAround(midPoint, Vector3.forward, Mathf.Sign(angle) * rotationSpeed);
                grid.tiles[selectedThree[2]].transform.RotateAround(midPoint, Vector3.forward, Mathf.Sign(angle) * rotationSpeed);
                outline.transform.RotateAround(midPoint, Vector3.forward, Mathf.Sign(angle) * rotationSpeed);
                yield return new WaitForSeconds(Time.deltaTime / rotationSpeed);
            }
            rotateDictionaries(Mathf.Sign(angle) > 0);

            if (CheckSelectedTiles())    // Check for match of selected tiles
            {
                if(FindObjectsOfType<Bomb>() != null)
                {
                    foreach (Bomb bomb in FindObjectsOfType<Bomb>())
                    {
                        bomb.CountDownTheBomb();
                        
                    }
                }
                grid.rotating = false;
                yield break;
            }
        }
        grid.rotating = false;
    }
    
    // Explode all Tiles when game over
    public void ExplodeAllTiles()
    {
        List<Vector2> matchList = new List<Vector2>();
        foreach (Tile tile in FindObjectsOfType<Tile>())
        {
            matchList.Add(tile.TileGridPosition);
        }
        grid.gameOver = true;
        ExplodeTiles(matchList);
    }

    private int SortByDistance(float p1, float p2)
    {
        return p1.CompareTo(p2);
    }

    private void SetSelectedTiles()
    {
        if (selectedThree.Count > 0)
        {
            grid.tiles[selectedThree[0]].GetComponent<Tile>().Selected = true;
            grid.tiles[selectedThree[1]].GetComponent<Tile>().Selected = true;
            grid.tiles[selectedThree[2]].GetComponent<Tile>().Selected = true;
        }
    }

    private void ResetSelectedTiles()
    {
        if (selectedThree.Count > 0)
        {
            if (grid.tiles[selectedThree[0]] != null) { grid.tiles[selectedThree[0]].GetComponent<Tile>().Selected = false; }
            if (grid.tiles[selectedThree[1]] != null) { grid.tiles[selectedThree[1]].GetComponent<Tile>().Selected = false; }
            if (grid.tiles[selectedThree[2]] != null) { grid.tiles[selectedThree[2]].GetComponent<Tile>().Selected = false; }
        }
    }

    // changes dictionary positions according to rotated position
    private void rotateDictionaries(bool direction)
    {
        
        Vector2[] gridPositions = new Vector2[3];
        Vector3[] realPositions = new Vector3[3];
        GameObject[] gridTiles = new GameObject[3];

        for (int i = 0; i < 3; i++)
        {
            gridPositions[i] = selectedThree[i];
            realPositions[i] = grid.tiles[selectedThree[i]].GetComponent<Tile>().TargetPosition;
            gridTiles[i] = grid.tiles[gridPositions[i]];
        }

        if(!direction)
        {
            for (int i = 0; i < 3; i++)
            {
                grid.tiles[selectedThree[i]].GetComponent<Tile>().TargetPosition = realPositions[(i + 1) % 3];
                grid.tiles[selectedThree[i]].GetComponent<Tile>().TileGridPosition = gridPositions[(i + 1) % 3];
                grid.tiles[gridPositions[i]] = gridTiles[(i + 2) % 3];
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                grid.tiles[selectedThree[i]].GetComponent<Tile>().TargetPosition = realPositions[(i + 2) % 3];
                grid.tiles[selectedThree[i]].GetComponent<Tile>().TileGridPosition = gridPositions[(i + 2) % 3];
                grid.tiles[gridPositions[i]] = gridTiles[(i + 1) % 3];
            }
        }
    }

    // checks 3 selected tiles if they have a match, than explodes them 
    private bool CheckSelectedTiles()
    {
        List<Vector2> matchList = new List<Vector2>();
        foreach (Vector2 selected in selectedThree)
        {
            Vector2 gridPosition = selected;
            CheckForMatch(matchList, gridPosition);
        }
        if (matchList.Count > 0)
        {
            return true;
        }
        return false;
    }

    //Function to check if there is a match on  given position, if there is than explode them
    public void CheckForMatch(List<Vector2> matchList, Vector2 gridPosition)
    {
        if(grid.tiles[gridPosition] == null) return;
        if(grid.tiles[gridPosition].GetComponent<Tile>() == null ) return;
        Tile centerTile = grid.tiles[gridPosition].GetComponent<Tile>();
        List<Vector2> neighbors = grid.GetNeighBors(gridPosition);
        for (int i = 0; i < neighbors.Count; i++)
        {
            if (grid.tiles[neighbors[i]] == null || grid.tiles[neighbors[(i + 1) % neighbors.Count]] == null) continue;
            Tile tile = grid.tiles[neighbors[i]].GetComponent<Tile>();
            Tile nextTile = grid.tiles[neighbors[(i + 1) % neighbors.Count]].GetComponent<Tile>();
            if (tile.TileColor == nextTile.TileColor && tile.TileColor == centerTile.TileColor)
            {
                if (!matchList.Contains(neighbors[i])) { matchList.Add(neighbors[i]); }
                if (!matchList.Contains(neighbors[(i + 1) % neighbors.Count])) { matchList.Add(neighbors[(i + 1) % neighbors.Count]); }
                if (!matchList.Contains(gridPosition)) { matchList.Add(gridPosition); }
            }
        }
        if (matchList.Count > 0)
        {
            ExplodeTiles(matchList);
        }
    }
    
    //Explodes the founded matches
    private void ExplodeTiles(List<Vector2> matchList)
    {
        explosion.transform.localScale = outline.transform.localScale;
        foreach (Vector2 tile in matchList)
        {
            if(grid.tiles[tile] != null)
            {
            ParticleSystem.MainModule settings = explosion.GetComponent<ParticleSystem>().main;
            settings.startColor = new ParticleSystem.MinMaxGradient(grid.tiles[tile].GetComponent<SpriteRenderer>().color);
            Instantiate(explosion, grid.tiles[tile].transform.position, Quaternion.Euler(0,0,180));

            gameController.EarnPoints(5);
            Destroy(grid.tiles[tile]);
            grid.tiles[tile] = null;
            }
        }
        ResetSelectedTiles();
    }


}
