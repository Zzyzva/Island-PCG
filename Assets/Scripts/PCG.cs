using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

/**
 * Script contains all logic for generating the world
 * 
 */
public class PCG : MonoBehaviour
{
    /**
     * Private class holding representation for each tile. 
     * A 2d array is made of tiles and each one is read by TileScript to decided what sprite it should be
     */
    private class tile
    {
        public Enums.TileID id; //What type of tile it is
        public int x; //X position
        public int y; //Y position
        public int info; //Any extra info, like orientation
        public Boolean coast = false;

        public tile(Enums.TileID id, int x, int y)
        {
            this.id = id;
            this.x = x;
            this.y = y;
            info = 0;
        }
    }


    public GameObject Tile; //GameObject representation of single tile in world
    public GameObject Camera; //Main Camera

    //Initial setup
    public int size; //x and y size of the world

    //Continent Borders Poisson
    public int contTotalGrass; //Total nodes to be in the continent
    public int contNearbyCount; //Number of nodes to be placed near each node
    public int contNearbyRange; //Range of node placement
    public int contNodeSize; //Size around the node the fill in
    public int contMax; //Total number of grass from extra continents
    public int contSize; //Size of extra continents

    //Rivers
    public int riverNum; //Number of rivers to generate

    //Mountains
    private int totalElevation = 0; //Total elevation of all land
    private int totalLand = 0; //Total number of land tiles
    public float mountainScale; // Multiplied by average elevation to define min elevation of mountains
    public int montNumRanges;
    public int montRangeSize;



    tile[,] tiles; //2D array representation of all tile sin the world
    /**
     * Called on start, sets up the world making a size x size grid of water tiles
     * No PCG here, always runs teh same
     */
    void Start()
    {
        tiles = new tile[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                tiles[i, j] = new tile(Enums.TileID.sea, i, j);
                
            }
        }

        BuildContinent();
        BuildContinent2();
        for ( int i  = 0; i < montNumRanges; i++)
        {
            BuildMountains();
        }
        
        SmoothContinent();

        for ( int i = 0; i < riverNum; i ++)
        {
           SpawnRiver();
        }
        setTundra();
        setIce();
        Camera.GetComponent<CameraScript>().setup(size);
        SetCoasts();
        GenerateWorld();

    }

    /**
     * Takes the Tile Array and generates tiles for it
     */
    void GenerateWorld()
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                GameObject t = Instantiate(Tile);
                t.GetComponent<TileScript>().setPosition(i, j);
                t.GetComponent<TileScript>().setID(tiles[i, j].id, tiles[i, j].info, tiles[i,j].coast);
            }
        }
    }

 

    /**
     * Builds the continent, setting random tiles equal to grass tiles
     */
    void BuildContinent()
    {
        //Select random start of continent
        List<tile> grassQueue = new List<tile>();
        List<tile> grassNodes = new List<tile>();
        int x = size / 2;
        int y = size / 2;
        tiles[x, y].id = Enums.TileID.grass;
        grassQueue.Add(tiles[x, y]);
        grassNodes.Add(tiles[x, y]);

        //Adds grass nodes around start to grow the continet randomly from the middle
        int count = 0;
        while (count < contTotalGrass && grassQueue.Count != 0)
        {
            tile temp = grassQueue[0];
            for (int i = 0; i < contNearbyCount; i++)
            {

                int x2 = temp.x + Random.Range(-contNearbyRange, contNearbyRange);
                int y2 = temp.y + Random.Range(-contNearbyRange, contNearbyRange);
                if (x2 < (contNodeSize + 1) || x2 >= size - (contNodeSize + 1) || y2 < (contNodeSize + 1) || y2 >= size - (contNodeSize + 1))
                {
                    break;
                }
                else if (tiles[x2, y2].id != Enums.TileID.grass)
                {
                    tiles[x2, y2].id = Enums.TileID.grass;
                    count++;
                    grassQueue.Add(tiles[x2, y2]);
                    grassNodes.Add(tiles[x2, y2]);
                    totalLand++;
                }
            }
            grassQueue.Remove(temp);
        }

        //Surround each node with grass
        foreach( tile node in grassNodes)
        {
            int x3 = node.x;
            int y3 = node.y;
            for ( int i = -contNodeSize; i < (contNodeSize + 1); i++)
            {
                for ( int j = -contNodeSize ; j < (contNodeSize + 1); j++)
                {
                    if ( Mathf.Abs(i) + Mathf.Abs(j) <= contNodeSize && tiles[x3 + i, y3 + j].id != Enums.TileID.grass)
                    {
                        totalLand++;
                        tiles[x3 + i, y3 + j].id = Enums.TileID.grass;
                    }
                }
            } 
        }
    }

    /**
     * Adds small coninents to main landmass until continent is big enough
     */
    void BuildContinent2()
    {
        
        int total = 0;
        do
        {
            int x = Random.Range((int)(size * .3), (int)(size * .7));
            int y = Random.Range((int)(size * .3), (int)(size * .7));
            while (tiles[x, y].id != Enums.TileID.grass)
            {
                x = Random.Range((int)(size * .3), (int)(size * .7));
                y = Random.Range((int)(size * .3), (int)(size * .7));
            }
            HashSet<tile> newContinent = new HashSet<tile>();
            HashSet<tile> newContOverlay = new HashSet<tile>();
            int rTemp = -1;
            for (int j = 0; j < contSize; j++)
            {
                if (x > 2 && x < size - 2 && y > 2 && y < size - 2)
                {
                    newContOverlay.Add(tiles[x + 1, y]);
                    if (tiles[x + 1, y].id != Enums.TileID.grass)
                    {
                        newContinent.Add(tiles[x + 1, y]);
                    }
                    newContOverlay.Add(tiles[x - 1, y]);
                    if (tiles[x - 1, y].id != Enums.TileID.grass)
                    {
                        newContinent.Add(tiles[x - 1, y]);
                    }
                    newContOverlay.Add(tiles[x, y + 1]);
                    if (tiles[x, y + 1].id != Enums.TileID.grass)
                    {
                        newContinent.Add(tiles[x, y + 1]);
                    }
                    newContOverlay.Add(tiles[x, y - 1]);
                    if (tiles[x, y - 1].id != Enums.TileID.grass)
                    {
                        newContinent.Add(tiles[x, y - 1]);
                    }
                }

                //Ensures the continent does not go the same way twice, leading to narrow passages
                int r = Random.Range(0, 4);
                while ( r == rTemp)
                {
                    r = Random.Range(0, 4);
                }
                rTemp = r;

                if (r == 0)
                {
                    x++;
                }
                else if (r == 1)
                {
                    x--;
                }
                else if (r == 2)
                {
                    y++;
                }
                else
                {
                    y--;
                }
            }
            if ( newContinent.Count > 5)
            {
                foreach ( tile t in newContinent)
                {
                    totalLand++;
                    total++;
                    t.id = Enums.TileID.grass;
                }
            }
        } while (total < contMax);    
    }





    /**
     * Removes "bad" features of the continent
     * Removes land with 3 sea edges
     * Removes sea with 4 land edges
     * Removes Lone mountains
     */
    void SmoothContinent()
    {
        foreach (tile node in tiles)
        {
            int x = node.x;
            int y = node.y;
            int seaEdges = 0;
            int grassEdges = 0;
            int mountainEdges = 0;
            if ( x == 0 || x == size - 1 || y == 0 || y == size - 1)
            {
                continue;        
            }
            //For each tile, talley what its neighbors are
            if (tiles[x + 1, y].id == Enums.TileID.sea)
                seaEdges++;
            else if (tiles[x + 1, y].id == Enums.TileID.grass)
                grassEdges++;
            else if (tiles[x + 1, y].id == Enums.TileID.mountain)
                mountainEdges++;
            if (tiles[x - 1, y].id == Enums.TileID.sea)
                seaEdges++;
            else if (tiles[x - 1, y].id == Enums.TileID.grass)
                grassEdges++;
            else if (tiles[x - 1, y].id == Enums.TileID.mountain)
                mountainEdges++;
            if (tiles[x, y + 1].id == Enums.TileID.sea)
                seaEdges++;
            else if (tiles[x, y + 1].id == Enums.TileID.grass)
                grassEdges++;
            else if (tiles[x, y + 1].id == Enums.TileID.mountain)
                mountainEdges++;
            if (tiles[x, y - 1].id == Enums.TileID.sea)
                seaEdges++;
            else if (tiles[x, y - 1].id == Enums.TileID.grass)
                grassEdges++;
            else if (tiles[x, y - 1].id == Enums.TileID.mountain)
                mountainEdges++;

            //If any neighbor combination is "bad", set it to an eacceptable tile
            if (seaEdges == 3 && node.id != Enums.TileID.sea )
            {
                node.id = Enums.TileID.sea;
            }
            if (grassEdges == 4 && node.id == Enums.TileID.sea)
            {
                node.id = Enums.TileID.grass;
            }
            if (mountainEdges == 0 && node.id == Enums.TileID.mountain)
            {
                node.id = Enums.TileID.grass;
            }
        }
    }

    /**
     * Sets the coasts of the land after continent generation,
     * letting beaches be drawn onto continent
     */
    void SetCoasts()
    {
        foreach (tile node in tiles)
        {
            int x = node.x;
            int y = node.y;
            if (x == 0 || x == size - 1 || y == 0 || y == size - 1 )
            {
                continue;
            }
            //For each grass node, check sournding edges and make "binary" number for each edge, stored in info
            //(South)(East)(North(West)
            //If there is a one in that slot, TileScript will draw a coast on that side
            if (  node.id != Enums.TileID.sea && node.id != Enums.TileID.river && node.id != Enums.TileID.ice)
            {
                int info = 0;
                Boolean isCoast = false;
                if (tiles[x - 1, y].id == Enums.TileID.sea)//West
                {
                    info += 1;
                    isCoast = true;
                }


                if (tiles[x, y + 1].id == Enums.TileID.sea)//North
                {
                    info += 10;
                    isCoast = true;
                }

                if (tiles[x + 1, y].id == Enums.TileID.sea)//East
                {
                    info += 100;
                    isCoast = true;
                }

                if (tiles[x, y - 1].id == Enums.TileID.sea)//South
                {
                    info += 1000;
                    isCoast = true;
                }
                if (isCoast)
                {
                    tiles[x, y].coast = true;
                    tiles[x, y].info = info;
                }
            } 
        }
            
    }





    /**
     * Build the mountains based on tile elevation
     */
    void BuildMountains()
    {
        int x;
        int y;
        int errorCount = 0;
        do
        {
            x = Random.Range((int)(size * .3), (int)(size * .7));
            y = Random.Range((int)(size * .3), (int)(size * .7));
            errorCount++;
            if(errorCount == 100)
            {
                throw new Exception("Error creating mountain range)");
            }
        } while (tiles[x, y].id != Enums.TileID.grass);
        int x2 = x;
        int y2 = y;
        tiles[x2, y2].id = Enums.TileID.mountain;
        for ( int i = 0; i < montRangeSize; i++)
        {
            int r = Random.Range(0, 4);
            if ( r == 0 )
            {
                x2++;
            } else if ( r == 1 )
            {
                x2--;
            } else if ( r == 2 )
            {
                y2++;
            } else if ( r == 3)
            {
                y2--;
            }
            if(tiles[x2, y2].id != Enums.TileID.sea)
            {
                tiles[x2, y2].id = Enums.TileID.mountain;
            } else
            {
                i--;
                x2 = x;
                y2 = y;
                if ( i == -1)
                {
                    throw new Exception("Error creating mountain range)");
                }
            }
            
        }
    }







    void SpawnRiver()
    {
        Boolean validStart = true;
        int x;
        int y;
        int errorCount = 0;
        do
        {
            x = Random.Range((int)(size * .4), (int)(size * .6));
            y = Random.Range((int)(size * .4), (int)(size * .6));
            validStart = true;

            for (int i = -2; i <= 2; i++)
            {
                for (int j = -2; j <= -2; j++)
                {
                    if (tiles[x + i, y + j].id == Enums.TileID.river)
                    {
                        validStart = false;
                        errorCount++;
                    }

                }
            }
            if ( errorCount > 250)
            {
                throw new Exception("Error when making river");
            }
        } while (!validStart);
        

        tiles[x, y].id = Enums.TileID.river;
        int direction = Random.Range(0, 4);
        int justTurned = 0;
        while (true)
        {

            //Check if we have reached the sea
            if ( tiles[x + 1, y].id == Enums.TileID.sea || tiles[x - 1, y].id == Enums.TileID.sea || tiles[x, y + 1].id == Enums.TileID.sea || tiles[x, y - 1].id == Enums.TileID.sea)
            {
                break;
            }

            

            //Pick a new direction
            int tempDirection = direction;
            if ( justTurned == 0) 
            {
                double turnFactor = NextGaussianDouble();
                if (turnFactor < -.9)
                {
                    tempDirection--;
                    justTurned = 2;
                }
                else if (turnFactor > .9)
                {
                    tempDirection++;
                    justTurned = 2;
                }
            } else
            {
                justTurned--;
            }



            //Move in the new direction
            Boolean collision = false;
            if ( tempDirection == 0 || tempDirection == 4) //North
            {
                if ( tiles[x,y + 1].id == Enums.TileID.river)
                {
                    collision = true;
                }
                tiles[x, y].info += 1;
                tiles[x, ++y].id = Enums.TileID.river;
                tiles[x, y].info += 100;
            } else if ( tempDirection == 1) //East
            {
                if (tiles[x + 1, y].id == Enums.TileID.river)
                {
                    collision = true;
                }
                tiles[x, y].info += 10;
                tiles[++x, y].id = Enums.TileID.river;
                tiles[x, y].info += 1000;
            } else if (tempDirection == 2) //South
            {
                if (tiles[x, y - 1].id == Enums.TileID.river)
                {
                    collision = true;
                }
                tiles[x, y].info += 100;
                tiles[x, --y].id = Enums.TileID.river;
                tiles[x, y].info += 1;
            } else //West
            {
                if (tiles[x - 1, y].id == Enums.TileID.river)
                {
                    collision = true;
                }
                tiles[x, y].info += 1000;
                tiles[--x, y].id = Enums.TileID.river;
                tiles[x, y].info += 10;
            }
            if ( collision)
            {
                break;
            }

        }
    }


    public void setIce()
    {
        for ( int i = 0; i < size; i++)
        {
            tiles[i, 0].id = Enums.TileID.ice;
            tiles[i, size - 1].id = Enums.TileID.ice;
        }
        for ( int i = 0; i < size / 8; i++)
        {
            int r = Random.Range(0, size);
            tiles[r, 1].id = Enums.TileID.ice;
            r = Random.Range(0, size);
            tiles[r, size - 2].id = Enums.TileID.ice;
        }
    }

    public void setTundra()
    {
        for ( int i = 0; i < size / 4; i++)
        {
            for ( int j = 0; j < size; j++)
            {
                if ( tiles[j,i].id == Enums.TileID.grass)
                {
                    if( i == size / 4 - 1)
                    {
                        int r = Random.Range(0, 2);
                        if( r == 0)
                        {
                            tiles[j, i].id = Enums.TileID.snow;
                        }
                    } else
                    {
                        tiles[j, i].id = Enums.TileID.snow;
                    }
                    
                }
                if (tiles[j, size - 1 - i].id == Enums.TileID.grass)
                {
                    if (i == size / 4 - 1)
                    {
                        int r = Random.Range(0, 2);
                        if (r == 0)
                        {
                            tiles[j, size - 1 - i].id = Enums.TileID.snow;
                        }
                    }
                    else
                    {
                        tiles[j, size - 1 - i].id = Enums.TileID.snow;
                    }
                }
            }
        }
    }


    public static double NextGaussianDouble()
    {
        float u, v, S;

        do
        {
            u = 2 * Random.value - 1;
            v = 2 * Random.value - 1;
            S = u * u + v * v;
        }
        while (S >= 1.0);

        double fac = Mathf.Sqrt(-2 * Mathf.Log(S) / S);
        return u * fac;
    }
}



