using UnityEngine;

namespace SwordClash
{
    class BackFlipState : TentacleState
    {
        // to reenter projectile state
        private Vector2 SwipeVelocityVector_before;
        private float SwipeAngle_before;
        private readonly short BrollCount;
        private readonly short JukeCount;

        private Vector2 SwipeDirection;
        private float SwipeAngle;

        private Vector2 ReentrantSwipeVelocity;
        private float ReentrantSwipeAngle;

        // snowboard degrees, 720 == two spins;
        private float CurrentDegreesRotated;
        private float TotalDegreesToRotate;
        private float BackFlipTimeRemaining;
        // how long BackFlip always takes in seconds
        private const float BackFlipTime = 2.0f;
        private const float ScaleSpriteDelta = 0.002f; // too lil 0.0005f // too mush 0.005f
        private const float WaterDepthPerUpdate = 0.88f; //TODO: need to relate these to inspector editor properties & make them proportional

        private float RotateSpriteDelta;
        private float timeElapsed;
        private float waterDepth;
        private Quaternion FreshRotation;

        private float TargetAngle;
        private float rotationDirection;


        private bool JustCollidedWithWall;
        private bool JustCollidedWithWallVert;
       

        public BackFlipState(TentacleController tc) : base(tc)
        {
            OnStateEnter();
        }

        // initialize with another state
        public BackFlipState(TentacleState oldState)
            : base(oldState.TentaControllerInstance)
        {
            OnStateEnter();

        }

        // initialize with another state w/ SPTC aka ProjectileState to enter BackFlipState
        public BackFlipState(TentacleState oldState, SinglePlayerTentaController SPTC,
            Vector2 oldDirection, float oldRotation, short brollCount, short jukeCount,
            Vector2 flipDir, float flipAngle)
            : base(oldState, SPTC)
        {
            SwipeVelocityVector_before = oldDirection;
            Debug.Log( "<color=maroon>*** " + "SwipeVelocityVector_before " + SwipeVelocityVector_before + " ***</color>");
            SwipeAngle_before = oldRotation;
            Debug.Log("<color=maroon>*** " + "oldRotation " + oldRotation + " ***</color>");

            BrollCount = brollCount;
            JukeCount = jukeCount;

            SwipeDirection = flipDir;
            Debug.Log("<color=maroon>*** " + "flipDir " + flipDir + " ***</color>");

            SwipeAngle = flipAngle;
            Debug.Log("<color=maroon>*** " + "FlipAngle " + flipAngle + " ***</color>");

            // Negative backflip swipe is end direction ;; flip entire vector by multiplying its components by -1
            Vector2 EndDirectionofFlip = SwipeDirection * -1.0f;

            //TargetAngle = Vector2.Angle( EndDirectionofFlip, oldDirection );
            //TargetAngle = Vector2.SignedAngle(EndDirectionofFlip, Vector2.up);

            // Root cause of these angle problems is that inspector view is EULER angle in world coords,
            //      but in script land we are dealing with local QUATERNION COORDINATES which cause all kinds of conversion problems
            // https://docs.unity3d.com/ScriptReference/Mathf.Atan2.html
            // Angle of swipe * -1 to reverse it, : Atan2(X, Y) will get angle of reversed swipe in correct orientation, *-1 to make right and left angle sign correct again
            // DO NOT be tempted to simplify this code, the math and what unity is doing under the hood is very difficult to understand!
            TargetAngle = (Mathf.Rad2Deg * Mathf.Atan2(EndDirectionofFlip.x, EndDirectionofFlip.y)) * -1.0f; // flip angle sign +left -right since went from down to up vectors to get angle

            Debug.Log("<color=maroon>*** " + "TARGET ANGLE " + TargetAngle + " ***</color>");

            //
            //TotalDegreesToRotate = Mathf.Abs(SPTentaControllerInstance.transform.position.z - TargetAngle);
           // TotalDegreesToRotate = Mathf.Abs(TargetAngle) - Mathf.Abs((Mathf.Rad2Deg * Mathf.Atan2(oldDirection.x, oldDirection.y)));
            Debug.Log("<color=maroon>*** " + "TotalDegreesToRotate " + TotalDegreesToRotate + " ***</color>");


            ReentrantSwipeVelocity = IncreaseMagnitudeofVectorbyProjectileSpeed(EndDirectionofFlip);
            ReentrantSwipeAngle = TargetAngle;



        }

        public override void HandleCollisionByTag(string ObjectHitTag, Rigidbody2D ObjectHitRB2D)
        {
            if (ObjectHitTag == WallGameObjectTag)
            {

                JustCollidedWithWall = true;
            }
            else if (ObjectHitTag == WallVerticalGameObjectTag)
            {
                JustCollidedWithWallVert = true;
            }
            else if (ObjectHitTag == JellyfishEnemyGameObjectTag)
            {
                // Play whoop cant catch me sound effect
                SoundManagerScript.PlaySound("miss");

            }
            
        }

        public override void OnStateEnter()
        {
            LowerAllInputFlags();
            JustCollidedWithWall = false;
            JustCollidedWithWallVert = false;

            BackFlipTimeRemaining = BackFlipTime;
            timeElapsed = 0.0f;
            waterDepth = 0.0f;
        }



        public override void OnStateExit()
        {

            LowerAllInputFlags();
        }


        public override void ProcessState()
        {
            // Do a back flip;
            // shrink over time then grow over time then shrink down to normal sprite size
            // while going backwards;
            // while rotating to new angle
            // overwrite stored projectile angle with new one.

            // NOT Free to process here!
            IsCurrentlyProcessing = true;

            // Move slowly every frame.
            SPTentaControllerInstance.TT_MoveTentacleTip_WhileBackFlip(SwipeDirection);

            // Rotate slowly each frame // many hours of pain converting from 4d quaternion local coords to 3d euler world coords
            SPTentaControllerInstance.BackFlippin_rotate(Quaternion.Euler(0,0,TargetAngle), Time.deltaTime);



            // decrease time remaining
            BackFlipTimeRemaining -= Time.deltaTime;

            // Shrink then grow, also fade in color when going deeper underwater
            // one way to shrink and grow is with 3 different lerps on local scale to shrink, grow, shrink over time.
            // timeElapsed += Time.deltaTime;
            // float halftime = BackFlipTime / 2.0f;
            if (BackFlipTimeRemaining > BackFlipTime / 2.0f)
            {
                waterDepth += WaterDepthPerUpdate;
            }
            else
            {
                waterDepth -= WaterDepthPerUpdate;
            }

            // change water depth in color fade shader
            SPTentaControllerInstance.TT_WaterDepth_WhileBackFlip(waterDepth);

            // drop a frame input processing if hit wall, so players can't glitch past it.
            if (JustCollidedWithWall)
            {
                OnWallCollision();
                // also makes swipe angle = - swipe angle for now as side effect.
                ReflectTentacleVelocity(Vector2.up);
                JustCollidedWithWall = false;


            }
            else if (JustCollidedWithWallVert)
            {
                OnWallCollision();

                ReflectTentacleVelocity(Vector2.left);
                JustCollidedWithWallVert = false;
            }


            if (BackFlipTimeRemaining < 0.0f)
            {
                // TODO: LEFT OFF HERE 5/16/2020 still need to scale transform as it goes through water and need better logic below
                // now that we rotate too this code is not proportional...

                // transition back to projectile state
                SPTentaControllerInstance.CurrentTentacleState = new ProjectileState
                    (this, SPTentaControllerInstance, ReentrantSwipeVelocity, 
                    ReentrantSwipeAngle, BrollCount, JukeCount);
            }

        }

        public override void ProcessState(ITentacleInputCommandInput input)
        {
            // MULTIPLAYER LOGIC HERE

            ProcessState();
        }

        public override void ProcessCommand(TentacleInputCommand command)
        {
            // MULTIPLAYER LOGIC HERE  

        }

        // Bounce tentacle off in opposite direction same speed "successful bounce" from state diagram
        // later will add a 'crumple' wall that scrunches you and slows you down if you don't bounce off it (slime vs. steel)
        public void ReflectTentacleVelocity(Vector2 surfaceNormal)
        {

            SwipeDirection = CalcVelocityVectorReflection(SwipeDirection, surfaceNormal);

            // try just -neg flipping for angle?
            SwipeAngle = SwipeAngle * -1;
        }
        // TODO: refactor these to have additional penalties for crashing/juking INTO a wall and reward for juking away / swiping away from wall
        private void OnWallCollision()
        {
            LowerAllInputFlags(); // drop frame of human input
        }

        private Vector2 IncreaseMagnitudeofVectorbyProjectileSpeed(Vector2 BFswipeDir)
        {
            // increase magnitude of vector to match projectile speed
            var velocityVector = new Vector2(BFswipeDir.x * (SPTentaControllerInstance.UPswipeSpeedConstant + SPTentaControllerInstance.UPswipeSpeedModifier),
                BFswipeDir.y * (SPTentaControllerInstance.UPswipeSpeedConstant + SPTentaControllerInstance.UPswipeSpeedModifier));

            return velocityVector;
        }

        public override void ProcessCommandFromPlayerTwo(TentacleInputCommand command)
        {
            //throw new NotImplementedException();
        }
    }
}
