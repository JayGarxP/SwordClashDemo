using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SwordClash
{
    class JukingState : TentacleState
    {

        public JukingState(TentacleController tc) : base(tc)
        {
            OnStateEnter();
        }

        // initialize with another state to enter recovery state
        public JukingState(TentacleState oldState)
            : base(oldState.TentaControllerInstance)
        {
            OnStateEnter();

        }

        // initialize with another state to enter recovery state
        public JukingState(TentacleState oldState, SinglePlayerTentaController SPTC)
            : base(SPTC)
        {
            OnStateEnter();

        }
    
        public override void HandleCollisionByTag(string ObjectHitTag, Rigidbody2D ObjectHitRB2D)
        {
            //throw new NotImplementedException();
        }

        public override void OnStateEnter()
        {
         

        }

        public override void OnStateExit()
        {
  
            LowerAllInputFlags();
        }


        public override void ProcessState()
        {
            

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

        public override void ProcessCommandFromPlayerTwo(TentacleInputCommand command)
        {
            //throw new NotImplementedException();
        }
    }
}
