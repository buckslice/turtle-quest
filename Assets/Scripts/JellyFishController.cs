using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JellyFishController : MonoBehaviour {
    Rigidbody body;
    float timeSinceVelChange = 0.0f;
    Vector3 dir; // try having target point to reach and impules towards
    bool applyingForce = false;
    // Start is called before the first frame update
    void Start() {
        body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update() {
        timeSinceVelChange -= Time.deltaTime;
        if (timeSinceVelChange < 0.0f) {
            dir = Random.onUnitSphere * Random.Range(1f, 3f);
            body.velocity = dir;
            timeSinceVelChange = Random.Range(5f, 10f);
        }
    }

    private void FixedUpdate() {
        if (body.velocity.y < -1.0f) {
            applyingForce = true;

        }

        if(body.velocity.y > 1.0f) {
            applyingForce = false;
        }

        if (applyingForce) {
            body.AddForce(Vector3.up*Random.Range(5f,10f), ForceMode.Acceleration);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        timeSinceVelChange = 0.0f;
        applyingForce = true;
    }
}
