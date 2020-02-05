using UnityEngine;

namespace SwordClash
{
    class BackFlipState : TentacleState
    {
        // to reenter projectile state
        private readonly Vector2 SwipeVelocityVector_before;
        private readonly float SwipeAngle_before;
        private readonly short BrollCount;
        private readonly short JukeCount;

        private Vector2 SwipeDirection;
        private float SwipeAngle;

        // snowboard degrees, 720 == two spins;
        private float CurrentDegreesRotated;
        private float TotalDegreesToRotate;

        //private float CurrentJukeTravelTime;
        //private float JukeTravelTime;

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
            Vector2 flipDir, float flipRotation)
            : base(oldState, SPTC)
        {
            SwipeVelocityVector_before = oldDirection;
            SwipeAngle_before = oldRotation;
            BrollCount = brollCount;
            JukeCount = jukeCount;

            SwipeDirection = flipDir;
            SwipeAngle = flipRotation;

            TotalDegreesToRotate = oldRotation - flipRotation;

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

            // TODO: Make own BarrelRollin_Rotate for BackFlip here;
            CurrentDegreesRotated = SPTentaControllerInstance.BarrelRollin_rotate(CurrentDegreesRotated);

            // check if done back flipping
            // TODO: rework timing of method to work off of Sprite Animation timing of the grow and shrink
            //                plus color mute when in deep water, color brighten in shallower water
            // TODO: write shader that simulates underwater color change via depth underwater as a function of time more time = deeper under water away from the light of God and the grace of her angels.
            if (CurrentDegreesRotated >= TotalDegreesToRotate)
            {
                SPTentaControllerInstance.ResetTentacleTipRotation();

                // Change to projectile state.
                SPTentaControllerInstance.CurrentTentacleState = new ProjectileState(this, SPTentaControllerInstance, SwipeDirection, SwipeAngle, BrollCount, JukeCount);
            }
            else
            {
                // Animate sprite

            }







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


            
            //if (CurrentJukeTravelTime >= JukeTravelTime)
            //{
            //    // transition back to projectile state
            //    SPTentaControllerInstance.CurrentTentacleState = new ProjectileState
            //        (this, SPTentaControllerInstance, SwipeVelocityVector_before, SwipeAngle_before, BrollCount, JukeCount);
            //}

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

        public override void ProcessCommandFromPlayerTwo(TentacleInputCommand command)
        {
            //throw new NotImplementedException();
        }
    }
}
