using System;
using UnityEngine;

public class Level_Generator : MonoBehaviour
{
    int[,] level;
    public GameObject caveWalls;
    public int wallWidth;
    public int wallHeight;
    public int fillPercent;
    public int smoothIterations;
    public int cellularAutomataNumber;
    public string randomSeed;

    static int[,] createLevel(int tileWidth, int tileHeight, string randomSeed, int fillPercent)
    {

        //Give random seed to generate different levels
        //Same seed = same level
        randomSeed = Time.time.ToString(); //Generate random seed
        System.Random random = new System.Random(randomSeed.GetHashCode()); //Converting seed to a number

        // Initialising the level
        int[,] level = new int[tileWidth, tileHeight];

        // For every x point coordinate
        for (int x = 0; x < level.GetUpperBound(0); ++x)
        {
            // For every y point coordinate
            for (int y = 0; y < level.GetUpperBound(1); ++y)
            {
                //To make the boundaries of the level as a cave and not water
                if (x == 0 || x == level.GetUpperBound(0) - 1 || y == 0 || y == level.GetUpperBound(1) - 1)
                {
                    // This tile is on the edge, it has to be a cave wall
                    level[x, y] = 1;
                }
                else
                {
                    // This tile is not on the edge, so it may or may not be water
                    // Randomly generate the tile of the level
                    // If this random value is less than or equal to our fillpercent's value then make a cave wall else water
                    if (random.Next(0, 100) <= fillPercent)
                        level[x, y] = 1;
                    else
                        level[x, y] = 0;
                }
            }
        }
        return level;
    }

    static int getNeighbourWallCount(int[,] level, int tilex, int tiley)
    {
        int neighbourWallCount = 0;
        //Iterate on a 3x3 tile grid, which is centered on the tile "x" and tile "y"
        for (int neighbourTilex = tilex - 1; neighbourTilex <= tilex + 1; ++neighbourTilex)
        {
            for (int neighbourTiley = tiley - 1; neighbourTiley <= tiley + 1; ++neighbourTiley)
            {
                //Inside the level
                if (neighbourTilex >= 0 && neighbourTilex < level.GetUpperBound(0) && neighbourTiley >= 0 && neighbourTiley < level.GetUpperBound(1))
                {
                    //On the current tile of "x" and "y", we do not want to count that
                    if ((neighbourTilex != tilex || neighbourTiley != tiley))
                    {
                        neighbourWallCount += level[neighbourTilex, neighbourTiley];
                    }
                }
            }
        }
        return neighbourWallCount;
    }

    static int[,] cellularAutomata(int[,] level, int smoothIterations, int cellularAutomataNumber)
    {
        // This loops everything depending on the number of times we choose to smooth
        for (int i = 0; i < smoothIterations; i++)
        {
            // For every tile
            for (int x = 0; x < level.GetUpperBound(0); ++x)
            {
                for (int y = 0; y < level.GetUpperBound(1); ++y)
                {
                    // We get the number of surrounding tiles
                    int surroundingTiles = getNeighbourWallCount(level, x, y);

                    // If the tile we are looking is at the edge
                    if (x == 0 || x == level.GetUpperBound(0) - 1 || y == 0 || y == level.GetUpperBound(1) - 1)
                    {
                        level[x, y] = 1;
                    }

                    // If not at the edge, the number of surrounding tiles is greater than the cellular automata number
                    // Rules for cellular automata, level generation
                    else if (surroundingTiles > cellularAutomataNumber)
                    {
                        // The tile becomes a cave wall
                        level[x, y] = 1;
                    }
                    // Else, less than cellular automata number
                    else if (surroundingTiles < cellularAutomataNumber)
                    {
                        // The will be water
                        level[x, y] = 0;
                    }
                }
            }
        }
        // Return the modified map
        return level;
    }

    private void generateLevel()
    {
        // Clear out any cave walls that are in the level already
        foreach (GameObject wall in GameObject.FindGameObjectsWithTag("Cave"))
        {
            Destroy(wall);
        }

        // Level create
        level = createLevel(wallWidth, wallHeight, randomSeed, fillPercent);
        // Level cellular automata 
        level = cellularAutomata(level, smoothIterations, cellularAutomataNumber);

        for (int x = 0; x < level.GetUpperBound(0); ++x)
        {
            for (int y = 0; y < level.GetUpperBound(1); ++y)
            {
                // Tile is a wall, create a cave wall gameobject
                if (level[x, y] == 1)
                {
                    Instantiate(caveWalls, new Vector3(x, 0f, y), Quaternion.Euler(Vector3.zero));
                }
            }
        }
    }

    private void Start()
    {
        generateLevel();
    }

    private void Update()
    {
        //Generate a new level on each left mouse click
        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("Mouse clicked");
            generateLevel();
        }
    }

}
