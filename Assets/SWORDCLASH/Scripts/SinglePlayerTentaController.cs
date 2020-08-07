using System;
using UnityEngine;

namespace SwordClash
{
    public class SinglePlayerTentaController : MonoBehaviour
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
        // degrees to rotate TT per fixed update
        public float BackFlipDegreestoRotatePerUpdate;

        // snowboard style degrees of rotation until barrel roll ends, two rotations = 720.
        public float BarrelRollEndSpinRotationDegrees;
        // 2 times means each up-swipe launch player gets two barrel rolls, reset once coiled
        public short TimesCanBarrelRoll;

        // RB2D.position units to teleport right--> by, 1 is default
        public float TTJukePosRightAmount;
        // RB2D.position units to teleport left<-- by, default is 1
        public float TTJukePosLeftAmount;
        // How fast to Juke, multiplied by fixedDeltaTime
        public float TTJumpSpeed;

        //3 means 3 taps can happen in one 'strike' or projectile state
        public float TTTimesAllowedToJuke;


        public float TTJukeTravelTime;
        public float TTClashTravelTime;

        // Sprite to change TentacleTip sprite into after colliding with jellyfish
        public Sprite TTStungSprite;

        public Sprite TTPlayerTwoSprite;


        [Tooltip("how long BackFlip takes in seconds")]
        public float BackFlipTime;
        public float ScaleSpriteDelta;
        [Tooltip("How far TT tilts during backflip")]
        public float SnootDroopAmount;
        public float WaterDepthPerUpdate;
        public float DroopSpeed;


        #endregion

        // Gang of Four State pattern, state machine for inputs allowed during tentacle movement
        public TentacleState CurrentTentacleState { get; set; }
        private DigitalRubyShared.GestureRecognizer DrawSwipeBroadcaster;
        private JellyfishController JelFishController;

        // for training mode 'ghosts', possibly save last input swipe?
        public Vector2 TTMovePositionVelocityRequested;
        public float TTMoveRotationAngleRequested;

        public Vector2 TTBackFlipNormalDirRequested;

        internal Vector3 GetDefaultLocalScale()
        {
            return this.transform.localScale;
        }

        public void SetLocalScale(Vector3 newScale)
        {
            this.transform.localScale = newScale;
        }

        public float TTBackFlipAngleRequested;

        private Rigidbody2D TentacleTipRB2D;
        private float StartTentacleLength;
        private Vector2 TentacleReadyPosition;
        // Where food is brought to die
        private Vector2 TentacleEatingPosition;
        private float StartTentacleRotation;
        private SpriteRenderer TTSpriteRenderer;
        private Sprite TTSceneSprite; //sprite object starts with
        private Material TTSpriteMaterial;

        private GameObject GameWorld;
        // Reference to game logic controller in scene
        //private GameLogicController GLCInstance;
        private SinglePlayerLogicController GLCInstance;


        public bool HoldingFoodRightNow;

        // Setup the component you are on right now (the "this" object); before all Start()s
        void Awake()
        {
            StartTentacleLength = 0;
        }

        void Start()
        {
            TentacleTipRB2D = TentacleTip.GetComponent<Rigidbody2D>();
            TentacleReadyPosition = TentacleTipRB2D.position;
            TentacleEatingPosition = new Vector2(TentacleReadyPosition.x, TentacleReadyPosition.y - EatingZoneOffsetFromStart);
            StartTentacleLength = TentacleReadyPosition.magnitude;
            maxTentacleLength = StartTentacleLength * 2; //TODO: fix maxtentacleLength solution
            StartTentacleRotation = TentacleTipRB2D.rotation;
            HoldingFoodRightNow = false;

            // Set sprite renderer reference so tentacle can change color
            TTSpriteRenderer = GetComponent<SpriteRenderer>();
            TTSceneSprite = TTSpriteRenderer.sprite;
            TTSpriteMaterial = TTSpriteRenderer.material;


            //// Redundant cast seems to help avoid null reference in update loop
            //this.CurrentTentacleState = new CoiledState(((TentacleController)this));
            this.CurrentTentacleState = new CoiledState(this);
        }

        // Update is called once per frame
        void Update()
        {
            if (GLCInstance == null)
            {
                GameWorld = GameObject.FindWithTag("GameWorld");

                if (GameWorld != null && GameWorld.tag != "/")
                {
                    GLCInstance = GameWorld.GetComponent<SinglePlayerLogicController>();
                    if (GLCInstance == null)
                    {
                        // bad thing happen; GLCInstance is still Null somehow
                        Debug.Log("Chris this GLCINstance is still null somehow plz help plz help");
                    }
                }

            }
            else
            {
                ////ACTUAL GAME LOOP:

                if (this.CurrentTentacleState == null)
                {
                    this.CurrentTentacleState = new CoiledState(this);
                }

                //ITentacleInputCommandInput input = TentacleInputCommand.Create();

                if (this.CurrentTentacleState != null && GLCInstance != null)
                {
                    // Actual game loop logic code, contained in each ProcessState() method of concrete TentacleState s 
                    this.CurrentTentacleState.ProcessState();
                }

            }


        }

        public void TT_WaterDepth_WhileBackFlip(float waterDepth)
        {
            TTSpriteMaterial.SetFloat("_WaterDepth", waterDepth);
        }

        public void TT_MoveTentacleTip(Vector2 swipePositionVelocity, float swipeAngle)
        {
            //be careful! if physics update is not finished and you MovePosition() in same update frame, unexpected behavior will occur!
            //Position = current position + (Velocity vector of swipe per physics frame) 
            TentacleTipRB2D.MovePosition(TentacleTipRB2D.position + swipePositionVelocity * Time.fixedDeltaTime);
            //Set in PlayerController, updated here, consider adding if(bool angleSet), here it doesn't need to change, not sure which is faster...
            TentacleTipRB2D.rotation = swipeAngle;
            // TODO: consider seperataitng rotation from position in movement
        }

        public void TT_MoveTentacleTipAtSpeed(Vector2 swipePositionVelocity, float swipeAngle, float speedMultiplier)
        {
            TentacleTipRB2D.MovePosition(TentacleTipRB2D.position + swipePositionVelocity * Time.fixedDeltaTime * speedMultiplier);
            TentacleTipRB2D.rotation = swipeAngle;
        }

        public void TT_MoveTentacleTip_WhileBroll(Vector2 swipePositionVelocity)
        {
            //Move at half delta time speed ~around sqrt the normal speed. Do not rotate, rotate seperately in another method.
            TentacleTipRB2D.MovePosition(TentacleTipRB2D.position + (swipePositionVelocity * (Time.fixedDeltaTime * 0.5f)));
        }

        public void TT_MoveTentacleTip_WhileBackFlip(Vector2 swipeVelocity)
        {
            TentacleTipRB2D.MovePosition(TentacleTipRB2D.position + (swipeVelocity * (Time.fixedDeltaTime * 0.1f)));
        }



        public bool IsTentacleAtMaxExtension()
        {

            return TentacleTipRB2D.position.magnitude >= maxTentacleLength;
        }

        public Vector2 TT_GetPosition()
        {
            return TentacleTipRB2D.position;
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

        // Quaternion.Slerp on Tentacle instance GameObject Transform Rotation
        public void BackFlippin_rotate(Quaternion targetRotation, float deltaTime)
        {
            //TentacleTip.transform.Rotate(0, 0, rotationAmountPerUpdate, Space.World);
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, deltaTime);
        }

        // https://docs.unity3d.com/2018.2/Documentation/ScriptReference/Transform.Rotate.html
        // public void Rotate(Vector3 axis, float angle, Space relativeTo = Space.Self); 
        // TIP: Avoid multiple calls to transform, do them sequentially, do not try to combine them in one complicated rotation either.
        public void TT_Tilt(float maxAngle, float deltatimeMultiplier)
        {
            // Vector3.right == x axis == transform.localEulerAngles.x    with LOCAL coordinates
            // Lerp from current local x axis to maxAngle (~45 - 70 degrees) to tilt downward/upward.
            Vector3 euler = transform.localEulerAngles;
            euler.x = Mathf.LerpAngle(euler.x, maxAngle, deltatimeMultiplier * Time.deltaTime);
            transform.localEulerAngles = euler;

        }

        // pass in true to jump left, false to get a jump right end position vector as return value
        public Vector2 TT_CalculateEndJumpPosition(bool YesJumpingLeft)
        {
            Vector2 whereToJumpTo;

            if (YesJumpingLeft == true)
            {
                whereToJumpTo = new Vector2(TentacleTipRB2D.position.x - TTJukePosLeftAmount, TentacleTipRB2D.position.y);

            }
            else
            {
                // jumping right
                whereToJumpTo = new Vector2(TentacleTipRB2D.position.x + TTJukePosRightAmount, TentacleTipRB2D.position.y);
                Debug.Log("MARIA jumping right EndJumpPos ");
            }

            return whereToJumpTo;
        }


        public bool TT_MoveSideWays_TillEnd(Vector2 endOfJumpPosition, Vector2 direction)
        {

            TentacleTip_MoveSideways(direction);

            //bool CancelJump = false;

            // TRUE means the endOfJumpPosition has been reached
            return (TentacleTipRB2D.position == endOfJumpPosition);

        }

        private void TentacleTip_MoveSideways(Vector2 velocity)
        {
            // Moveposition moves gradually and does trigger discrete collisions
            TentacleTipRB2D.MovePosition(TentacleTipRB2D.position + velocity * Time.fixedDeltaTime * TTJumpSpeed);
        }

        private void TentacleTip_JumpSideways(Vector2 whereToJumpTo)
        {
            // RB2D.position means teleport, which means a discrete collision can be skipped over...
            TentacleTipRB2D.position = Vector2.MoveTowards(TentacleTipRB2D.position, whereToJumpTo, Time.fixedDeltaTime * TTJumpSpeed);

        }

        // TODO: put magic numbers as editor fields.
        public void TT_ClashWobble(Vector2 StartPosition, float Scalar)
        {

            TentacleTipRB2D.position = Vector2.MoveTowards(TentacleTipRB2D.position,
                new Vector2(TentacleTipRB2D.position.x + Mathf.Sin(Time.time * 8.0f) * Scalar, StartPosition.y),
                Time.fixedDeltaTime * TTJumpSpeed);

        }


        public bool TT_HitPosition(Vector2 Position)
        {
            return (TentacleTipRB2D.position == Position);

        }

        // callback method to drag tenta here



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

        public void TT_ReelBackToStartPosition()
        {
            // Higher floating point scalar multiplier means faster MoveTowards()
            float step = 0.98f * Time.fixedDeltaTime;
            // Move towards start position, slowly.
            TentacleTipRB2D.position = Vector2.MoveTowards(TentacleTipRB2D.position, TentacleReadyPosition, step);

            float targetX = TentacleReadyPosition.x - TentacleTipRB2D.position.x;
            float targetY = TentacleReadyPosition.y - TentacleTipRB2D.position.y;

            // actually working pretty good! snaps right to correct position, but needs to be lerped over time to be more smooth.
            float angle = (Mathf.Atan2(targetY, targetX) * Mathf.Rad2Deg) + 90f;

            // TODO: copy gradual rotation method in BarrelRollin_rotate(degrees) OR find a suitable lerp / slerp to make the rotation smooth.

            TentacleTipRB2D.rotation = angle;
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
            // Move towards starting position each frame
            TentacleTipRB2D.position = Vector2.MoveTowards(TentacleTipRB2D.position, TentacleEatingPosition, Time.fixedDeltaTime);
            moveMeAsWell.position = Vector2.MoveTowards(moveMeAsWell.position, TentacleEatingPosition, Time.fixedDeltaTime);
        }

        public void TTTeleportPosition(Vector3 unityWorldCoordPosition)
        {
            TentacleTip.transform.position = unityWorldCoordPosition;
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

        public void ChangeMyStateToCoiled()
        {
            this.CurrentTentacleState = new CoiledState(this);
        }

        public void TT_ScaleTransform(float addToScale)
        {
            Vector3 originalTrans = TentacleTip.transform.localScale;
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

        public void PleaseRecoilTentacle()
        {
            int ReelBack = (int)TentacleState.HotInputs.BackFlip;
            this.CurrentTentacleState.RaiseTentacleFlag_Request(ReelBack);
        }
        public void TT_RecoilTentacle()
        {
            ResetTentacletoStartingPosition();
        }

        public void BackFlipTentacle_Please(Vector2 SwipeDirectionVector, float SwipeAngle_Unity)
        {
            TTBackFlipNormalDirRequested = SwipeDirectionVector;
            TTBackFlipAngleRequested = SwipeAngle_Unity;

            int BackFlipID = (int)TentacleState.HotInputs.BackFlip;
            this.CurrentTentacleState.RaiseTentacleFlag_Request(BackFlipID);
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

        public void ResetTentacleTipRotation()
        {
            TentacleTipRB2D.rotation = StartTentacleRotation;
        }

        public void ResetTentacleTipSprite()
        {
            //reset tentacle tip sprite to starting sprite; reference set in the Start() method
            TTSpriteRenderer.sprite = TTSceneSprite;


            // default sprite color in Unity is white, undoing black burnt sprite from being zapped.
            TTSpriteRenderer.color = Color.white;
        }

        public void SetDrawSwipeAbility(DigitalRubyShared.GestureRecognizer DrawSwipeBroadcaster)
        {
            this.DrawSwipeBroadcaster = DrawSwipeBroadcaster;
        }

        public void DisableDragSwipeGesture()
        {
            if (DrawSwipeBroadcaster != null)
            {
                DrawSwipeBroadcaster.Enabled = false;
            }
        }

        public void EnableDragSwipeGesture()
        {
            if (DrawSwipeBroadcaster != null)
            {
                DrawSwipeBroadcaster.Enabled = true;
            }
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
            BackFlipDegreestoRotatePerUpdate = 5.0f;
            BarrelRollEndSpinRotationDegrees = 720;
            TimesCanBarrelRoll = 2;
            TTJukePosLeftAmount = 1;
            TTJukePosRightAmount = 1;
            TTJumpSpeed = 10.0f;
            TTTimesAllowedToJuke = 3;
            TTJukeTravelTime = 0.7f;
            TTClashTravelTime = 0.4f;

            //backflip
            BackFlipTime = 1.0f;
            ScaleSpriteDelta = 0.004f;
            SnootDroopAmount = 65.0f;
            WaterDepthPerUpdate = 2.0f;
            DroopSpeed = 4.5f;

    }


    }
}