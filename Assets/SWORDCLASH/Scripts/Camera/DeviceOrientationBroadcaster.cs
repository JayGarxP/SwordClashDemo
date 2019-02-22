using System;
using System.Collections;
using UnityEngine;

/*
 * Doug McFarlane's original DeviceChange script; unaltered. OCT 2015
 * https://forum.unity.com/threads/device-screen-rotation-event.118638/ 
 * 
 */

public class DeviceOrientationBroadcaster : MonoBehaviour
{
    public static event Action<DeviceOrientation> OrientationChangeEvent; //TODO: understand events, delegates, Actions, and what static does here.
    public static float pollRate = 0.7f;        // How long to wait until we check again. static???

    static DeviceOrientation lastOrientation;        // Recently Polled Device Orientation
    static bool isAlive = true;                    // Keep this script running? //How else unsubscribe events???

    void Start()
    {
        lastOrientation = Input.deviceOrientation;
        StartCoroutine(PollDeviceOrientation());
    }

    IEnumerator PollDeviceOrientation()
    {
        //Probably need better solution to stop coroutine and or manage all event listeners/subscribers.
        while (isAlive)
        {

            // Check for an Orientation Change
            switch (Input.deviceOrientation)
            {
                case DeviceOrientation.Unknown:
                case DeviceOrientation.FaceUp:
                case DeviceOrientation.FaceDown:
                    // Ignore
                    break;
                default:
                    if (lastOrientation != Input.deviceOrientation)
                    {
                        lastOrientation = Input.deviceOrientation;
                        OnOrientationChange(lastOrientation);
                    }
                    break;
            }

            //suspend execution until WaitForSeconds(float seconds) returns; 0.5f half second seems okay. Probaly need to ignore the device orientation changing too quickly somehow tho
            yield return new WaitForSeconds(pollRate);
        } //is WaitForSeconds object garbage collected each while loop exit??????????? Better way to do this???????
    }


    void OnOrientationChange(DeviceOrientation currentOrientation)
    {
        //if event has subscribers: broadcast event
        if (OrientationChangeEvent != null)
        {
            //Broadcast Device Orientation
            OrientationChangeEvent(currentOrientation);
        }
    }

    void OnDestroy()
    {
        isAlive = false; //can this ever actually be called??? Does it work because the bool is static? Then would break if multiple events???
    }

}
