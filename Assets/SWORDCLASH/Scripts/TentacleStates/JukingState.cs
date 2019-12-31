using UnityEngine;

namespace SwordClash
{
    class JukingState : TentacleState
    {
        // to reenter projectile state
        private readonly Vector2 SwipeVelocityVector_before;
        private readonly float SwipeAngle_before;
        private readonly short BrollCount;

        private short JukeCount;
      
        private int JukeDirCode;

        private Vector2 JukeDirection;
        //TODO: put rotation angle on a wave function to make it diddle diddle shake
        private float JukeAngle;

        private float CurrentJukeTravelTime;
        private float JukeTravelTime;

        // Might put enum direction lookup here later to have an oldschool compass NE SW D-pad way to juke


        public JukingState(TentacleController tc) : base(tc)
        {
            OnStateEnter();
        }

        // initialize with another state to enter juking state
        public JukingState(TentacleState oldState)
            : base(oldState.TentaControllerInstance)
        {
            OnStateEnter();

        }

        // initialize with another state to enter recovery state
        public JukingState(TentacleState oldState, SinglePlayerTentaController SPTC,
            Vector2 oldDirection, float oldRotation, short brollCount, short jukeCount, int jukeDirection)
            : base(SPTC)
        {
            OnStateEnter(); // assume OSE is always called first; since it will be in future refactors...

            SwipeVelocityVector_before = oldDirection;
            SwipeAngle_before = oldRotation;
            BrollCount = brollCount;
            JukeCount = jukeCount;
            JukeDirCode = jukeDirection;

            SetDirectionAndAngle();
            

        }

        public override void HandleCollisionByTag(string ObjectHitTag, Rigidbody2D ObjectHitRB2D)
        {
            //throw new NotImplementedException();
        }

        public override void OnStateEnter()
        {
            LowerAllInputFlags();

            // Set travel time of JUKE
            JukeTravelTime = SPTentaControllerInstance.TTJukeTravelTime;
            CurrentJukeTravelTime = 0.0f;

        }



        public override void OnStateExit()
        {

            LowerAllInputFlags();
        }


        public override void ProcessState()
        {
            // Always move every frame.
            SPTentaControllerInstance.TT_MoveTentacleTip(JukeDirection, JukeAngle);
           


            // increment distance traveled
            CurrentJukeTravelTime += Time.fixedDeltaTime;

            // if travel time >= jumpdistance, stop juking
            if (CurrentJukeTravelTime >= JukeTravelTime)
            {
                // transition back to projectile state
                SPTentaControllerInstance.CurrentTentacleState = new ProjectileState
                    (this, SPTentaControllerInstance, SwipeVelocityVector_before, SwipeAngle_before, BrollCount, JukeCount);

            }

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

        private void SetDirectionAndAngle()
        {
            JukeAngle = SwipeAngle_before;

            if (JukeDirCode == 1)
            {
                JukeDirection = Vector2.left;
                // set angle here too 
                
            }
            else if (JukeDirCode == 2)
            {
                JukeDirection = Vector2.right;
                // set angle here too 

            }
        }

        public override void ProcessCommandFromPlayerTwo(TentacleInputCommand command)
        {
            //throw new NotImplementedException();
        }
    }
}
