using UnityEngine;
using System.Collections;

public class PersonMovementController : MonoBehaviour {

	public Animator anim;
	private bool hasBumped = false;
	private float angleToRotate = 0.0f;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator>();
		anim.SetFloat("Speed_f",0.2f);
	}
	
	// Update is called once per frame
	void Update () {
		if (hasBumped) {

			transform.rotation = Quaternion.Slerp(transform.rotation, 
			                                      Quaternion.AngleAxis (angleToRotate, Vector3.up), 
			                                      5 * Time.deltaTime);

		}
	}
	
	void OnControllerColliderHit(ControllerColliderHit hit) {
		if (hit.collider.CompareTag ("wall") && !hasBumped) {
			hasBumped = true;
			angleToRotate = Random.Range (0.0f, 360.0f);
			Invoke("stopBumped", 6 * Time.deltaTime);
		}
	}

	private void stopBumped() {
		hasBumped = false;
	}
}
