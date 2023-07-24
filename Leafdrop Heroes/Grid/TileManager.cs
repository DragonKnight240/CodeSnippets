using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public static TileManager Instance;
    public int Height = 10;
    public int Width = 10;
    public GameObject GridParent;
    public GameObject TilePrefab;
    public float TileSize;
    internal GameObject[,] Grid;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        GridParent = gameObject;
        CreateGrid();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void CreateGrid()
    {
        Grid = new GameObject[Height,Width];

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Grid[x, y] = Instantiate(TilePrefab, new Vector3(x * TileSize, 0, y * TileSize), Quaternion.identity, transform);
                Grid[x, y].GetComponent<Tile>().OGMaterial = Grid[x, y].GetComponent<Renderer>().material;
                Grid[x, y].name = (x + " , " + y);
                Grid[x, y].GetComponent<Tile>().GridPosition = new int[2];
                Grid[x, y].GetComponent<Tile>().GridPosition[0] = x;
                Grid[x, y].GetComponent<Tile>().GridPosition[1] = y;
            }
        }

        foreach(GameObject Tile in Grid)
        {
            Tile.GetComponent<Tile>().FindAdjacentTiles();
        }

        UnitManager.Instance.PlaceUnits();

    }
}
