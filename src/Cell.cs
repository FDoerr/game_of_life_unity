using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    static int populationCounter = 0;
    [SerializeField] Color cellColorAlive = Color.white;
    [SerializeField] Color cellColorDead  = Color.black;
        
    Vector2Int gridPos;
    bool       isActive = false;

    SpriteRenderer spriteRenderer;

    static bool mouseDown = false;

    private void Awake()
    {
        gridPos              = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        spriteRenderer       = GetComponent<SpriteRenderer>();
        spriteRenderer.color = cellColorDead;        
    }

    
    private void OnMouseDown()
    {
        ChangeState();
        FindObjectOfType<MainLoop>().PlaceCurrentConfig(gridPos);
        mouseDown = true;
    }


    private void OnMouseUp()
    {
        mouseDown = false;
    }


    private void OnMouseExit()
    {
        if (mouseDown && !isActive)
        {
            ChangeState();
        }
    }


    public bool GetState()
    {
        return isActive;
    }


    public void ChangeState()
    {
        
        isActive = !isActive;
        if (isActive)
        {
            spriteRenderer.color = cellColorAlive;
            populationCounter++;
        }
        else
        {
            spriteRenderer.color = cellColorDead;
            populationCounter--;
        }
    }


    public void DestroyCell()
    {
        Destroy(gameObject);
    }


    public int GetPopulationCount()
    {
        return populationCounter;
    }


    public void ZeroPopulationCounter()
    {
        populationCounter = 0;
    }
}
