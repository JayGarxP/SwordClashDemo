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
                //Emit is smart enough to NOT emit if they already exist!!!
                BubbleParticlePrefab.Emit(1);
            //}
        }

        if (Input.GetMouseButtonDown(0))
        {
            var emitParams = new ParticleSystem.EmitParams();

            //var clickPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
            Debug.Log("mousePostion: " + Input.mousePosition.ToString());
            // Debug.Log("clickPosition: " + clickPosition.ToString());
            //emitParams.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            //mousePos.x = currentEvent.mousePosition.x;
            //mousePos.y = cam.pixelHeight - currentEvent.mousePosition.y;
            //point = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane))

            //  float xPoint = Input.mousePosition.x;
            //  // float yPoint = Camera.main.pixelHeight - Input.mousePosition.y;
            //  float yPoint =   Input.mousePosition.y - Camera.main.pixelHeight;
            //  // float yPoint =   Input.mousePosition.y - Camera.main.scaledPixelHeight;
            //  //float yPoint = Input.mousePosition.y * -1;


            //  //var test2 = Camera.main.ScreenToWorldPoint(new Vector3(xPoint * 4, yPoint * 2, 0));

            ////  test2.y += 10;


            //  var test = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //  //Debug.Log(test);
            //  test.y += 10;
            //  //Debug.Log(test);

            // emitParams.position = (test2);

            //// Get the mouse position in pixels, and convert to camera view by dividing by the number of pixels the camera is displaying.
            //var x = Input.mousePosition.x / Camera.main.scaledPixelWidth;
            //var y = Input.mousePosition.y / Camera.main.scaledPixelHeight;
            //emitParams.position = new Vector3(x, y, 0);


            emitParams.position = Camera.main.ScreenToWorldPoint((Input.mousePosition + 
                new Vector3(0, 0, 5f)));

            Quaternion rotation = Quaternion.Euler(90f, 0, 0);
            //Vector3 myVector = Vector3.one;
            Vector3 rotateVector = rotation * emitParams.position;

            emitParams.position = rotateVector;

            //emitParams.position


            Debug.Log("emitParams.postion: " + emitParams.position.ToString());

            //emitParams.velocity = new Vector3(0.0f, 0.0f, -2.0f);
            BubbleParticlePrefab.Emit(emitParams, 1);



        }


    }
}
