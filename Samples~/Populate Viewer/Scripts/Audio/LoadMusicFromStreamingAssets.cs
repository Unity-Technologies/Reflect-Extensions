using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

[RequireComponent(typeof(AudioSource))]
public class LoadMusicFromStreamingAssets : MonoBehaviour
{
    string filePath;

    private void Awake()
    {
        var path = Path.Combine(Application.streamingAssetsPath, "Music");
        if (!Directory.Exists(path))
        {
            enabled = false;
            return;
        }

        var files = Directory.GetFiles(path, "*.wav");
        if (files.Length == 0)
        {
            enabled = false;
            return;
        }

        filePath = files[0];
    }

    private IEnumerator Start()
    {
        UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.WAV);
        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            Debug.LogWarning(request.error + "\n" + filePath);
        }
        else
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
            var audioSource = GetComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}