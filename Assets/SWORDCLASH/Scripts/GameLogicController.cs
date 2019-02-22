using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// SwordClash Game Logic Controller; needs some way to talk to Nanogames menu scene
public class GameLogicController : MonoBehaviour
{
    public short NumberofRounds;
    public static int PlayerOneScoreUIVal = 0;
    public Text PlayerOneScoreUIText;
    public Text WinnerPopUpText;

    // Food tentacles fight over
    [SerializeField]
    private GameObject Snack;
    [SerializeField]
    private Sprite[] SnackSpriteArray;


    private short PlayerOnePoints;
    private short PlayerTwoPoints;


    // Center of camera game screen in world units
    private Vector3 CenterCameraCoord;
    private SpriteRenderer SnackSprite;
    private Rigidbody2D SnackBody;


    // Start is called before the first frame update
    void Start()
    {
        PlayerOnePoints = 0;
        PlayerTwoPoints = 0;

        CenterCameraCoord = Camera.main.ScreenToWorldPoint(new Vector2((Screen.width / 2),
            (Screen.height / 2)));
        // Zero out z coordinate, for some reason setting it to zero in ScreenToWorldPoint results in -10 for z...
        CenterCameraCoord.z = 0;

        //SpawnFoodInCenter();

        // WinnerPopUpText.text = "";
        // PlayerOneScoreUIText.text = "P1 Score: " + PlayerOneScoreUIVal;

        // Spawn in UI from prefabs
        // WinnerPopUpText = Instantiate();

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

    public int SpawnFoodInCenter()
    {
        Snack = Instantiate(Snack, CenterCameraCoord, Quaternion.identity);
        SnackBody = Snack.GetComponent<Rigidbody2D>();
        SnackSprite = Snack.GetComponent<SpriteRenderer>();
        // The index is saved so the correct sprite can be synced across the Bolt network.
        int spriteIndex =
        AssignRandomSnackSprite();
        return spriteIndex;
    }

    private int AssignRandomSnackSprite()
    {
        // Assuming that the SnackSpriteArray is of size 20, with 20 food sprites set in the Unity Editor Inspector
        //  Pick a random sprite to be the current SnackSprite;
        // Array.Length returns number of elements, -1 at end prevents catastrophe if last element is chosen.
        int randomIndexIntoSnackSpriteArray = Random.Range(1, SnackSpriteArray.Length) - 1;
        SnackSprite.sprite = SnackSpriteArray[randomIndexIntoSnackSpriteArray];
        return randomIndexIntoSnackSpriteArray;
    }

    // For inbetween rounds
    private void NextRoundFoodInCenter()
    {
        AssignRandomSnackSprite();
        SnackBody.position = (CenterCameraCoord);
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
