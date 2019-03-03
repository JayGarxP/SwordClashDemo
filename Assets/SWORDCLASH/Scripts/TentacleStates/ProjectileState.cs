using UnityEngine;

namespace SwordClash
{
    public class ProjectileState : TentacleState
    {
        // Can't do anything until finished with side-to-side movement
        private bool CurrentlyJuking;
        private Vector2 WhereJumpingTo;

        private Vector2 SwipeVelocityVector;
        private float SwipeAngle;
        private short JukeCount;
        private short BarrelRollCount;

        /// <summary>  
        ///  Constructor to Initialize this state with another, (transition from coiled probably)
        ///  <para> 
        ///  Sets SwipeVelocityVector from swipeNormalVector using MultipyVectorComponentBySpeed()
        ///  </para>
        /// </summary>  
        /// <param name="oldState">CoiledState for example;;;</param>
        ///  <para>
        ///  <param name="swipeNormalVector">Unit vector (vector with magnitude of 1) 
        ///  representing swipe direction</param>
        ///  <param name="swipeAngle">Atan unit circle units -90.0f == Unity.RigidBody2D.Rotation angle,
        ///  responsibility of caller to convert angle to proper range</param>
        ///  </para>
        public ProjectileState(TentacleState oldState, Vector2 swipeNormalVector, float swipeAngle)
            : base(oldState.TentaControllerInstance)
        {
            SwipeVelocityVector = swipeNormalVector;
            SwipeAngle = swipeAngle;
            BarrelRollCount = 0;

            SwipeVelocityVector = MultiplyVectorComponentsBySpeed(
                SwipeVelocityVector,
                TentaControllerInstance.UPswipeSpeedConstant + TentaControllerInstance.UPswipeSpeedModifier
                );

            OnStateEnter();
            // Set JukeCount to zero
            JukeCount = 0;
        }

        // initialize from BarrelRollState which increments BrollCount as side-effect; BAD CODE
        public ProjectileState(TentacleState oldState, Vector2 swipeVelocityVector, float swipeAngle,
            short BrollCount, short jukeCount)
            : base(oldState.TentaControllerInstance)
        {
            SwipeVelocityVector = swipeVelocityVector;
            SwipeAngle = swipeAngle;
            //check if in bad range here???
            //TODO: fix tight coupling of Moving and Barrel Roll state
            BarrelRollCount = BrollCount;
            // BarrelRolling does NOT reset jukeCount
            JukeCount = jukeCount;
            OnStateEnter();

        }

        public ProjectileState(ProjectileState copy, TentacleController copyTC)
            : base(copyTC)
        {
            this.SwipeVelocityVector = copy.SwipeVelocityVector;
            SwipeAngle = copy.SwipeAngle;
            BarrelRollCount = 0;

            SwipeVelocityVector = MultiplyVectorComponentsBySpeed(
                SwipeVelocityVector,
                TentaControllerInstance.UPswipeSpeedConstant + TentaControllerInstance.UPswipeSpeedModifier
                );

            OnStateEnter();
            // Set JukeCount to zero
            JukeCount = 0;
        }



        // Lower all inputflags set times juked count to zero
        public override void OnStateEnter()
        {
            // Set actual tentacle movement vars, save the previous ones if needed
            //  not needed right now...
            LowerAllInputFlags();

        }

        // Recoil Tentacle and lower all input flags.
        public override void OnStateExit()
        {
            //just teleport for now
            TentaControllerInstance.PleaseRecoilTentacle();
            LowerAllInputFlags();
        }

        // ProjectileState hits many things
        public override void HandleCollisionByTag(string ObjectHitTag, UnityEngine.Rigidbody2D objectHit)
        {
            // Get stung and change sprite + recover
            if (ObjectHitTag == JellyfishEnemyGameObjectTag)
            {
                // Change color/ZAP! also go into recovery state
                TentaControllerInstance.PleaseStingTentacleSprite();
                // Play shock noise sound effect
                SoundManagerScript.PlaySound("taser");
                // change into RecoveryState
                TentaControllerInstance.CurrentTentacleState = new RecoveryState(this);
            }
            else if (ObjectHitTag == FoodpickupGameObjectTag)
            {
                // Change state to HoldingFood and give reference to which food hit in constructor
                TentaControllerInstance.CurrentTentacleState = new HoldingFoodState(this, objectHit);
            }
        }

        // WIP, See Game Design Doc for ProcessState's transition table
        // code summary here for projectile.processstate()
        public override void ProcessState()
        {
            //// Free to process here,
            //IsCurrentlyProcessing = false;

            //// Check if barrel roll flag and haven't already brolled too much
            //if ((BarrelRollCount < TentaControllerInstance.TimesCanBarrelRoll) &&
            //    (InputFlagArray[(int)HotInputs.BarrelRoll]))
            //{
            //    TentaControllerInstance.CurrentTentacleState = new BarrelRollState(this, SwipeVelocityVector,
            //        SwipeAngle, BarrelRollCount, JukeCount);
            //}


            //// check if tapping after checking if tapped out
            //if (JukeCount < TentaControllerInstance.TTTimesAllowedToJuke)
            //{
            //    //// if juke - right input received
            //    //if (InputFlagArray[(int)HotInputs.RudderRight])
            //    //{
            //    //    //TODO: make seperate jump methods for coiled jumps
            //    //    TentaControllerInstance.TT_JumpRight(); 
            //    //    InputFlagArray[(int)HotInputs.RudderRight] = false;
            //    //    ++JukeCount;
            //    //}

            //    //if (InputFlagArray[(int)HotInputs.RudderLeft])
            //    //{
            //    //    TentaControllerInstance.TT_JumpLeft();
            //    //    InputFlagArray[(int)HotInputs.RudderLeft] = false;
            //    //    ++JukeCount;
            //    //}

            //}

            //// move tentacle tip
            //TentaControllerInstance.TT_MoveTentacleTip(SwipeVelocityVector, SwipeAngle);

            //// Check if done moving
            //if (TentaControllerInstance.IsTentacleAtMaxExtension())
            //{
            //    //TODO: Recovery mode state

            //    OnStateExit();
            //    TentaControllerInstance.CurrentTentacleState = new CoiledState(this);
            //}


        }

        //private Vector2 MultiplyVectorYComponentBySpeed(Vector2 DirectionVector, float speed)
        //{
        //    //Return velocity vector with [x, y*=speed]
        //    return new Vector2(DirectionVector.x, DirectionVector.y * speed);
        //}

        private Vector2 MultiplyVectorComponentsBySpeed(Vector2 DirectionVector, float speed)
        {
            //Return velocity vector with [x*=speed, y*=speed]
            return new Vector2(DirectionVector.x * speed, DirectionVector.y * speed);
        }

        public override void ProcessState(ITentacleInputCommandInput input)
        {
            // Free to process here,
            IsCurrentlyProcessing = false;

            // Check if barrel roll flag and haven't already brolled too much
            if ((BarrelRollCount < TentaControllerInstance.TimesCanBarrelRoll) &&
                (InputFlagArray[(int)HotInputs.BarrelRoll]))
            {
                input.BarrelRoll = true;
            }


            // check if tapping after checking if tapped out
            if (JukeCount < TentaControllerInstance.TTTimesAllowedToJuke)
            {
                // if juke - right input received
                if (InputFlagArray[(int)HotInputs.RudderRight])
                {
                    //TODO: make seperate jump methods for projectile jumps
                    input.RightTap = true;
                    InputFlagArray[(int)HotInputs.RudderRight] = false;
                    ++JukeCount;
                }

                if (InputFlagArray[(int)HotInputs.RudderLeft])
                {
                    input.LeftTap = true;
                    InputFlagArray[(int)HotInputs.RudderLeft] = false;
                    ++JukeCount;
                }

            }

            // move tentacle tip
            TentaControllerInstance.TT_MoveTentacleTip(SwipeVelocityVector, SwipeAngle);

            Debug.Log("######## AM IN PROJECTILE STATE #########");
            Debug.Log("2@2@2@2 UP SWIPE Projectile state 2@2@2@2");


            // Check if done moving
            if (TentaControllerInstance.IsTentacleAtMaxExtension())
            {
                //TODO: Recovery mode state

                OnStateExit();
                TentaControllerInstance.CurrentTentacleState = new CoiledState(this);
            }

        }

        public override void ProcessCommand(TentacleInputCommand command)
        {
            if (CurrentlyJuking == false)
            {
                if (command.Input.BarrelRoll)
                {
                    TentaControllerInstance.CurrentTentacleState = new BarrelRollState(this, SwipeVelocityVector,
                        SwipeAngle, BarrelRollCount, JukeCount);
                }


                // if juke input received, actaully juke using TentacleController callback method
                if (command.Input.RightTap || command.Input.LeftTap)
                {
                    CurrentlyJuking = true;
                    // false parameter to jump RIGHT, true parameter to jump LEFT
                    WhereJumpingTo = TentaControllerInstance.TT_CalculateEndJumpPosition(command.Input.LeftTap);
                    // Set CurrentlyJuking to true if still need to keep moving, when done juking set CurrentlyJuking to false
                    CurrentlyJuking = TentaControllerInstance.TT_JumpSideways(WhereJumpingTo);
                }
            }
            else
            {
                // Keep moving sideways
                CurrentlyJuking = TentaControllerInstance.TT_JumpSideways(WhereJumpingTo);
            }
        }
    }
}
