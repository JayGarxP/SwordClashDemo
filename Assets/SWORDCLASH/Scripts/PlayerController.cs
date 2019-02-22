﻿using UnityEngine;
using UnityEngine.UI;
using DigitalRubyShared; //Fingers bought full version 1/10/2019
using System;
using System.Collections.Generic;

namespace SwordClash
{
    public class PlayerController : MonoBehaviour
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
       
        public GameObject LeftTentacle;

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

        
        private TentacleController TentaController;
        private Vector2 TentacleTipStartPosition;
        private bool TTPrefabset;
        private bool AmIPlayerTwo;

        // Use this for initialization
        void Start()
        {
            //CreateDoubleTapGesture(); //TODO: find event order solution: https://stackoverflow.com/questions/374398/are-event-subscribers-called-in-order-of-subscription
            
            //CreateDoubleTapGesture(); // test if order matters; it does sadly... :(
            CreateSwipeGestures();
            CreateTapGesture();

            LeftTentacle = null;
            TTPrefabset = false;
            AmIPlayerTwo = false;
        }

        ////FixedUpdate is called at a fixed interval and is independent of frame rate. Put physics code here.
        //void FixedUpdate()
        //{

        //}

        void Update()
        {
            // Hack to keep player input non-bolt networked for now, object is null until it spawns in from
            // NetworkCallbacks.cs Bolt Global
            if (TTPrefabset == false)
            {
                LeftTentacle = GameObject.FindWithTag("TentacleTip");
                
                // check if component is unattached or null here? Not sure best way to make tightly-coupled components know of each other
                if (LeftTentacle != null && LeftTentacle.tag != "/")
                {
                    TentaController = LeftTentacle.GetComponent<TentacleController>();
                    if (TentaController == null)
                    {
                        // Message using rich text.
                        Debug.Log("<color=red>GetComponent Error: </color>LeftTentacle's TentacleController not found");
                    }
                    else
                    {
                        TentacleTipStartPosition = TentaController.GetComponent<Rigidbody2D>().position;
                        TTPrefabset = true;
                    }
                }
                else
                {
                    // Maybe we are player two then...
                  LeftTentacle = GameObject.FindWithTag("TentacleTipP2");
                    if (LeftTentacle != null && LeftTentacle.tag != "/")
                    {
                        TentaController = LeftTentacle.GetComponent<TentacleController>();
                        if (TentaController != null)
                        {
                            TentacleTipStartPosition = TentaController.GetComponent<Rigidbody2D>().position;
                            TTPrefabset = true;
                            AmIPlayerTwo = true;
                            TentaController.PleaseMakeMePlayerTwo();
                            
                        }
                        else
                        {
                            // Message using rich text.
                            Debug.Log("<color=red>GetComponent Error: </color>LeftTentacle's TentacleController not found");
                        }
                    }
                }
            }
        }

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
                if (gesture.State == GestureRecognizerState.Ended)
                {
                    // Code to do something where player touched, show bubbles/sparkles like in Shadowverse for example.
                    //Vector2 touchPosinWorldSpace = CameraReference.ScreenToWorldPoint(new Vector2(gesture.FocusX, gesture.FocusY));
                    //SpawnDot(touchPosinWorldSpace.x, touchPosinWorldSpace.y);

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
            if (gesture.State == GestureRecognizerState.Ended)
            {
                //DebugText("Double tapped at {0}, {1}", gesture.FocusX, gesture.FocusY);
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
            if (gesture.State == GestureRecognizerState.Ended)
            {
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
            if (gesture.State == GestureRecognizerState.Ended)
            {
                // Request to retract tentacle.
                TentaController.PleaseRecoilTentacle();
            }
        }

        private void SwipeGestureCallback_LEFT(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Ended)
            {
                //FlickTentacle(gesture as SwipeGestureRecognizer);

            }
        }

        private void SwipeGestureCallback_RIGHT(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Ended)
            {
                //FlickTentacle(gesture as SwipeGestureRecognizer);

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


        // When this component is reset, renamed, or placed in new scene: set default values of editor fields
        private void Reset()
        {
            // When playtesting these, you can copy during Unity Play mode then paste the value in during Edit mode,
            // as quick way to tweak the editor fields.

            // Default values of editor fields:
            UPSwipeGestureDirectionThreshold = 1;
            LeftRightDownSwipeGestureDirectionThreshold = 3;
            DoubleTapTimeThreshold = 1;
        }


    }
}