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

        private Vector2 JukeVelocity;
        private float JukeAngle;

        private float CurrentJukeTravelTime;
        private float JukeTravelTime;

        // Might put enum direction lookup here later to have an oldschool compass NE SW D-pad way to juke

        private bool JustCollidedWithFood;
        private bool JustCollidedWithWall;
        private bool JustCollidedWithWallVert;
        private Rigidbody2D FoodHitRef;

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
            : base(oldState, SPTC)
        {
            SwipeVelocityVector_before = oldDirection;
            SwipeAngle_before = oldRotation;
            BrollCount = brollCount;
            JukeCount = jukeCount;
            JukeDirCode = jukeDirection;

            SetDirectionAndAngle();
        }

        public override void HandleCollisionByTag(string ObjectHitTag, Rigidbody2D ObjectHitRB2D)
        {
            if (ObjectHitTag == WallGameObjectTag)
            {

                JustCollidedWithWall = true;
            }
            else if (ObjectHitTag == WallVerticalGameObjectTag)
            {
                JustCollidedWithWallVert = true;
            }
            else if (ObjectHitTag == JellyfishEnemyGameObjectTag)
            {
                // Play oregon duck whoop cant catch me sound effect
                SoundManagerScript.PlaySound("miss");

            }
            else if (ObjectHitTag == FoodpickupGameObjectTag)
            {
                JustCollidedWithFood = true;
                FoodHitRef = ObjectHitRB2D;
            }

            // Consider looking up having debug constants in Unity;
            // can i do that with a global deltatime modifier or having a simple reset preset but for debug
            // e.g. move slowly in debug

        }

        public override void OnStateEnter()
        {
            LowerAllInputFlags();
            JustCollidedWithFood = false ;
         JustCollidedWithWall = false;
        JustCollidedWithWallVert = false;
        Rigidbody2D FoodHitRef = null;

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
            // cool rentering atmosphere shake apart effect if just use random vals.
            //float angleModifier = Random.Range(-5f, 5f);

            // A * Cos(f) + c = yHeight;_; Amplitude makes it shake more/further Freq (period w/shift) makes it wiggle faster 
            float angleModifier = 2.0f * Mathf.Cos(Time.time * 20.0f);

            // Always move every frame.
            SPTentaControllerInstance.TT_MoveTentacleTip(JukeVelocity, JukeAngle + angleModifier);


            // drop a frame input processing if hit wall, so players can't glitch past it.
            if (JustCollidedWithWall)
            {
                OnWallCollision();
                // also makes swipe angle = - swipe angle for now as side effect.
                ReflectTentacleVelocity(Vector2.up);
                JustCollidedWithWall = false;


            }
            else if (JustCollidedWithWallVert)
            {
                OnWallCollision();

                ReflectTentacleVelocity(Vector2.left);
                JustCollidedWithWallVert = false;
            }
            else if (JustCollidedWithFood)
            {
                // Change state to HoldingFood and give reference to which food hit in constructor
                SPTentaControllerInstance.CurrentTentacleState = new HoldingFoodState(this, SPTentaControllerInstance, FoodHitRef);
                // TODO: grab sound FX 
            }
            else if (InputFlagArray[(int)HotInputs.BackFlip])
            {
                // TODO: play animation cancel sound FX
                // TODO: Briefly flash colors to black??? later do a turning animation
                SPTentaControllerInstance.CurrentTentacleState = new BackFlipState(this, SPTentaControllerInstance,
                   SwipeVelocityVector_before,
                   SwipeAngle_before, BrollCount, JukeCount,
                   SPTentaControllerInstance.TTBackFlipNormalDirRequested,
                   SPTentaControllerInstance.TTBackFlipAngleRequested);
            }


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
                JukeVelocity = Vector2.left;
                // set angle here too 
                JukeAngle = 90f; // 90 is left
            }
            else if (JukeDirCode == 2)
            {
                JukeVelocity = Vector2.right;
                // set angle here too 
                JukeAngle = -90f;

            }

            // TODO: connect constants into unity editor and work on a debug switch that moves game at halfspeed; turns on god mode etc. to not have to recompile scripts
            // juke should move faster
            JukeVelocity = new Vector2(
                JukeVelocity.x * 2.5f, JukeVelocity.y);

        }

        // Bounce tentacle off in opposite direction same speed "successful bounce" from state diagram
        // later will add a 'crumple' wall that scrunches you and slows you down if you don't bounce off it (slime vs. steel)
        public void ReflectTentacleVelocity(Vector2 surfaceNormal)
        {

            JukeVelocity = CalcVelocityVectorReflection(JukeVelocity, surfaceNormal);

            // try just -neg flipping for angle?
            JukeAngle = JukeAngle * -1;
        }
        // TODO: refactor these to have additional penalties for crashing/juking INTO a wall and reward for juking away / swiping away from wall
        private void OnWallCollision()
        {
            // No need to exit juking state for now...
            // JUKING its own state, so if you juke into a wall, you get extra crumpled.

            LowerAllInputFlags(); // drop frame of human input

            //TODO: display  EXPLOSION animation and sound effect enter recovery mode (burnt sprite)

        }

        public override void ProcessCommandFromPlayerTwo(TentacleInputCommand command)
        {
            //throw new NotImplementedException();
        }
    }
}
