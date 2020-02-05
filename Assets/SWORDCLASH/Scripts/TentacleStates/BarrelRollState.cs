using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SwordClash
{
    // End goal is to get players to paint/trace patterns on the glass, like fighting game inputs
    class BarrelRollState : TentacleState
    {
        //Same members across multiple states, could put into abstract base class,
        //  could also create object pool for natural playing of game (pool of 10 states or so)
        //  worried about memory hogging / fragmentations from recreating vectors over and over, but
        //      maybe the constructor uses a reference somehow??? Not sure how it works???????
        private Vector2 SwipeVelocityVector;
        private float SwipeAngle;
        private short JukeCount;

        //BAD CODE; two concrete classes share same variables: put in base class???
        //  maintain object/fields in Controller that tracts these variables?
        //  Should barrel roll be sub-state of projectile somehow??? then lose collision code...
        private short BarrelRollDuringThisProjectileCount;

        // snowboard degrees, 720 == two spins; checked against max value set in public editor field set in TentacleController.cs
        private float CurrentBrollDegreesRotated;
        private int GameLoopTicksBeforeSync;

        // Enter barrel roll from Projectile State, still WIP, so XML comment is coming.
        public BarrelRollState(TentacleState oldState, Vector2 swipeVelocityVector, float swipeAngle, short brollCount, short jukeCount)
              : base(oldState.TentaControllerInstance)
        {
            this.SwipeVelocityVector = swipeVelocityVector;
            this.SwipeAngle = swipeAngle;
            this.BarrelRollDuringThisProjectileCount = brollCount;
            this.JukeCount = jukeCount;
            OnStateEnter();
        }

        // Enter barrel roll from Projectile State, still WIP, so XML comment is coming.
        public BarrelRollState(TentacleState oldState, SinglePlayerTentaController SPTC,
            Vector2 swipeVelocityVector, float swipeAngle, short brollCount, short jukeCount)
              : base(oldState, SPTC)
        {
            this.SwipeVelocityVector = swipeVelocityVector;
            this.SwipeAngle = swipeAngle;
            this.BarrelRollDuringThisProjectileCount = brollCount;
            this.JukeCount = jukeCount;
        }

        // Player 2 client sync constructor from BoltNetwork State values.
        public BarrelRollState(TentacleController tc) : base(tc)
        {
            this.SwipeVelocityVector = TentaControllerInstance.state.LatestSwipe;
            this.SwipeAngle = TentaControllerInstance.state.LatestSwipeAngle;

            this.BarrelRollDuringThisProjectileCount = (short)TentaControllerInstance.state.BrollCount;
            this.JukeCount = (short)TentaControllerInstance.state.JukeCount;

            OnStateEnter();

            SwipeVelocityVector = TentaControllerInstance.state.LatestSwipe;
            SwipeAngle = TentaControllerInstance.state.LatestSwipeAngle;

            Debug.Log("Chris SYNC CONSTRUCTOR ACTIVATED broll: " + SwipeVelocityVector.ToString());
      
            OnStateEnter();
        }

        public override void HandleCollisionByTag(string ObjectHitTag, UnityEngine.Rigidbody2D objDodged)
        {
            if (ObjectHitTag == JellyfishEnemyGameObjectTag)
            {
                //TODO: whoop sound / dodge effect to show dodged jellyfish
            }
            //else if (ObjectHitTag )
            //{

            //}
        }

        // Reset degrees rotated count to zero, and lower all input flags
        public override void OnStateEnter()
        {
            CurrentBrollDegreesRotated = 0.0f;
            LowerAllInputFlags();
            GameLoopTicksBeforeSync = 0;
            StringRep = "BarrelRoll";
            Debug.Log("Chris $$$$$$$ BARREL ROLL OnStateEnter() $$$$$$$$$$$$$");
        }

        public override void OnStateExit()
        {
            LowerAllInputFlags();
        }



        // Rotate, move at half speed while rotating, check if done rotating to return to moving state pass in barrelRoll
        // don't check any input flags, don't process any bad collision events, still do good ones tho
        public override void ProcessState()
        {
            StringRep = "BarrelRoll";
           
            // NOT Free to process here!
            IsCurrentlyProcessing = true;

            CurrentBrollDegreesRotated = SPTentaControllerInstance.BarrelRollin_rotate(CurrentBrollDegreesRotated);

            // still move, but more slowly
            SPTentaControllerInstance.TT_MoveTentacleTip_WhileBroll(SwipeVelocityVector);


            // If the barrelroll is over; the total spin 360, 720, etc. has been overcome by degrees of rotation per frame
            if (CurrentBrollDegreesRotated >= SPTentaControllerInstance.BarrelRollEndSpinRotationDegrees)
            {
                SPTentaControllerInstance.ResetTentacleTipRotation();
                
                //increment barrel roll count
                BarrelRollDuringThisProjectileCount++;

                // Change to projectile state.
                SPTentaControllerInstance.CurrentTentacleState = new ProjectileState(this, SPTentaControllerInstance, SwipeVelocityVector, SwipeAngle, BarrelRollDuringThisProjectileCount, JukeCount);
            }
            //GameLoopTicksBeforeSync++;

        }


        // Barrel Roll online input is same as single player barrel roll
        public override void ProcessState(ITentacleInputCommandInput input)
        {
            StringRep = "BarrelRoll";
            IsCurrentlyProcessing = false;


            // SYNC STATE HERE
            if (BoltNetwork.IsClient && TentaControllerInstance.state.AmIPlayer2)
            {
                // Check if out of sync with server
                if (StringRep != TentaControllerInstance.state.CurrentStateString)
                {
                    Debug.Log("Chris StringRep " + StringRep + "does not equal state.Current   "
                        + TentaControllerInstance.state.CurrentStateString);

                    if (TentaControllerInstance.state.CurrentStateString == "Projectile")
                    {
                        // barrel roll out
                        TentaControllerInstance.CurrentTentacleState = new ProjectileState(TentaControllerInstance);
                        Debug.Log("Chris SWITHCING TO proj in BarrelRollState.ProcState(...)");
                        StringRep = "Projectile";

                    }


                }
            }
        }

        public override void ProcessCommand(TentacleInputCommand command)
        {
            // NOT Free to process here!
            IsCurrentlyProcessing = true;

            CurrentBrollDegreesRotated = TentaControllerInstance.BarrelRollin_rotate(CurrentBrollDegreesRotated);

            // still move, but more slowly
            TentaControllerInstance.TT_MoveTentacleTip_WhileBroll(SwipeVelocityVector);


            // If the barrelroll is over; the total spin 360, 720, etc. has been overcome by degrees of rotation per frame
            if (CurrentBrollDegreesRotated >= TentaControllerInstance.BarrelRollEndSpinRotationDegrees)
            {
                TentaControllerInstance.ResetTentacleTipRotation();
                OnStateExit();
                //increment barrel roll count
                BarrelRollDuringThisProjectileCount++;

                // Change to projectile state.
                TentaControllerInstance.CurrentTentacleState = new ProjectileState(this, SwipeVelocityVector, SwipeAngle, BarrelRollDuringThisProjectileCount, JukeCount);
                TentaControllerInstance.SetBoltTentaStateString("Projectile");
            }
            else
            {
                TentaControllerInstance.SetBoltTentaStateString("BarrelRoll");
            }

            GameLoopTicksBeforeSync++;

        }

        public override void ProcessCommandFromPlayerTwo(TentacleInputCommand command)
        {
            throw new NotImplementedException();
        }
    }
}
