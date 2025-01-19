using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class MainLoop : MonoBehaviour
{
    [SerializeField] Cell       cellPrefab;
    [SerializeField] Vector2Int gridSize = new Vector2Int(100, 100);
    int xMax;
    int yMax;
    
    [SerializeField] TMP_InputField gridSizeXField, gridSizeYField;
    [SerializeField] List<Cell>     cellsInGrid;

    [SerializeField] int            numberOfSeeds = 5;
    [SerializeField] TMP_InputField numberOfSeedsField;
    Vector2Int[] seedPoints;

    [SerializeField] TMP_Text populationCounter;
    int    population = 0;
    string populationFormat = "00000000.##";
    string populationString;

    [SerializeField] TMP_Text generationCounter;
    int    generation = 0;
    string generationString;

    List<Cell> cellsToChange = new List<Cell> { };
    float stepTime = 0f;
    bool  isRunning = true;
    
    [SerializeField] int            seed = 42;
    [SerializeField] TMP_InputField randomSeedField;
    [SerializeField] bool           shouldWrap = true;
    [SerializeField] Toggle         shouldWrapToggle;
    List<Vector2Int>                savedCellsVector2    = new List<Vector2Int> { };
    List<int>                       currentConfigToPlace = new List<int> { };


    public Vector2Int GetGridSize()
    {
        return gridSize;
    }

    
    void Start()
    {
        yMax = gridSize.y;
        xMax = gridSize.x;

        GenerateGrid();

        RandomizeSeed();
        GenerateSeedPoints();
        SetSeedPoints();     
        
        StartCoroutine(GetAndChangeCells());

        Pause();
        FindObjectOfType<PauseLabelController>().OnPauseButtonPressed();
    }
  

    void LateUpdate()
    {
        updatePopulationText();
        updateGenerationText();
    }


    public void updatePopulationText()
    {
        population = cellsInGrid[0].GetPopulationCount();
        populationString = population.ToString(populationFormat);
        populationCounter.text = ("Population: " + populationString);
    }


    public void updateGenerationText()
    {
        generationString = generation.ToString(populationFormat);
        generationCounter.text = ("Generation: " + generationString);
    }

    IEnumerator GetAndChangeCells()
    {
        while(isRunning)
        {
            GetCellsThatShouldChange(); //find out which cells change in next generation            
            ChangeCellState();
            generation++;
            yield return new WaitForSeconds(stepTime);
        }        
    }


    public void changeStepTime(float value)
    {
        stepTime = value;
    }


    public void Pause()
    {
        isRunning = !isRunning;
        if (isRunning == true)
        {
            StartCoroutine(GetAndChangeCells());
        }
        else
        {
            StopCoroutine(GetAndChangeCells());
        }
    }


    private void GenerateGrid()
    {
        generation = 0;
        cellsInGrid = new List<Cell> { };

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Cell Cell = Instantiate(cellPrefab, new Vector3(x, y), Quaternion.identity, gameObject.transform);
                cellsInGrid.Add(Cell);
            }
        }
    }


    public void GenerateSeedPoints() 
    { 
        seedPoints = new Vector2Int[numberOfSeeds];

        for (int i = 0; i < numberOfSeeds; )
        {
           Vector2Int seedPoint = new Vector2Int(Random.Range(0, gridSize.x), Random.Range(0, gridSize.y));
           
            for (int j = 0; j < numberOfSeeds; j++)            
            {
                if (seedPoint != seedPoints[j]) // if it's not duplicate
                {
                    if (j == numberOfSeeds - 1) // is it last seed?
                    {
                        seedPoints[i] = seedPoint;
                        i++;
                    }
                }
                else break;
                
            }                    
        }
    }


    public void SetSeedPoints()
    {
        foreach (Vector2Int seedPoint in seedPoints)
        {
            cellsInGrid[GridPosToListPosConverter(seedPoint)].ChangeState();
        }
    }


    private void GetCellsThatShouldChange()
    {       
        cellsToChange = new List<Cell> { };

        for (int listPos = 0; listPos < cellsInGrid.Count; listPos++)       
            
            if (cellsInGrid[listPos].GetState()) // is cell alive?
            {
                int numberOfAliveNeighbours = 0;
                
                if (shouldWrap)
                {
                    numberOfAliveNeighbours = GetNumberOfAliveNeighboursWrapAround(listPos);
                }
                else
                {
                    numberOfAliveNeighbours = GetNumberOfAliveNeighbours(listPos);
                }

                //Any live cell with two or three living neighbours survives.
                if (numberOfAliveNeighbours != 2 ) 
                {
                    if ( numberOfAliveNeighbours != 3)
                    {
                        cellsToChange.Add(cellsInGrid[listPos]);
                    }
                }
                // else survive
            }
            else //  cell is dead
            {
                int numberOfAliveNeighbours = 0;

                if (shouldWrap)
                {
                    numberOfAliveNeighbours = GetNumberOfAliveNeighboursWrapAround(listPos);
                }
                else
                {
                    numberOfAliveNeighbours = GetNumberOfAliveNeighbours(listPos);
                }
                //Any dead cell with three live neighbours becomes a living cell.
                if (numberOfAliveNeighbours == 3)
                {                    
                    cellsToChange.Add(cellsInGrid[listPos]);
                }
                //else stay dead
            }
    }


    private int GetNumberOfAliveNeighbours(int listPos)
    {
        int numberOfAliveNeighbours = 0;

        bool bottomLeftState  = false;
        bool leftState        = false;
        bool topLeftState     = false;
        bool bottomState      = false;
        bool topState         = false;
        bool bottomRightState = false;
        bool rightState       = false;
        bool topRightState    = false;

        
        //check if edgecase        
        if (listPos < yMax) // left edge
        {
            bottomLeftState = false;
            leftState       = false;
            topLeftState    = false;
            if (listPos % yMax == 0)// left bottom
            {
                bottomRightState = false;
                bottomState      = false;
                topState      = GetTopState(listPos);
                rightState    = GetRightState(listPos);
                topRightState = GetTopRightState(listPos);
            }
            else if ((listPos + 1) % yMax == 0) //left top edge
            {
                topState      = false;
                topRightState = false;
                bottomState      = GetBottomState(listPos);
                bottomRightState = GetBottomRightState(listPos);
                rightState       = GetRightState(listPos);
            }
            else // left edge
            {
                bottomState      = GetBottomState(listPos);
                topState         = GetTopState(listPos);
                bottomRightState = GetBottomRightState(listPos);
                rightState       = GetRightState(listPos);
                topRightState    = GetTopRightState(listPos);
            }
        }
        else if (listPos >= yMax * xMax - yMax) // right edge
        {
            bottomRightState = false;
            rightState       = false;
            topRightState    = false;
            if (listPos % yMax == 0)// right bottom
            {
                bottomLeftState = false;
                bottomState     = false;
                leftState    = GetLeftState(listPos);
                topLeftState = GetTopLeftState(listPos);
                topState     = GetTopState(listPos);
            }
            else if ((listPos + 1) % yMax == 0) // right top edge
            {
                topLeftState = false;
                topState     = false;
                leftState       = GetLeftState(listPos);
                bottomLeftState = GetBottomLeftState(listPos);
                bottomState     = GetBottomState(listPos);
            }
            else // right edge
            {
                bottomLeftState = GetBottomLeftState(listPos);
                leftState       = GetLeftState(listPos);
                topLeftState    = GetTopLeftState(listPos);
                bottomState     = GetBottomState(listPos);
                topState        = GetTopState(listPos);
            }
        }
        else if (listPos % yMax == 0) // bottom edge
        {
            bottomLeftState  = false;
            bottomRightState = false;
            bottomState      = false;
            leftState     = GetLeftState(listPos);
            topLeftState  = GetTopLeftState(listPos);
            topState      = GetTopState(listPos);
            topRightState = GetTopRightState(listPos);
            rightState    = GetRightState(listPos);
        }
        else if ((listPos + 1) % yMax == 0) // top edge
        {
            topLeftState  = false;
            topRightState = false;
            topState      = false;
            bottomLeftState  = GetBottomLeftState(listPos);
            leftState        = GetLeftState(listPos);
            bottomState      = GetBottomState(listPos);
            bottomRightState = GetBottomRightState(listPos);
            rightState       = GetRightState(listPos);
        }
        else
        {
            bottomLeftState  = GetBottomLeftState(listPos);
            leftState        = GetLeftState(listPos);
            topLeftState     = GetTopLeftState(listPos);
            bottomState      = GetBottomState(listPos);
            topState         = GetTopState(listPos);
            bottomRightState = GetBottomRightState(listPos);
            rightState       = GetRightState(listPos);
            topRightState    = GetTopRightState(listPos);
        }

        if (bottomLeftState)  { numberOfAliveNeighbours++; }
        if (leftState)        { numberOfAliveNeighbours++; }
        if (topLeftState)     { numberOfAliveNeighbours++; }
        if (bottomState)      { numberOfAliveNeighbours++; }
        if (topState)         { numberOfAliveNeighbours++; }
        if (bottomRightState) { numberOfAliveNeighbours++; }
        if (rightState)       { numberOfAliveNeighbours++; }
        if (topRightState)    { numberOfAliveNeighbours++; }

        return numberOfAliveNeighbours;
    }


    private int GetNumberOfAliveNeighboursWrapAround(int listPos)
    {
        int numberOfAliveNeighbours = 0;

        bool bottomLeftState  = false;
        bool leftState        = false;
        bool topLeftState     = false;
        bool bottomState      = false;
        bool topState         = false;
        bool bottomRightState = false;
        bool rightState       = false;
        bool topRightState    = false;

        
        //check if edgecase        
        if (listPos < yMax) // left edge
        {
            
            leftState = wrapLeftEdgeGetLeft(listPos); //wrapLeftEdgeGetLeft
            
            if (listPos % yMax == 0)// left bottom
            {
                bottomLeftState  =  cellsInGrid[xMax * yMax - 1].GetState();  //unique
                topLeftState     =  wrapLeftEdgeGetTopLeft      (listPos);    
                bottomRightState =  wrapBottomEdgeGetBottomRight(listPos);    
                bottomState      =  wrapBottomedgeGetBottom     (listPos);    
                topState         =  GetTopState                 (listPos);
                rightState       =  GetRightState               (listPos);
                topRightState    =  GetTopRightState            (listPos);
            }
            else if ((listPos + 1) % yMax == 0) //left top edge
            {
                bottomLeftState  = wrapLeftEdgeGetBottomLeft   (listPos);     
                topLeftState     = cellsInGrid[yMax * (xMax - 1)].GetState(); //unique
                topState         = wrapTopEdgeGetTop           (listPos);     
                topRightState    = wrapTopEdgeGetTopRight      (listPos);     
                bottomState      = GetBottomState              (listPos);
                bottomRightState = GetBottomRightState         (listPos);
                rightState       = GetRightState               (listPos);
            }
            else // left edge
            {
                bottomLeftState  = wrapLeftEdgeGetBottomLeft   (listPos); 
                topLeftState     = wrapLeftEdgeGetTopLeft      (listPos); 
                bottomState      = GetBottomState              (listPos);
                topState         = GetTopState                 (listPos);
                bottomRightState = GetBottomRightState         (listPos);
                rightState       = GetRightState               (listPos);
                topRightState    = GetTopRightState            (listPos);
            }
        }
        else if (listPos >= yMax * xMax - yMax) // right edge
        {
            rightState = wrapRightEdgeGetRight(listPos);
            
            if (listPos % yMax == 0)// right bottom
            {
                bottomRightState = cellsInGrid[yMax - 1].GetState();        // unique
                topRightState    = wrapRightedgeGetTopRight      (listPos);
                bottomLeftState  = wrapBottomEdgeGetBottomLeft   (listPos);
                bottomState      = wrapBottomedgeGetBottom       (listPos);
                leftState        = GetLeftState                  (listPos);
                topLeftState     = GetTopLeftState               (listPos);
                topState         = GetTopState                   (listPos);
            }
            else if ((listPos + 1) % yMax == 0) // right top edge
            {
                bottomRightState = wrapRightEdgeGetBottomRight   (listPos);
                topRightState    = cellsInGrid[0]; // unique
                topLeftState     = wrapTopEdgeGetTopLeft         (listPos);
                topState         = wrapTopEdgeGetTop             (listPos);
                leftState        = GetLeftState                  (listPos);
                bottomLeftState  = GetBottomLeftState            (listPos);
                bottomState      = GetBottomState                (listPos);
            }
            else // right edge
            {
                bottomRightState = wrapRightEdgeGetBottomRight (listPos);
                topRightState    = wrapRightedgeGetTopRight    (listPos);
                bottomLeftState  = GetBottomLeftState          (listPos);
                leftState        = GetLeftState                (listPos);
                topLeftState     = GetTopLeftState             (listPos);
                bottomState      = GetBottomState              (listPos);
                topState         = GetTopState                 (listPos);
            }
        }
        else if (listPos % yMax == 0) // bottom edge
        {
            bottomLeftState  = wrapBottomEdgeGetBottomLeft  (listPos);
            bottomRightState = wrapBottomEdgeGetBottomRight (listPos);
            bottomState      = wrapBottomedgeGetBottom      (listPos);
            leftState        = GetLeftState                 (listPos);
            topLeftState     = GetTopLeftState              (listPos);
            topState         = GetTopState                  (listPos);
            topRightState    = GetTopRightState             (listPos);
            rightState       = GetRightState                (listPos);
        }
        else if ((listPos + 1) % yMax == 0) // top edge
        {
            topLeftState     = wrapTopEdgeGetTopLeft  (listPos);
            topRightState    = wrapTopEdgeGetTopRight (listPos);
            topState         = wrapTopEdgeGetTop      (listPos);
            bottomLeftState  = GetBottomLeftState     (listPos);
            leftState        = GetLeftState           (listPos);
            bottomState      = GetBottomState         (listPos);
            bottomRightState = GetBottomRightState    (listPos);
            rightState       = GetRightState          (listPos);
        }
        else
        {
            bottomLeftState  = GetBottomLeftState   (listPos);
            leftState        = GetLeftState         (listPos);
            topLeftState     = GetTopLeftState      (listPos);
            bottomState      = GetBottomState       (listPos);
            topState         = GetTopState          (listPos);
            bottomRightState = GetBottomRightState  (listPos);
            rightState       = GetRightState        (listPos);
            topRightState    = GetTopRightState     (listPos);
        }

        if (bottomLeftState) { numberOfAliveNeighbours++; }
        if (leftState)       { numberOfAliveNeighbours++; }
        if (topLeftState)    { numberOfAliveNeighbours++; }
        if (bottomState)     { numberOfAliveNeighbours++; }
        if (topState)        { numberOfAliveNeighbours++; }
        if (bottomRightState){ numberOfAliveNeighbours++; }
        if (rightState)      { numberOfAliveNeighbours++; }
        if (topRightState)   { numberOfAliveNeighbours++; }


        return numberOfAliveNeighbours;
    }


    #region getStates
    private bool GetTopRightState            (int listPos) => cellsInGrid   [listPos + yMax + 1]                     .GetState();
    private bool GetRightState               (int listPos) => cellsInGrid   [listPos + yMax]                         .GetState();   
    private bool GetBottomRightState         (int listPos) => cellsInGrid   [listPos + yMax - 1]                     .GetState();  
    private bool GetTopState                 (int listPos) => cellsInGrid   [listPos + 1]                            .GetState();    
    private bool GetBottomState              (int listPos) => cellsInGrid   [listPos - 1]                            .GetState();   
    private bool GetTopLeftState             (int listPos) => cellsInGrid   [listPos - yMax + 1]                     .GetState();   
    private bool GetLeftState                (int listPos) => cellsInGrid   [listPos - yMax]                         .GetState();   
    private bool GetBottomLeftState          (int listPos) => cellsInGrid   [listPos - yMax - 1]                     .GetState();
                                                      

    private bool wrapLeftEdgeGetBottomLeft   (int listPos) => cellsInGrid   [listPos + gridSize.x * (yMax - 1) - 1]  .GetState();
    private bool wrapLeftEdgeGetLeft         (int listPos) => cellsInGrid   [listPos + gridSize.x * (yMax - 1)]      .GetState();
    private bool wrapLeftEdgeGetTopLeft      (int listPos) => cellsInGrid   [listPos + gridSize.x * (yMax - 1) + 1]  .GetState();

    private bool wrapRightEdgeGetBottomRight (int listPos) => cellsInGrid   [listPos - yMax * (xMax - 1) - 1]        .GetState();
    private bool wrapRightEdgeGetRight       (int listPos) => cellsInGrid   [listPos - yMax * (xMax -1) ]            .GetState();
    private bool wrapRightedgeGetTopRight    (int listPos) => cellsInGrid   [listPos - yMax * (xMax - 1) + 1]        .GetState();

    private bool wrapTopEdgeGetTopLeft       (int listPos) => cellsInGrid   [listPos - 2* yMax + 1]                  .GetState();
    private bool wrapTopEdgeGetTop           (int listPos) => cellsInGrid   [listPos - yMax + 1]                     .GetState();
    private bool wrapTopEdgeGetTopRight      (int listPos) => cellsInGrid   [listPos + 1]                            .GetState();

    private bool wrapBottomEdgeGetBottomLeft (int listPos) => cellsInGrid   [listPos - 1]                            .GetState();
    private bool wrapBottomedgeGetBottom     (int listPos) => cellsInGrid   [listPos + yMax - 1]                     .GetState();
    private bool wrapBottomEdgeGetBottomRight(int listPos) => cellsInGrid   [listPos + 2* yMax - 1]                  .GetState();
    #endregion


    private void ChangeCellState()
    {
        foreach (Cell cell in cellsToChange)
        {
            cell.ChangeState();
        }
    }


    private int GridPosToListPosConverter(Vector2Int gridPos)
    {
        return gridPos.x* gridSize.y + gridPos.y;
    }


    private Vector2Int ListPosToGridPosConverter(int listPos)
    {
        int xPos;
        int yPos;

        yPos = listPos % gridSize.y; 
        xPos = (listPos - yPos) / gridSize.y;
        
        return new Vector2Int(xPos, yPos); 
    }


    public void GenerateGridButton()
    {
        if (isRunning)
        {
            Pause(); // stop coroutines
            FindObjectOfType<PauseLabelController>().OnPauseButtonPressed();
        }

        cellsInGrid[0].ZeroPopulationCounter();

        foreach (Cell cell in cellsInGrid)
        {
            cell.DestroyCell();
        }
        
        Vector2Int tempGridSize = new Vector2Int(int.Parse(gridSizeXField.text), int.Parse(gridSizeYField.text));
        
        int tempNumberOfSeeds = int.Parse(numberOfSeedsField.text);        
        if (tempNumberOfSeeds >= tempGridSize.x*tempGridSize.y)
        {
            gridSize = new Vector2Int(2,2);
            xMax = gridSize.x;
            yMax = gridSize.y;
            numberOfSeeds = 2;
            numberOfSeedsField.text = "too many Seeds";
            Debug.Log("Number of Seeds to High");            
        }
        else
        {
            gridSize = tempGridSize;
            xMax = gridSize.x;
            yMax = gridSize.y;
            numberOfSeeds = tempNumberOfSeeds;
        }

        GetSeedFromField();
        GenerateGrid();
        GenerateSeedPoints();
        SetSeedPoints();
    }


    public void NoiseButton()
    {
        GetSeedFromField();
        if (int.Parse(numberOfSeedsField.text) >= yMax*xMax)
        {
            Debug.Log("Too Many Seeds");
        }
        else
        {
            numberOfSeeds = int.Parse(numberOfSeedsField.text);
            GenerateSeedPoints();
            SetSeedPoints();
        }
    }


    public void RandomizeSeed()
    {
        seed = Random.Range(10000, 100000);
        randomSeedField.text = seed.ToString();
        Random.InitState(seed);
    }


    private void GetSeedFromField()
    {
        seed = int.Parse(randomSeedField.text);
        Random.InitState(seed);
    }


    public void WrapAroundToggle()
    {
        shouldWrap = !shouldWrap;
    }


    #region storing cell config
    public void StoreConfiguration() // checks for all actives Cells and adds GridPos of active Ones
    {
        List<int> savedCellsListPos = new List<int> { };
        savedCellsVector2 = new List<Vector2Int> { };


        for (int i = 0; i < cellsInGrid.Count; i++)        
        {
            if(cellsInGrid[i].GetState())
            {                
                savedCellsListPos.Add(i);
            }
        }

        foreach (int listPos in savedCellsListPos)
        {
            savedCellsVector2.Add(ListPosToGridPosConverter(listPos));
        }        
    }


    public void ConvertSavedCellsToArray(List<Vector2Int> savedCellsVector2) // takes list of gridPos and converts it to List of listPos
    {        
        currentConfigToPlace = new List<int> { };

        for (int i = 0; i < savedCellsVector2.Count; i++)
        {
            currentConfigToPlace.Add(GridPosToListPosConverter(savedCellsVector2[i]));
        }
    }


    public void PlaceCurrentConfig(Vector2Int gridPos)
    {
        ConvertSavedCellsToArray(savedCellsVector2);
        int listPos = GridPosToListPosConverter(gridPos);

        foreach (int cellPos in currentConfigToPlace)
        {
            
            if (!cellsInGrid[listPos + cellPos].GetState())
            {
                cellsInGrid[listPos + cellPos].ChangeState();
            }
        }
    }


    public void ResetStoredCellConfig()
    {
        currentConfigToPlace = new List<int> {};
        savedCellsVector2 = new List<Vector2Int> { };
    }
    #endregion
}