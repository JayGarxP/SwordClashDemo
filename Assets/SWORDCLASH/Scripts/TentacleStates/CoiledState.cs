using UnityEngine;

namespace SwordClash
{
    // possible this could be a static state, since all instances of it are the same...
    public class CoiledState : TentacleState
    {
        // Can't do anything until finished with side-to-side movement
        public bool CurrentlyJuking;
        Vector2 WhereJumpingTo;
        int GameLoopTicksBeforeSync;

        // initialize with another state, resuming coiled state
        public CoiledState(TentacleState oldState)
            : base(oldState.TentaControllerInstance)
        {
            OnStateEnter();

        }

        // single player resume coiled state constructor
        public CoiledState(TentacleState oldState, SinglePlayerTentaController SPTC)
          : base(SPTC)
        {
            OnStateEnter();

        }

        // initialize with new tentacle controller, first coil of game
        public CoiledState(TentacleController tsc) : base(tsc)
        {
            OnStateEnter();
        }

        public CoiledState(SinglePlayerTentaController stsc) : base(stsc)
        {
            OnStateEnter();
        }

        public override void HandleCollisionByTag(string ObjectHitTag, Rigidbody2D ObjectHitRB2D)
        {
            //throw new NotImplementedException();
        }

        public override void OnStateEnter()
        {
            // set all flags false
            LowerAllInputFlags();
            CurrentlyJuking = false;
            IsCurrentlyProcessing = false;
            GameLoopTicksBeforeSync = 0;

            // Reset position and sprite of tentacle tip
            if (TentaControllerInstance != null)
            {
                TentaControllerInstance.TT_RecoilTentacle();
            }
            else if (SPTentaControllerInstance != null)
            {
                //TODO: need better solution for callback methods, either Provider pattern or refactor into child parent classes
                SPTentaControllerInstance.TT_RecoilTentacle();
            }
            
            StringRep = "Coiled";



        }

        public override void OnStateExit()
        {
            //StringRep = "Unknown";
            //var stateString = TentaControllerInstance.SetBoltTentaStateString("Unknown");
            //Debug.Log("Chris    StateString is now: " + stateString);

        }


        // compile Sandcastle XML comments with: -doc:DocFileName.xml 

        /// <summary>
        /// Processes InputFlags and makes callbacks to TentacleController move thangs
        /// </summary>
        /// <remarks>
        /// Called inside Unity update loops, CoiledState.ProcessState() does not block inputflag sets
        /// </remarks>
        public override void ProcessState()
        {
            // Coiled States ProcessState does NOT block flag changes while executing.
            IsCurrentlyProcessing = false;
            StringRep = "Coiled";

            SPTentaControllerInstance.ResetTentacleTipSprite();

            // if player up-swipe, they tryna  *L A U N C H*
            if (InputFlagArray[(int)HotInputs.LaunchSwipe])
            {
                var UpSwipe = new Vector3(SPTentaControllerInstance.TTMovePositionVelocityRequested.x,
                    SPTentaControllerInstance.TTMovePositionVelocityRequested.y, 0);


                var SwipeAngle = SPTentaControllerInstance.TTMoveRotationAngleRequested;

                //actually move tentacle here:
                SPTentaControllerInstance.CurrentTentacleState = new ProjectileState(this, SPTentaControllerInstance,
                         UpSwipe,
                         SwipeAngle);


                // Ok to directly set this like this???
                InputFlagArray[(int)HotInputs.LaunchSwipe] = false;

            }

            // if juke-right input received, actaully juke right using TentacleController callback method
            if (InputFlagArray[(int)HotInputs.RudderRight])
            {
                // actually move tentacle:
                CurrentlyJuking = true;
                // false parameter to jump RIGHT, true parameter to jump LEFT
                WhereJumpingTo = SPTentaControllerInstance.TT_CalculateEndJumpPosition(false);
                // Set CurrentlyJuking to true if still need to keep moving, when done juking set CurrentlyJuking to false
                CurrentlyJuking = ! SPTentaControllerInstance.TT_MoveSideWays_TillEnd(WhereJumpingTo, Vector2.right);


                InputFlagArray[(int)HotInputs.RudderRight] = false;

                Debug.Log("MARIA RudderRight reached in Coiled State: " + WhereJumpingTo);

            }
            else if (InputFlagArray[(int)HotInputs.RudderLeft])
            {
                // actually move tentacle:
                CurrentlyJuking = true;
                // false parameter to jump RIGHT, true parameter to jump LEFT
                WhereJumpingTo = SPTentaControllerInstance.TT_CalculateEndJumpPosition(true);
                // Set CurrentlyJuking to true if still need to keep moving, when done juking set CurrentlyJuking to false
                CurrentlyJuking = ! SPTentaControllerInstance.TT_MoveSideWays_TillEnd(WhereJumpingTo, Vector2.left);


                InputFlagArray[(int)HotInputs.RudderLeft] = false;

                Debug.Log("MARIA RddrLEFT rchd n Coiled State: " + WhereJumpingTo);
            }
        }

        public override void ProcessState(ITentacleInputCommandInput input)
        {
            // Coiled States ProcessState does NOT block flag changes while executing.
            IsCurrentlyProcessing = false;
            StringRep = "Coiled";

            TentaControllerInstance.ResetTentacleTipSprite();

            if (BoltNetwork.IsClient && TentaControllerInstance.state.AmIPlayer2)
            {
                //Debug.Log("Chris IS this code unreachable???");

                // Check if out of sync with server
                if (StringRep != TentaControllerInstance.state.CurrentStateString )//&& GameLoopTicksBeforeSync > 3)
                {
                    Debug.Log("Chris StringRep " + StringRep + "does not equal state.Current   "
                        + TentaControllerInstance.state.CurrentStateString);


                    if (TentaControllerInstance.state.CurrentStateString == "Projectile")
                    {
                        //  //become projectile
                        TentaControllerInstance.CurrentTentacleState = new ProjectileState(TentaControllerInstance);

                        StringRep = "Projectile";
                    }


                }
            }


            // if player up-swipe, they tryna  *L A U N C H*
            if (InputFlagArray[(int)HotInputs.LaunchSwipe])
            {
                input.UpSwipe = new Vector3(TentaControllerInstance.TTMovePositionVelocityRequested.x,
                    TentaControllerInstance.TTMovePositionVelocityRequested.y, 0);


                input.SwipeAngle = TentaControllerInstance.TTMoveRotationAngleRequested;


                //if (BoltNetwork.IsClient)
                //{
                //    Debug.Log("Chris <b>LaunchSwipe in CoiledState.ProcessState(input)</b>");
                //    Debug.Log("Chris <b>Input.UpSwipe = </b>" + input.UpSwipe.ToString());

                //}

                // Ok to directly set this like this???
                InputFlagArray[(int)HotInputs.LaunchSwipe] = false;

            }

            // if juke-right input received, actaully juke right using TentacleController callback method
            if (InputFlagArray[(int)HotInputs.RudderRight])
            {
                input.RightTap = true;
                InputFlagArray[(int)HotInputs.RudderRight] = false;

                //if (BoltNetwork.IsClient)
                //{
                //    Debug.Log("Chris <b>RudderRight in CoiledState.ProcessState(input)</b>");
                //}
            }
            else if (InputFlagArray[(int)HotInputs.RudderLeft])
            {
                input.LeftTap = true;
                InputFlagArray[(int)HotInputs.RudderLeft] = false;

                //if (BoltNetwork.IsClient)
                //{
                //    Debug.Log("Chris <b>RudderLeft in CoiledState.ProcessState(input)</b>");
                //}

            }
        }


        // After ProcessState sets the Bolt Command input, ProcessCommand does stuff in-game
        // Only EVER runs on server side P1 and server side P2; NEVER NEVER on client!!!
        public override void ProcessCommand(TentacleInputCommand command)
        {
            TentaControllerInstance.state.CurrentStateString = "Coiled";
            GameLoopTicksBeforeSync++;

          
            // Trying to keep P2 sprite orange after CoiledState.cs resets the sprite.
            // p2 on server; 
            if (! TentaControllerInstance.entity.hasControl && TentaControllerInstance.entity.isOwner)
            {
                TentaControllerInstance.TTChangeTentacleSpritetoPlayerTwo();
            }
         

            // Only accept input if NOT in middle of animation
            if (CurrentlyJuking == false)
            {
                //// if player up-swipe, they tryna  *L A U N C H*
                ///
                // if (command.Input.UpSwipe.x != 0.0 || command.Input.UpSwipe.y != 0.0)
                // Above short circuits into always true, but only on client...


                if (command.Input.UpSwipe != Vector3.zero)
                {
                    //Debug.Log("Chris <b>LaunchSwipe in CoiledState.ProcessCommand(command)</b>");
                    //Debug.Log("Chris <b>Input.UpSwipe = </b>" + command.Input.UpSwipe.ToString());

                    TentaControllerInstance.state.LatestSwipe = command.Input.UpSwipe;
                    TentaControllerInstance.state.LatestSwipeAngle = command.Input.SwipeAngle;

                    // BOLT NETWORK STATE SYNC LOGIC MUST BE RUN SERVER SIDE BY 'OWNER'
                    if (command.Input.CommandFromP2)
                    {
                        // Use inverse quaternion identity to flip the swipe so it works on flipped camera
                        var P2Swipe = new Vector2(command.Input.UpSwipe.x * -1, command.Input.UpSwipe.y * -1);
                        TentaControllerInstance.state.LatestSwipe = P2Swipe;

                        TentaControllerInstance.CurrentTentacleState = new ProjectileState(this,
                     P2Swipe,
                     command.Input.SwipeAngle);

                    }
                    else
                    {

                        TentaControllerInstance.CurrentTentacleState = new ProjectileState(this,
                          command.Input.UpSwipe,
                          command.Input.SwipeAngle);

                    }

                    TentaControllerInstance.state.CurrentStateString = "Projectile";
                    TentaControllerInstance.SetBoltTentaStateString("Projectile");

                }


                // if juke input received, actaully juke using TentacleController callback method
                if (command.Input.RightTap || command.Input.LeftTap)
                {
                    //Debug.Log("$$$$$ IN BIKINI BOTTOM IM WITH SANDY $$$$$$$");

                    CurrentlyJuking = true;
                    // false parameter to jump RIGHT, true parameter to jump LEFT
                    WhereJumpingTo = TentaControllerInstance.TT_CalculateEndJumpPosition(command.Input.LeftTap);
                    // Set CurrentlyJuking to true if still need to keep moving, when done juking set CurrentlyJuking to false
                    CurrentlyJuking = TentaControllerInstance.TT_JumpSideways(WhereJumpingTo);
                }


            }
            else
            {
                // Still currently juking
                CurrentlyJuking = TentaControllerInstance.TT_JumpSideways(WhereJumpingTo);
               // TentaControllerInstance.state.CurrentStateString = "Coiled";
            }
        }

        // Coiled state is NOT the same for P1 and P2
        public override void ProcessCommandFromPlayerTwo(TentacleInputCommand command)
        {
           
        }
    }
}
