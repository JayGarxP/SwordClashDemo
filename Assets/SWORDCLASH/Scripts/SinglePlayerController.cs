using DigitalRubyShared; //Fingers bought full version 1/10/2019
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SwordClash
{
    public class SinglePlayerController : MonoBehaviour
    {
        #region EDITOR_FIELDS
        public float swipeCircleRayCastRadius; //~10.0f

        //TODO: need more fine-grained control than just dividend, need clamp and smoothing + better gesture properties to not have deadzones!
        // = 1f; // where 1.5 means 1.5* greater y axis movement needed to for gesture event to fire.
        public float UPSwipeGestureDirectionThreshold;
        // left right down swipes need to be more precise
        public float LeftRightDownSwipeGestureDirectionThreshold;
        // time between taps allowed for double tap
        public float DoubleTapTimeThreshold;

        public Camera CameraReference;
        public Text SwipeAngleText;

        //public GameObject LeftTentacle;

        // set manually in editor; did not put prefab in resources folder, but it is in scene
        public ParticleSystem BubbleParticlePrefab;

        // ImageScript set in editor, to recognize circles
        public FingersImageGestureHelper_SC_BarrelRoll ImageReconzrScript;
        #endregion

        private TapGestureRecognizer TapGesture; //juke by which half of screen tapped
        private TapGestureRecognizer DoubleTapGesture; //dodge roll if double tap on tentacle tip
        //TODO: consider making a Swipe.Any gesture and deciding which kind it is in the callback method
        private SwipeGestureRecognizer UpSwipeGesture; //send out tentacle
        private SwipeGestureRecognizer LeftSwipeGesture; //wall jump off right wall
        private SwipeGestureRecognizer RightSwipeGesture; //wall jump off left wall
        private SwipeGestureRecognizer DownSwipeGesture; //Reel in tentacle


        /// <summary>
        /// Fingers DemoScriptDragSwipe TEST
        /// to get drag positioning while in coiled state
        /// </summary>
        [Tooltip("Speed (in units) that will cause the card to get swiped off the screen")]
        [Range(25.0f, 100.0f)]
        public float SwipeAwaySpeed = 60.0f;

        private LongPressGestureRecognizer longPress;
        private Transform draggingCard;
        private Vector3 dragOffset;
        private Vector3 startDragPosition; // both for dragSwipe
        private readonly List<Transform> swipedCards = new List<Transform>();
        /// <summary>
        /// end
        /// </summary>

        private SinglePlayerTentaController TentaController;
        private Vector2 TentacleTipStartPosition;
        private bool TTPrefabset;
        //private short TTInSceneCount;
       // private bool AmIPlayerTwo;

        // Use this for initialization
        void Start()
        {
            //CreateDoubleTapGesture(); //TODO: find event order solution: https://stackoverflow.com/questions/374398/are-event-subscribers-called-in-order-of-subscription

            //CreateDoubleTapGesture(); // test if order matters; it does sadly... :(
            CreateSwipeGestures();
            CreateTapGesture();

            //LeftTentacle = null;
            TTPrefabset = false;
            //TTInSceneCount = 0;
            //AmIPlayerTwo = false;

            ////////////////////// SHOW TOUCHES
            FingersScript.Instance.ShowTouches = true;
            ///////////////////

        }

        // TODO: massage this method to eventually move DAT MF THANG in coiled state.
        private void LongPress_StateUpdated(DigitalRubyShared.GestureRecognizer gesture)
        {
          
            if (gesture.State == GestureRecognizerState.Began)
            {
                Vector3 gestureWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(gesture.FocusX, gesture.FocusY, 0.0f));
               
            //    // apply an offset from the center of the card so it drags from wherever it was touched on the card
                dragOffset = TentaController.TentacleTip.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(gesture.FocusX, gesture.FocusY, 0.0f));
                dragOffset.z = 0.0f;

                startDragPosition = TentaController.TentacleTip.transform.position;
            }
            else if (gesture.State == GestureRecognizerState.Executing)
            {
                // drag the card
                Vector3 dragCurrent = Camera.main.ScreenToWorldPoint(new Vector3(gesture.FocusX,
                    gesture.FocusY, 0.0f));
                dragCurrent.z = TentaController.TentacleTip.transform.position.z;

                TentaController.TentacleTip.transform.position = 
                    new Vector3(dragCurrent.x + dragOffset.x, startDragPosition.y, dragCurrent.z);

            }
            else
            {
                // if not begin or execute state, null out the dragging card
                // reset gesture, the swipe away finishes the gesture
                //gesture.Reset(); // ERROR causes STack Overflow bug by resetting too often
            }
        }

        ////FixedUpdate is called at a fixed interval and is independent of frame rate. Put physics code here.
        //void FixedUpdate()
        //{

        //}

        private void Update()
        {
            if (TentaController == null)
            {
                var TENT = GameObject.FindGameObjectWithTag("TentacleTip");
                var TCInstance = TENT.GetComponent<SinglePlayerTentaController>();
                if (TCInstance != null)
                {
                   TentaController = TCInstance;
                }
            }

        } // end unity update


        // after MonoBehavior.Update(); see https://docs.unity3d.com/Manual/ExecutionOrder.html
        private void LateUpdate()
        {
            ImageGestureImage match = ImageReconzrScript.CheckForImageMatch();
            if (match != null && match.Name == "Circle")
            {
                // send barrel roll flag
                bool temp_Circled_soBROLL = TentaController.BarrelRoll_Please();

                // image gesture must be manually reset when a shape is recognized AND after calling BarrelRoll_Please()
                ImageReconzrScript.ResetMatchedImage();
            }

        }

        // Call create swipe gesture methods or all-in-one swipe gesture here
        private void CreateSwipeGestures()
        {
            CreateUpSwipeGesture();
            CreateDownSwipeGesture();
            CreateLeftSwipeGesture();
            CreateRightSwipeGesture();

            CreateDragGesture();
        }

        private void CreateDragGesture()
        {
            // create a long press gesture to drag and swipe cards around
            longPress = new LongPressGestureRecognizer();
            longPress.StateUpdated += LongPress_StateUpdated;
            FingersScript.Instance.AddGesture(longPress);
        }

        private void CreateTapGesture()
        {
            TapGesture = new TapGestureRecognizer();
            TapGesture.StateUpdated += TapGestureCallback;
            TapGesture.RequireGestureRecognizerToFail = DoubleTapGesture;
            FingersScript.Instance.AddGesture(TapGesture);
        }

        private void TapGestureCallback(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (TentaController != null)
            {

                //PARTICLE EFFECT TEST //FAILED did not show up; probably needs to be in update() OR was on wrong layer
                //BubbleParticlePrefab.Emit(4);

                if (gesture.State == GestureRecognizerState.Ended)
                {
                    // Code to do something where player touched, show bubbles/sparkles like in Shadowverse for example.
                    //Vector2 touchPosinWorldSpace = CameraReference.ScreenToWorldPoint(new Vector2(gesture.FocusX, gesture.FocusY));
                    //SpawnDot(touchPosinWorldSpace.x, touchPosinWorldSpace.y);

                    //Instantiate(BubbleParticlePrefab,
                    //    new Vector3(gesture.FocusX, gesture.FocusY, 0),
                    //    Quaternion.identity);

                    // CameraScaledWidth lines up nicely with gesture.Focus units somehow...
                    float screeenWidth = CameraReference.scaledPixelWidth;
                    screeenWidth = screeenWidth / 2.0f;

                    //Determine what side of screen is tapped, 'juke' to that side.
                    if (gesture.FocusX >= screeenWidth)
                    {
                            TentaController.JukeRight_Please();
                    }
                    else
                    {
                            TentaController.JukeLeft_Please();
                    }
                }
            }
        }

        private void CreateDoubleTapGesture()
        {
            DoubleTapGesture = new TapGestureRecognizer();
            DoubleTapGesture.NumberOfTapsRequired = 2;
            DoubleTapGesture.ThresholdSeconds = DoubleTapTimeThreshold;
            DoubleTapGesture.StateUpdated += DoubleTapGestureCallback;
            //doubleTapGesture.RequireGestureRecognizerToFail = tripleTapGesture;
            FingersScript.Instance.AddGesture(DoubleTapGesture);
        }

        private void DoubleTapGestureCallback(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (TentaController != null)
            {
                if (gesture.State == GestureRecognizerState.Ended)
                {
                    //DebugText("Double tapped at {0}, {1}", gesture.FocusX, gesture.FocusY);
                }
            }
        }

        private void CreateUpSwipeGesture()
        {
            UpSwipeGesture = new SwipeGestureRecognizer();
            UpSwipeGesture.Direction = SwipeGestureRecognizerDirection.Up;
            UpSwipeGesture.StateUpdated += SwipeGestureCallback_UP;
            // Still has 6 degree dead zone??? 39 to 32 if dirThresh is set to 1
            UpSwipeGesture.DirectionThreshold = UPSwipeGestureDirectionThreshold;

            FingersScript.Instance.AddGesture(UpSwipeGesture);
        }

        // Launch Projectile, still WIP, hence many odd comments inside
        private void SwipeGestureCallback_UP(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (TentaController != null)
            {
                if (gesture.State == GestureRecognizerState.Ended)
                {
                    Debug.Log("1@1@1@1UP SWIPE DETECTED UP SWIPE DETECTED1@1@1@1");

                    Vector2 normalizedSwipeVelocityVector = new Vector2(gesture.VelocityX, gesture.VelocityY).normalized;

                    // BAD! Do not do!!! FingersLite uses arbitrary Iphone inch pixel units, not actual pixels, ScrrentoWorldPoint() has big rounding errors!
                    //Vector2 forceofSwipe = CameraReference.ScreenToWorldPoint(velocityPixels);


                    // swipe angle is the swipe gesture's launch angle = inverse tan(change in y position / change x position)
                    float swipeAngle = Mathf.Rad2Deg * Mathf.Atan2(gesture.DeltaY, gesture.DeltaX);

                    // need to subtract 90 since RB2D.rotation units are clockwise: 0 @noon, -90 @3pm, -179 @5:59pm, 180 @6pm, 90 @9pm
                    //  versus the normal unit circle units that Atan2 spits out clockwise: 90 @noon, 0 @3pm, -89 @5:59pm, -90 @6pm, -180 @9pm -270 @midnight, -360 @3am 
                    //  rotation units are wonky, only go to 180 to negative 180 and straight up is 0 degrees not 90.
                    //  Since the up swipe only allows swipe angle to be unit circle degrees ~39 to ~136, simply subtracting 90 translates fine.

                    // rotation has little precision, rounding feels better/smoother in-game
                    swipeAngle = Mathf.Round(swipeAngle - 90.0f);

                    // Now, instead of directly setting it, make request to swipe tentacle
                    TentaController.LaunchTentacle_Please(normalizedSwipeVelocityVector, swipeAngle);
                }
            }
        }

        private void CreateRightSwipeGesture()
        {
            CreateDotSwipeGesture(RightSwipeGesture, SwipeGestureRecognizerDirection.Right, SwipeGestureCallback_RIGHT);
        }

        private void CreateLeftSwipeGesture()
        {
            CreateDotSwipeGesture(LeftSwipeGesture, SwipeGestureRecognizerDirection.Left, SwipeGestureCallback_LEFT);
        }

        private void CreateDownSwipeGesture()
        {
            CreateDotSwipeGesture(DownSwipeGesture, SwipeGestureRecognizerDirection.Down, SwipeGestureCallback_DOWN);
        }

        private void SwipeGestureCallback_DOWN(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (TentaController != null)
            {
                if (gesture.State == GestureRecognizerState.Ended)
                {
                    // Request to retract tentacle.
                    TentaController.PleaseRecoilTentacle();
                }
            }
        }

        private void SwipeGestureCallback_LEFT(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (TentaController != null)
            {
                if (gesture.State == GestureRecognizerState.Ended)
                {
                    //FlickTentacle(gesture as SwipeGestureRecognizer);

                }
            }
        }

        private void SwipeGestureCallback_RIGHT(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (TentaController != null)
            {
                if (gesture.State == GestureRecognizerState.Ended)
                {
                    //FlickTentacle(gesture as SwipeGestureRecognizer);

                }
            }
        }

        private void CreateDotSwipeGesture(SwipeGestureRecognizer whichSwipe, SwipeGestureRecognizerDirection direction, GestureRecognizerStateUpdatedDelegate GestureCallback)
        {
            whichSwipe = new SwipeGestureRecognizer();
            whichSwipe.Direction = direction;
            whichSwipe.StateUpdated += GestureCallback;
            whichSwipe.DirectionThreshold = LeftRightDownSwipeGestureDirectionThreshold;
            FingersScript.Instance.AddGesture(whichSwipe);
        }

        private void FlipCameraUpsideDown()
        {
            CameraReference.transform.Rotate(0, 0, 180);
            //transform.Rotate(0, 0 * Time.deltaTime, 180);
        }

        // When this component is reset, renamed, or placed in new scene: set default values of editor fields
        private void Reset()
        {
            // When playtesting these, you can copy during Unity Play mode then paste the value in during Edit mode,
            // as quick way to tweak the editor fields.

            // Default values of editor fields:
            UPSwipeGestureDirectionThreshold = 1;
            LeftRightDownSwipeGestureDirectionThreshold = 3;
            DoubleTapTimeThreshold = 1;
            swipeCircleRayCastRadius = 0.2f;
        }




    }
}