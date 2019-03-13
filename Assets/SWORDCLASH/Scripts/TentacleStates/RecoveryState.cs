using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SwordClash
{
    class RecoveryState : TentacleState
    {
        public RecoveryState(TentacleController tc) : base(tc)
        {
            OnStateEnter();
        }

        // initialize with another state to enter recovery state
        public RecoveryState(TentacleState oldState)
            : base(oldState.TentaControllerInstance)
        {
            OnStateEnter();

        }

        public override void HandleCollisionByTag(string ObjectHitTag, Rigidbody2D ObjectHitRB2D)
        {
            //throw new NotImplementedException();
        }

        public override void OnStateEnter()
        {
            // change sprite to deflated
            TentaControllerInstance.PleaseDarkenTentacleSprite();

        }

        public override void OnStateExit()
        {
            LowerAllInputFlags();
        }



        public override void ProcessState()
        {
           
            TentaControllerInstance.PleaseDarkenTentacleSprite();


          
        }

        public override void ProcessState(ITentacleInputCommandInput input)
        {
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
                        Debug.Log("Chris SWITHCING TO COILED STATE in Recovery.ProcState(...)");
                        StringRep = "Coiled";
                    }
                   


                }
            }

            ProcessState();
        }

        public override void ProcessCommand(TentacleInputCommand command)
        {
            // move towards start position slowly, while wiggling
            TentaControllerInstance.TT_WiggleBackToStartPosition();

            // Check if made it home safe
            if (TentaControllerInstance.CheckifTTAtStartPosition())
            {
                OnStateExit();
                TentaControllerInstance.CurrentTentacleState = new CoiledState(this);
                TentaControllerInstance.state.CurrentStateString = "Coiled";
            }

        }

        public override void ProcessCommandFromPlayerTwo(TentacleInputCommand command)
        {
            //throw new NotImplementedException();
        }
    }
}
