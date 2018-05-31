using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayVideo : MonoBehaviour
{

    public RawImage image;
    private VideoPlayer videoPlayer;
    private VideoClip clips;
    public Text time;
    public Button close;

    
    
    void Start()
    {
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        Application.runInBackground = true;
        StartCoroutine(_PlayVideo(Data.instance._GetName()));

    }

    IEnumerator _PlayVideo(string name)
    {

        videoPlayer.playOnAwake = false;
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = _LoadVideo(name);
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }
        image.texture = videoPlayer.texture;
        videoPlayer.Play();

        while (videoPlayer.isPlaying)
        {
            time.GetComponent<Text>().text = (Mathf.FloorToInt((float)videoPlayer.time)).ToString();
            yield return null;
        }
        time.GetComponent<Text>().text = "Kết thúc";
    }

    // Update is called once per frame
    void Update()
    {


        if (!videoPlayer.isPlaying)
        {
            close.gameObject.SetActive(true);
        }
        else
        {
            close.gameObject.SetActive(false);
        }


    }

    private VideoClip _LoadVideo(string name)
    {
        return (VideoClip)Resources.Load("Video/" + name);

    }
}
