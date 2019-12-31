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




            //TODO: collision handling and wall bounce support; restore jukes etc.

            // Consider looking up having debug constants in Unity;
            // can i do that with a global deltatime modifier or having a simple reset preset but for debug

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
            // Always move every frame.
            SPTentaControllerInstance.TT_MoveTentacleTip(JukeDirection, JukeAngle);


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

            // juke should move faster
            JukeDirection *= SPTentaControllerInstance.UPswipeSpeedModifier * 1.5f;

        }

        // Bounce tentacle off in opposite direction same speed "successful bounce" from state diagram
        // later will add a 'crumple' wall that scrunches you and slows you down if you don't bounce off it (slime vs. steel)
        public void ReflectTentacleVelocity(Vector2 surfaceNormal)
        {

            JukeDirection = CalcVelocityVectorReflection(JukeDirection, surfaceNormal);

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
