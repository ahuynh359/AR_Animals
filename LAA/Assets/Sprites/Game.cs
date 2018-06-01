using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public List<Button> buttonList = new List<Button>();
    public Sprite images;

    public Sprite[] sprites;

    public List<Sprite> spriteList = new List<Sprite>();
  

     void Awake()
    {
        sprites = Resources.LoadAll<Sprite>("png");  
    }
    // Use this for initialization
    void Start()
    {
        _GetButtons();
        _ClickButton();
        AddButton();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void _GetButtons()
    {
        GameObject[] buttons = GameObject.FindGameObjectsWithTag("Button");

        for (int i = 0; i < buttons.Length; i++)
        {
            buttonList.Add(buttons[i].GetComponent<Button>());
            buttonList[i].image.sprite = images;
        }
    }

   

    public void _ClickButton()
    {
        foreach(Button b in buttonList)
        {
            b.onClick.AddListener(() => _ButtonEvent());
        }
    }

    public void _ButtonEvent()
    {
        Debug.Log("metv,");
    }

    public void AddButton()
    {
        int count = buttonList.Count;
        int j = 0;
         
        for(int i = 0; i <count; i++)
        {
            if (j == count / 2) i = 0;
            spriteList.Add(sprites[j]);
            j++;
        }

      
    }
}
