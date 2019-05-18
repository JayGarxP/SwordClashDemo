using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SwordClash
{

    // Will make this AN INTERFACE later to have all GLC's implement it,
    // can't make it base class since BOLT objects must be inherited from Bolt.Object which 
    //       overrides stuff in Unity.MonoBehavior
    public class SinglePlayerLogicController : MonoBehaviour
    {
        public short NumberofRounds;
 // what level we are in 
        public short LevelIndex;

        // reference to our database manager object in the scene
        public SimpleSQL.SimpleSQLManager dbManager;

        // reference to the gui text object in our scene that will be used for output
        public Text outputText;
        // reference to popup win message
        public Image happySquidPopupImage;

        private GameObject UIManager;
        private SwordClashUIManager SCUIManager;

        // Food tentacles fight over
        [SerializeField]
        private GameObject Snack;
        private bool SnackInstantiated;
        private SinglePlayerFoodController SCFoodController;

        // Center of camera game screen in world units
        private Vector3 CenterCameraCoord;
        public static Vector3 HardCodedCenterCameraCoord;
        public Vector3 LevelTwoFoodSpawnPosition;

        // use this vector3 to reposition food
        private Vector3 FoodSpawnPosition;

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

            SnackInstantiated = false;

            CenterCameraCoord = Camera.main.ScreenToWorldPoint(new Vector2((Screen.width / 2),
                (Screen.height / 2)));

            // Zero out z coordinate, for some reason setting it to zero in ScreenToWorldPoint results in -10 for z...
            CenterCameraCoord.z = 0;

            HardCodedCenterCameraCoord = CenterCameraCoord;

            PlayerOnePoints = 0;
            PlayerTwoPoints = 0;
            FoodSpawned = false;


            SpawnSnack();
            Debug.Log("Chris Inserting into PlayerTable: ");
            InsertPlayerTable(1);
            //PrintPlayerTable();
            
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
            if (SnackInstantiated == false)
            {
                switch (LevelIndex)
                {
                    case 1:
                        InstantiateFood(CenterCameraCoord);
                        break;
                    case 2:
                        InstantiateFood(LevelTwoFoodSpawnPosition);
                        break;
                    default:
                        Console.WriteLine("Level Index in Inspector for GameWorld object not set correctly!!!");
                        break;
                }



            }
        }

        private void InstantiateFood(Vector3 newFoodSpawnPosition)
        {
            Snack = Instantiate(Snack, newFoodSpawnPosition, Quaternion.identity);
            FoodSpawnPosition = newFoodSpawnPosition;
            SnackInstantiated = true;
        }

        // Update is called once per frame
        void Update()
        {

            PrintPlayerTable();


            if (SCFoodController == null)
            {
                if (Snack != null)
                {
                    SCFoodController = Snack.GetComponent<SinglePlayerFoodController>();

                    NextRoundFoodInCenter();
                    SCFoodController.MoveFoodToCenterSameSprite(FoodSpawnPosition);
                   
                }
                else
                {
                    SpawnSnack();
                }

            }
            else if (FoodSpawned == false)
            {
                NextRoundFoodInCenter();
                SCFoodController.MoveFoodToCenterSameSprite(FoodSpawnPosition);
               

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
                switch (LevelIndex)
                {
                    case 1:
                        SCUIManager.UpdateSinglePlayerLevel(" One ");
                        break;
                    case 2:
                        SCUIManager.UpdateSinglePlayerLevel("@ Two @");
                        break;
                    default:
                        Console.WriteLine("Level Index in Inspector for GameWorld object not set correctly!!!");
                        break;
                }
               

            }
        }

        // Monobehavior reset when component is first dropped into scene, set default editor fields here
        void Reset()
        {
            NumberofRounds = 3;
            LevelTwoFoodSpawnPosition = new Vector3(-4.2f, 0.2f, 0.0f);
            LevelIndex = 1;
        }

        // For inbetween rounds
        private void NextRoundFoodInCenter()
        {
            // Use SC controller methods to assign sprite and move food to center
            if (SCFoodController != null)
            {
                SCFoodController.MoveFoodToCenter(FoodSpawnPosition);
                FoodSpawned = true;
                
            }
            else
            {
                SCFoodController = Snack.GetComponent<SinglePlayerFoodController>();
                Debug.Log("Chris SCFoodController was null somehow...");
                SCFoodController.MoveFoodToCenter(FoodSpawnPosition);
                FoodSpawned = true;
                            
                            }

        }



        public void OnFoodEaten(string EaterPlayerID)
        {
            // Spawn in new food
            NextRoundFoodInCenter();

            if (EaterPlayerID == "Player1")
            {
                PlayerOnePoints += 1;
                UpdatePlayerTableWinsInLocalDatabase(1);

                // Hardcoded scene indexes found in File -> Build Settings
                switch (LevelIndex)
                {
                    case 1:
                        DisplayWinPopupGotoNextLevel(2);
                        break;
                    case 2:
                        DisplayWinPopupGotoNextLevel(0); // 0 is main menu
                        break;
                    default:
                        Console.WriteLine("Level Index in Inspector for GameWorld object not set correctly!!!");
                        break;
                }
                
                
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

        private void UpdatePlayerTableWinsInLocalDatabase(int PlayerID)
        {
            // first test if totalWins is null or zero; set it to zero before updating then.
            // Gather a list of Players from their table in localDB	
            List<PlayerTable> playerList = dbManager.Query<PlayerTable>(
                                                            "SELECT *" +
                                                            "FROM " +
                                                                "Player W " +
                                                            "WHERE " +
                                                                "W.PlayerID = 1 AND (W.TotalWins = 0 OR W.TotalWins is null)"
                                                            );

            if (playerList.Count == 1)
            {
                // column + 1 seems to not work if null; defualt value of TotalWins is not being set on Insert
                // test null with 'is null' on sqlite3.dll strings loaded by dbManager.Query()
                Debug.Log("Chris unset Playerwins conditoin hit!!!!!!!!!!!!!!!!!!!");
                string sql = "UPDATE " + PlayerStatsTableName +
//" SET TotalWins = TotalWins + 1 " + 
" SET TotalWins = 1 " +
"WHERE " +
"PlayerID = ?";
                dbManager.Execute(sql, PlayerID);
            }
            else
            {
                Debug.Log("Chris normal Playerwins condishun @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");

                string sql = "UPDATE " + PlayerStatsTableName +
" SET TotalWins = TotalWins + 1 " +
"WHERE " +
"PlayerID = ?";
                dbManager.Execute(sql, PlayerID);
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
            // Gather a list of Players from their table in localDB	
            List<PlayerTable> playerList = dbManager.Query<PlayerTable>(
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
            foreach (PlayerTable playerRecord in playerList)
            {
                outputText.text += "<color=#1abc9c>ID</color>: '" + playerRecord.PlayerID.ToString() + "' " +
                                    "<color=#1abc9c>LvlsBeat</color>:" + playerRecord.HighestLevelComplete.ToString() + " " +
                                    "<color=#1abc9c>Deaths</color>:" + playerRecord.TotalDeaths.ToString() + "\n" +
                                    "<color=#33ff00>Wins</color>:" + playerRecord.TotalWins.ToString() + " " +
                                    "<color=#1abc9c>GamesPlayed</color>:" + playerRecord.TotalGamesPlayed.ToString() + "\n";
            }

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

        void DisplayWinPopupGotoNextLevel(int sceneIndex)
        {
            if (happySquidPopupImage != null)
            {
                happySquidPopupImage.gameObject.SetActive(true);
                StartCoroutine(DelaySceneLoad(sceneIndex));
            }
        }

        IEnumerator DelaySceneLoad(int sceneIndex)
        {
            // wait for float seconds b4 switching to new scene
            yield return new WaitForSeconds(2.0f);
            LoadSceneByIndex(sceneIndex);
        }

        private void LoadSceneByIndex(int index)
        {
            happySquidPopupImage.gameObject.SetActive(false);

            // load scene index
            SceneManager.LoadScene(index);
        }


    }
}