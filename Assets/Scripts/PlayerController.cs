using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    void OnTriggerEnter(Collider other) {
        if (other.tag != "Goal") return;

        GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().GoalReached();
    }

}
