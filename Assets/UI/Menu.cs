using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour {
    public bool gameStarted;
    public GameObject menu;
    public static Menu instance;
    // Start is called before the first frame update
    void Start() {
        instance = this;
    }

    // Update is called once per frame
    void Update() {
        // Mobile Input
        if (Input.touchCount == 1 && !gameStarted) {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Ended) {
                gameStarted = true;
                menu.SetActive(false);
            }
        }

        // Keyboard Input
        if (Input.GetMouseButtonUp(0)) {
            gameStarted = true;
            menu.SetActive(false);
        }
    }
}
