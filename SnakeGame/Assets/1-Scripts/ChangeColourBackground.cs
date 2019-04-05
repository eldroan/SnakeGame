using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColourBackground : MonoBehaviour
{
    [SerializeField] private Camera myCamera;
    private List<Color> colorlist;
    private int index;
    void Awake()
    {
        myCamera = this.gameObject.GetComponent<Camera>();
        
        colorlist = new List<Color>();
        colorlist.Add(Color.red);
        colorlist.Add(Color.blue);
        colorlist.Add(Color.cyan);
        colorlist.Add(Color.green);
        colorlist.Add(Color.grey);
    }

    // Update is called once per frame
    public void ChangeBackGroundColour()
    {
        if (myCamera != null)
        {
            myCamera.backgroundColor = colorlist[index++ % colorlist.Count];
        }
    }
}
