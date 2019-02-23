using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SwordClash
{

    // SwordClash Game Logic Controller; needs some way to talk to Nanogames menu scene
    public class GameLogicController : Bolt.EntityEventListener<ISwordClashGameWorldState>
    {
        public short NumberofRounds;
        public static int PlayerOneScoreUIVal = 0;
        public Text PlayerOneScoreUIText;
        public Text WinnerPopUpText;

        // Food tentacles fight over
        [SerializeField]
        private GameObject Snack;
        private SwordClashFoodController SCFoodController;

        // Center of camera game screen in world units
        private Vector3 CenterCameraCoord;

        private short PlayerOnePoints;
        private short PlayerTwoPoints;

        //// Start is called before the first frame update
        //void Start()
        //{
        //}

        // Bolt network UnityEngine.Start()
        public override void Attached()
        {
            CenterCameraCoord = Camera.main.ScreenToWorldPoint(new Vector2((Screen.width / 2),
                (Screen.height / 2)));

            // Zero out z coordinate, for some reason setting it to zero in ScreenToWorldPoint results in -10 for z...
            CenterCameraCoord.z = 0;

            PlayerOnePoints = 0;
            PlayerTwoPoints = 0;

            // Spawn in snack just off screen.
            Snack = BoltNetwork.Instantiate(Snack, CenterCameraCoord * 10.0f, Quaternion.identity);
            SCFoodController = Snack.GetComponent<SwordClashFoodController>();

            NextRoundFoodInCenter();

            //TODO: programmatic UI spawner here
            // Spawn in UI from prefabs
            // WinnerPopUpText = Instantiate();
            // WinnerPopUpText.text = "";
            // PlayerOneScoreUIText.text = "P1 Score: " + PlayerOneScoreUIVal;



        }

        //// Update is called once per frame
        //void Update()
        //{

        //}

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

            }
        }



        public void OnFoodEaten(string EaterPlayerID)
        {
            if (EaterPlayerID == "Player1")
            {
                PlayerOnePoints += 1;
                PlayerOneScoreUIVal = PlayerOnePoints;
                PlayerOneScoreUIText.text = "P1 Score: " + PlayerOneScoreUIVal;

            }
            else if (EaterPlayerID == "Player2")
            {
                PlayerTwoPoints += 1;
                // no p2 yet
            }
            else
            {
                // Bad playerID ...
            }

            // Best outta 3 rounds means whoever hits 2 wins is the big match winner.
            short pointsToWin = (short)((NumberofRounds / 2) + 1);

            if (PlayerOnePoints >= pointsToWin)
            {
                // Player one wins!!!
                WinnerPopUpText.text = EaterPlayerID + "  Wins!!!";
                //TODO: remove wait, set speed to zero, ignore inputs and popup pause menu
                StartCoroutine(WaitUnityCoroutine());


                PlayerOnePoints = 0;
                PlayerTwoPoints = 0;
                PlayerOneScoreUIVal = 0;
                PlayerOneScoreUIText.text = "P1 Score: " + PlayerOneScoreUIVal;

            }
            else if (PlayerTwoPoints >= pointsToWin)
            {
                // Player two wins...
            }

            // Spawn in new food
            NextRoundFoodInCenter();
        }


        // Clear Winner PopUpText
        IEnumerator WaitUnityCoroutine()
        {
            print(Time.time);
            yield return new WaitForSeconds(3);
            print(Time.time);
            WinnerPopUpText.text = "";
        }








    }
}