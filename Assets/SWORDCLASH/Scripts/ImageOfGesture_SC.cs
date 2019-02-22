using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalRubyShared;
using UnityEngine;

namespace SwordClash
{
    // No longer used as of 1/20/2019; Now PlayerController handles the LateUpdate calls
    class ImageOfGesture_SC : MonoBehaviour
    {

       
        public FingersImageGestureHelper_SC_BarrelRoll ImageScript;

        // after MonoBehavior.Update(); see https://docs.unity3d.com/Manual/ExecutionOrder.html
        private void LateUpdate()
        {
            //if (Input.GetKeyDown(KeyCode.Escape))
            //{
            //    ImageScript.Reset();
            //}
            
                ImageGestureImage match = ImageScript.CheckForImageMatch();
                if (match != null)
                {
                    // send barrel roll flag, already done in ImageScript
                    
                }
                else
                {
                    
                }

              
        }

    }
}
