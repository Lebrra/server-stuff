using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LayoutController : MonoBehaviour
{
    public float outSpacing, inSpacing;
    
    public List<LayoutElement> LayoutElements;

    private HorizontalLayoutGroup layoutGroup;

    private void Awake()
    {
        layoutGroup = GetComponent<HorizontalLayoutGroup>();
    }

    // Start is called before the first frame update
    void Start()
    {
        LayoutElements = GetComponentsInChildren<LayoutElement>().Where(element => element.ignoreLayout == false).ToList();
    }

    public void fanOut()
    {
        layoutGroup.spacing = outSpacing;
    }

    public void squeezeIn()
    {
        // LayoutElements
        layoutGroup.spacing = inSpacing;
    }

    int getNumberOfLayoutElements()
    {
        LayoutElements = GetComponentsInChildren<LayoutElement>().Where(element => element.ignoreLayout == false).ToList();
        return LayoutElements.Count;
    }
    
}
