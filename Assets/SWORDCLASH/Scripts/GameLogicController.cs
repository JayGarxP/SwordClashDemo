using System.Collections;
using UnityEngine;

namespace SwordClash
{

    // SwordClash Game Logic Controller; needs some way to talk to Nanogames menu scene
    public class GameLogicController : Bolt.EntityEventListener<ISwordClashGameWorldState>
    {
        public short NumberofRounds;
        //public static int PlayerOneScoreUIVal = 0;
        //public Text PlayerOneScoreUIText;
        //public Text WinnerPopUpText;

        private GameObject UIManager;
        private SwordClashUIManager SCUIManager;

        // Food tentacles fight over
        [SerializeField]
        private GameObject Snack;
        private SwordClashFoodController SCFoodController;

        // Center of camera game screen in world units
        private Vector3 CenterCameraCoord;
        public static Vector3 HardCodedCenterCameraCoord;

        //private short PlayerOnePoints;
        //private short PlayerTwoPoints;
        private bool FoodSpawned;
        private short PointsToWin;

        //// Start is called before the first frame update
        //void Start()
        //{
        //}

        // Bolt network UnityEngine.Start()
        public override void Attached()
        {
            // Best outta 3 rounds means whoever hits 2 wins is the big match winner.
            PointsToWin = (short)((NumberofRounds / 2) + 1);

            CenterCameraCoord = Camera.main.ScreenToWorldPoint(new Vector2((Screen.width / 2),
                (Screen.height / 2)));

            // Zero out z coordinate, for some reason setting it to zero in ScreenToWorldPoint results in -10 for z...
            CenterCameraCoord.z = 0;

            HardCodedCenterCameraCoord = CenterCameraCoord;

            state.P1Score = 0;
            state.P2Score = 0;
            FoodSpawned = false;


            // Spawn in snack just off screen.
            if (this.entity.isOwner)
            {
                Snack = BoltNetwork.Instantiate(Snack, CenterCameraCoord * 10.0f, Quaternion.identity);
            }



            if (UIManager == null)
            {
                UIManager = GameObject.FindWithTag("UIMan");

                if (UIManager != null && UIManager.tag != "/")
                {
                    // Get an instance of SwordClashUIManager script attached to the UIManager Gameobject
                    SCUIManager = UIManager.GetComponent<SwordClashUIManager>();
                    if (SCUIManager == null)
                    {
                        // Did you forget to put a UIManager game object in the scene?
                    }
                }

            }

        }

        // Update is called once per frame
        void Update()
        {
            // Need to set local references too, not just on server????
            if (SCFoodController == null)
            {
                SCFoodController = Snack.GetComponent<SwordClashFoodController>();

                NextRoundFoodInCenter();
            }


            // In each player's game local update loop, check the state of shared/synced network variables
            //      to have specific behavior.
            if (state.P1Score >= PointsToWin)
            {
                // Player one wins!!!
                if (SCUIManager != null)
                {
                    SCUIManager.UpdateWinnerPopupMessage("Player1");
                }

                //TODO: remove wait, set speed to zero, ignore inputs and popup pause menu
                StartCoroutine(WaitUnityCoroutine());

            }
            else if (state.P2Score >= PointsToWin)
            {
                // Player two wins...
                if (SCUIManager != null)
                {
                    SCUIManager.UpdateWinnerPopupMessage("Player Two");
                }

                StartCoroutine(WaitUnityCoroutine());

            }

            if (SCUIManager != null)
            {
                SCUIManager.UpdatePlayerOneScore(this.state.P1Score.ToString());
                SCUIManager.UpdatePlayerTwoScore(this.state.P2Score.ToString());

            }
        }

        // BoltNetwork Update()
        //  The computer which called BoltNetwork.Instantiate will always be considered the 'Owner'
        // See https://doc.photonengine.com/en-us/bolt/current/reference/glossary
        // SimulateController executes one time per frame.
        public override void SimulateOwner()
        {
            if (SCFoodController == null)
            {
                SCFoodController = Snack.GetComponent<SwordClashFoodController>();

                NextRoundFoodInCenter();
                SCFoodController.MoveFoodToCenterSameSprite(CenterCameraCoord);
            }
            else if (FoodSpawned == false)
            {
                NextRoundFoodInCenter();
                SCFoodController.MoveFoodToCenterSameSprite(CenterCameraCoord);

            }
        }

        // Monobehavior reset when component is first dropped into scene, set default editor fields here
        void Reset()
        {
            NumberofRounds = 3;
        }

        // For inbetween rounds
        private void NextRoundFoodInCenter()
        {
            // Use SC controller methods to assign sprite and move food to center
            if (SCFoodController != null)
            {
                SCFoodController.MoveFoodToCenter(CenterCameraCoord);
                FoodSpawned = true;
                SCFoodController.MoveFoodToCenterSameSprite(CenterCameraCoord);

            }
            else
            {
                SCFoodController = Snack.GetComponent<SwordClashFoodController>();
                Debug.Log("Chris SCFoodController was null somehow...");
                SCFoodController.MoveFoodToCenter(CenterCameraCoord);
                FoodSpawned = true;
                SCFoodController.MoveFoodToCenterSameSprite(CenterCameraCoord);

            }

        }



        public void OnFoodEaten(string EaterPlayerID)
        {
            // Spawn in new food
            NextRoundFoodInCenter();
            SCFoodController.MoveFoodToCenterSameSprite(CenterCameraCoord);


            if (EaterPlayerID == "Player1")
            {
                this.state.P1Score += 1;
            }
            else if (EaterPlayerID == "Player2")
            {
                this.state.P2Score += 1;

            }
            else
            {
                // Bad playerID ...
            }

        }


        // Clear Winner PopUpText
        IEnumerator WaitUnityCoroutine()
        {
            //print(Time.time);
            yield return new WaitForSeconds(2);
            //print(Time.time);

            // reset points to zero
            this.state.P1Score = 0;
            this.state.P2Score = 0;
            //FoodSpawned = false; // causes flicker bug
            //NextRoundFoodInCenter();

            // reset all UI elements 
            if (SCUIManager != null)
            {
                SCUIManager.ClearWinnerPopupMessage();
                SCUIManager.UpdatePlayerOneScore("");
                SCUIManager.UpdatePlayerTwoScore("");
            }
        }








    }
}