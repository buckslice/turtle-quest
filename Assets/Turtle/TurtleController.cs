
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://alastaira.wordpress.com/2013/08/24/the-7dfps-game-jam-augmented-reality-and-spectral-echoes/
public class TurtleController : MonoBehaviour {
    Rigidbody body;
    Transform cam;
    Rigidbody camBody;
    float moveSpeed = 4.0f;
    const float initSpeed = 4;
    const float boostSpeed = 10;
    Animator anim;
    public Material shellMat;
    int happy;
    public AnimationCurve happyCurve;

    // Start is called before the first frame update
    void Start() {
        cam = Camera.main.transform;
        camBody = cam.root.GetComponent<Rigidbody>();

        Input.gyro.enabled = true;

        body = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();

        happy = Shader.PropertyToID("_HappyMode");
        shellMat.SetFloat(happy, 0.0f);
    }

    // Update is called once per frame
    void Update() {
        Quaternion att = new Quaternion(Input.gyro.attitude.x, Input.gyro.attitude.y, -Input.gyro.attitude.z, -Input.gyro.attitude.w);

        cam.localRotation = att; // * Quaternion.Inverse(origin); // doesnt work anymore when combined. not sure if even needed

        if (Input.touchCount > 0) {
            camBody.velocity = Vector3.zero;
        } else {
            camBody.velocity = cam.forward * moveSpeed;
        }

        Vector3 targetPoint = cam.position + cam.forward * 3.0f;
        Vector3 dir = targetPoint - body.position;
        body.velocity = dir.normalized * moveSpeed * 1.5f * dir.magnitude;

        if (body.velocity.sqrMagnitude > 0.1f) {
            body.rotation = Quaternion.Slerp(body.rotation, Quaternion.LookRotation(body.velocity, Vector3.up), Time.deltaTime * 5.0f);
        }

    }

    Coroutine speedRoutine = null;
    private void OnCollisionEnter(Collision collision) {
        if (collision.collider.CompareTag("Jelly")) {
            Destroy(collision.collider.gameObject);
            //moveSpeed += 1.0f;
            //Game.instance.IncrementScore();
            anim.SetTrigger("Eat");
            if (speedRoutine != null) {
                StopCoroutine(speedRoutine);
            }
            speedRoutine = StartCoroutine(BoostSpeedRoutine());
        }
        if (collision.collider.CompareTag("Shark")) {
            //Game.instance.EndGame();
        }
        if (collision.collider.CompareTag("Trash")) {
            //Game.instance.DecrementScore();
        }
    }

    IEnumerator BoostSpeedRoutine() {
        float t = 0.0f;
        float timer = 8.0f;

        while (t < timer) {
            t += Time.deltaTime;
            float c = happyCurve.Evaluate(t / timer);
            moveSpeed = Mathf.Lerp(initSpeed, boostSpeed, c);
            shellMat.SetFloat(happy, c);
            anim.speed = Mathf.Lerp(1.0f, 3.0f, c);
            yield return null;
        }
        moveSpeed = initSpeed;
        shellMat.SetFloat(happy, 0.0f);
        anim.speed = 1.0f;
        speedRoutine = null;
    }
}
