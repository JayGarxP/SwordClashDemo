using DigitalRubyShared;
using System.Collections.Generic;

namespace SwordClash
{
    /// <summary>  
    ///  Component for recognizing image gestures (circle, X, swirly, etc.) or finger traces
    /// </summary>  
    /// <remarks>
    /// ImageScript set in editor, to recognize circles,
    /// see Image and Shape Recognition Training with Fingers - Touch Gestures for Unity by Jeff Johnson Digital Ruby
    /// https://www.youtube.com/watch?v=ljQkuqo1dV0
    /// </remarks> 
    public class FingersImageGestureHelper_SC_BarrelRoll : ImageGestureRecognizerComponentScript
    {
        private ImageGestureImage matchedInputImage;

        // Generic gesture callback that catches image drawn on screen
        public void GestureCallback(DigitalRubyShared.GestureRecognizer gesture)
        {

            if (gesture.State == GestureRecognizerState.Began)
            {
            }
            else if (gesture.State == GestureRecognizerState.Executing)
            {
            }
            else if (gesture.State == GestureRecognizerState.Ended)
            {
                // save off the matched image, the gesture may reset if max path count has been reached
                matchedInputImage = this.Gesture.MatchedGestureImage;
            }
            //else
            //{

            //    return;
            //}
        }

        // Required gestures to fail before image can be matched; don't need this if allowing simultaneous execution
        public int RequireTheseGesturesToFail(List<GestureRecognizer> needTaFails)
        {
            int numGestureRecsFiredBeforeBarrelRollCircleImage = 0;
            foreach (var jester in needTaFails)
            {
                Gesture.AddRequiredGestureRecognizerToFail(jester);
                numGestureRecsFiredBeforeBarrelRollCircleImage++;
            }

            return numGestureRecsFiredBeforeBarrelRollCircleImage;
        }

        public ImageGestureImage CheckForImageMatch()
        {
            if (matchedInputImage == null)
            {
                // "No match found!";
            }
            else
            {
                // Gesture image match!

                // image gesture must be manually reset when a shape is recognized
                Gesture.Reset();
            }

            return matchedInputImage;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
        }

        /// <summary>
        /// Reset state, set matched inputImage to null
        /// </summary>
        public void ResetMatchedImage()
        {
            this.Gesture.Reset();

            // jp added this to solve all gestures == barrel roll bug
            // need to manually call Gesture.Reset() and set the currently matched image to null manually,
            //      to avoid re-raising the barrel roll flag right away in PlayerController.LateUpdate()
            matchedInputImage = null; 
        }
    }
}