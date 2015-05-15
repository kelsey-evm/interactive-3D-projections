// This script enables an object to be "grabbed" using a Leap-enabled hand gesture.
// Associate to a "grabbableObj" by adding this script as a component to the object.

using UnityEngine;
using System.Collections;
using Leap;

//[ExecuteInEditMode]

public class ObjMovementController : MonoBehaviour {

	public GameObject grabbableObj;

	private bool isGrabbing = false;

	private ArrayList lastFewGrabStrengths = new ArrayList();
	private ArrayList lastFewPinchStrengths = new ArrayList();
	private ArrayList lastFewHandPositions = new ArrayList();


	private float getMean(ArrayList l, int N) {
		float sum = 0.0f;
		int toSumQuantity = Mathf.Min(N, l.Count);

		if (toSumQuantity == 0.0f)
			return 0.0f;

		for(int i=0; i<toSumQuantity; i++) {
			sum += (float) l[i];
		}

		return sum / (float)toSumQuantity;
	}
	private Vector3 getPositionsMean(ArrayList positions, int N) {
		Vector3 sum = new Vector3(0.0f, 0.0f, 0.0f);
		int toSumQuantity = Mathf.Min(N, positions.Count);
		
		if (toSumQuantity == 0)
			return sum;
		
		for(int i=0; i<toSumQuantity; i++) {
			sum = sum + (Vector3)positions[i];
		}
		
		return sum / (float)toSumQuantity;
	}

	void Update() {
		HandModel hand_model = GetComponent<HandModel> ();
		if (hand_model == null)
			return;
		Hand leap_hand = hand_model.GetLeapHand ();
		if (leap_hand == null)
			return;

		Vector3 handCoordinates = getHandCoordinates (leap_hand);

		// Smooth hand coordinate signal from Leap by averaging with the most recent such coordinates
		Vector3 meanLastFewHandPositions = getPositionsMean (lastFewHandPositions, 4);

		if(lastFewHandPositions.Count > 8 && Vector3.Distance(meanLastFewHandPositions, handCoordinates) > 2.0f) {
			// Handle outlier
		}

		// Same smoothing applied to the grab/pinch strengths
		float meanLastFewGrabStrengths = getMean (lastFewGrabStrengths, 4);
		float meanLastFewPinchStrengthss = getMean (lastFewPinchStrengths, 4);

		if(lastFewGrabStrengths.Count > 6 && 
		   (Mathf.Abs(meanLastFewGrabStrengths - (float) leap_hand.GrabStrength) > 0.6f ||
		   Mathf.Abs(meanLastFewPinchStrengthss - (float) leap_hand.PinchStrength) > 0.6f)) {
			// Handle outlier
		}

		lastFewGrabStrengths.Add ((float) leap_hand.GrabStrength);
		lastFewPinchStrengths.Add ((float) leap_hand.PinchStrength);
		if (lastFewGrabStrengths.Count >= 10) {
			lastFewGrabStrengths.RemoveAt(0);
			lastFewPinchStrengths.RemoveAt(0);
		}

		lastFewHandPositions.Add(handCoordinates);
		if (lastFewHandPositions.Count >= 10) {
			lastFewHandPositions.RemoveAt(0);
		}
		Vector3 newHandPosition = getPositionsMean(lastFewHandPositions, 3);

		// Ensure that the grab gesture is intentional and the behavior is expected
		if (isGrabbing && lastFewGrabStrengths.Count > 8 && (getMean(lastFewGrabStrengths, 8) > 0.7 || getMean(lastFewPinchStrengths, 8) > 0.7)) {
			isGrabbing = true;
		} else if (!isGrabbing && lastFewGrabStrengths.Count > 8 && 
		           (getMean(lastFewGrabStrengths, 8) > 0.8 || getMean(lastFewPinchStrengths, 8) > 0.8)) {
			isGrabbing = true;
		} else {
			isGrabbing = false;
		}

		if (isGrabbing) {
			grabbableObj.GetComponent<Animator> ().SetBool ("Grounded_b", false);
			grabbableObj.GetComponent<Animator> ().SetFloat ("Speed_f", 0.0f);
			Debug.Log(newHandPosition.y);
			grabbableObj.transform.position = newHandPosition;
		} else {
			grabbableObj.GetComponent<Animator> ().SetBool ("Grounded_b", true);
		}

	}

	private Vector3 getHandCoordinates(Hand leap_hand) {
		Vector leapHandPos = leap_hand.PalmPosition;

		Vector3 pos = grabbableObj.transform.position;
		
		pos.x = -1.0f * leapHandPos.x / 60.0f;
		pos.y = Mathf.Min (leapHandPos.y / 80.0f, 3.3f);
		pos.z = 1.0f * leapHandPos.z / 70.0f;
		return pos;
	}

}
