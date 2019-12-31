using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManagerScript : MonoBehaviour
{
    public static AudioClip JellyStingNoise;
    public static AudioClip DodgeEffect;
    static AudioSource AudioSrc;

    // Start is called before the first frame update
    void Start()
    {
        JellyStingNoise = Resources.Load<AudioClip>("SoundEffects/taser");
        DodgeEffect = Resources.Load<AudioClip>("SoundEffects/405548__raclure__cancel-miss-chime");
        AudioSrc = GetComponent<AudioSource>();
    }

    public static void PlaySound(string soundToPlay)
    {
        if (AudioSrc != null)
        {
            if (soundToPlay == "taser")
            {
                AudioSrc.PlayOneShot(JellyStingNoise);
            }
            else if (soundToPlay == "miss")
            {
                AudioSrc.PlayOneShot(DodgeEffect);
            }

           
        }
        else
        {
            // You forgot to set JellyStingNoise in SoundManagerScript in Unity props editor!!!
            //Debug.Log("");
        }

      
    }

    //// Update is called once per frame
    //void Update()
    //{
        
    //}
}
