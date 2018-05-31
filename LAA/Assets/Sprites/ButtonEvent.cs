using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonEvent : MonoBehaviour
{

    public GameObject panel;
    public Slider slider;
    private AsyncOperation async;



    public void _LoadScene0()
    {
        StartCoroutine(_LoadScene(0));
    }

    public void _LoadScene1()
    {
        StartCoroutine(_LoadScene(1));
    }


    public void _LoadScene2()
    {
        StartCoroutine(_LoadScene(2));
    }

    public void _LoadScene3()
    {
        StartCoroutine(_LoadScene(3));
    }

    public void _CloseButton()
    {
        Application.Quit();
    }

    public void _URLButton()
    {
        Application.OpenURL("https://docs.google.com/document/d/1eSJO6RFLWEy-wxfoeNqXEj-9OM2o-BMDV6DPCXT4_5o/edit");
    }

    IEnumerator _LoadScene(int index)
    {
        panel.SetActive(true);
        async = SceneManager.LoadSceneAsync(index);
        async.allowSceneActivation = false;
        while (async.isDone == false)
        {
            slider.value = async.progress;

            if (async.progress == 0.9f)
            {
                slider.value = 1;
                async.allowSceneActivation = true;
            }

            yield return null;
        }


    }
}
