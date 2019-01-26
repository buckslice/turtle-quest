
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://alastaira.wordpress.com/2013/08/24/the-7dfps-game-jam-augmented-reality-and-spectral-echoes/
public class TurtleController : MonoBehaviour {
    Rigidbody body;
    Quaternion origin = Quaternion.identity;
    Transform cam;
    Rigidbody camBody;
    float moveSpeed = 2.0f;

    // Start is called before the first frame update
    void Start() {
        cam = Camera.main.transform;
        camBody = cam.root.GetComponent<Rigidbody>();

        Input.gyro.enabled = true;
        origin = Input.gyro.attitude;

        body = GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update() {
        Quaternion att = new Quaternion(Input.gyro.attitude.x, Input.gyro.attitude.y, -Input.gyro.attitude.z, -Input.gyro.attitude.w);

        if (Input.touchCount > 0 || origin == Quaternion.identity)
            origin = Input.gyro.attitude;

        cam.localRotation = att; // * Quaternion.Inverse(origin); // doesnt work anymore when combined. not sure if even needed
        camBody.velocity = cam.forward * moveSpeed;

        Vector3 targetPoint = cam.position + cam.forward * 4.0f;
        Vector3 dir = targetPoint - body.position;
        body.velocity = dir.normalized * 5.0f * dir.magnitude;

        body.rotation = Quaternion.Slerp(body.rotation, Quaternion.LookRotation(body.velocity, Vector3.up), Time.deltaTime * 5.0f);

    }
}
