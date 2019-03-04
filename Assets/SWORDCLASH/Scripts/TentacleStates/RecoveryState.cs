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
            // move towards start position slowly, while wiggling
            TentaControllerInstance.TT_WiggleBackToStartPosition();

            // Check if made it home safe
            if (TentaControllerInstance.CheckifTTAtStartPosition())
            {
                OnStateExit();
                TentaControllerInstance.CurrentTentacleState = new CoiledState(this);
            }
        }

        public override void ProcessState(ITentacleInputCommandInput input)
        {
            ProcessState();
        }

        public override void ProcessCommand(TentacleInputCommand command)
        {
            //throw new NotImplementedException();
        }

        public override void ProcessCommandFromPlayerTwo(TentacleInputCommand command)
        {
            //throw new NotImplementedException();
        }
    }
}
