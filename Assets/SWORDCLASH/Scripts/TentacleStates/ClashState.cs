using System;
using UnityEngine;

namespace SwordClash
{
    class ClashState : TentacleState
    {
        // clash state vars
        private Vector2 ClashVelocity;
        private float ClashAngle;
        private const float BOUNCE_SPEED = 3.0f; // should bounce at twice the speed of normal projectile
        private float CurrentClashTravelTime;
        private float ClashTravelTime;

        // to reenter projectile state
        private readonly Vector2 SwipeVelocityVector_before;
        private readonly float SwipeAngle_before;
        private readonly short BrollCount;

        private short JukeCount;

        private bool JustCollidedWithFood;
        private bool JustCollidedWithWall;
        private bool JustCollidedWithWallVert;
        private Rigidbody2D FoodHitRef;

        // initialize with another state to enter recovery state
        public ClashState(TentacleState oldState, SinglePlayerTentaController SPTC,
            Vector2 oldDirection, float oldRotation, short brollCount, short jukeCount)
            : base(oldState, SPTC)
        {
            SwipeVelocityVector_before = oldDirection;
            SwipeAngle_before = oldRotation;
            BrollCount = brollCount;
            JukeCount = jukeCount;

            //set velocity and angle of clash
            SetClashDirection();
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
                // TAKE EXTRA DAMAGE

            }
            else if (ObjectHitTag == FoodpickupGameObjectTag)
            {
                JustCollidedWithFood = true;
                FoodHitRef = ObjectHitRB2D;
            }

        }

        // CAlled BEFORE constructor by the way...
        public override void OnStateEnter()
        {
            LowerAllInputFlags();
            JustCollidedWithFood = false;
            JustCollidedWithWall = false;
            JustCollidedWithWallVert = false;
            Rigidbody2D FoodHitRef = null;
            // Increment Clash Count
            ClashCount++;

            // Set travel time of CLASH
            ClashTravelTime = SPTentaControllerInstance.TTClashTravelTime;
            CurrentClashTravelTime = 0.0f;

          

        }

        public override void OnStateExit()
        {

            LowerAllInputFlags();
        }


        public override void ProcessState()
        {
            // bounce off thing hit, play cool sound effect
            // hover for 2 seconds in place before entering a FAST recovery back to coiled state
            // Allow 3 clashes before recovery then on the fourth a microgame will start that will cause loser to be slow recovered,
            //    no matter which player / entity has 3 clashes!!!
            // clash can be forward or back but not juked
            // hit while in clash state will cause slow recovery

            //TODO: add second collider behind TT (crit spot) and along its arm for being "cut off", make over extending into enemy territory more risky.

            // Always move every frame.
           SPTentaControllerInstance.TT_MoveTentacleTipAtSpeed(ClashVelocity, ClashAngle, BOUNCE_SPEED);


            // drop a frame input processing if hit wall, so players can't glitch past it.
            if (JustCollidedWithWall)
            {
                OnWallCollision();
                
                ReflectTentacleVelocity(Vector2.up);
                JustCollidedWithWall = false;


            }
            else if (JustCollidedWithWallVert)
            {
                OnWallCollision();

                ReflectTentacleVelocity(Vector2.left);
                JustCollidedWithWallVert = false;
            }
            else if (JustCollidedWithFood)
            {
                // whiff / miss food
                
            }
            else if (InputFlagArray[(int)HotInputs.BackFlip])
            {
                SPTentaControllerInstance.CurrentTentacleState = new BackFlipState(this, SPTentaControllerInstance,
                   SwipeVelocityVector_before,
                   SwipeAngle_before, BrollCount, JukeCount,
                   SPTentaControllerInstance.TTBackFlipNormalDirRequested,
                   SPTentaControllerInstance.TTBackFlipAngleRequested);
            }
            else if (InputFlagArray[(int)HotInputs.LaunchSwipe])
            {
                
                
                    var UpSwipe = new Vector3(SPTentaControllerInstance.TTMovePositionVelocityRequested.x,
                        SPTentaControllerInstance.TTMovePositionVelocityRequested.y, 0);


                    var SwipeAngle = SPTentaControllerInstance.TTMoveRotationAngleRequested;

                    //actually move tentacle here:
                    SPTentaControllerInstance.CurrentTentacleState = new ProjectileState(this, SPTentaControllerInstance,
                             UpSwipe,
                             SwipeAngle);

                
            }


            // increment distance traveled
            CurrentClashTravelTime += Time.fixedDeltaTime;

            // if travel time >= jumpdistance, stop juking
            if (CurrentClashTravelTime >= ClashTravelTime)
            {
                // stop moving
                ClashVelocity = Vector2.zero;
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

        public void ReflectTentacleVelocity(Vector2 surfaceNormal)
        {

            ClashVelocity = CalcVelocityVectorReflection(ClashVelocity, surfaceNormal);

            // try just -neg flipping for angle?
            ClashAngle = ClashAngle * -1;
        }
       
        private void OnWallCollision()
        {
            LowerAllInputFlags(); // drop frame of human input

        }


        // just reflect for now assume hitting sword head on...
        private void SetClashDirection()
        {
            // seems to work OK for now, but will add better logic if swipe velocity becomes a public property other colliders can use...
            ClashVelocity = Vector2.Reflect(SwipeVelocityVector_before, Vector2.up);
            ClashAngle = SwipeAngle_before * -1;
        }


        public override void ProcessCommandFromPlayerTwo(TentacleInputCommand command)
        {
            //throw new NotImplementedException();
        }
    }
}
