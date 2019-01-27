using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour {
    public static Game instance;
    public bool GameOver { get; private set; }
    public int Score { get; private set; }
    // Start is called before the first frame update
    void Start() {
        instance = this;
        Score = 0;
    }

    // Update is called once per frame
    void Update() {

    }

    public void EndGame() {
        GameOver = true;
        if(Score > PlayerPrefs.GetInt("High Score", 0)) {
            PlayerPrefs.SetInt("High Score", Score);
        }

    }

    public void IncrementScore() {
        if (!GameOver) {
            Score++;
        }
    }

    public void DecrementScore() {
        if (!GameOver) {
            Score--;
        }
    }
}
