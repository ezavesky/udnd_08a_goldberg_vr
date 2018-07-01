﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using UnityEngine.SceneManagement;

public class HintToggle: MonoBehaviour  {
	public float blinkTransitionSpeed = 1.0f;
	public string[] tagSet = new string[0];
	public List<GameObject> listHints = new List<GameObject>();
	protected VRTK_HeadsetFade headsetFade = null;
    public AudioClip clipOn = null;
    public AudioClip clipOff = null;
    public bool enableHintTesting = false;

	void Start() 
	{
		headsetFade = GetComponent<VRTK_HeadsetFade>();
	}

	public void RediscoverHints(bool bForgetPrior=true, object sceneFilter=null)
	{
        if (bForgetPrior || (listHints.Count==0))
		{
            listHints.Clear();
			foreach (string tagSearch in tagSet)
			{
				foreach (GameObject objHint in GameObject.FindGameObjectsWithTag(tagSearch))
                {
                    if (sceneFilter != null && objHint.scene == (Scene)sceneFilter)
                    {
                        listHints.Add(objHint);
                    }
                }   //end scan of individual object
			}
		}
		Debug.Log(string.Format("[HintToggle]: Found a total of {0} hint objects", listHints.Count));
		if (enableHintTesting) 
        {
            //test mode places you into normal state and makes first object in hint list (e.g. the ball) non-kinematic
            GameManager.instance.state = GameManager.GAME_STATE.STATE_NORMAL;
            if (listHints.Count > 0 && listHints[0].transform.childCount > 0) 
            {
                GameObject objStartBall = listHints[0].transform.GetChild(0).gameObject;
                Rigidbody rb = objStartBall.GetComponent<Rigidbody>();
                if (rb != null) 
                {
                    rb.isKinematic = false;
                }
            } 
        }
        else 
        {
            DisableHints(false);
        }
	}

	public void EnableHints() 
	{
        GameManager.instance.state = GameManager.GAME_STATE.STATE_HINTS;
        if (clipOn != null) 
        {
            AudioSource.PlayClipAtPoint(clipOn, Camera.main.transform.position);
        }
		Fade();
	}

	public void DisableHints(bool bRestoreCamera) 
	{
		if (bRestoreCamera && headsetFade) 
		{
			headsetFade.Unfade(blinkTransitionSpeed);
            if (clipOff != null) 
            {
                AudioSource.PlayClipAtPoint(clipOff, Camera.main.transform.position);
            }
            GameManager.instance.state = GameManager.GAME_STATE.STATE_RETURN_TO_LAST;
		}
		foreach (GameObject objHint in listHints) 
		{
			objHint.SetActive(false);
		}	
    }

	protected virtual void Fade()
	{
		if (!headsetFade)
			return;		
		headsetFade.HeadsetFadeComplete += new HeadsetFadeEventHandler(ActivateHints);
		headsetFade.Fade(new Color(1.0f, 1.0f, 1.0f, 0.1f), blinkTransitionSpeed);
	}

	protected void ActivateHints(object sender, HeadsetFadeEventArgs args) 
    {
		foreach (GameObject objHint in listHints) 
		{
			objHint.SetActive(true);
		}
		headsetFade.HeadsetFadeComplete -= new HeadsetFadeEventHandler(ActivateHints);
	}


}
