using System;
using UnityEngine;

public class Level_Generator : MonoBehaviour
{
    public int tileWidth;
    public int tileHeight;
    [Range(0, 100)]
    public int fillPercentRandom;
    public string SpecificSeed;
    public bool useRandomSeed;

    int[,] level;

    private void Start()
    {
        generateLevel();
    }

    private void Update()
    {
        //Generate new level on each left mouse click
        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("Mouse clicked");
            generateLevel();
        }
    }

    void fillLevelRandomly()
    {
        //Give random seed to generate different levels
        //Same seed == same level
        if (useRandomSeed)
            SpecificSeed = Time.time.ToString(); //Generate random seed
        System.Random random = new System.Random(SpecificSeed.GetHashCode()); //Converting seed to a number

        for (int x = 0; x < tileWidth; ++x)
        {
            for (int y = 0; y < tileHeight; ++y)
            {
                //To make the boundaries of the level as a cave and not water
                if (x == 0 || x == tileWidth - 1 || y == 0 || y == tileHeight - 1)
                {
                    level[x, y] = 1;
                }
                else
                {
                    level[x, y] = random.Next(0, 100);
                    //if this random value is less than our fillpercent's value then make a wall else water
                    if (level[x, y] <= fillPercentRandom)
                        level[x, y] = 1;
                    else
                        level[x, y] = 0;
                }
            }
        }
    }

    int getNeighbourWallCount(int tilex, int tiley)
    {
        int neighbourWallCount = 0;
        //Iterate on a 3x3 tile grid, which is centered on the tile "x" and tile "y"
        for (int neighbourTilex = tilex - 1; neighbourTilex <= tilex + 1; ++neighbourTilex)
        {
            for (int neighbourTiley = tiley - 1; neighbourTiley <= tiley + 1; ++neighbourTiley)
            {
                //Inside the level
                if (neighbourTilex >= 0 && neighbourTilex < tileWidth && neighbourTiley >= 0 && neighbourTiley < tileHeight)
                {
                    //On the current tile of "x" and "y"
                    if ((neighbourTilex != tilex || neighbourTiley != tiley))
                    {
                        neighbourWallCount += level[neighbourTilex, neighbourTiley];
                    }
                }
                //On the edge tile of the level
                else
                {
                    ++neighbourWallCount;
                }
            }
        }
        return neighbourWallCount;
    }

    void levelSmooth()
    {
        for (int x = 0; x < tileWidth; ++x)
        {
            for (int y = 0; y < tileHeight; ++y)
            {
                int neighbourWallCount = getNeighbourWallCount(x, y);
                //Rules for cellular automata, level generation
                if (neighbourWallCount > 4)
                {
                    level[x, y] = 1;
                }
                else if (neighbourWallCount < 4)
                {
                    level[x, y] = 0;
                }
            }
        }
    }

    private void generateLevel()
    {
        level = new int[tileWidth, tileHeight];
        fillLevelRandomly();
        //Level smoothening iteration
        for(int i=0; i < 5; ++i)
        {
            levelSmooth();
        }

        //Calling mesh generator from "Mesh_Generator" script
        Mesh_Generator genMesh = GetComponent<Mesh_Generator>();
        genMesh.meshGenerate(level, 1);   
    }

    /*void OnDrawGizmos()
    {
        //Draw the caves and water
        if (level != null)
        {
            for (int x = 0; x < tileWidth; ++x)
            {
                for (int y = 0; y < tileHeight; ++y)
                {
                    if (level[x, y] == 1)
                        Gizmos.color = Color.grey;
                    else
                        Gizmos.color = Color.cyan;
                    //Giving the center position of the level/gizmos in the scene
                    Vector3 position = new Vector3((-tileWidth * 0.5f) + x + 0.5f, 0, (-tileHeight * 0.5f) + y + 0.5f);
                    Gizmos.DrawCube(position, Vector3.one);
                }
            }
        }
    }*/
}
