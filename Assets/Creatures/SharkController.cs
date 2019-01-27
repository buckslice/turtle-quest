using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharkController : MonoBehaviour {
    public float chargeSpeed;
    public float idleSpeed;
    Rigidbody body;
    Transform turtle;
    public enum SharkState { CHARGING, IDLE, RISING }
    public SharkState currentState;
    float chargeTimer = 0.0f;
    float cooldownTimer = 0.0f;
    Vector3 targetVel = Vector3.zero;
    bool clockwise = true;
    // Start is called before the first frame update
    void Start() {
        turtle = GameObject.Find("Turtle").transform;
        body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update() {
        switch (currentState) {
            case SharkState.CHARGING:
                chargeTimer -= Time.deltaTime;
                if (chargeTimer < 0.0f) {
                    currentState = SharkState.RISING;
                }
                break;
            case SharkState.IDLE:
                Vector3 dir = ((turtle.position + turtle.forward * 5.0f + Random.insideUnitSphere * 5.0f) - transform.position).normalized;
                float dist = Vector3.Distance(transform.position, turtle.position);
                if (dist < 80.0f && cooldownTimer < 0.0f) {
                    currentState = SharkState.CHARGING;
                    cooldownTimer = Random.Range(15.0f, 30.0f);
                    targetVel = dir * chargeSpeed;
                    chargeTimer = dist / chargeSpeed + 4.0f;
                } else {
                    cooldownTimer -= Time.deltaTime;
                    Vector3 xzdir = dir;
                    xzdir.y = 0.0f;
                    xzdir = xzdir.normalized;
                    targetVel = (clockwise ? 1 : -1) * Vector3.Cross(dir, Vector3.up).normalized * idleSpeed + xzdir * Mathf.Lerp(-10f, 10f, dist / 100.0f) * idleSpeed / 3.0f;
                }
                break;
            case SharkState.RISING:
                targetVel = Vector3.up * idleSpeed;
                if (transform.position.y > 50f) {
                    currentState = SharkState.IDLE;
                    clockwise = Random.value > 0.5f;
                }
                break;
            default:
                break;
        }
        body.velocity = Vector3.Lerp(body.velocity, targetVel, Time.deltaTime * 5.0f);
        //transform.rotation = Quaternion.LookRotation(body.velocity.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(body.velocity.normalized, Vector3.up), Time.deltaTime * 2.0f);
    }

    private void OnCollisionEnter(Collision collision) {
        currentState = SharkState.RISING;
    }
}
