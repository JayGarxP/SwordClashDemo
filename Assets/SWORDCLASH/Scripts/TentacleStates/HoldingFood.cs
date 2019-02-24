using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SwordClash
{
    class HoldingFoodState : TentacleState
    {
        private Rigidbody2D FoodHeld;

        public HoldingFoodState(TentacleController tc) : base(tc)
        {
            OnStateEnter();
        }

        // initialize with another state to enter food holding state
        public HoldingFoodState(TentacleState oldState, UnityEngine.Rigidbody2D foodToHold)
            : base(oldState.TentaControllerInstance)
        {
            this.FoodHeld = foodToHold;
            OnStateEnter();

        }

        public override void HandleCollisionByTag(string ObjectHitTag, Rigidbody2D ObjectHitRB2D)
        {
            //throw new NotImplementedException();
        }

        public override void OnStateEnter()
        {
            TentaControllerInstance.TTPickupFood(FoodHeld);

        }

        public override void OnStateExit()
        {
            LowerAllInputFlags();
        }



        public override void ProcessState()
        {
            // move towards start position
            TentaControllerInstance.TTMoveTowardsEatingZone(FoodHeld);


            // Check if made it home safe
            if (TentaControllerInstance.CheckifTTAtEatingPosition())
            {
                // Scoring logic here
                TentaControllerInstance.TTEatFood();

                OnStateExit();
                TentaControllerInstance.CurrentTentacleState = new CoiledState(this);
            }
        }

        // Called from Bolt.SimulateController()
        public override void ProcessState(ITentacleInputCommandInput input)
        {
            ProcessState();
        }

        public override void ProcessCommand(TentacleInputCommand command)
        {
            //throw new NotImplementedException();
        }
    }
}