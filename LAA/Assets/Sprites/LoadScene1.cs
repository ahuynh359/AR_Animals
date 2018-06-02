using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadScene1 : MonoBehaviour
{

    public Slider slider;
    private AsyncOperation async;

    void Start()
    {
        StartCoroutine(_Load());

    }

    IEnumerator _Load()
    {
        async = SceneManager.LoadSceneAsync(1);
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
	
	

