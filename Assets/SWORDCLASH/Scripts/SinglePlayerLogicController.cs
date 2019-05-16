using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SwordClash
{

    // Will make this AN INTERFACE later to have all GLC's implement it,
    // can't make it base class since BOLT objects must be inherited from Bolt.Object which 
    //       overrides stuff in Unity.MonoBehavior
    public class SinglePlayerLogicController : MonoBehaviour
    {
        public short NumberofRounds;

        // reference to our database manager object in the scene
        public SimpleSQL.SimpleSQLManager dbManager;

        // reference to the gui text object in our scene that will be used for output
        public Text outputText;

        private GameObject UIManager;
        private SwordClashUIManager SCUIManager;

        // Food tentacles fight over
        [SerializeField]
        private GameObject Snack;
        private SinglePlayerFoodController SCFoodController;

        // Center of camera game screen in world units
        private Vector3 CenterCameraCoord;
        public static Vector3 HardCodedCenterCameraCoord;

        private short PlayerOnePoints;
        private short PlayerTwoPoints;
        private bool FoodSpawned;
        private short PointsToWin;

        private const string PlayerStatsTableName = "Player";

        // Use this for initialization
        void Start()
        {
            // Best outta 3 rounds means whoever hits 2 wins is the big match winner.
            PointsToWin = (short)((NumberofRounds / 2) + 1);

            CenterCameraCoord = Camera.main.ScreenToWorldPoint(new Vector2((Screen.width / 2),
                (Screen.height / 2)));

            // Zero out z coordinate, for some reason setting it to zero in ScreenToWorldPoint results in -10 for z...
            CenterCameraCoord.z = 0;

            HardCodedCenterCameraCoord = CenterCameraCoord;

            PlayerOnePoints = 0;
            PlayerTwoPoints = 0;
            FoodSpawned = false;


            SpawnSnack();
            InsertPlayerTable(1);
            PrintPlayerTable();

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

        private void SpawnSnack()
        {
            if (Snack == null)
            {
                Snack = Instantiate(Snack, CenterCameraCoord * 10.0f, Quaternion.identity);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (SCFoodController == null)
            {
                if (Snack != null)
                {
                    SCFoodController = Snack.GetComponent<SinglePlayerFoodController>();

                    NextRoundFoodInCenter();
                    SCFoodController.MoveFoodToCenterSameSprite(CenterCameraCoord);
                    SCFoodController.MoveFoodToCenterSameSprite(CenterCameraCoord);
                    SCFoodController.MoveFoodToCenterSameSprite(CenterCameraCoord);
                }
                else
                {
                    SpawnSnack();
                }

            }
            else if (FoodSpawned == false)
            {
                NextRoundFoodInCenter();
                SCFoodController.MoveFoodToCenterSameSprite(CenterCameraCoord);
                SCFoodController.MoveFoodToCenterSameSprite(CenterCameraCoord);
                SCFoodController.MoveFoodToCenterSameSprite(CenterCameraCoord);

            }


            if (PlayerOnePoints >= PointsToWin)
            {
                // Player one wins!!!
                if (SCUIManager != null)
                {
                    SCUIManager.UpdateWinnerPopupMessage("Player1");
                }

                //TODO: remove wait, set speed to zero, ignore inputs and popup pause menu
                StartCoroutine(WaitUnityCoroutine());

            }
            else if (PlayerTwoPoints >= PointsToWin)
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
                SCUIManager.UpdatePlayerOneScore(PlayerOnePoints.ToString());
                //SCUIManager.UpdatePlayerTwoScore(PlayerTwoPoints.ToString());
                SCUIManager.UpdateSinglePlayerLevel(" One ");

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
                //SCFoodController.MoveFoodToCenterSameSprite(CenterCameraCoord);
                //SCFoodController.MoveFoodToCenterSameSprite(CenterCameraCoord);
                //SCFoodController.MoveFoodToCenterSameSprite(CenterCameraCoord);


            }
            else
            {
                SCFoodController = Snack.GetComponent<SinglePlayerFoodController>();
                Debug.Log("Chris SCFoodController was null somehow...");
                SCFoodController.MoveFoodToCenter(CenterCameraCoord);
                FoodSpawned = true;
                //SCFoodController.MoveFoodToCenterSameSprite(CenterCameraCoord);
                //SCFoodController.MoveFoodToCenterSameSprite(CenterCameraCoord);
                //SCFoodController.MoveFoodToCenterSameSprite(CenterCameraCoord);


            }

        }



        public void OnFoodEaten(string EaterPlayerID)
        {
            // Spawn in new food
            NextRoundFoodInCenter();
            //SCFoodController.MoveFoodToCenterSameSprite(CenterCameraCoord);
            //SCFoodController.MoveFoodToCenterSameSprite(CenterCameraCoord);
            // SCFoodController.MoveFoodToCenterSameSprite(CenterCameraCoord);



            if (EaterPlayerID == "Player1")
            {
                PlayerOnePoints += 1;
            }
            else if (EaterPlayerID == "Player2")
            {
                PlayerTwoPoints += 1;

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
            PlayerOnePoints = 0;
            PlayerTwoPoints = 0;

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

        void PrintPlayerTable()
        {
            // Gather a list of weapons and their type names pulled from the weapontype table		
            List<PlayerTable> weaponList = dbManager.Query<PlayerTable>(
                                                            "SELECT " +
                                                                "W.PlayerID, " +
                                                                "W.HighestLevelComplete, " +
                                                                "W.TotalDeaths, " +
                                                                "W.TotalWins, " +
                                                                "W.TotalGamesPlayed " +
                                                            "FROM " +
                                                                "Player W " +
                                                            "ORDER BY " +
                                                                "W.PlayerID "
                                                            );

            // output the list of weapons
            outputText.text = "PLAYERS: \n\n";
            foreach (PlayerTable weaponRecord in weaponList)
            {
                outputText.text += "<color=#1abc9c>ID</color>: '" + weaponRecord.PlayerID.ToString() + "' " +
                                    "<color=#1abc9c>LvlsBeat</color>:" + weaponRecord.HighestLevelComplete.ToString() + " " +
                                    "<color=#1abc9c>Deaths</color>:" + weaponRecord.TotalDeaths.ToString() + "\n" +
                                    "<color=#1abc9c>Wins</color>:" + weaponRecord.TotalWins.ToString() + " " +
                                    "<color=#1abc9c>GamesPlayed</color>:" + weaponRecord.TotalGamesPlayed.ToString() + "\n";
            }


            //// get the first weapon record that has a WeaponID > 4
            //outputText.text += "\nFirst weapon record where the WeaponID > 4: ";
            //bool recordExists;
            //Weapon firstWeaponRecord = dbManager.QueryFirstRecord<Weapon>(out recordExists, "SELECT WeaponName FROM Weapon WHERE WeaponID > 4");
            //if (recordExists)
            //    outputText.text += firstWeaponRecord.WeaponName + "\n";
            //else
            //    outputText.text += "No record found\n";
        }

        void InsertPlayerTable(int PlayerID)
        {
            /*
             * 

INSERT INTO TABLENAME (PK Column)
SELECT 'new value'
WHERE NOT EXISTS (SELECT 1 FROM TABLENAME WHERE PKColumn = 'new value');
 
             */

            string sqlInsertIfNotAlreadyExist = "INSERT INTO " + PlayerStatsTableName +
" (PlayerID) " +
                " SELECT '" + PlayerID.ToString() + "'" +
                "WHERE NOT EXISTS (SELECT 1 FROM " + PlayerStatsTableName + " WHERE PlayerID = '" +
                PlayerID.ToString() + "')";


            dbManager.Execute(sqlInsertIfNotAlreadyExist, PlayerID);

        }


    }
}