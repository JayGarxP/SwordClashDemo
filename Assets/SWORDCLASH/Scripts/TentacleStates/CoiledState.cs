using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SwordClash
{
    // possible this could be a static state, since all instances of it are the same...
    public class CoiledState : TentacleState
    {

        // Can't do anything until finished with side-to-side movement
        bool CurrentlyJuking;
        Vector2 WhereJumpingTo;

        // initialize with another state, resuming coiled state
        public CoiledState(TentacleState oldState)
            : base(oldState.TentaControllerInstance)
        {
            OnStateEnter();

        }

        // initialize with new tentacle controller, first coil of game
        public CoiledState(TentacleController tsc) : base(tsc)
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

            if (BoltNetwork.IsServer)
            {
                // Reset position and sprite of tentacle tip
                TentaControllerInstance.TT_RecoilTentacle();
            }
            
        }

        public override void OnStateExit()
        {
            //throw new NotImplementedException();

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

        }

        public override void ProcessState(ITentacleInputCommandInput input)
        {

            // Coiled States ProcessState does NOT block flag changes while executing.
            IsCurrentlyProcessing = false;

            // if player up-swipe, they tryna  *L A U N C H*
            if (InputFlagArray[(int)HotInputs.LaunchSwipe])
            {
                input.UpSwipe = new Vector3(TentaControllerInstance.TTMovePositionVelocityRequested.x,
                    TentaControllerInstance.TTMovePositionVelocityRequested.y, 0);
            }

            // if juke-right input received, actaully juke right using TentacleController callback method
            if (InputFlagArray[(int)HotInputs.RudderRight])
            {
                input.RightTap = true;
                InputFlagArray[(int)HotInputs.RudderRight] = false;
            }
            else if (InputFlagArray[(int)HotInputs.RudderLeft])
            {
                input.LeftTap = true;
                InputFlagArray[(int)HotInputs.RudderLeft] = false;

            }
        }


        // After ProcessState sets the Bolt Command input, ProcessCommand does stuff in-game
        // Only EVER runs on server P1
        public override void ProcessCommand(TentacleInputCommand command)
        {
            // Only accept input if NOT in middle of animation
            if (CurrentlyJuking == false)
            {
                //Debug.Log("$$$$$ ProcessCommand Up swipe logic reached $$$$$$$");

                //// if player up-swipe, they tryna  *L A U N C H*
                if (command.Input.UpSwipe.x != 0 || command.Input.UpSwipe.y != 0)
                {
                    //Debug.Log("$$$$$ ProcessCommand Up swipe logic reached $$$$$$$");

                    var JebediahTheProjectileState = new ProjectileState(this,
                      command.Input.UpSwipe,
                      TentaControllerInstance.TTMoveRotationAngleRequested);

                    if (command.Input.CommandFromP2)
                    {
                        Debug.Log("$$$$$ ProcessCommand Up swipe WAS from Player two btw $$$$$$$");

                        //TentaControllerInstance.ChangeOpponentState(JebediahTheProjectileState);
                        this.TentaControllerInstance.ChangeOpponentToProjectile(JebediahTheProjectileState);
                    }
                    else
                    {
                        this.TentaControllerInstance.CurrentTentacleState = JebediahTheProjectileState;
                    }

                    InputFlagArray[(int)HotInputs.LaunchSwipe] = false;
                }


                // if juke input received, actaully juke using TentacleController callback method
                if (command.Input.RightTap || command.Input.LeftTap)
                {
                    Debug.Log("$$$$$ IN BIKINI BOTTOM IM WITH SANDY $$$$$$$");

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
            }
        }





    }
}
