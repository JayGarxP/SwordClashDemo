﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwordClash
{
    /// <summary>  
    ///  Abstract state class, attempting to fit the OO gang of four 'state' pattern into Unity
    ///  Each state is own class, responsible for its own transitions.
    ///  Context (TentacleController.cs instance) keeps a reference to current state with this class
    /// </summary>  
    public abstract class TentacleState
    {
        public TentacleController TentaControllerInstance;

        /*raised flags == true; means that player controller has received a gesture and set the appropriate values, flags, scalars, coords, etc.
          The tentaclecontroller responds in different ways to the same inputs (same flags) depending on tentacle state machine, see Game Design Doc. 
             */

        public bool AmIPlayerTwo;

        // mutex / semaphore??? should this be?
        protected bool IsCurrentlyProcessing;

        // Game object tags used by collisions
        protected string JellyfishEnemyGameObjectTag = "JellyfishEnemy";
        protected string FoodpickupGameObjectTag = "FoodPickup";

        // HotInputs is a way to map input flags to ints, used by other classes to index into InputFlagArray
        public enum HotInputs
        {
            RudderRight = 0,
            RudderLeft = 1,
            BarrelRoll = 2,
            ReelBack = 3,
            LaunchSwipe = 4
        };

        // Inputs received each frame of physics FixedUpdate (for now)
        protected bool[] InputFlagArray;
        // number of HotInput values
        protected int InputFlagCount;


        /// <summary>  
        ///  Sets all values in InputFlagArray to false, using InputFlagCount to iterate
        ///  <para>InputFlagCount should be same size value as HotInputs enum; 
        ///  the size value is calculated automatically in the TentacleState constructor</para>
        /// </summary>  
        public void LowerAllInputFlags()
        {
            for (int i = 0; i < InputFlagCount; i++)
            {
                InputFlagArray[i] = false;
            }
        }


        /// <summary>  
        ///  Constructor sets IsCurrentlyProcessing to false, and calculates number of input flags
        ///  <para>Setting InputFlagArray elements to false</para>
        /// </summary>  
        /// <param name="tc">TentacleController instance is 'Context' of state pattern</param>
        public TentacleState(TentacleController tc)
        {
            TentaControllerInstance = tc;
            IsCurrentlyProcessing = false;
            AmIPlayerTwo = false;

            // initialize input flag array to length of InputFlag_Enum, default value is false.
            InputFlagCount = Enum.GetNames(typeof(HotInputs)).Length;
            InputFlagArray = new bool[InputFlagCount];

            //OnStateEnter(); not working if abstract method; must be VIRTUAL to work
        }



        /// <summary>  
        /// <para>Used by other classes to attempt to raise a flag.</para>  
        /// please pass in InputFlag_Enum.FlagValue as parameter,
        /// assume InputFlag_Enum starts at 0
        /// </summary>
        /// 
        /// <returns> true if flag was raised false if currently processing state. </returns>   
        public bool RaiseTentacleFlag_Request(int requestedFlagtoRaise)
        {
            bool yesFlagRaised = false;

            // only try to raise if: requested flag is inside InputFlagEnum range
            if ((requestedFlagtoRaise >= 0) && (requestedFlagtoRaise < InputFlagCount))
            {
                if (IsCurrentlyProcessing == false)
                {
                    //TODO: try catch here?
                    InputFlagArray[requestedFlagtoRaise] = true;
                    yesFlagRaised = true;
                }
            }
            return yesFlagRaised;
        }

        // Do not check 'if currenly processing' bool before setting InputFlagArray[requestedFlag]
        protected bool RaiseTentacleFlag_Force(int requestedFlagtoRaise)
        {
            bool yesFlagRaised = false;

            // only try to raise if: requested flag is inside InputFlagEnum range
            if (requestedFlagtoRaise >= 0 && requestedFlagtoRaise < InputFlagCount)
            {
                InputFlagArray[requestedFlagtoRaise] = true;
                yesFlagRaised = true;
            }

            return yesFlagRaised;
        }

        protected bool LowerTentacleFlag_Force(int requestedFlagtoLower)
        {
            bool yesFlagLowered = false;

            // only try to raise if: requested flag is inside InputFlagEnum range
            if (requestedFlagtoLower >= 0 && requestedFlagtoLower < InputFlagCount)
            {
                InputFlagArray[requestedFlagtoLower] = false;
                yesFlagLowered = true;
            }
            return yesFlagLowered;
        }

        public abstract void OnStateEnter();
        public abstract void OnStateExit();
        public abstract void ProcessState();
        public abstract void ProcessState(ITentacleInputCommandInput input);
        public abstract void ProcessCommand(TentacleInputCommand command);
        public abstract void HandleCollisionByTag(string ObjectHitTag, UnityEngine.Rigidbody2D ObjectHitRB2D);
    }



}