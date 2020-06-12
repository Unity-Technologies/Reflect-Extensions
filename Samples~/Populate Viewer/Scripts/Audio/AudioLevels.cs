using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioLevels : MonoBehaviour
{
    [SerializeField] AudioMixer mixer = default;

    [SerializeField] private string musicParamName = "Music Volume", sfxParamName = "Birds Volume";
    [SerializeField] private Slider musicSlider = default, sfxSlider = default;

    private float musicLvl = 0, sfxLvl = 0;

    public float MusicLevel
    {
        get
        {
            mixer.GetFloat(musicParamName, out musicLvl);
            musicLvl = Mathf.Pow(10, musicLvl * 0.05f);
            return musicLvl;
        }
        set
        {
            musicLvl = Mathf.Log(value) * 20;
            mixer.SetFloat(musicParamName, musicLvl);
        }
    }

    public float SfxLevel
    {
        get
        {
            mixer.GetFloat(sfxParamName, out sfxLvl);
            sfxLvl = Mathf.Pow(10, sfxLvl * 0.05f);
            return sfxLvl;
        }
        set
        {
            sfxLvl = Mathf.Log(value) * 20;
            mixer.SetFloat(sfxParamName, sfxLvl);
        }
    }

    private void Start()
    {
        mixer.GetFloat(musicParamName, out musicLvl);
        musicLvl = Mathf.Pow(10, musicLvl * 0.05f);
        musicSlider?.SetValueWithoutNotify(musicLvl);

        mixer.GetFloat(sfxParamName, out sfxLvl);
        sfxLvl = Mathf.Pow(10, sfxLvl * 0.05f);
        sfxSlider?.SetValueWithoutNotify(sfxLvl);
    }
}