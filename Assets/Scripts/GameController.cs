using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityStandardAssets.Characters.ThirdPerson;

public class GameController : MonoBehaviour {

    [SerializeField]
    GameObject playerRef;
    [SerializeField]
    Transform spawnPosition;

    List<string> dbRaceTimes;

    enum gameStates {
        NameInput = 0,
        Race = 1,
        Results = 2
    }

    [SerializeField]
    string gameState = gameStates.NameInput.ToString();

    float elapsedSeconds = 0;
    TimeSpan raceTime; 
    string GetRaceTime() {
        return raceTime.ToString().Substring(0, raceTime.ToString().Length - 4);
    }

    void Start () {
        elapsedSeconds = 0;
        playerRef.GetComponent<ThirdPersonUserControl>().freeze = true;
    }
	
	void Update () {
	    switch (gameState) {
            case "NameInput":
                if (Input.GetKeyDown(KeyCode.Return)) {
                    StartGame();
                }
                break;
            case "Race":
                elapsedSeconds += Time.deltaTime;
                raceTime = new TimeSpan(0, 0, 0, (int) elapsedSeconds, (int)((elapsedSeconds - (int)elapsedSeconds) * 1000));
                break;
        }
	}

    const string INITIAL_TEXT = "Introduce tu nombre";
    string playerName = INITIAL_TEXT;
    public string GetPlayerName() {
        return playerName;
    }

    const int TEXTFIELD_WIDTH = 200;
    const int TEXTFIELD_HEIGHT = 20;

    void OnGUI() {
        switch (gameState) {
            case "NameInput":
                playerName = GUI.TextField(
                    new Rect((Screen.width - TEXTFIELD_WIDTH) / 2, (Screen.height - TEXTFIELD_HEIGHT) / 2, TEXTFIELD_WIDTH, TEXTFIELD_HEIGHT),
                    playerName,
                    25);
                if (GUI.Button(new Rect((Screen.width + TEXTFIELD_WIDTH) / 2 - 100, (Screen.height + TEXTFIELD_HEIGHT) / 2, 100, 30), "Aceptar")) {
                    StartGame();
                }
                break;
            case "Race":
                GUI.Label(new Rect(10, 10, 100, 20), GetRaceTime());
                break;
            case "Results":
                GUI.Label(new Rect(10, 10, 100, 20), GetRaceTime());
                GUI.Box(new Rect((Screen.width - Screen.width/2.5f) / 2, 20, Screen.width / 2.5f, Screen.height * 0.8f), "Top 10");
                float leftPosX = (Screen.width - Screen.width / 2.5f) / 2;
                float posY = 20 + 30;
                float rightPosX = leftPosX + Screen.width / 2.5f - 100;
                GUI.Label(new Rect(leftPosX + Screen.width / 2.5f/8, posY, 100, 25), "Jugador");
                GUI.Label(new Rect(rightPosX - Screen.width / 2.5f/8 + 55, posY, 100, 25), "Tiempo");
                posY += 30;
                foreach (string dbRaceTime in dbRaceTimes) {
                    GUI.Label(new Rect(leftPosX + 20, posY, 100, 25), dbRaceTime.Split(';')[0]);
                    GUI.Label(new Rect(rightPosX, posY, 150, 25), dbRaceTime.Split(';')[1]);
                    posY += 20;
                }
                if (GUI.Button(new Rect((Screen.width - 150) / 2, Screen.height * 0.9f, 150, 30), "Volver a empezar")) {
                    ResetGame();
                }
                break;
        }
        
    }

    void StartGame() {
        if (playerName == "" && playerName == INITIAL_TEXT) return;

        gameState = gameStates.Race.ToString();
        playerRef.GetComponent<ThirdPersonUserControl>().freeze = false;
    }

    public void GoalReached() {
        gameState = gameStates.Results.ToString();
        playerRef.GetComponent<ThirdPersonUserControl>().freeze = true;
        GetComponent<DatabaseController>().SetNewScore(playerName, GetRaceTime());
        dbRaceTimes = GetComponent<DatabaseController>().TopTenScores();
        GetComponent<DatabaseController>().SyncDatabase();
    }

    void ResetGame() {
        gameState = gameStates.NameInput.ToString();
        playerRef.transform.position = spawnPosition.transform.position;
        elapsedSeconds = 0;
        playerRef.GetComponent<ThirdPersonUserControl>().freeze = true;
    }
}
