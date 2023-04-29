// ECS7016P - Interactive Agents and Procedural Generation
// Adarsh Gupta - 220570653
// References:
// 1) Procedural Generation with Cellular Automata, Bronson Zgeb. Available at: https://bronsonzgeb.com/index.php/2022/01/30/procedural-generation-with-cellular-automata/
// 2) Procedural cave generation, YouTube. Available at: https://www.youtube.com/watch?v=v7yyZZjF1z4&amp
// 3) Cellular automata method for generating random cave-like levels, RogueBasin. Available at: http://www.roguebasin.com/index.php/Cellular_Automata_Method_for_Generating_Random_Cave-Like_Levels 
// 4) Constructive generation methods for dungeons and levels. Available at: https://qmplus.qmul.ac.uk/mod/resource/view.php?id=2201434

using UnityEngine;
using Random = System.Random;

public class Level_Generator : MonoBehaviour
{
    int[,] level;
    public GameObject caveTiles;
    public int levelWidth;
    public int levelHeight;
    public int levelFillPercent;
    public int smoothingIterations;
    public int cellularAutomataNumber;

    int[,] createLevel(int levelWidth, int levelHeight, int levelFillPercent)
    {
        // Give random seed to generate different levels
        // Same seed = same level
        string randomSeed = Time.time.ToString(); //Generate random seed
        Random random = new Random(randomSeed.GetHashCode()); //Converting seed to a number

        // Defining the level
        int[,] level = new int[levelWidth, levelHeight];

        // In level, for every "x" tile point coordinate
        for (int tilex = 0; tilex < levelWidth; ++tilex)
        {
            // In level, for every "y" tile point coordinate
            for (int tiley = 0; tiley < levelHeight; ++tiley)
            {
                    // This tile may or may not be a cave tile
                    // Randomly generate a value for a tile of the level
                    // If this random value is less than or equal to our fillpercent's value then make a cave tile else water (no tile)
                    if (random.Next(0, 100) <= levelFillPercent)
                        level[tilex, tiley] = 1;
                    else
                        level[tilex, tiley] = 0;
            }
        }
        return level;
    }

    int getNeighbourTilesCount(int[,] level, int tilex, int tiley, int levelWidth, int levelHeight)
    {
        int neighbourTileCount = 0;
        // Iterate on a 3x3 tile grid, which is centered on the tile "x" and tile "y"
        for (int neighbourTilex = tilex - 1; neighbourTilex <= tilex + 1; ++neighbourTilex)
        {
            for (int neighbourTiley = tiley - 1; neighbourTiley <= tiley + 1; ++neighbourTiley)
            {
                // Inside the level
                if (neighbourTilex >= 0 && neighbourTilex < levelWidth && neighbourTiley >= 0 && neighbourTiley < levelHeight)
                {
                    // On the current tile of "x" and "y" coordinate, we will not count that tile
                    if ((neighbourTilex != tilex || neighbourTiley != tiley))
                    {
                        neighbourTileCount += level[neighbourTilex, neighbourTiley];
                    }
                }
            }
        }
        return neighbourTileCount;
    }

    int[,] cellularAutomata(int[,] level, int smoothIterations, int cellularAutomataNumber, int levelWidth, int levelHeight)
    {
        // Loop to smooth the level with smoothIterations and cellularAutomataNumber
        for (int i = 0; i < smoothIterations; ++i)
        {
            // For every tile coordinates
            for (int tilex = 0; tilex < levelWidth; ++tilex)
            {
                for (int tiley = 0; tiley < levelHeight; ++tiley)
                {
                    // Get the number of surrounding tiles
                    int neighbourTilesCount = getNeighbourTilesCount(level, tilex, tiley, levelWidth, levelHeight);

                    // To make the boundaries of the level as a cave and not water, checking if the tile is at the edge
                    if (tilex == 0 || tilex == levelWidth - 2 || tiley == 0 || tiley == levelHeight - 2)
                    {
                        // This one should be cave tile, because it is on the edge of the level
                        level[tilex, tiley] = 1;
                    }

                    // If not at the edge, the number of surrounding tiles is greater than the cellular automata number
                    // Rules for cellular automata, level generation
                    else if (neighbourTilesCount > cellularAutomataNumber)
                    {
                        // The tile becomes a cave tile
                        level[tilex, tiley] = 1;
                    }
                    // Less than cellular automata number
                    else if (neighbourTilesCount < cellularAutomataNumber)
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

    void generateLevel()
    {
        // Clear out any cave tiles that are in the level already (for generating next level)
        foreach (GameObject caveTile in GameObject.FindGameObjectsWithTag("Cave"))
        {
            Destroy(caveTile);
        }

        // Level create
        level = createLevel(levelWidth, levelHeight, levelFillPercent);
        // Level cellular automata 
        level = cellularAutomata(level, smoothingIterations, cellularAutomataNumber, levelWidth, levelHeight);
        // For every tile corrdinates, checking if it is cave tile
        for (int x = 0; x < levelWidth; ++x)
        {
            for (int y = 0; y < levelHeight; ++y)
            {
                // This is a cave tile
                if (level[x, y] == 1)
                {
                    // Instantiating the caveTiles prefab for the level
                    Instantiate(caveTiles, new Vector3(x, 0.0f, y), Quaternion.Euler(Vector3.zero));
                }
            }
        }
    }

    void Start()
    {
        // Generate level at the start
        generateLevel();
    }

    void Update()
    {
        // Generate a new level on each left mouse click
        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("Mouse clicked");
            generateLevel();
        }
    }

}
