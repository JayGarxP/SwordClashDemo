using Bolt;
using UnityEngine;
using UnityEngine.UI;

namespace SwordClash
{
    /// <summary>  
    ///  Controls how the tentacle responds to Input, how it is drawn, and how it collides with things.
    /// </summary>  
    public class TentacleController : Bolt.EntityEventListener<ITentacleTipState>
    {
        #region PUBLIC EDITOR FIELDS
        //TODO: make these private with [SerializeField] atrribute so they still appear in editor
        // Collision object
        public GameObject TentacleTip;
        // constant speed of tentacle
        public float UPswipeSpeedConstant;
        // Added to constant speed of tentacle
        public float UPswipeSpeedModifier;
        // how far tentacle can go before retracting, currently 1/19/19 ununused.
        public float maxTentacleLength;
        // How far behind screen the eating zone is
        public float EatingZoneOffsetFromStart;
        // Degrees to rotate tentacle tip per physics update, ~20 looks good.
        public float BarrelRollDegreestoRotatePerUpdate;
        // snowboard style degrees of rotation until barrel roll ends, two rotations = 720.
        public float BarrelRollEndSpinRotationDegrees;
        // 2 times means each up-swipe launch player gets two barrel rolls, reset once coiled
        public short TimesCanBarrelRoll;
        // real-time readout of tentacleTip's RB2D rotation value, will remove in future update
        public Text RotationValue_UI_Text;
        // RB2D.position units to teleport right--> by, 1 is default
        public float TTJukePosRightAmount;
        // RB2D.position units to teleport left<-- by, default is 1
        public float TTJukePosLeftAmount;
        // How fast to Juke, multiplied by fixedDeltaTime
        public float TTJumpSpeed;

        //3 means 3 taps can happen in one 'strike' or projectile state
        public float TTTimesAllowedToJuke;

        // Sprite to change TentacleTip sprite into after colliding with jellyfish
        public Sprite TTStungSprite;

        public Sprite TTPlayerTwoSprite;
        
        #endregion

        // Gang of Four State pattern, state machine for inputs allowed during tentacle movement
        public TentacleState CurrentTentacleState { get; set; }

        private JellyfishController JelFishController;

        // for training mode 'ghosts', possibly save last input swipe?
        public Vector2 TTMovePositionVelocityRequested;
        public float TTMoveRotationAngleRequested;

        private Rigidbody2D TentacleTipRB2D;
        private float StartTentacleLength;
        private Vector2 TentacleReadyPosition;
        // Where food is brought to die
        private Vector2 TentacleEatingPosition;
        private float StartTentacleRotation;
        private SpriteRenderer TTSpriteRenderer;
        private Sprite TTSceneSprite; //sprite object starts with

        private bool AmIPlayerTwo;
        private GameObject GameWorld;
        // Reference to game logic controller in scene
        private GameLogicController GLCInstance;



        // Setup the component you are on right now (the "this" object); before all Start()s
        void Awake()
        {
            //MovePositionVelocity_TT_Active = Vector2.zero;
            StartTentacleLength = 0;
            //AmIPlayerTwo = false;
        }

        //// Use this for initialization; Here you setup things that depend on other components.
        //void Start()
        //{
        //        var GameWorld = GameObject.FindWithTag("GameWorld");
        //    // check if component is unattached or null here? Not sure best way to make tightly-coupled components know of each other
        //    if (GameWorld != null && GameWorld.tag != "/")
        //    {
        //        GLCInstance = GameWorld.GetComponent<GameLogicController>();
        //        if (GLCInstance == null)
        //        {
        //            // bad thing happen
        //        }
        //    }

        //    TentacleTipRB2D = TentacleTip.GetComponent<Rigidbody2D>();
        //    TentacleReadyPosition = TentacleTipRB2D.position;
        //    TentacleEatingPosition = new Vector2(TentacleReadyPosition.x, TentacleReadyPosition.y - EatingZoneOffsetFromStart);
        //    StartTentacleLength = TentacleReadyPosition.magnitude;
        //    maxTentacleLength = StartTentacleLength * 2; //TODO: fix maxtentacleLength solution
        //    StartTentacleRotation = TentacleTipRB2D.rotation;


        //    // Set sprite renderer reference so tentacle can change color
        //    TTSpriteRenderer = this.GetComponent<SpriteRenderer>();
        //    TTSceneSprite = TTSpriteRenderer.sprite;

        //    // Redundant cast seems to help avoid null reference in update loop
        //    CurrentTentacleState = new CoiledState(((TentacleController)this));
        //}

        // BoltNetwork Start()
        public override void Attached()
        {
            TentacleTipRB2D = TentacleTip.GetComponent<Rigidbody2D>();
            TentacleReadyPosition = TentacleTipRB2D.position;
            TentacleEatingPosition = new Vector2(TentacleReadyPosition.x, TentacleReadyPosition.y - EatingZoneOffsetFromStart);
            StartTentacleLength = TentacleReadyPosition.magnitude;
            maxTentacleLength = StartTentacleLength * 2; //TODO: fix maxtentacleLength solution
            StartTentacleRotation = TentacleTipRB2D.rotation;


            // Set sprite renderer reference so tentacle can change color
            TTSpriteRenderer = GetComponent<SpriteRenderer>();
            TTSceneSprite = TTSpriteRenderer.sprite;

            // Redundant cast seems to help avoid null reference in update loop
            this.CurrentTentacleState = new CoiledState(((TentacleController)this));


            // Bolt Entity Transform sync
            this.state.SetTransforms(this.state.TTTransform, this.transform);

        }

        // BoltNetwork Update()
        // is used to collect inputs from your game and putting it into a Command. 
        // SimulateController executes one time per frame.
        public override void SimulateController()
        {

            if (GameWorld == null)
            {
                GameWorld = GameObject.FindWithTag("GameWorld");

                if (GameWorld != null && GameWorld.tag != "/")
                {
                    GLCInstance = GameWorld.GetComponent<GameLogicController>();
                    if (GLCInstance == null)
                    {
                        // bad thing happen; GLCInstance is still Null somehow
                    }
                }

            }
            else
            {
                // IrigidbodyPlayerCommandInput input = rigidbodyPlayerCommand.Create();
                ITentacleInputCommandInput input = TentacleInputCommand.Create();

                if (this.CurrentTentacleState != null && GLCInstance != null)
                {
                    // Actual game loop logic code, contained in each ProcessState() method of concrete TentacleState s 
                    // change values of input
                    this.CurrentTentacleState.ProcessState(input);
                }

                this.entity.QueueInput(input);
            }
           

            

        }


        public override void ExecuteCommand(Command command, bool resetState)
        {
            TentacleInputCommand cmd = (TentacleInputCommand)command;

            if (BoltNetwork.IsServer)
            {
                this.CurrentTentacleState.ProcessCommand(cmd);
            }

            //transform.GetChild(0).GetChild(3).transform.rotation = Quaternion.LookRotation(cmd.Input.lookDirection);

        }



        //// Update is called once per frame
        //void Update()
        //{

        //}

        //void FixedUpdate()
        //{
        //    if (this.CurrentTentacleState != null)
        //    {
        //        // Actual game loop logic code, contained in each ProcessState() method of concrete TentacleState s 
        //        this.CurrentTentacleState.ProcessState();
        //    }


        //    //    if (entity.isControllerOrOwner == false)
        //    //    {
        //    //        transform.GetChild(0).GetChild(3).transform.rotation = state.lookRotation;
        //    //    }

        //}

        public void PleaseRecoilTentacle()
        {
            int ReelBack = (int)TentacleState.HotInputs.ReelBack;
            this.CurrentTentacleState.RaiseTentacleFlag_Request(ReelBack);
        }
        public void TT_RecoilTentacle()
        {
            ResetTentacletoStartingPosition();
        }


        public void TT_MoveTentacleTip(Vector2 swipePositionVelocity, float swipeAngle)
        {
            //be careful! if physics update is not finished and you MovePosition() in same update frame, unexpected behavior will occur!
            //Position = current position + (Velocity vector of swipe per physics frame) 
            TentacleTipRB2D.MovePosition(TentacleTipRB2D.position + swipePositionVelocity * Time.fixedDeltaTime);
            //Set in PlayerController, updated here, consider adding if(bool angleSet), here it doesn't need to change, not sure which is faster...
            TentacleTipRB2D.rotation = swipeAngle;
        }

        public void TT_MoveTentacleTip_WhileBroll(Vector2 swipePositionVelocity)
        {
            //Move at half delta time speed ~around sqrt the normal speed. Do not rotate, rotate seperately in another method.
            TentacleTipRB2D.MovePosition(TentacleTipRB2D.position + (swipePositionVelocity * (Time.fixedDeltaTime * 0.5f)));
        }



        public bool IsTentacleAtMaxExtension()
        {
            return TentacleTipRB2D.position.magnitude >= maxTentacleLength;
        }

        private void ResetTentacletoStartingPosition()
        {
            // just teleport for now. Later change state.
            TentacleTipRB2D.MovePosition(TentacleReadyPosition);
            ResetTentacleTipRotation();
            ResetTentacleTipSprite();
        }

        public float BarrelRollin_rotate(float degreesRotatedSoFar)
        {
            //TentacleTip_RB2D.rotation = TentacleTip_RB2D.rotation + (20); //rotates along wrong centroid, from collider, not centerered....
            TentacleTip.transform.Rotate(0, 0, BarrelRollDegreestoRotatePerUpdate, Space.World); //rotate gameobject via transform Space.World centroid, looks cooler.
            degreesRotatedSoFar += BarrelRollDegreestoRotatePerUpdate;
            return degreesRotatedSoFar;
        }

        // pass in true to jump left, false to get a jump right end position vector as return value
        public Vector2 TT_CalculateEndJumpPosition( bool YesJumpingLeft)
        {
            Vector2 whereToJumpTo;

            if (YesJumpingLeft)
            {
                whereToJumpTo = new Vector2(TentacleTipRB2D.position.x - TTJukePosLeftAmount, TentacleTipRB2D.position.y);

            }
            else
            {
                // jumping right
                whereToJumpTo = new Vector2(TentacleTipRB2D.position.x + TTJukePosRightAmount, TentacleTipRB2D.position.y);
            }

            return whereToJumpTo;
        }

        // For now is x position '-' instead of rights '+'; but juking may change in future, so leave as
        // two seperate methods.
        public bool TT_JumpSideways(Vector2 endOfJumpPosition)
        {
            //Vector2 whereToJumpTo = new Vector2(TentacleTipRB2D.position.x - TTJukePosLeftAmount, TentacleTipRB2D.position.y);
            
                TentacleTip_JumpSideways(endOfJumpPosition);

            bool HaventReachedEndJumpPosition = true;

            if (TentacleTipRB2D.position == endOfJumpPosition)
            {
                HaventReachedEndJumpPosition = false;
            }

            return HaventReachedEndJumpPosition; 
        }
        private void TentacleTip_JumpSideways(Vector2 whereToJumpTo)
        {

            TentacleTipRB2D.position = Vector2.MoveTowards(TentacleTipRB2D.position, whereToJumpTo, Time.fixedDeltaTime * TTJumpSpeed);

        }

      
        public void PleaseStingTentacleSprite()
        {
            ChangeTentacleSpritetoSting();
        }
        private void ChangeTentacleSpritetoSting()
        {
            // change sprite to StungSprite set in editor field
            TTSpriteRenderer.sprite = TTStungSprite;
        }

        public void TTChangeTentacleSpritetoPlayerTwo()
        {
            // change sprite to p2 set in editor field inside inspector view of Tentacle_Tip
            TTSpriteRenderer.sprite = TTPlayerTwoSprite;
        }


        public void PleaseDarkenTentacleSprite()
        {
            TTSpriteRenderer.color = Color.black;
        }

        public void PleaseMakeMePlayerTwo()
        {
            AmIPlayerTwo = true;
            this.CurrentTentacleState.AmIPlayerTwo = true;
        }


        public void TT_WiggleBackToStartPosition()
        {
            // Higher floating point scalar multiplier means faster MoveTowards()
            float step = 0.75f * Time.fixedDeltaTime;
            // Move towards start position, slowly.
            TentacleTipRB2D.position = Vector2.MoveTowards(TentacleTipRB2D.position, TentacleReadyPosition, step);
            // Wiggle to and fro whilst returning to start
            // Add small Xcoord value, Cos(time since game start) * tiny scaling factor, each frame
            // Scaling factor is how far to move left or right each frame, 1 is the width of the sprite
            TentacleTipRB2D.MovePosition(new Vector2(TentacleTipRB2D.position.x + (Mathf.Cos(
                Time.time) * 0.01f)
                , TentacleTipRB2D.position.y));

        }

        // Return True means that yes, the tentacle tip is near start position
        public bool CheckifTTAtStartPosition()
        {
            return (TentacleTipRB2D.position == TentacleReadyPosition);
        }

        // Return True means that yes, the tentacle tip is near eating position
        public bool CheckifTTAtEatingPosition()
        {
            return (TentacleTipRB2D.position == TentacleEatingPosition);
        }

        public void TTEatFood()
        {
            string PlayerID = "Player1";
            if (AmIPlayerTwo)
            {
                PlayerID = "Player2";
            }


            if (GLCInstance != null)
            {
                GLCInstance.OnFoodEaten(PlayerID);
            }
            else
            {
                Debug.Log("GLCInstance null in TTEatFood()");
            }
        }

        public void TTMoveTowardsEatingZone(Rigidbody2D moveMeAsWell)
        {
            // Move towards starting position each frame
            TentacleTipRB2D.position = Vector2.MoveTowards(TentacleTipRB2D.position, TentacleEatingPosition, Time.fixedDeltaTime);
            moveMeAsWell.position = Vector2.MoveTowards(moveMeAsWell.position, TentacleEatingPosition, Time.fixedDeltaTime);
        }

        public void TTPickupFood(Rigidbody2D foodTouching)
        {
            foodTouching.position = new Vector2(TentacleTipRB2D.position.x - 0.5f, TentacleTipRB2D.position.y + 0.5f);
            IndicateHoldingFood();
        }
        private void IndicateHoldingFood()
        {
            // turn sprite a bit transparent, RGBa; Red Grn Blu alpha transparency, where 1,1,1 == white == default color
            TTSpriteRenderer.color = new Color(1f, 1f, 1f, 0.4f);
        }

        //private void OnDestroy()
        //{

        //}


        //TODO: rename methods to have Pleasefirst and remove the underscores
        //Juke to the right, eventaully will only work 3 times either way; called by player controller
        public void JukeRight_Please()
        {
            //Use InputFlag enum in tentacle state to raise correct flag, casted to int
            int RudderRight = (int)TentacleState.HotInputs.RudderRight;
            this.CurrentTentacleState.RaiseTentacleFlag_Request(RudderRight);
        }
        //TODO: spawn bubbles on Right side; spawn bubs on left for JukeRight()
        public void JukeLeft_Please()
        {
            int RudderLeft = (int)TentacleState.HotInputs.RudderLeft;
            this.CurrentTentacleState.RaiseTentacleFlag_Request(RudderLeft);
        }

        public void LaunchTentacle_Please(Vector2 SwipeDirectionVector, float SwipeAngle_Unity)
        {
            //Save requested swipe (linear intepolation of swipes over time, 
            //  with angles in RB2D.rotation friendly range)
            TTMovePositionVelocityRequested = SwipeDirectionVector;
            TTMoveRotationAngleRequested = SwipeAngle_Unity;

            int LaunchTentFlagID = (int)TentacleState.HotInputs.LaunchSwipe;
            this.CurrentTentacleState.RaiseTentacleFlag_Request(LaunchTentFlagID);
        }


        public bool BarrelRoll_Please()
        {
            int barrelRollFlagID = (int)TentacleState.HotInputs.BarrelRoll;
            bool successfullyRaised =
            this.CurrentTentacleState.RaiseTentacleFlag_Request(barrelRollFlagID);
            return successfullyRaised;
        }

        // when the tentacle collides with any 2D trigger, poll the attached game objects tag and do stuff
        void OnTriggerEnter2D(Collider2D col)
        {
            Debug.Log("HIT: " + col.gameObject.name + " : " + this.gameObject.name + " : " + Time.time);

            // Handle Collision logic inside current tentacle state instance
            this.CurrentTentacleState.HandleCollisionByTag(col.tag, col.attachedRigidbody);

        }


        public void ResetTentacleTipRotation()
        {
            TentacleTipRB2D.rotation = StartTentacleRotation;
        }

        private void ResetTentacleTipSprite()
        {
            //reset tentacle tip sprite to starting sprite; reference set in the Start() method
            TTSpriteRenderer.sprite = TTSceneSprite;
            // default sprite color in Unity is white, undoing black burnt sprite from being zapped.
            TTSpriteRenderer.color = Color.white;
        }

        // Monobehavior reset when component is first dropped into scene, set default editor fields here
        void Reset()
        {
            // Sets this value in editor when component is reset, or if the scene is renamed etc. 
            //      Otherwise, the default value is zero
            UPswipeSpeedConstant = 5;
            UPswipeSpeedModifier = -2;
            maxTentacleLength = 0;
            EatingZoneOffsetFromStart = 2.5f;
            BarrelRollDegreestoRotatePerUpdate = 20.0f;
            BarrelRollEndSpinRotationDegrees = 720;
            TimesCanBarrelRoll = 2;
            TTJukePosLeftAmount = 1;
            TTJukePosRightAmount = 1;
            TTJumpSpeed = 10.0f;
            TTTimesAllowedToJuke = 3;
        }

    }
}