using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class Game : MonoBehaviour
{
    public List<Button> buttonList = new List<Button>();
    public Sprite bgImage;
    public Text win;

    public Sprite[] sprites;
    public Button playAgian;
    public List<Sprite> spriteList = new List<Sprite>();

    private bool fGuess, sGuees;

    private int countGuess;
    private int coutCorrectGuesseses;
    private int gameGuesseses;

    private int frirstGuessIndex, secondsGuessIndex;

    private string fristGuessPuzzle, secondGuessPuzzle;


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
        _Random(spriteList);
        gameGuesseses = spriteList.Count / 2;
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
            buttonList[i].image.sprite = bgImage;
        }
    }



    public void _ClickButton()
    {
        foreach (Button b in buttonList)
        {
            b.onClick.AddListener(() => _ButtonEvent());
        }
    }

    public void _ButtonEvent()
    {


        if (!fGuess)
        {
            fGuess = true;
            frirstGuessIndex = int.Parse(EventSystem.current.currentSelectedGameObject.name);
            fristGuessPuzzle = spriteList[frirstGuessIndex].name;
            buttonList[frirstGuessIndex].image.sprite = spriteList[frirstGuessIndex];
        }
        else if (!sGuees)
        {
            sGuees = true;
            secondsGuessIndex = int.Parse(EventSystem.current.currentSelectedGameObject.name);
            secondGuessPuzzle = spriteList[secondsGuessIndex].name;
            buttonList[secondsGuessIndex].image.sprite = spriteList[secondsGuessIndex];

            StartCoroutine(_Check());
        }
    }

   

    public void AddButton()
    {
        int count = buttonList.Count;
        int j = 0;

        System.Random random = new System.Random();
        j = random.Next(0, 41);

        for (int i = 0; i < count; i++)
        {
            if (j == count / 2) j = 0;
            spriteList.Add(sprites[j]);
            j++;
        }



    }

    IEnumerator _Check()
    {
        yield return new WaitForSeconds(0.2f);

        if (fristGuessPuzzle == secondGuessPuzzle)
        {
            yield return new WaitForSeconds(0.2f);
            buttonList[frirstGuessIndex].interactable = false;
            buttonList[secondsGuessIndex].interactable = false;

            buttonList[frirstGuessIndex].image.color = new Color(0, 0, 0, 0);
            buttonList[secondsGuessIndex].image.color = new Color(0, 0, 0, 0);

            _GameIsFinished();
        } else
        {
            buttonList[frirstGuessIndex].image.sprite = bgImage;
            buttonList[secondsGuessIndex].image.sprite = bgImage;
        }

        yield return new WaitForSeconds(0.2f);
        fGuess = sGuees = false;
    }

    private void _GameIsFinished()
    {
        coutCorrectGuesseses++;
        if(coutCorrectGuesseses == gameGuesseses)
        {
           
            win.gameObject.SetActive(true);
            win.text = "Bạn đã thắng";
            playAgian.gameObject.SetActive(true);
          
            
        }
    }

    void _Random(List<Sprite> list)
    {
        for(int i = 0; i < list.Count; i++)
        {
            Sprite sp = list[i];
            int ranom = UnityEngine.Random.Range(i, list.Count);
            list[i] = list[ranom];
            list[ranom] = sp;

        }

    }

  public  void _PLayAgain()
    {
        playAgian.gameObject.SetActive(false);
        win.gameObject.SetActive(false);

        StartCoroutine(_Load());
      
    }

    IEnumerator _Load()
    {
        yield return new WaitForSeconds(0.25f);
        SceneManager.LoadScene(4);
    }
}
