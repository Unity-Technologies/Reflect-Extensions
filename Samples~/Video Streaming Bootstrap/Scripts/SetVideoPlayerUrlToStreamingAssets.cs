using System.IO;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class SetVideoPlayerUrlToStreamingAssets : MonoBehaviour
{
    private void Awake()
    {
        var videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;

        var path = Path.Combine(Application.streamingAssetsPath, "Videos");
        if (!Directory.Exists(path))
        {
            enabled = false;
            return;
        }

        var files = Directory.GetFiles(path, "*.mp4");
        if (files.Length == 0)
        {
            enabled = false;
            return;
        }

        videoPlayer.url = files[0];
        videoPlayer.Play();
    }
}