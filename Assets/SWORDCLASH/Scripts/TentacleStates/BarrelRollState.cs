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
        }

        public override void OnStateExit()
        {
            LowerAllInputFlags();
        }



        // Rotate, move at half speed while rotating, check if done rotating to return to moving state pass in barrelRoll
        // don't check any input flags, don't process any bad collision events, still do good ones tho
        public override void ProcessState()
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
                TentaControllerInstance.CurrentTentacleState = new ProjectileState(this, SwipeVelocityVector, SwipeAngle, BarrelRollDuringThisProjectileCount, JukeCount);
            }
        }


        // Barrel Roll online input is same as single player barrel roll for now 2/11/2019
        public override void ProcessState(ITentacleInputCommandInput input)
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
                TentaControllerInstance.CurrentTentacleState = new ProjectileState(this, SwipeVelocityVector, SwipeAngle, BarrelRollDuringThisProjectileCount, JukeCount);
            }
        }

        public override void ProcessCommand(TentacleInputCommand command)
        {
            // no commands to process for now
            //throw new NotImplementedException();
        }

        public override void ProcessCommandFromPlayerTwo(TentacleInputCommand command)
        {
            throw new NotImplementedException();
        }
    }
}
