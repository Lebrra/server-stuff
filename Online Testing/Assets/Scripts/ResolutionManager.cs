using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Determines the device resolution and adjusts canvas accordingly
/// </summary>
public class ResolutionManager : MonoBehaviour
{
    public float convertionRatio = -1F;

    private void Awake()
    {
        Debug.Log("Screen resolution: " + Screen.currentResolution);

        int canvasCount = 0;
        foreach (CanvasScaler canvas in FindObjectsOfType<CanvasScaler>())
        {
            //if(Screen.currentResolution.width < 2000)
            //    canvas.referenceResolution = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);

            float widthComparator;

            if (Screen.currentResolution.width / Screen.currentResolution.height <= 1.5F)
            {
                widthComparator = 1920F;
            }
            else
            {
                widthComparator = 2220F;
            }

            float height = Screen.currentResolution.height * widthComparator / Screen.currentResolution.width;
            canvas.referenceResolution = new Vector2(widthComparator, height);
            convertionRatio = height / 2220F * 2F;

            canvasCount++;
        }
        Debug.Log("Canvases altered: " + canvasCount);
        Debug.Log("conversion ratio: " + convertionRatio);
    }
}
