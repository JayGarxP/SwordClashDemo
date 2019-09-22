using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleBubsTest : MonoBehaviour {

    // set manually in editor; did not put prefab in resources folder, but it is in scene
    public ParticleSystem BubbleParticlePrefab;

    public ParticleSystem BubblePSystemPlayer2;

    bool AmIPlayer2;

    // Use this for initialization
    void Start () {
        AmIPlayer2 = false;
	}
	
	// Update is called once per frame
	void Update () {

        // If player two object is intantiated, we are player2
        var Player2DummyObject = GameObject.FindWithTag("Player2");
        if (Player2DummyObject != null && Player2DummyObject.tag != "/")
        {
            AmIPlayer2 = true;
        }

        if (BoltNetwork.IsClient)
        {
            AmIPlayer2 = true;
        }

        Input.simulateMouseWithTouches = true;
        foreach (Touch touch in Input.touches)
        {
            //if (touch.phase == TouchPhase.Ended)
            //{
            // Emit is smart enough to NOT emit if they already exist!!!
            //BubbleParticlePrefab.Emit(1);
            //}
            if (AmIPlayer2)
            {
                EmitP2Bubbles();
            }
            else
            {
                EmitBubbleRenderer_WORLD_ALIGNED(); //looks good on low dpi/low touch count device; might look bad on high quality phone
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (AmIPlayer2)
            {
                EmitP2Bubbles();
            }
            else
            {
                EmitBubbleRenderer_WORLD_ALIGNED();
            }
        }
    }

    public void EmitBubble()
    {
        var emitParams = new ParticleSystem.EmitParams();

        Debug.Log("mousePostion: " + Input.mousePosition.ToString());

        // add z coordinate in case the game object using the particle system's base transform is on low Z value
        emitParams.position = Camera.main.ScreenToWorldPoint((Input.mousePosition +
              new Vector3(0, 0, 5f)));


        Debug.Log("emitParams.postion BEFORE EULER: " + emitParams.position.ToString());

        // Quaternion to 'undo' -90.0f x rotation units of GameObject transform, or anything with world units.
        Quaternion rotation = Quaternion.Euler(90f, 0, 0);
        //  Particle system prefabs like the one used here often are rotated -90 degrees to float 'up';
        //  this undoes that to work with ScreenToWorldPoint() more naturally without messing with emitParams.rotation



        // Quaternion * vector you're rotating; QuatIdentity must be on LEFT side of multiplication!!!
        Vector3 rotateVector = rotation * emitParams.position;

        emitParams.position = rotateVector;


        Debug.Log("emitParams.postion: " + emitParams.position.ToString());

        BubbleParticlePrefab.Emit(emitParams, 1);
    }

    public void EmitBubbleRenderer_WORLD_ALIGNED()
    {
        var emitParams = new ParticleSystem.EmitParams();

        Debug.Log("mousePostion: " + Input.mousePosition.ToString());

        // Add to Z to help particles sort on top
        emitParams.position = Camera.main.ScreenToWorldPoint((Input.mousePosition +
              new Vector3(0, 0, 5f)));


        BubbleParticlePrefab.Emit(emitParams, 1);
    }


    public void EmitP2Bubbles()
    {
       //Debug.Log("Chris Player2 Bubbles incoming!!!!!!!!!!!!!!!");
        var emitParams = new ParticleSystem.EmitParams();

        //Debug.Log("mousePostion: " + Input.mousePosition.ToString());

        emitParams.position = Camera.main.ScreenToWorldPoint((Input.mousePosition +
            new Vector3(0, 0, 5f)));

        // Quaternion to 'undo' -90.0f x rotation units of GameObject transform, or anything with world units.
        Quaternion rotation = Quaternion.Euler(90f, 0, 0);
        //  Particle system prefabs like the one used here often are rotated -90 degrees to float 'up';
        //  this undoes that to work with ScreenToWorldPoint() more naturally without messing with emitParams.rotation

        // Quaternion * vector you're rotating; QuatIdentity must be on LEFT side of multiplication!!!
        Vector3 rotateVector = rotation * emitParams.position;

        emitParams.position = rotateVector;

        BubblePSystemPlayer2.Emit(emitParams, 1);
    }
}
