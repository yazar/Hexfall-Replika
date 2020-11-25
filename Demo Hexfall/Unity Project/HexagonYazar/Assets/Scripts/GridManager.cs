using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GridManager : MonoBehaviour
{
    public Dictionary<Vector2, GameObject> tiles = new Dictionary<Vector2, GameObject>();
    public Dictionary<Vector2, Vector2> realPositions = new Dictionary<Vector2, Vector2>();

    [SerializeField] GameObject tile;
    [SerializeField] GameObject tileHolder;
    [SerializeField] GameObject outline;
    [SerializeField] GameObject bomb;
    [SerializeField] int rowSize = 9;
    [SerializeField] int columnSize = 8;
    [SerializeField] float tileGap = 0.5f;
    [SerializeField] float instantiationDelay = 0.05f;
    [SerializeField] List<Color> colorList;

    public bool createdGrid = false;
    public bool isMoving = false;
    public bool generateBomb = false;
    public bool gameOver = false;
    public bool rotating = false;

    private void Start() {
        CreateEmptyGrid();
        StartCoroutine(tileGenerator());
    }

    private void Update() {
        if(createdGrid && !isMoving && !gameOver)
        {
            FillTopRow();
        }
        if(gameOver)
        {
            outline.SetActive(false);
        }
            
    }

    // Filling the blank spaces on top with new tiles
    public void FillTopRow()
    {
        for (int i = 0; i < columnSize; i++)
        {
            Vector2 gridPos = new Vector2(rowSize - 1, i);
            if(tiles[gridPos] == null)
            {
                Vector2 instancePosition = new Vector2(realPositions[gridPos].x, 6);
                GenerateTile(realPositions[gridPos], gridPos, instancePosition);
            }
            
        }
    }

    // Function to find neighbors of given cordinate
    public List<Vector2> GetNeighBors(Vector2 gridPosition)
    {
        Vector2 leftTile = new Vector2(gridPosition.x, gridPosition.y - 1);
        Vector2 rightTile = new Vector2(gridPosition.x, gridPosition.y + 1);
        Vector2 upTile = new Vector2(gridPosition.x + 1, gridPosition.y);
        Vector2 downTile = new Vector2(gridPosition.x - 1, gridPosition.y);
        Vector2 upRightTile = new Vector2(gridPosition.x + 1, gridPosition.y + 1);
        Vector2 upLeftTile = new Vector2(gridPosition.x + 1, gridPosition.y - 1);
        Vector2 downRightTile = new Vector2(gridPosition.x - 1, gridPosition.y + 1);
        Vector2 downLeftTile = new Vector2(gridPosition.x - 1, gridPosition.y - 1);

        List<Vector2> neighborsEven = new List<Vector2>() { leftTile, upLeftTile, upTile, upRightTile, rightTile, downTile };
        List<Vector2> neighborsOdd = new List<Vector2>() { leftTile, upTile, rightTile, downRightTile, downTile, downLeftTile };

        if (gridPosition.y % 2 == 0)
        {
            return neighborsEven;
        }
        else
        {
            return neighborsOdd;
        }
    }

    // Checks given grid positions if it has any matches
    public void CheckMatch(Vector2 tileGridPosition)
    {
        List<Vector2> matchList = new List<Vector2>();
        GetComponent<TileManager>().CheckForMatch(matchList, tileGridPosition);
    }

    // Generating all tiles
    private IEnumerator tileGenerator()
    {
        ScaleTile();

        // Distance Between rows and columns
        float columdDistance = ((tile.transform.localScale.x / 2) * 1.5f) + tileGap;
        float crossPositionOffset = (((tile.transform.localScale.x / 2) * Mathf.Sqrt(3)) / 2) + (tileGap / 2);
        float rowDistance = ((tile.transform.localScale.x / 2) * Mathf.Sqrt(3)) + tileGap;

        // Offset value for grid to keep it on the middle of screen
        float positionOffsetX = ((columdDistance) * (columnSize - 1)) / 2;
        float positionOffsetY = ((rowDistance) * (rowSize - 1)) / 2;

        for (int i = 0; i < rowSize; i++)
        {
            for (int j = 0; j < columnSize; j++)
            {
                Vector2 instancePosition;
                Vector2 targetPosition;
                if (j % 2 == 0)
                {
                    targetPosition = new Vector3((j * columdDistance) - positionOffsetX, (i * rowDistance) - positionOffsetY);
                }
                else
                {
                    targetPosition = new Vector3((j * columdDistance) - positionOffsetX, (i * rowDistance) - crossPositionOffset - positionOffsetY);
                }
                instancePosition = new Vector2(targetPosition.x, 6);

                Vector2 gridPos = new Vector2(i, j);

                GenerateTile(targetPosition, gridPos, instancePosition);
                yield return new WaitForSeconds(instantiationDelay);
            }
        }
        createdGrid = true;
    }

    // Scales hexagon tile according to column and row size
    private void ScaleTile()
    {
        float scale = (6f - (tileGap * (columnSize - 1))) / (columnSize - 1);

        if ((rowSize / 2) + (scale / 2) > 4.3f)
        {
            if (4.3f / ((rowSize / 2) + (0.5f)) < scale)
                scale = 4.3f / ((rowSize / 2) + (0.5f));
        }
        tile.transform.localScale = new Vector3(scale, scale, 1);
        outline.transform.localScale = tile.transform.localScale;
        bomb.transform.localScale = tile.transform.localScale;
    }

    // Generate a tile on given positions
    private void GenerateTile(Vector2 targetPosition, Vector2 gridPos, Vector2 instancePosition)
    {
        GameObject newTile;
        if (generateBomb) 
        { 
            newTile = Instantiate(bomb, instancePosition, Quaternion.identity);
            generateBomb = false;
        }else{
            newTile = Instantiate(tile, instancePosition, Quaternion.identity);
        }
        
        int s = Random.Range(0, colorList.Count);
        Color tileColor = colorList[s];
        newTile.GetComponent<Tile>().TargetPosition = targetPosition;
        newTile.GetComponent<Tile>().TileGridPosition = gridPos;
        realPositions[gridPos] = targetPosition;
        tiles[gridPos] = newTile;
        newTile.GetComponent<Tile>().TileColor = tileColor;
        while (IsMatch(gridPos))
        {
            s++;
            tileColor = colorList[s % colorList.Count];
            newTile.GetComponent<Tile>().TileColor = tileColor;
        }
        newTile.GetComponent<SpriteRenderer>().color = tileColor;
        newTile.transform.SetParent(tileHolder.transform);
    }

    // Checks neighbor for first generation of tiles, if random tile color matches with neighbors then choose another color
    private bool IsMatch(Vector2 gridPosition)
    {
        List<Vector2> neighbors = GetNeighBors(gridPosition);
        Tile centerTile = tiles[gridPosition].GetComponent<Tile>();
        for (int i = 0; i < neighbors.Count; i++)
        {
            if (tiles[neighbors[i]] == null || tiles[neighbors[(i + 1) % neighbors.Count]] == null) continue;
            Tile tile = tiles[neighbors[i]].GetComponent<Tile>();
            Tile nextTile = tiles[neighbors[(i + 1) % neighbors.Count]].GetComponent<Tile>();
            if (tile.TileColor == nextTile.TileColor && tile.TileColor == centerTile.TileColor)
            {
                return true;
            }
        }
        return false;
    }

    // Creating empty dictionary that has an extra empty border around grid, so we wont get "not existing key error" on sides
    private void CreateEmptyGrid()
    {
        for (int i = -1; i <= rowSize; i++)
        {
            for (int j = -1; j <= columnSize; j++)
            {
                Vector2 XY = new Vector2(i, j);
                tiles[XY] = null;
            }
        }
    }
}
