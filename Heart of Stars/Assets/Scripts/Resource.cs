using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class Resource : MonoBehaviour
{
    private Action<ResourceData> OnClicked = delegate { };

    ResourceData myResource;
    List<ResourceData> myDependance;

    public void AssignResource(ResourceData data)
    {
        myResource = data;
        myDependance = new List<ResourceData>();
        string[] str = data.requirements.Split(";");

        //Started to disect the requirements from the data. Need to finish this.
    }

    void ClickedButton()
    {
        //Do something
        //check resources
        //remove resources
        //update resources
        
    }
}
