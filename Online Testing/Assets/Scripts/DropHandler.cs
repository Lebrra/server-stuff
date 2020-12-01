using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropHandler : MonoBehaviour, IDropHandler
{
    public List<CardButton> cards; 
    
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("dropped", gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public bool checkValidDrop(CardButton card)
    {
        // logic check
        
        // number check
        
        // suit check
        
        // run check

        return false;
        
        return true;
    }
}
