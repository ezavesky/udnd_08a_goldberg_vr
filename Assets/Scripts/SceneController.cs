﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRTK;

public class SceneController : MonoBehaviour {
    //public string[] nameLevels = new string[0];
    public List<string> listLevelNames = new List<string>();
    public TeleportObjToggle teleporterController = null;
    public HintToggle hintController = null;
	
    // [Header("Body Collision Settings")]
    //[Tooltip("If checked then the body collider and rigidbody will be used to check for rigidbody collisions.")]
    protected VRTK_HeadsetFade headsetFade = null;
    protected string nameSceneNext = null;
    protected float timeSceneLoadFade = 1.0f;
    public AudioClip clipOn = null;

	// Use this for initialization
	void Start () {
        GameManager.instance.RegisterSceneController(this);
		headsetFade = GetComponent<VRTK_HeadsetFade>();
        if (headsetFade) 
        {
        	headsetFade.HeadsetFadeComplete += new HeadsetFadeEventHandler(HeadsetFadeComplete);
        }
	
        //FinishSceneInit(SceneManager.GetActiveScene()); 

        // load scene / operation
        if (listLevelNames.Count > 0) {
            // alternate is fade in to new temporary stage
            SceneLoad(listLevelNames[0]);
        }
	}
	
    // called by a goal or game manager
    public void SceneLoad(string strName) 
    {
        nameSceneNext = strName;
        if (headsetFade)        // if we have a valid fade, attempt to do that first
        {
            Invoke("HeadsetFadeBeforeLevel", 0.5f);     //for delay for correct op
        }
        else                    // if we do not, just jump right to scene transition
        {
            StartCoroutine(LoadAsyncScene(strName));
        }
    }

    protected void HeadsetFadeBeforeLevel() 
    {
        headsetFade.Fade(new Color(0f, 0f, 0f, 1f), timeSceneLoadFade);
    }

    // event complete for end of fade, proceed to scene load
	protected void HeadsetFadeComplete(object sender, HeadsetFadeEventArgs args) 
    {
        StartCoroutine(LoadAsyncScene(nameSceneNext));
        nameSceneNext = null;
	}

    // enumeror for scene load completion
    protected IEnumerator LoadAsyncScene(string strName)
    {
        // load the scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(strName, LoadSceneMode.Additive);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }   //end wait for scene load

        Scene sceneNew = SceneManager.GetSceneByName(strName); 
        if (!sceneNew.IsValid())
        {
            Debug.LogError(string.Format("[SceneController]: Attempted to load scene '{0}', but returned invalid!", strName));
            yield break;            
        }
        FinishSceneInit(sceneNew);
    }

    //finish loading a scene
    protected void FinishSceneInit(Scene sceneNew) 
    {
        // finally change the state back to initial
        GameManager.instance.state = GameManager.GAME_STATE.STATE_INITIAL;

        //rediscover hints in new scene
        if (hintController != null) 
        {
            hintController.RediscoverHints(true, sceneNew);
        }

        // on complete, find all of the collectables under new scene
        foreach (GameObject objRoot in sceneNew.GetRootGameObjects()) 
        {
            // start teleporter rediscover
            if (teleporterController != null) 
            {
                teleporterController.RediscoverTeleporters(objRoot);
            }

            GoalController sceneGoal = objRoot.GetComponent<GoalController>();
            if (sceneGoal != null) 
            {
                //reset game manager to find collectables
                sceneGoal.RediscoverCollectables();                    
                // teleport user to spawn within new scene
                sceneGoal.TeleportUser(true);
            }
        }   //end search of goal

        // unfade the screen
        headsetFade.Unfade(timeSceneLoadFade*2);

    }   //end async scene load
}
