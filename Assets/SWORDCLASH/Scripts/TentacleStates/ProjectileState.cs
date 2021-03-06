﻿using UnityEngine;

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
        private int GameLoopTicksBeforeSync;
        private bool BoltStateStringSet;
        private bool JustCollidedWithFood;
        private bool JustCollidedWithWall;
        private bool JustCollidedWithWallVert;
        private bool JustStung;
        private bool JustCollidedWithSword; // three clash states in a row without recovery will result in a microgame

        private Rigidbody2D FoodHitRef;

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

            MagnitudeSwipeDir();

            Debug.Log("Chris SwipeVelocityVector in ProjectileState Constructor: " + SwipeVelocityVector.ToString());

            OnStateEnter();
            // Set JukeCount to zero
            JukeCount = 0;
        }

        public ProjectileState(TentacleState oldState, SinglePlayerTentaController SPTC,
            Vector2 swipeNormalVector, float swipeAngle)
           : base(oldState, SPTC)
        {
            SwipeVelocityVector = swipeNormalVector;
            SwipeAngle = swipeAngle;
            BarrelRollCount = 0;

            MagnitudeSwipeDir();

            //Debug.Log("Chris SwipeVelocityVector in ProjectileState Constructor: " + SwipeVelocityVector.ToString());

            // Set JukeCount to zero
            JukeCount = 0;
        }

        public ProjectileState(TentacleController tc) : base(tc)
        {
            SwipeVelocityVector = TentaControllerInstance.state.LatestSwipe;
            SwipeAngle = TentaControllerInstance.state.LatestSwipeAngle;

            Debug.Log("Chris SwipeVelocityVector in ProjectileState(tc) from tc.state: " + SwipeVelocityVector.ToString());


            BarrelRollCount = 0;

            SwipeVelocityVector = MultiplyVectorComponentsBySpeed(
                SwipeVelocityVector,
                TentaControllerInstance.UPswipeSpeedConstant + TentaControllerInstance.UPswipeSpeedModifier
                );

            Debug.Log("Chris SwipeVelocityVector in ProjectileState(tc) Constructor: " + SwipeVelocityVector.ToString());

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

        // SinglePlayer initialize from BarrelRollState which increments BrollCount as side-effect
        public ProjectileState(TentacleState oldState, SinglePlayerTentaController SPTC,
            Vector2 swipeVelocityVector, float swipeAngle,
            short BrollCount, short jukeCount)
            : base(oldState, SPTC)
        {
            // re-entrant; do not apply speed constant to velocity twice!!!
            SwipeVelocityVector = swipeVelocityVector;
            SwipeAngle = swipeAngle;

            //check if in bad range here???
            //TODO: fix tight coupling of Moving and Barrel Roll state
            BarrelRollCount = BrollCount;
            // BarrelRolling does NOT reset jukeCount
            JukeCount = jukeCount;

        }

        public ProjectileState(ProjectileState copy, TentacleController copyTC)
            : base(copyTC)
        {
            SwipeVelocityVector = copy.SwipeVelocityVector;
            SwipeAngle = copy.SwipeAngle;
            BarrelRollCount = 0;
            MagnitudeSwipeDir();

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
            IsCurrentlyProcessing = false;
            JustCollidedWithFood = false;
            JustCollidedWithWall = false;
            JustCollidedWithWallVert = false;
            JustStung = false;

            GameLoopTicksBeforeSync = 0;
            StringRep = "Projectile";
            BoltStateStringSet = false;


        }

        // Recoil Tentacle and lower all input flags.
        public override void OnStateExit()
        {
            //just teleport for now
            if (TentaControllerInstance != null)
            {
                TentaControllerInstance.PleaseRecoilTentacle();

            }
            else
            {
                // single player TC instance reset tentatip position
                SPTentaControllerInstance.PleaseRecoilTentacle();
            }
            LowerAllInputFlags();


            //StringRep = "Unknown";
            // var stateString = TentaControllerInstance.SetBoltTentaStateString(StringRep);
            //  Debug.Log("Chris    StateString is now: " + stateString);

        }

        // ProjectileState hits many things
        public override void HandleCollisionByTag(string ObjectHitTag, UnityEngine.Rigidbody2D objectHit)
        {
            if (ObjectHitTag == WallGameObjectTag)
            {

                JustCollidedWithWall = true;
            }
            else if (ObjectHitTag == WallVerticalGameObjectTag)
            {
                JustCollidedWithWallVert = true;
            }

            // https://opengameart.org/content/katana-1
            // Collide with weapon / enemy tentacle (telegraph with laser sight animation that 'cuts' through tentacle body, tip is impervious)
            if (ObjectHitTag == BladeObjectTag)
            {
                // TODO: Play one of many sword clash sound effects;
                // bounce off in semi-random or exactly dot product of vectors?

                JustCollidedWithSword = true;
            }

            // Get stung and change sprite + recover
            if (ObjectHitTag == JellyfishEnemyGameObjectTag)
            {
                // Change color/ZAP! also go into recovery state
                if (TentaControllerInstance != null)
                {
                    TentaControllerInstance.PleaseStingTentacleSprite();
                }
                else
                {
                    SPTentaControllerInstance.PleaseStingTentacleSprite();
                }
                // Play shock noise sound effect
                SoundManagerScript.PlaySound("taser");

                // change into RecoveryState flag; sprites happen in ProcessState(); physics movement in ProcessCommand();
                JustStung = true;

            }
            else if (ObjectHitTag == FoodpickupGameObjectTag)
            {
                JustCollidedWithFood = true;
                FoodHitRef = objectHit;
            }
        }

        // WIP, See Game Design Doc for ProcessState's transition table
        // code summary here for projectile.processstate()
        public override void ProcessState()
        {
            // Free to process here,
            IsCurrentlyProcessing = false;
            StringRep = "Projectile";

            // Always move every frame.
            SPTentaControllerInstance.TT_MoveTentacleTip(SwipeVelocityVector, SwipeAngle);

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
            else if (JustCollidedWithSword)
            {
                SPTentaControllerInstance.CurrentTentacleState = new ClashState(this,
                            SPTentaControllerInstance,
                            SwipeVelocityVector,
                            SwipeAngle, BarrelRollCount, JukeCount);
            }
            else
            {

                if (JustCollidedWithFood)
                {
                    // Change state to HoldingFood and give reference to which food hit in constructor
                    SPTentaControllerInstance.CurrentTentacleState = new HoldingFoodState(this, SPTentaControllerInstance, FoodHitRef);

                    //Debug.Log("Chris Changing to HoldingFoodState... .... ....");
                }
                else if (JustStung)
                {
                    SPTentaControllerInstance.CurrentTentacleState = new RecoveryState(this, SPTentaControllerInstance);
                    // Debug.Log("Chris Changing to RecoveryState... .... ....");
                }
                else if (InputFlagArray[(int)HotInputs.BackFlip])
                {
                    SPTentaControllerInstance.CurrentTentacleState = new BackFlipState(this, SPTentaControllerInstance,
                       SwipeVelocityVector,
                       SwipeAngle, BarrelRollCount, JukeCount,
                       SPTentaControllerInstance.TTBackFlipNormalDirRequested,
                       SPTentaControllerInstance.TTBackFlipAngleRequested);
                }


                // Check if barrel roll flag and haven't already brolled too much
                if ((BarrelRollCount < SPTentaControllerInstance.TimesCanBarrelRoll) &&
                    (InputFlagArray[(int)HotInputs.BarrelRoll]))
                {
                    SPTentaControllerInstance.CurrentTentacleState = new BarrelRollState(this, SPTentaControllerInstance,
                        SwipeVelocityVector,
                        SwipeAngle, BarrelRollCount, JukeCount);

                }

                // check if tapping after checking if tapped out; in future make wall bounces restore one juke?
                if (JukeCount < SPTentaControllerInstance.TTTimesAllowedToJuke)
                {

                    // if juke - right input received
                    if (InputFlagArray[(int)HotInputs.RudderRight])
                    {
                        InputFlagArray[(int)HotInputs.RudderRight] = false;
                        ++JukeCount;

                        SPTentaControllerInstance.CurrentTentacleState = new JukingState(this,
                            SPTentaControllerInstance,
                            SwipeVelocityVector,
                            SwipeAngle, BarrelRollCount, JukeCount, 2);

                    }
                    else if (InputFlagArray[(int)HotInputs.RudderLeft])
                    {

                        InputFlagArray[(int)HotInputs.RudderLeft] = false;
                        ++JukeCount;

                        SPTentaControllerInstance.CurrentTentacleState = new JukingState(this,
                      SPTentaControllerInstance,
                      SwipeVelocityVector,
                      SwipeAngle, BarrelRollCount, JukeCount, 1);


                    }

                }

                // Check if done moving
                if (SPTentaControllerInstance.IsTentacleAtMaxExtension())
                {
                    //TODO: Recovery mode state
                    SPTentaControllerInstance.CurrentTentacleState = new CoiledState(this, SPTentaControllerInstance);
                }

            }



        }

        //private Vector2 MultiplyVectorYComponentBySpeed(Vector2 DirectionVector, float speed)
        //{
        //    //Return velocity vector with [x, y*=speed]
        //    return new Vector2(DirectionVector.x, DirectionVector.y * speed);
        //}

        private Vector2 MultiplyVectorComponentsBySpeed(Vector2 DirectionVector, float speed)
        {
            //Return velocity vector with [x*=speed, y*=speed]
            var velocityVector = new Vector2(DirectionVector.x * speed, DirectionVector.y * speed);


            //Debug.Log("Chris velocityVector: " + velocityVector.ToString());
            return velocityVector;

        }

        private void MagnitudeSwipeDir()
        {
            SwipeVelocityVector = MultiplyVectorComponentsBySpeed(
             SwipeVelocityVector,
             SPTentaControllerInstance.UPswipeSpeedConstant + SPTentaControllerInstance.UPswipeSpeedModifier
             );
        }

        public override void ProcessState(ITentacleInputCommandInput input)
        {
            // Free to process here,
            IsCurrentlyProcessing = false;
            StringRep = "Projectile";


            // SYNC STATE HERE
            if (BoltNetwork.IsClient && TentaControllerInstance.state.AmIPlayer2)
            {
                //Debug.Log("Chris " + "BoltNetwork.IsClient && state.AmIPlayerTwo");

                // Check if out of sync with server
                //TODO: see if these sync logics can be refactored with Bolt state callback events
                if (StringRep != TentaControllerInstance.state.CurrentStateString)
                {
                    Debug.Log("Chris StringRep " + StringRep + "does not equal state.Current   "
                        + TentaControllerInstance.state.CurrentStateString);

                    if (TentaControllerInstance.state.CurrentStateString == "Coiled")
                    {
                        //  //become coiled
                        TentaControllerInstance.CurrentTentacleState = new CoiledState(this);
                        Debug.Log("Chris SWITHCING TO COILED STATE in ProjectileState.ProcState(...)");
                        StringRep = "Coiled";
                    }
                    else if (TentaControllerInstance.state.CurrentStateString == "BarrelRoll")
                    {
                        // barrel roll out
                        TentaControllerInstance.CurrentTentacleState = new BarrelRollState(TentaControllerInstance);
                        Debug.Log("Chris SWITHCING TO BARRELROLL STATE in ProjectileState.ProcState(...)");
                        StringRep = "BarrelRoll";
                    }
                    else if (TentaControllerInstance.state.CurrentStateString == "HoldingFood")
                    {
                        // holding food, food ref may be null should probably check;
                        if (FoodHitRef != null)
                        {
                            TentaControllerInstance.CurrentTentacleState = new HoldingFoodState(this, FoodHitRef);
                            Debug.Log("Chris SWITHCING TO holdingfood in ProjectileState.ProcState(...)");
                            StringRep = "HoldingFood";

                        }
                        else
                        {
                            Debug.Log("Chris FoodHitRef was null sadly, can't change to HoldingFood state yet :(");
                        }
                    }
                    else if (TentaControllerInstance.state.CurrentStateString == "Recovery")
                    {
                        TentaControllerInstance.CurrentTentacleState = new RecoveryState(this);
                        StringRep = "Recovery";
                        Debug.Log("Chris Changing to RecoveryState... in Projectile.ProcState ....");
                    }


                }
            }

            if (JustCollidedWithFood)
            {
                input.JustHitFood = true;
            }
            else if (JustStung)
            {
                // Now in command loop the input from p2 on client can be synced with p2 proxy on server
                input.JustStung = true;

            }

            // Check if barrel roll flag and haven't already brolled too much
            if ((BarrelRollCount < TentaControllerInstance.TimesCanBarrelRoll) &&
                (InputFlagArray[(int)HotInputs.BarrelRoll]))
            {
                input.BarrelRoll = true;
                Debug.Log("Chris TRYNA BARRELROLL in ProjectileState.ProcessState(Input)");
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

        }

        // Only move the synced transform in ProcessCommand since it is run server-side!!!
        public override void ProcessCommand(TentacleInputCommand command)
        {

            // Move that tentacle tip baby
            TentaControllerInstance.TT_MoveTentacleTip(SwipeVelocityVector, SwipeAngle);
            GameLoopTicksBeforeSync++;

            if (BoltStateStringSet == false)
            {
                TentaControllerInstance.SetBoltTentaStateString("Projectile");
                BoltStateStringSet = true;
            }

            //TODO: turn on boolean and change state in ProcessCommand();
            // there is bug where P2 has advantage and can snatch food too easily...

            // also need to add holding food to processstate state sync check // can also chekc state from opp. instance
            if (command.Input.CommandFromP2)
            {
                if (command.Input.JustHitFood && TentaControllerInstance.OpponentTCInstance.state.CurrentStateString != "HoldingFood")
                {
                    if (FoodHitRef != null)
                    {
                        // Only can grab food if it is in fresh position, the state variables are not synced for some reason...
                        Vector2 freshFoodSpawnPosition = GameLogicController.HardCodedCenterCameraCoord;
                        if (FoodHitRef.position == freshFoodSpawnPosition)
                        {
                            // temporary solution for 3/14 / 2019 DEMO DAY
                            // Change state to HoldingFood and give reference to which food hit in constructor
                            TentaControllerInstance.CurrentTentacleState = new HoldingFoodState(this, FoodHitRef);

                            TentaControllerInstance.SetBoltTentaStateString("HoldingFood");
                            Debug.Log("Chris Changing to HoldingFoodState... .... ....");
                        }
                    }

                }
                else if (command.Input.JustHitFood && TentaControllerInstance.OpponentTCInstance.state.CurrentStateString == "HoldingFood")
                {
                    // opponent is holding food
                    // freeze; show backswipe prompt
                    // if win then now holding food
                    // if lose, go to zapped state I guess.
                    //TODO: this code is unreachable right now, need better soultion for collision,
                    // probably need flags in ProcessState or more bolt variables
                    // also UI is not syncing properly now UI manager needs some help.
                    //    probably not syning over bolt network only doing it locally.
                    Debug.Log("Maria I have hit the enemy player, they are holding food fufufufufufuff hahahaha");
                }
            }
            else //Commands from player 1 work correctly.
            {
                if (command.Input.JustHitFood && TentaControllerInstance.OpponentTCInstance.state.CurrentStateString != "HoldingFood")
                {
                    // Change state to HoldingFood and give reference to which food hit in constructor
                    TentaControllerInstance.CurrentTentacleState = new HoldingFoodState(this, FoodHitRef);

                    TentaControllerInstance.SetBoltTentaStateString("HoldingFood");
                    Debug.Log("Chris Changing to HoldingFoodState... .... ....");
                }
            }

            if (command.Input.JustStung)
            {
                //TODO: refactor all state change logic into something better
                //OnStateExit();
                TentaControllerInstance.CurrentTentacleState = new RecoveryState(this);
                TentaControllerInstance.SetBoltTentaStateString("Recovery");
                Debug.Log("Chris Changing to RecoveryState... .... ....");

            }



            if (CurrentlyJuking == false)
            {
                if (command.Input.BarrelRoll)
                {
                    TentaControllerInstance.SetBoltTentaStateString("BarrelRoll");
                    TentaControllerInstance.state.BrollCount = BarrelRollCount;
                    TentaControllerInstance.state.JukeCount = JukeCount;

                    TentaControllerInstance.CurrentTentacleState = new BarrelRollState(this, SwipeVelocityVector,
                        SwipeAngle, BarrelRollCount, JukeCount);

                    Debug.Log("Chris SWITHCING TO BARRELROLL STATE in ProjectileState.ProcCOMMAND(.|.|.)");
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

            // Check if done moving
            if (TentaControllerInstance.IsTentacleAtMaxExtension())
            {
                //TODO: Recovery mode state

                OnStateExit();
                var stateString = TentaControllerInstance.SetBoltTentaStateString("Coiled");
                Debug.Log("PS.PCOMMAND StateString is now: " + stateString);
                //TentaControllerInstance.state.CurrentStateString = "Coiled";

                TentaControllerInstance.CurrentTentacleState = new CoiledState(this);
            }

        }

        public override void ProcessCommandFromPlayerTwo(TentacleInputCommand command)
        {
            //throw new System.NotImplementedException();
        }

        // Bounce tentacle off in opposite direction same speed "successful bounce" from state diagram
        // later will add a 'crumple' wall that scrunches you and slows you down if you don't bounce off it (slime vs. steel)
        public void ReflectTentacleVelocity(Vector2 surfaceNormal)
        {

            SwipeVelocityVector = CalcVelocityVectorReflection(SwipeVelocityVector, surfaceNormal);

            // try just -neg flipping for angle?
            SwipeAngle = SwipeAngle * -1;
        }

        private void OnWallCollision()
        {
            // No need to exit juking state for now...
            // JUKING its own state, so if you juke into a wall, you get extra crumpled.

            LowerAllInputFlags(); // drop frame of human input

        }

    }
}
