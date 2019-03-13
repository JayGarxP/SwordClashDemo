using Bolt;
using com.ootii.Utilities.Debug;
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
        // Placeholder instantiated if player2 so local scripts can use tag lookup more easily.
        public GameObject P2Pog;
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

        private GameObject GameWorld;
        // Reference to game logic controller in scene
        private GameLogicController GLCInstance;

        // Reference to OTHER player's tentacle
        //private GameObject OpponentTentacleGO;
        public TentacleController OpponentTCInstance;

        //private GameObject PlayerControllerGO;
        // private PlayerController PlayerControllerScriptInstance;
        private bool AmIPlayerTwo;
        public short WhichPlayerIBe; // 0 is NOBODY;  1 is p1;   2 is p2
        public bool HoldingFoodRightNow;

        private Command LatestStateChangeCommand;

        // Setup the component you are on right now (the "this" object); before all Start()s
        void Awake()
        {
            //MovePositionVelocity_TT_Active = Vector2.zero;
            StartTentacleLength = 0;
            // AmIPlayerTwo = null;
            WhichPlayerIBe = 0;

        }

        // BoltNetwork Start()
        public override void Attached()
        {
            TentacleTipRB2D = TentacleTip.GetComponent<Rigidbody2D>();
            TentacleReadyPosition = TentacleTipRB2D.position;
            TentacleEatingPosition = new Vector2(TentacleReadyPosition.x, TentacleReadyPosition.y - EatingZoneOffsetFromStart);
            StartTentacleLength = TentacleReadyPosition.magnitude;
            maxTentacleLength = StartTentacleLength * 2; //TODO: fix maxtentacleLength solution
            StartTentacleRotation = TentacleTipRB2D.rotation;
            WhichPlayerIBe = 0;
            HoldingFoodRightNow = false;

            // Set sprite renderer reference so tentacle can change color
            TTSpriteRenderer = GetComponent<SpriteRenderer>();
            TTSceneSprite = TTSpriteRenderer.sprite;


            // Redundant cast seems to help avoid null reference in update loop
            this.CurrentTentacleState = new CoiledState(((TentacleController)this));
            this.state.CurrentStateString = "Coiled";
            Debug.Log("Chris Current State string: " + this.state.CurrentStateString);

            // Bolt Entity Transform sync; entire point of using Photon Bolt :)
            this.state.SetTransforms(this.state.TTTransform, this.transform);

            //TODO: P2 Proxy on Server Side is NOT being set to AmIPlayerTwo = true; correctly...
            if (BoltNetwork.IsClient)
            {
                if (this.entity.isOwner == false && this.entity.hasControl == true)
                {
                    Debug.Log("Chris Hi I am player2, " + this.entity.networkId);

                    AmIPlayerTwo = true;
                    WhichPlayerIBe = 2;
                    state.AmIPlayer2 = true;

                    // Instantiate P2Pog with tag "Player2" ONLY if player 2
                    Instantiate(P2Pog);
                    // P2Pog is used in PlayerController to know if the local scene is supposed to be Player2.

                    PleaseMakeMePlayerTwo();
                }
            }

            if (BoltNetwork.IsServer)
            {
                if (this.entity.isOwner)
                {
                    //Debug.Log("@@@@@@@@@@@@@@@Hi I am player1111, " + this.entity.networkId.ToString());
                    this.entity.TakeControl(); //Why do I need to do this?!?!?!
                }

                // If I am running on server machine
                if (this.entity.isOwner && this.entity.hasControl)
                {
                    Debug.Log("Hi I am player1111, " + this.entity.networkId.ToString());

                    AmIPlayerTwo = false;
                    WhichPlayerIBe = 1;
                    state.AmIPlayer2 = false;

                }

            }


        }



        // BoltNetwork Update()
        // is used to collect inputs from your game and putting it into a Command. 
        // SimulateController executes one time per frame.
        public override void SimulateController()
        {
            if (GLCInstance != null)
            {
                ////ACTUAL GAME LOOP:

                if (this.CurrentTentacleState == null)
                {
                    // Why is P2 starting in Projectile State?!?!
                    this.CurrentTentacleState = new CoiledState(this);
                    this.state.CurrentStateString = "Coiled";
                }

                // IrigidbodyPlayerCommandInput input = rigidbodyPlayerCommand.Create();
                ITentacleInputCommandInput input = TentacleInputCommand.Create();

                if (this.CurrentTentacleState != null && GLCInstance != null)
                {
                    // Actual game loop logic code, contained in each ProcessState() method of concrete TentacleState s 
                    // change values of input
                    //Debug.Log("Chris Calling ProcessState on: " + gameObject.GetInstanceID().ToString());
                    this.CurrentTentacleState.ProcessState(input);
                }

                // BoltNetwork.isClient could work too?
                if (AmIPlayerTwo && BoltNetwork.IsClient)
                {
                    input.CommandFromP2 = true;
                }

                this.entity.QueueInput(input);

            }


        }

        // Bolt method called after SimulateController() sends commands based on Queued inputs.
        // Execute command is called on Owner (P1) and Controller (p1 & P2)
        // But IsServer check ensures this is Server Authoritative....
        public override void ExecuteCommand(Command command, bool resetState)
        {
            TentacleInputCommand cmd = (TentacleInputCommand)command;

            if (BoltNetwork.IsServer)
            {
                if (cmd.Input.CommandFromP2)
                {
                    OpponentTCInstance.CurrentTentacleState.ProcessCommand(cmd);
                }
                else
                {
                    this.CurrentTentacleState.ProcessCommand(cmd);
                }
            }



        }


        //// Why only taps work??? Swipes are not working???
        //// https://forum.photonengine.com/discussion/8085/do-you-only-have-movement-code-in-executecommand
        //public override void MissingCommand(Bolt.Command previous)
        //{
        //    if (previous == null) { return; }
        //    ExecuteCommand(previous, true);
        //}



        // Update is called once per frame
        void Update()
        {
                if (this.CurrentTentacleState != null)
                {
                    Log.ScreenWriteTop("l: " + this.CurrentTentacleState.StringRep);
                    Log.ScreenWriteTop("Bolt:       " + state.CurrentStateString);

                if (BoltNetwork.IsClient)
                {
                    // p1 proxy on client is not synced because processstate() is only called by controller!!!
                    if (! entity.hasControl)
                    {
                        if (state.CurrentStateString == "Recovery")
                        {
                            PleaseDarkenTentacleSprite();
                        }
                        else if (state.CurrentStateString == "Coiled")
                        {
                            ResetTentacleTipSprite();
                        }
                    }
                }

                }
            

            //Find reference to other tentacle, if I am player one, change the local p2 sprite to orange
            if (OpponentTCInstance == null)
            {
                var TentaControllaGOs = GameObject.FindGameObjectsWithTag("TentacleTip");

                if (TentaControllaGOs.Length == 2)
                {
                    // Attempt to change the sprite to player 2 orange, on server's local game instance.
                    foreach (var TCGO in TentaControllaGOs)
                    {
                        // Get the tentacle controller script instance, if it isn't this one
                        //if (TCGO.GetInstanceID() != gameObject.GetInstanceID())

                        OpponentTCInstance = TCGO.GetComponent<TentacleController>();

                        // if found something AND they do not reference same game object
                        if (OpponentTCInstance != null)
                        {

                            // If the other tentacle is in scene is NOT controlled by this TentacleController 'controller' human
                            if (OpponentTCInstance.entity.hasControl == false)
                            {
                                // Trying to make this only run on server...
                                if (this.entity.isOwner && this.entity.hasControl)
                                {
                                    // Change local server sprite to player 2 orange.
                                    OpponentTCInstance.TTChangeTentacleSpritetoPlayerTwo();
                                    OpponentTCInstance.ResetPlayer2EatingZone();
                                }
                            }

                        }
                    }


                }
            }

            if (GLCInstance == null)
            {
                GameWorld = GameObject.FindWithTag("GameWorld");

                if (GameWorld != null && GameWorld.tag != "/")
                {
                    GLCInstance = GameWorld.GetComponent<GameLogicController>();
                    if (GLCInstance == null)
                    {
                        // bad thing happen; GLCInstance is still Null somehow
                        Debug.Log("Chris this GLCINstance is still null somehow plz help plz help");
                    }
                }

            }


        }

        public string SetBoltTentaStateString(string id)
        {
            this.state.CurrentStateString = id;
            return this.state.CurrentStateString;
        }

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
            if (AmIPlayerTwo)
            {
                return maxTentacleLength >= TentacleTipRB2D.position.magnitude;

            }
            else
            {
                return TentacleTipRB2D.position.magnitude >= maxTentacleLength;
            }

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
        public Vector2 TT_CalculateEndJumpPosition(bool YesJumpingLeft)
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

        // pass in true to jump left, false to get a jump right end position vector as return value
        public Vector2 TT_P2CalculateEndJumpPosition(bool YesJumpingLeft)
        {
            Vector2 whereToJumpTo;

            if (YesJumpingLeft)
            {
                whereToJumpTo = new Vector2(OpponentTCInstance.TentacleTipRB2D.position.x - TTJukePosLeftAmount, OpponentTCInstance.TentacleTipRB2D.position.y);

            }
            else
            {
                // jumping right
                whereToJumpTo = new Vector2(OpponentTCInstance.TentacleTipRB2D.position.x + TTJukePosRightAmount, OpponentTCInstance.TentacleTipRB2D.position.y);
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


        public bool TT_P2JumpSideways(Vector2 endOfJumpPosition)
        {
            //Vector2 whereToJumpTo = new Vector2(TentacleTipRB2D.position.x - TTJukePosLeftAmount, TentacleTipRB2D.position.y);

            TentacleTipP2_JumpSideways(endOfJumpPosition);

            bool HaventReachedEndJumpPosition = true;

            if (OpponentTCInstance.TentacleTipRB2D.position == endOfJumpPosition)
            {
                HaventReachedEndJumpPosition = false;
            }

            return HaventReachedEndJumpPosition;
        }
        private void TentacleTipP2_JumpSideways(Vector2 whereToJumpTo)
        {

            OpponentTCInstance.TentacleTipRB2D.position = Vector2.MoveTowards(OpponentTCInstance.TentacleTipRB2D.position, whereToJumpTo, Time.fixedDeltaTime * TTJumpSpeed);

        }

        public void UpdateLatestStateChangeCommand(Command cmd)
        {
            // Save command in Tentaclecontroller
            LatestStateChangeCommand = cmd;

        }

        // Called on Client to keep TentacleState in sync with Server-side P2
        public void SyncTentacleStateWithClient(Command cmd)
        {
            if (this.CurrentTentacleState.StringRep != this.state.CurrentStateString)
            {
                Debug.Log("Chris StringRep " + this.CurrentTentacleState.StringRep + "does not equal state.Current   "
                    + this.state.CurrentStateString);
            }

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

        // Make me player 2two2
        public void PleaseMakeMePlayerTwo()
        {
            // TODO: how keep these bools in sync? Boltnetwork.State???
            AmIPlayerTwo = true;
            this.CurrentTentacleState.AmIPlayerTwo = true;

            TTChangeTentacleSpritetoPlayerTwo();
            ResetPlayer2EatingZone();
        }

        public void ResetPlayer2EatingZone()
        {
            TentacleEatingPosition = new Vector2(TentacleReadyPosition.x, TentacleReadyPosition.y + EatingZoneOffsetFromStart);
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

        public void TTEatFood(string PlayerID)
        {
            if (GLCInstance != null)
            {
                GLCInstance.OnFoodEaten(PlayerID);
            }
            else
            {
                Debug.Log("GLCInstance null in TTEatFood( " + PlayerID + " )");
            }
        }

        public void TTMoveTowardsEatingZone(Rigidbody2D moveMeAsWell)
        {
            //// Move towards starting position each frame
            //TentacleTipRB2D.position = Vector2.MoveTowards(TentacleTipRB2D.position, TentacleEatingPosition, Time.fixedDeltaTime);
            //moveMeAsWell.position = Vector2.MoveTowards(moveMeAsWell.position, TentacleEatingPosition, Time.fixedDeltaTime);

            // Move towards starting position each frame
            TentacleTipRB2D.position = Vector2.MoveTowards(TentacleTipRB2D.position, TentacleEatingPosition, Time.fixedDeltaTime / 2.0f);
            moveMeAsWell.position = Vector2.MoveTowards(moveMeAsWell.position, TentacleEatingPosition, Time.fixedDeltaTime / 2.0f);


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


        public bool IsOtherPlayerJuking()
        {
            // Is this legal in Canada???
            var SheSharp = OpponentTCInstance.CurrentTentacleState as CoiledState;
            return SheSharp.CurrentlyJuking;
        }

        public float GetOtherPlayerRequestedLaunchAngle()
        {
            return OpponentTCInstance.TTMoveRotationAngleRequested;
        }


        public int LowerOtherPlayerInputFlag(int whichFlagToLower)
        {
            bool YESflagLowered = OpponentTCInstance.CurrentTentacleState.LowerTentacleFlag_Request(whichFlagToLower);

            int whichFlagWasLowered = -1;

            if (YESflagLowered)
            {
                whichFlagWasLowered = whichFlagToLower;
            }

            return whichFlagWasLowered;
        }

        public void ChangeOpponentState(TentacleState TTS)
        {
            // may neeed to use new here not sure?????
            OpponentTCInstance.CurrentTentacleState = TTS;
        }

        public void ChangeMyStateToCoiled()
        {
            this.CurrentTentacleState = new CoiledState(this);
            this.state.CurrentStateString = "Coiled";
        }


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

            Debug.Log("Chris BarrelRoll_Please() called in TC: " + successfullyRaised.ToString());

            return successfullyRaised;
        }

        // when the tentacle collides with any 2D trigger, poll the attached game objects tag and do stuff
        void OnTriggerEnter2D(Collider2D col)
        {
            Debug.Log("HIT: " + col.gameObject.name + " : " + this.gameObject.name + " : " + Time.time);

            // Handle Collision logic inside current tentacle state instance
            this.CurrentTentacleState.HandleCollisionByTag(col.tag, col.attachedRigidbody);

        }

        public bool IsThisTentaclePlayer2()
        {
            return AmIPlayerTwo;
        }

        public void ResetTentacleTipRotation()
        {
            TentacleTipRB2D.rotation = StartTentacleRotation;
        }

        public void ResetTentacleTipSprite()
        {

            if (AmIPlayerTwo)
            {
                TTChangeTentacleSpritetoPlayerTwo();
                //Debug.Log("Chris THE THING I LOVE THE MOST LIVES IN A DEMON HOST");
            }
            else
            {
                //reset tentacle tip sprite to starting sprite; reference set in the Start() method
                TTSpriteRenderer.sprite = TTSceneSprite;
            }

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