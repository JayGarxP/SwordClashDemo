using UnityEngine;

namespace SwordClash
{

    public class CameraController : MonoBehaviour
    {

        //EDITOR_FIELDS
        public Camera DefaultStartCamera;
        public Camera PortraitCamera;
        public Camera LandscapeCamera;

        private Camera ActiveCamera;

        // Use this for initialization
        void Start()
        {
            ActiveCamera = DefaultStartCamera;
            //DeviceChange.OnOrientationChange += MyOrientationChangeCode; //TODO: Update to new script AND unsubscribe to event with -=
            //DeviceChange.OnResolutionChange += MyResolutionChangeCode;   
        }

        //// Update is called once per frame
        //void Update()
        //{

        //}

        void MyOrientationChangeCode(DeviceOrientation orientation)
        {

            switch (orientation)
            {
                case DeviceOrientation.LandscapeLeft:
                    SetActiveCamera(LandscapeCamera);
                    break;
                case DeviceOrientation.LandscapeRight:
                    SetActiveCamera(LandscapeCamera);
                    break;
                case DeviceOrientation.Portrait:
                    SetActiveCamera(PortraitCamera);
                    break;
                case DeviceOrientation.PortraitUpsideDown:
                    SetActiveCamera(PortraitCamera);
                    break;

                default:
                    SetActiveCamera(DefaultStartCamera);
                    break;
            }



        }

        void MyResolutionChangeCode(Vector2 resolution)
        {
        }

        void SetActiveCamera(Camera newActiveCamera)
        {
            if (newActiveCamera != ActiveCamera)
            {
                ActiveCamera.enabled = false;
                ActiveCamera = newActiveCamera;
                ActiveCamera.enabled = true;
            }
        }

    }
}
