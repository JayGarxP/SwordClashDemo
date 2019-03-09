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
            
        }

        // Called from Bolt.SimulateController()
        public override void ProcessState(ITentacleInputCommandInput input)
        {
            StringRep = "HoldingFood";

            // SYNC STATE HERE
            if (BoltNetwork.IsClient && AmIPlayerTwo)
            {
                // Check if out of sync with server
                if (StringRep != TentaControllerInstance.state.CurrentStateString)
                {
                    Debug.Log("Chris StringRep " + StringRep + "does not equal state.Current   "
                        + TentaControllerInstance.state.CurrentStateString);


                    if (TentaControllerInstance.state.CurrentStateString == "Coiled")
                    {
                        //  //become coiled
                        TentaControllerInstance.CurrentTentacleState = new CoiledState(this);

                        StringRep = "Coiled";
                    }


                }
            }

            }

        public override void ProcessCommand(TentacleInputCommand command)
        {
            // move towards start position
            TentaControllerInstance.TTMoveTowardsEatingZone(FoodHeld);


            // Check if made it home safe
            if (TentaControllerInstance.CheckifTTAtEatingPosition())
            {
                // Scoring logic here
                if (command.Input.CommandFromP2)
                {
                    TentaControllerInstance.TTEatFood("Player2");
                }
                else
                {
                    TentaControllerInstance.TTEatFood("Player1");
                }

                OnStateExit();
                TentaControllerInstance.CurrentTentacleState = new CoiledState(this);
                TentaControllerInstance.SetBoltTentaStateString("Coiled");
            }
            else
            {
                TentaControllerInstance.SetBoltTentaStateString("HoldingFood");
            }
        }

        public override void ProcessCommandFromPlayerTwo(TentacleInputCommand command)
        {
            throw new NotImplementedException();
        }
    }
}