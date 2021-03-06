﻿using System;
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

        // single player constructor
        public HoldingFoodState(TentacleState oldState, SinglePlayerTentaController SPTC, UnityEngine.Rigidbody2D foodToHold)
           : base(oldState, SPTC)
        {
            this.FoodHeld = foodToHold;
           
            SPTentaControllerInstance.HoldingFoodRightNow = true;
            SPTentaControllerInstance.TTPickupFood(FoodHeld);
        }

        public override void HandleCollisionByTag(string ObjectHitTag, Rigidbody2D ObjectHitRB2D)
        {
            // Both players stop moving; 
            // back swipe prompt on screen tug of war ~3 seconds with half sec pause

            if (ObjectHitTag == OtherPlayerGameObjectTag)
            {
                if (TentaControllerInstance != null)
                {
                    Debug.Log("Maria holding food tent: " + TentaControllerInstance.entity.networkId + "hit by enemy player!!!!" 
                        + TentaControllerInstance.OpponentTCInstance.entity.networkId);
                }
                else
                {
                    // single player mode can ignore this...
                }
                
                
            }

            //TODO: HoldingFood and hit by enemy collision

        }

        public override void OnStateEnter()
        {
            // trash code; will fix later hopefully
            if (TentaControllerInstance != null)
            {
                TentaControllerInstance.HoldingFoodRightNow = true;
                TentaControllerInstance.TTPickupFood(FoodHeld);

            }
        }

        public override void OnStateExit()
        {
            LowerAllInputFlags();
            if (TentaControllerInstance != null)
            {
                TentaControllerInstance.HoldingFoodRightNow = false;
            }
            else
            {
                SPTentaControllerInstance.HoldingFoodRightNow = false;
            }

        }



        public override void ProcessState()
        {
            StringRep = "HoldingFood";
            IsCurrentlyProcessing = false;

            // move towards start position
            SPTentaControllerInstance.TTMoveTowardsEatingZone(FoodHeld);

            // Check if made it home safe
            if (SPTentaControllerInstance.CheckifTTAtEatingPosition())
            {
                SPTentaControllerInstance.TTEatFood("Player1");
                
                SPTentaControllerInstance.CurrentTentacleState = new CoiledState(this, SPTentaControllerInstance);
            }
        }

        // Called from Bolt.SimulateController()
        public override void ProcessState(ITentacleInputCommandInput input)
        {
            StringRep = "HoldingFood";
            IsCurrentlyProcessing = false;

            // SYNC STATE HERE
            if (BoltNetwork.IsClient && TentaControllerInstance.state.AmIPlayer2)
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
                        Debug.Log("Chris SWITHCING TO COILED STATE in holdingfood.ProcState(...)");
                        StringRep = "Coiled";
                    }

                }
            }

        }

        public override void ProcessCommand(TentacleInputCommand command)
        {
            // move towards start position
            TentaControllerInstance.TTMoveTowardsEatingZone(FoodHeld);
           
            TentaControllerInstance.SetBoltTentaStateString("HoldingFood");
            

            // Check if made it home safe
            if (TentaControllerInstance.CheckifTTAtEatingPosition())
            {
                // Scoring logic here
                if (command.Input.CommandFromP2)
                {
                    //TentaControllerInstance.TTEatFood("Player2");
                    //attempt to fix flicker bug
                    TentaControllerInstance.OpponentTCInstance.TTEatFood("Player2");
                }
                else
                {
                    TentaControllerInstance.TTEatFood("Player1");
                }

                OnStateExit();
                TentaControllerInstance.CurrentTentacleState = new CoiledState(this);
                TentaControllerInstance.SetBoltTentaStateString("Coiled");
            }
            
        }

        public override void ProcessCommandFromPlayerTwo(TentacleInputCommand command)
        {
            throw new NotImplementedException();
        }
    }
}