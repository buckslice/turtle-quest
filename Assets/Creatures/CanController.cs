using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanController : MonoBehaviour {
    float timeTillDeath = 40.0f;

    void Update() {
        timeTillDeath -= Time.deltaTime;
        if (timeTillDeath < 0.0f) {
            Destroy(gameObject);
        }
    }

    bool dying = false;
    private void OnCollisionEnter(Collision collision) {
        if (!dying) {
            timeTillDeath = 10.0f;
            dying = true;
        }
    }
}
