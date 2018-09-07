using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class CreateButton : MonoBehaviour
{

    public Transform panel;
    public Button button;
    public int totalButton;

     void Awake()
    {
        for(int i = 0; i < totalButton; i++)
        {
            Button nButton = Instantiate(button);
            nButton.name = i.ToString();
            nButton.transform.SetParent(panel, false);
        }
    }



}
