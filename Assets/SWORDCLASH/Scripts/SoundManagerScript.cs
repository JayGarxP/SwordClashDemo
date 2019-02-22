using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManagerScript : MonoBehaviour
{
    public static AudioClip JellyStingNoise;
    static AudioSource AudioSrc;

    // Start is called before the first frame update
    void Start()
    {
        JellyStingNoise = Resources.Load<AudioClip>("SoundEffects/taser");
        AudioSrc = GetComponent<AudioSource>();
    }

    public static void PlaySound(string soundToPlay)
    {
        if (soundToPlay == "taser")
        {
            AudioSrc.PlayOneShot(JellyStingNoise);
        }
    }

    //// Update is called once per frame
    //void Update()
    //{
        
    //}
}
