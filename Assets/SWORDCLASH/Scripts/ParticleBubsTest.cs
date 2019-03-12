using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleBubsTest : MonoBehaviour {

    // set manually in editor; did not put prefab in resources folder, but it is in scene
    public ParticleSystem BubbleParticlePrefab;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        Input.simulateMouseWithTouches = true;
        foreach (Touch touch in Input.touches)
        {
            //if (touch.phase == TouchPhase.Ended)
            //{
            // Emit is smart enough to NOT emit if they already exist!!!
            //BubbleParticlePrefab.Emit(1);
            //}
            EmitBubble();
        }

        if (Input.GetMouseButtonDown(0))
        {
            EmitBubble();
        }
    }

    public void EmitBubble()
    {
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
        

        Debug.Log("emitParams.postion: " + emitParams.position.ToString());

        BubbleParticlePrefab.Emit(emitParams, 1);
    }

}
