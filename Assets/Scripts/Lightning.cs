using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lightning : MonoBehaviour
{
    public float minDelay = 10.0f;
    public float maxDelay = 40.0f;

    public float minDuration = 0.4f;
    public float maxDuration = 2.0f;
    public float blinkRepeat = 0.08f;
    float waitElapsed = 0;
    float blinkElapsed = 0;

    public CharacterManager charManager;

    public Color blink = Color.white;

    float nextDelay;
    float duration;

    Color original;
    AudioSource strike;

    Camera blinkingCam;
    
	// Use this for initialization
	void Start ()
    {
        blinkingCam = Camera.main;
        strike = GetComponent<AudioSource>();
        original = blinkingCam.backgroundColor;
        nextDelay = Random.Range(minDelay, maxDelay);
        waitElapsed = 0.0f;
        blinkElapsed = -1.0f;
	}
	
	// Update is called once per frame
	void Update () {
        if (waitElapsed >= 0)
        {
            waitElapsed += Time.deltaTime;
            if (waitElapsed >= nextDelay)
            {
                duration = Random.Range(minDuration, maxDuration);
                waitElapsed = -1.0f;
                blinkElapsed = 0.0f;
                strike.Play();
                InvokeRepeating("Blink", 0.2f, blinkRepeat);
                IEnumerator<Character> chars = charManager.GetCharactersIterator();
                while (chars.MoveNext())
                {
                    if (!chars.Current.Dead)
                    {
                        chars.Current.Talk(SpeechEntry.ScaredLightning);
                    }
                }                
            }
        }
        else if (blinkElapsed >= 0)
        {
            blinkElapsed += Time.deltaTime;
            if (blinkElapsed >= duration)
            {
                blinkElapsed = -1.0f;
                waitElapsed = 0;
                nextDelay = Random.Range(minDelay, maxDelay);
                blinkingCam.backgroundColor = original;
                CancelInvoke("Blink");
            }
        }
	}

    void Blink()
    {
        if (blinkingCam.backgroundColor == original)
        {
            blinkingCam.backgroundColor = blink;
        }
        else
        {
            blinkingCam.backgroundColor = original;
        }
    }
}
