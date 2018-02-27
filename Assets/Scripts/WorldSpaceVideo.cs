using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.IO;

public class WorldSpaceVideo : MonoBehaviour {

    public Material playButtonMaterial;
    public Material pauseButtonMaterial;
    public Renderer playButtonRenderer;
    public string[] videoClips;
    public Text currentMinutes;
    public Text currentSeconds;
    public Text totalMinutes;
    public Text totalSeconds;
    public PlayHeadMover playheadMover;

    private VideoPlayer videoPlayer;
    private AudioSource audioSource;
    private int videoClipIndex;

    private void Awake()
    {
        {
            videoPlayer = GetComponent<VideoPlayer>();
            audioSource = gameObject.AddComponent<AudioSource>();

        }
    }
    // Use this for initialization
    void Start () {
        // Need to release the video texture to clear the screen on start, 
        // otherwise the last played frame from the last session will stick around
        videoPlayer.targetTexture.Release();
        // Set video clip to the first one in the array
        PrepareVideoFromFile(videoClips[0]);
    }
	
	// Update is called once per frame
	void Update () {
		if (videoPlayer.isPlaying)
        {
            SetCurrentTimeUI();
            playheadMover.MovePlayHead(CalculatePlayedFraction());
        }
	}

    public async void PrepareVideoFromFile(string videofile)
    {
        string localvideofile = await AzureBlobStorageClient.instance.DownloadStorageBlockBlobSegmentedOperationAsync(videofile);

        if (File.Exists(localvideofile))
        {
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
                playButtonRenderer.material = playButtonMaterial;
            }
            videoPlayer.url = localvideofile;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.SetTargetAudioSource(0, audioSource);

            //videoPlayer.Prepare();
            ////Wait until video is prepared
            //while (!videoPlayer.isPrepared)
            //{
            //    Debug.Log("Preparing Video");
            //    yield return null;
            //}
            //Debug.Log("Done Preparing Video");
        }
    }

    public void SetNextClip()
    {
        videoClipIndex++;

        if (videoClipIndex >= videoClips.Length)
        {
            videoClipIndex = videoClipIndex % videoClips.Length;
        }
        PrepareVideoFromFile(videoClips[videoClipIndex]);
        SetTotalTimeUI();
        
        //videoPlayer.Play();
        //playButtonRenderer.material = pauseButtonMaterial;
    }

    public void PlayPause()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            playButtonRenderer.material = playButtonMaterial;
        } else
        {
            SetTotalTimeUI();
            videoPlayer.Play();
            playButtonRenderer.material = pauseButtonMaterial;
        }
    }

    void SetCurrentTimeUI()
    {
        string minutes = Mathf.Floor((int) videoPlayer.time / 60).ToString("00");
        string seconds = ((int)videoPlayer.time % 60).ToString("00");

        currentMinutes.text = minutes;
        currentSeconds.text = seconds;
    }
    void SetTotalTimeUI()
    {
        //string minutes = Mathf.Floor((int)videoPlayer.clip.length / 60).ToString("00");
        //string seconds = ((int)videoPlayer.clip.length % 60).ToString("00");

        //totalMinutes.text = minutes;
        //totalSeconds.text = seconds;
    }

    double CalculatePlayedFraction()
    {
        //double fraction = (double)videoPlayer.frame / (double)videoPlayer.clip.frameCount;
        //return fraction;
        return 0;
    }
}
