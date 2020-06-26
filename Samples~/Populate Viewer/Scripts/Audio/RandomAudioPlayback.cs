using System.Collections;
using UnityEngine;

[ExecuteAlways]
public class RandomAudioPlayback : MonoBehaviour
{
    [SerializeField] float interval_min = .5f;
    [SerializeField] float interval_max = 5f;

    [SerializeField] bool refreshAudioSources = false;

    [SerializeField] AudioClip[] clips = default;

    AudioSource[] _sources;

    private void Awake()
    {
        _sources = GetComponentsInChildren<AudioSource>();

        enabled = clips.Length != 0;
    }

    IEnumerator Start ()
    {
        while (true)
        {
            //Debug.Log(Time.time);
            var interval = Random.Range(interval_min, interval_max);

            if (refreshAudioSources)
                _sources = GetComponentsInChildren<AudioSource>();

            if (_sources.Length != 0)
                _sources[Random.Range(0, _sources.Length)].PlayOneShot(clips[Random.Range(0, clips.Length)]);

            yield return new WaitForSeconds(interval);
            //yield return new WaitForSecondsRealtime(interval);
        }
    }

    private void OnEnable()
    {
        StartCoroutine(Start());
    }
}