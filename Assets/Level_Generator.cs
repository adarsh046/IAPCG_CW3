using System;
using UnityEngine;

public class Level_Generator : MonoBehaviour
{
    int[,] level;
    public GameObject caveTiles;
    public int levelWidth;
    public int levelHeight;
    public int levelFillPercent;
    public int smoothingIterations;
    public int cellularAutomataNumber;

    static int[,] createLevel(int levelWidth, int levelHeight, int fillPercent)
    {

        // Give random seed to generate different levels
        // Same seed = same level
        string randomSeed = Time.time.ToString(); //Generate random seed
        System.Random random = new System.Random(randomSeed.GetHashCode()); //Converting seed to a number

        // Defining the level
        int[,] level = new int[levelWidth, levelHeight];

        // In level, for every x point coordinate
        for (int x = 0; x < level.GetUpperBound(0); ++x)
        {
            // In level, for every y point coordinate
            for (int y = 0; y < level.GetUpperBound(1); ++y)
            {
                // To make the boundaries of the level as a cave and not water
                if (x == 0 || x == level.GetUpperBound(0) - 1 || y == 0 || y == level.GetUpperBound(1) - 1)
                {
                    // This one should be cave tile, because it is on the edge of the level
                    level[x, y] = 1;
                }
                else
                {
                    // This one may or may not be water, since tile is not on the edge of the level
                    // Randomly generate the tile of the level
                    // If this random value is less than or equal to our fillpercent's value then make a cave tile else water
                    if (random.Next(0, 100) <= fillPercent)
                        level[x, y] = 1;
                    else
                        level[x, y] = 0;
                }
            }
        }
        return level;
    }

    static int getNeighbourTileCount(int[,] level, int tilex, int tiley)
    {
        int neighbourTileCount = 0;
        // Iterate on a 3x3 tile grid, which is centered on the tile "x" and tile "y"
        for (int neighbourTilex = tilex - 1; neighbourTilex <= tilex + 1; ++neighbourTilex)
        {
            for (int neighbourTiley = tiley - 1; neighbourTiley <= tiley + 1; ++neighbourTiley)
            {
                // Inside the level
                if (neighbourTilex >= 0 && neighbourTilex < level.GetUpperBound(0) && neighbourTiley >= 0 && neighbourTiley < level.GetUpperBound(1))
                {
                    // On the current tile of "x" and "y" coordinate, we do not want to count that tile
                    if ((neighbourTilex != tilex || neighbourTiley != tiley))
                    {
                        neighbourTileCount += level[neighbourTilex, neighbourTiley];
                    }
                }
            }
        }
        return neighbourTileCount;
    }

    static int[,] cellularAutomata(int[,] level, int smoothIterations, int cellularAutomataNumber)
    {
        // Loop to smooth the level with smoothIterations and cellularAutomataNumber
        for (int i = 0; i < smoothIterations; ++i)
        {
            // For every tile coordinates
            for (int tilex = 0; tilex < level.GetUpperBound(0); ++tilex)
            {
                for (int tiley = 0; tiley < level.GetUpperBound(1); ++tiley)
                {
                    // We get the number of surrounding tiles
                    int surroundingTiles = getNeighbourTileCount(level, tilex, tiley);

                    // If the tile we are looking is at the edge
                    if (tilex == 0 || tilex == level.GetUpperBound(0) - 1 || tiley == 0 || tiley == level.GetUpperBound(1) - 1)
                    {
                        level[tilex, tiley] = 1;
                    }

                    // If not at the edge, the number of surrounding tiles is greater than the cellular automata number
                    // Rules for cellular automata, level generation
                    else if (surroundingTiles > cellularAutomataNumber)
                    {
                        // The tile becomes a cave tile
                        level[tilex, tiley] = 1;
                    }
                    // Less than cellular automata number
                    else if (surroundingTiles < cellularAutomataNumber)
                    {
                        // This will be water (No tile)
                        level[tilex, tiley] = 0;
                    }
                }
            }
        }
        // Now, returning the level with cellular automata
        return level;
    }

    private void generateLevel()
    {
        // Clear out any cave tiles that are in the level already (for generating next level)
        foreach (GameObject caveTile in GameObject.FindGameObjectsWithTag("Cave"))
        {
            Destroy(caveTile);
        }

        // Level create
        level = createLevel(levelWidth, levelHeight, levelFillPercent);
        // Level cellular automata 
        level = cellularAutomata(level, smoothingIterations, cellularAutomataNumber);
        // For every tile corrdinates, checking if it is cave tile
        for (int x = 0; x < level.GetUpperBound(0); ++x)
        {
            for (int y = 0; y < level.GetUpperBound(1); ++y)
            {
                // This is a cave tile
                if (level[x, y] == 1)
                {
                    // Instantiating the caveTiles prefab for the level
                    Instantiate(caveTiles, new Vector3(x, 0f, y), Quaternion.Euler(Vector3.zero));
                }
            }
        }
    }

    private void Start()
    {
        // Generate level at the start
        generateLevel();
    }

    private void Update()
    {
        // Generate a new level on each left mouse click
        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("Mouse clicked");
            generateLevel();
        }
    }

}
