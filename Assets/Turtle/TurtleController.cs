
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//https://alastaira.wordpress.com/2013/08/24/the-7dfps-game-jam-augmented-reality-and-spectral-echoes/
public class TurtleController : MonoBehaviour {
    public GameObject text;
    Rigidbody body;
    Transform cam;
    Rigidbody camBody;
    float moveSpeed = 4.0f;
    float initSpeed = 4;
    float boostSpeed = 10;
    Animator anim;
    public Material shellMat;
    int happy;
    public AnimationCurve happyCurve;
    public Transform trashHolder;
    public Text text;
    int score = 0;
    float invuln = 0.0f;

    public List<GameObject> limbs;

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
        invuln -= Time.deltaTime;
        Quaternion att = new Quaternion(Input.gyro.attitude.x, Input.gyro.attitude.y, -Input.gyro.attitude.z, -Input.gyro.attitude.w);
        Quaternion textq = new Quaternion(Input.gyro.attitude.x, Input.gyro.attitude.y, 0, 0);
        Vector3 axis = Vector3.zero;
        textq.ToAngleAxis(out float angle, out axis);
        text.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
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

            //if (trashHolder.childCount > 0) {
            //    Destroy(trashHolder.GetChild(Random.Range(0, trashHolder.childCount)).gameObject);
            //}
        }
        if (collision.collider.CompareTag("Shark") && invuln < 0.0f) {
            //Game.instance.EndGame();
            //score -= 3;
            //UpdateText();
            invuln = 1.0f;

            if (limbs.Count == 0) {
                if (!reseting) {
                    reseting = true;
                    StartCoroutine(DeathRoutine());
                }
                //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

            int limbdex = Random.Range(0, limbs.Count);
            GameObject limb = limbs[limbdex];
            limbs.RemoveAt(limbdex);
            Destroy(limb);

            if (limbs.Count == 0) {
                if (!reseting) {
                    reseting = true;
                    StartCoroutine(DeathRoutine());
                }
                //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

        }

        if (collision.collider.CompareTag("Trash")) {
            //Game.instance.DecrementScore();
            Destroy(collision.collider.GetComponent<MonoBehaviour>());
            Destroy(collision.collider.GetComponent<Rigidbody>());
            collision.collider.transform.SetParent(trashHolder, true);
            //initSpeed -= 0.5f;
            //if(initSpeed < 1.0f) {
            //    initSpeed = 1.0f;
            //}
        }

        if (collision.collider.CompareTag("Recycler")) {
            for (int i = 0; i < trashHolder.childCount; ++i) {
                Destroy(trashHolder.GetChild(i).gameObject);
                score += 1;
            }
            UpdateText();
        }
    }

    void UpdateText() {
        text.text = "Recycled: " + score;
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

    bool reseting = false;
    IEnumerator DeathRoutine() {
        Time.timeScale = 0.0f;

        text.fontSize = 100;
        text.text = "GAME OVER! " + text.text;

        yield return new WaitForSecondsRealtime(5.0f);

        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
