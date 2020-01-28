using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SwordClash
{
    class RecoveryState : TentacleState
    {
        // 1 means voluntary 0 means involuntary (stung)
        private short ReelBackStatus;

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

        // initialize with another state to enter recovery state
        public RecoveryState(TentacleState oldState, SinglePlayerTentaController SPTC)
            : base(oldState, SPTC)
        {

        }
      
        public RecoveryState(TentacleState oldState, SinglePlayerTentaController SPTC, short voluntaryReelBack) : this(oldState, SPTC)
        {
            // base constructor is called first, then : this() then the one this comment is in right now.
            this.ReelBackStatus = voluntaryReelBack;
        }

        public override void HandleCollisionByTag(string ObjectHitTag, Rigidbody2D ObjectHitRB2D)
        {
            //throw new NotImplementedException();
        }

        public override void OnStateEnter()
        {
            // defaul reel back status is 0; involuntary; will need to rework these disaster constructors later.
            ReelBackStatus = 0;

            // change sprite to deflated
            if (TentaControllerInstance != null)
            {
                // to help multiplayer sprite darken
                TentaControllerInstance.PleaseDarkenTentacleSprite();

            }
          

        }

        public override void OnStateExit()
        {
            ReelBackStatus = 0;
            LowerAllInputFlags();
        }



        public override void ProcessState()
        {
            if (TentaControllerInstance != null)
            {
                TentaControllerInstance.PleaseDarkenTentacleSprite();

            }
            else
            {
                // stung, wiggle back slowly.
                if (ReelBackStatus == 0)
                {
                    // single player logic solution for now
                    SPTentaControllerInstance.PleaseDarkenTentacleSprite();

                    // move towards start position slowly, while wiggling
                    SPTentaControllerInstance.TT_WiggleBackToStartPosition();
                } else if (ReelBackStatus == 1)
                {
                    // voluntarily reeled in, move back fast but collide with things
                    SPTentaControllerInstance.TT_ReelBackToStartPosition();

                }

                // Check if made it home safe
                if (SPTentaControllerInstance.CheckifTTAtStartPosition())
                {
                    SPTentaControllerInstance.CurrentTentacleState = new CoiledState(this, SPTentaControllerInstance);
                }
            }



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
