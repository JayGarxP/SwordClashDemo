using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwordClashUIManager : MonoBehaviour {
    public Text P1Score;
    private string P1ScoreString;
    public Text P2Score;
    private string P2ScoreString;
    public Text WinnerPopup;
    private string WinnerPopupString;

	// Use this for initialization
	void Start ()
    {
        // setup the strings at first	
        P1ScoreString = "P1: ";
        P2ScoreString = "P2: ";
        WinnerPopupString = "";
    	}
	
	// Update is called once per frame
	void Update ()
    {
        // Reset the ui strings each frame for now i guess
        P1Score.text = P1ScoreString;
        P2Score.text = P2ScoreString;
        WinnerPopup.text = WinnerPopupString;
	}

    public void UpdatePlayerOneScore(string p1Score)
    {
        P1ScoreString = "P1: " + p1Score;
    }

    public void UpdatePlayerTwoScore(string p2Score)
    {
        P2ScoreString = "P2: " + p2Score;
    }

    public void UpdateSinglePlayerLevel(string level)
    {
        P2ScoreString = "Level: " + level;

    }

    public void UpdateWinnerPopupMessage(string winnerPlayerID)
    {
        WinnerPopupString = winnerPlayerID += " Wins!";
    }

    public void ClearWinnerPopupMessage()
    {
        // relying on implementation detail of how empty Text.strings are drawn...
        WinnerPopupString = "";
    }



}
