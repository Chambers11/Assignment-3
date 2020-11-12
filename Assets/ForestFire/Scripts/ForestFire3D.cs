﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// class that controls the forest fire cellular automaton
public class ForestFire3D : MonoBehaviour
{
    public int gridSizeX; // x size of the grid
    public int gridSizeY; // y size of the grid
    public int nlight; // the number of trees to set alight at the start of the game
    public int xC, yC; // used for picking random x, y points
    public int windDirection;

    public int rockChance; // the percentage chance a cell is assigned as rock
    public int grassChance; // the percentage chance a cell is assigned as grass

    public GameObject cellPrefab; // sprite used to represent a cell on the grid
    public Text gameRunningText; // text used to display whether the game is running
    // public int[,] gameArray = new int[0, 0];   // an array to hold the state data for each cell in the grid. int 0 represents dead state, int 1 represents alight state
    // public int[,] objectArray = new int[0, 0]; // an array to hold the type of object for each cell in the grid. int 0 represents rock, 1 represents grass, 2 represents tree
    // public int[,] fuelArray = new int[0, 0];   // an array to hold the amount of flammable material for each cell in the grid. 0 represents burnt out or non-flammable (e.g. rocks), >0 means potential to burn
    // public int[,] gameArrayNextGen = new int[0, 0]; // an array to hold the state data for each cell in the grid for the next generation of the game

    public ForestFireCell[,] forestFireCells = new ForestFireCell[0, 0]; // array of ForestFireCell objects
    public ForestFireCell.State[,] forestFireCellsNextGenStates = new ForestFireCell.State[0,0]; // array of cell states to be used in the next generation of the game 


    public GameObject[,] cellGameObjects = new GameObject[0, 0]; // an array to hold references to each gameobject that make up grid
    public SpriteRenderer[,] cellSpriteRenderers = new SpriteRenderer[0, 0]; // an array to hold references to the sprite renderer component attached to each gameobject
    public bool gameRunning = false; // bool controlling whether the game is currently running

    [Range(0.01f, 3f)]
    public float updateRate; // used to define how often will the game update (in seconds)
    private float _gameTimer; // a variable that will be used detect when the game should update 

    private Camera gameCamera; // the game camera pointing at the board

    public int nearestX;
    public int nearestY;



    /*private bool nearsetCellFound;*/  // By default the bool is false, this was relating to running the loop only once in line 75 and 117 for the players position/ finding the nearest cell 

    // Awake is a built-in Unity function that is only called once, before the Start function
    private void Awake()
    {
        // find the camera in the scene and store it for later
        gameCamera = FindObjectOfType<Camera>();
    }

    // Start is a built-in Unity function that is called before the first frame update
    private void Start()
    {
        CreateGrid(gridSizeX, gridSizeY);
        RandomiseGrid();
        PauseGame(true);
        UpdateGridVisuals();
        windDirection = UnityEngine.Random.Range(1, 8); // Setting random value to windDirection,the number 16 represents the diffrent cardinal directions, e.g. N, NNE, NE
    }

    // this function controls whether or not to pause the game
    private void PauseGame(bool setGamePause)
    {
        // if setGamePause is true the game should stop running
        if (setGamePause)
        {
            gameRunning = false;
            gameRunningText.text = "Game Paused";
            gameRunningText.color = Color.red;
        }
        else // else if setGamePause is false unpause the game
        {
            gameRunning = true;
            gameRunningText.text = "Game Running";
            gameRunningText.color = Color.green;
        }
    }

    private void FindNearestCell()
    {
        // Find nearest cell to player 

        ForestFireCell nearestCell = new ForestFireCell();  // Created a local version of the nearestCell (essentially an imaginary one) and the = new ForestFireCell it the code needed to make it more real. nearestCell is to to be an existing cell in line 99
        float currentClosestDistance = 10000; // Basically this is set very high so the when the loop runs for the first time the first cell distance can be assured of being found becuase the current distance between to player for the first cell will be less than 10000

        // Iterate through each cell in the rows and colums, 
        for (int xCount = 0; xCount < gridSizeX; xCount++)
        {
            // Check the current state of the cell and update visual 
            for (int yCount = 0; yCount < gridSizeY; yCount++)
            {

                // This will help us determine the distance beteen vectors a and b or in this case the distance from the player 
                /* Debug.Log(Vector3.Distance(forestFireCells[xCount, yCount].transform.position, gameCamera.transform.position)); // This line of code expresses the distance between the cell we are checking and the camera*/
                // The other way we could write line 87 is as follows:
                float distance;

                distance = Vector3.Distance(forestFireCells[xCount, yCount].transform.position, gameCamera.transform.position);

                // This is retriving the cell with the closest distance to the player which in this case is the camera game object, this will update checking each cell and taking the new shortest distance till the smallest distnce to the player is found 
                if (distance < currentClosestDistance) // This code only gets checked is the current distance between the the first cell checked and the player distance is less than the current set closest distance of 10000m between the player and the first cell checked. 
                {
                    nearestCell = forestFireCells[xCount, yCount]; // Nearst cell is equal to whatever is in the two numbers in the  square brackets in this case e.g. [20,20]

                    // We are also saving it here so that we can call upon the code in this script in the mini map script 
                    nearestX = xCount;
                    nearestY = yCount;

                    currentClosestDistance = distance;

                }
            }
        }
    }




    // Update is a built-in Unity function that is called once per frame 
    private void Update()
    {
        // this line of code is there to show how we run the loop only once, but because we awalys want to know the players position we need the loop to be constantly updating.
        /* if (nearsetCellFound == false)  
         {
         FindNearestCell();
         }*/

        FindNearestCell(); // This is calling upon the code in line 75 down for the loop, and thus constantly updating the players position

        // check if the spacebar key has been pressed. this key will toggle between whether the game is currently running or paused
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // if the gameRunning is true, pause the game
            if (gameRunning)
            {
                PauseGame(true);
            }
            else // if the gameRunning is false, unpause the game
            {
                PauseGame(false);
            }
        }

        // check if the R key has been pressed. this key will clear the grid and pause the game
        if (Input.GetKeyDown(KeyCode.R))
        {
            RandomiseGrid();
        }

        // update the visual state of each cell
        UpdateGridVisuals();

        // if the game is not running, return here to prevent the rest of the code in this Update function from running    
        if (gameRunning == false)
            return;

        if (_gameTimer < updateRate)
        {
            _gameTimer += Time.deltaTime;
        }
        else
        {
            UpdateCells();
            _gameTimer = 0f;
        }
    }

    private void RandomiseGrid()
    {
        nlight = 2; // how many trees to set on fire
                    // iterate through every cell in the cell in the grid and set its state to dead, decide what type of object is present and if flammable assign an amount of fuel
        for (int xCount = 0; xCount < gridSizeX; xCount++)
        {
            for (int yCount = 0; yCount < gridSizeY; yCount++)
            {
                // gameArray[xCount, yCount] = 0;  // nothing burning yet, set all objects to dead state
                // gameArrayNextGen[xCount, yCount] = 0;

               
                xC = UnityEngine.Random.Range(0, 100); // generate a random number between 0 and 100

                if (xC < rockChance) // if the random value is less than rock chance, assign cell as rock
                {                   
                    forestFireCells[xCount, yCount].SetRock();
                }
                else if (xC < grassChance) // if the random value is less than grass chance, assign cell as grass and set cell fuel
                {
                    forestFireCells[xCount, yCount].SetGrass();
                    forestFireCells[xCount, yCount].cellFuel = UnityEngine.Random.Range(1, 5);
                }
                else // if the random value is higher than rock and grass chance, assign as tree and set cell fuel
                {
                    forestFireCells[xCount, yCount].SetTree();
                    forestFireCells[xCount, yCount].cellFuel = UnityEngine.Random.Range(15, 25);
                }
            }
        }

        do
        {
            xC = UnityEngine.Random.Range(0, gridSizeX);// now pick some trees at random and set them alight
            yC = UnityEngine.Random.Range(0, gridSizeY);

            if (forestFireCells[xC, yC].cellState == ForestFireCell.State.Tree)
            {
                forestFireCells[xC, yC].SetAlight();

                nlight = nlight - 1;     // decrease number of trees to light by 1
            }
        } while (nlight > 0);  // when you've lit them all exit this loop

        // set the middle cell as grass which is where the player is placed
        forestFireCells[20, 20].SetGrass();
    }


    // update the status of each cell on grid according to the rules of the game
    private void UpdateCells()
    {
        xC = UnityEngine.Random.Range(0, 100); // Generates a random number between 0 and 100
        if (xC > 50)
        {
            windDirection = windDirection + UnityEngine.Random.Range(-1, 2);
            
            if (windDirection < 1)
            {
                windDirection = 8;
            }
            else if (windDirection > 8)
            {
                windDirection = 1;
            }

            
        }


        // iterate through each cell in the rows and columns
        for (int xCount = 0; xCount < gridSizeX; xCount++)
        {
            for (int yCount = 0; yCount < gridSizeY; yCount++)
            {
                // find out the number of alight neighbours this cell has
                int alightNeighbourCells = CountAlightNeighbourCells(xCount, yCount);

                if (forestFireCells[xCount, yCount].cellState == ForestFireCell.State.Alight) // if the cell is currently alight let it burn but reduce it's fuel and see if it goes out
                {
                     forestFireCells[xCount, yCount].cellFuel--; // reduce fuel by 1 (-- operater reduces an integer by 1)


                    if (forestFireCells[xCount, yCount].cellFuel <= 0) // has it burned all its fuel?
                    {
                        // cell has no fuel so is burned out 
                        forestFireCellsNextGenStates[xCount, yCount] = ForestFireCell.State.Burnt;
                    }
                    else // cell still has fuel so carries on burning
                    {
                        forestFireCellsNextGenStates[xCount, yCount] = forestFireCells[xCount, yCount].cellState;
                    }
                }
                else if (forestFireCells[xCount, yCount].cellFuel > 0)// the cell has fuel but is not alight yet
                {
                    // A dead cell with an alight neighbour which has fuel has a probablility of becoming an alight cell
                    if (alightNeighbourCells > 0)
                    {
                        xC = UnityEngine.Random.Range(0, 100); // generate a random number between 0 and 100

                        if (xC < 10 * alightNeighbourCells) // the more alight neighbours the greater the probability of catching light
                                                            // e.g. 1 alight neighbour = 10 * 1 = 10% chance of catching fire, 2 alight neighbours = 10 * 2 = 20% chance of catching fire, etc.
                        {
                            forestFireCellsNextGenStates[xCount, yCount] = ForestFireCell.State.Alight;  // a 10% chance of catching fire
                        }
                    }
                    else // no neighbours on fire, keep it dead for the next generation of the game
                    {
                        forestFireCellsNextGenStates[xCount, yCount] = forestFireCells[xCount, yCount].cellState;
                    }
                }
            }
        }

        // now the state of each cell has been calculated, apply the results by setting the current game array values to that of the next generation
        for (int xCount = 0; xCount < gridSizeX; xCount++)
        {
            for (int yCount = 0; yCount < gridSizeY; yCount++)
            {
                forestFireCells[xCount, yCount].cellState = forestFireCellsNextGenStates[xCount, yCount];
            }
        }
    }

    // count the alight cells surrounding a specified cell on the grid 
    private int CountAlightNeighbourCells(int cellPositionX, int cellPositionY)
    {
        // create local variable to keep track of alight neighbour cells
        int alightNeighbourCells = 0;

        // the code below tries to iterate through the neighbour cells immediately surrounding the specified cell on the grid as well as the specified cell
        //
        // On the grid the it would like this
        //
        //  N N N
        //  N C N
        //  N N N
        //
        // N = neighbour C = cell that's neighbours are being counted

        for (int xPosition = cellPositionX - 1; xPosition < cellPositionX + 2; xPosition++)
        {
            for (int yPosition = cellPositionY - 1; yPosition < cellPositionY + 2; yPosition++)
            {
                // only continue if the neighbour is valid
                if (IsNeighbourValid(xPosition, yPosition))
                {
                    // if the cell is currently alight (1), increase the count of alight neighbours by one
                    if (forestFireCells[xPosition, yPosition].cellState == ForestFireCell.State.Alight)
                    {
                        alightNeighbourCells++;

                        if (xPosition == cellPositionX + 1 && yPosition == cellPositionY + 1 && windDirection == xC)
                        {
                            alightNeighbourCells--;
                        }

                        // we don't want to check if the specified cell is alight, only its neighbours so it was added, subtract it
                        if (xPosition == cellPositionX && yPosition == cellPositionY)
                        {
                            alightNeighbourCells--;
                        }
                    }
                }
            }
        }

        // return the number of alight neighbour cells
        return alightNeighbourCells;
    }

    // make sure that the cell we are trying to count is not beyond the range of the game grid (edges of the game board)
    private bool IsNeighbourValid(int cellPositionX, int cellPositionY)
    {
        if (cellPositionX < 0 || cellPositionY < 0)
            return false;

        if (cellPositionX >= gridSizeX || cellPositionY >= gridSizeY)
            return false;

        return true;
    }

    // this function creates the grid of the game
    private void CreateGrid(int sizeX, int sizeY)
    {
        // initialise the game and fuel arrays to the size of grid
        // gameArray = new int[sizeX, sizeY];
        // objectArray = new int[sizeX, sizeY];
        // fuelArray = new int[sizeX, sizeY];

        forestFireCells = new ForestFireCell[sizeX, sizeY];
       // forestFireCellsNextGen = new ForestFireCell[sizeX, sizeY];

       forestFireCellsNextGenStates = new ForestFireCell.State[sizeX, sizeY];

        // initialise the game array that will contain the state for each cell in the next generation fo the game
       // gameArrayNextGen = new int[sizeX, sizeY];

        // initialise the array of gameobjects that will hold the sprite renderers on the grid
        cellGameObjects = new GameObject[sizeX, sizeY];

        // initialise the array of sprite renderers that will visualise the grid
        //cellSpriteRenderers = new SpriteRenderer[sizeX, sizeY];

        int xSpacing = 0;
        int ySpacing = 0;

        for (int xCount = 0; xCount < sizeX; xCount++)
        {
            for (int yCount = 0; yCount < sizeY; yCount++)
            {
                // create cell object and name it according to its position (3 adds extra space between cells)
                GameObject newCell = Instantiate(cellPrefab, new Vector3(xCount, 0, yCount), Quaternion.identity);
                newCell.name = "cell " + xCount + " " + yCount;

                //position the cell on the grid, spacing them out using the x and y count as coordinates 
                newCell.transform.position = new Vector3(xCount + xSpacing, 0, yCount + ySpacing);

                // add a sprite renderer to the cell object and assign the sprite it will use
                //newCell.AddComponent<SpriteRenderer>().sprite = cellSprite;

                // add a reference of this sprite renderer to the array so we can change it later quickly
                // cellSpriteRenderers[xCount, yCount] = newCell.GetComponent<SpriteRenderer>();

                // the size of the sprite is quite small, so increase the scale so there are no visable gaps in the grid
                //newCell.transform.localScale = new Vector3(7.5f, 7.5f, 0f);

                // add a box collider to the cell so we can detect clicks from the mouse
                // newCell.AddComponent<BoxCollider>();

               
               

                // add the gameobject of the cell to the array that stores references of the cell sprites
                cellGameObjects[xCount, yCount] = newCell;

                // add to array
                ForestFireCell forestFireCell = newCell.GetComponent<ForestFireCell>();
                forestFireCells[xCount, yCount] = forestFireCell;
                
                 // add reference to camera
                forestFireCell.playerCamera = gameCamera.gameObject;                

                ySpacing += 3;
            }

            ySpacing = 0;
            xSpacing += 3;
        }
    }

    // udpate the grid sprites colours according to their current state
    // this function will be called every frame of the game so the grid is always up to date 
    private void UpdateGridVisuals()
    {
        // iterate through each cell in the rows and columns
        for (int xCount = 0; xCount < gridSizeX; xCount++)
        {
            // check current state of cell an update visual
            for (int yCount = 0; yCount < gridSizeY; yCount++)
            {
                if (forestFireCells[xCount, yCount].cellState == ForestFireCell.State.Alight)
                {
                    forestFireCells[xCount, yCount].SetAlight();
                }
                else if (forestFireCells[xCount, yCount].cellState == ForestFireCell.State.Rock)
                {
                    forestFireCells[xCount, yCount].SetRock();
                }
                else if (forestFireCells[xCount, yCount].cellState != ForestFireCell.State.Rock && forestFireCells[xCount, yCount].cellFuel <= 0)// it's not a rock but it's fuel is zero, therefore it must be burnt out grass or tree
                {
                    forestFireCells[xCount, yCount].SetBurnt();
                }
                else if (forestFireCells[xCount, yCount].cellState == ForestFireCell.State.Grass)
                {
                    forestFireCells[xCount, yCount].SetGrass();
                }
                else if (forestFireCells[xCount, yCount].cellState == ForestFireCell.State.Tree)
                {
                    forestFireCells[xCount, yCount].SetTree();
                }
            }
        }
    }
}