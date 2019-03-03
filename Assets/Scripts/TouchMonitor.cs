using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class TouchMonitor : MonoBehaviour {

	short pressure;

	short minVibe = 0;
	short maxVibe = 4095;
	short maxPressure = 1000;

	public Text thumbText, indexText, middleText, ringText, pinkyText;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

	void OnCollisionEnter(Collision collision) {
//		Debug.Log ("Entered " + collision.contacts[0].thisCollider.name);
		pressure = (short) (collision.impulse / Time.deltaTime).magnitude;
	}

	/* set all vibes to 0 when we aren't touching anything */
	void OnCollisionExit(Collision collision) {
		setVibes ("", 0);
	}

	void OnCollisionStay(Collision collision) {

		if (collision.impulse.magnitude != 0) {
			pressure = (short)(collision.impulse / Time.fixedDeltaTime).magnitude;
		}

		short setVibe = map(pressure, 0, maxPressure, minVibe, maxVibe);

		Debug.Log (collision.contacts[0].thisCollider.name + " hit " + collision.collider.name);
//		Debug.Log ("Impulse is: " + collision.impulse.magnitude);
//		Debug.Log ("Velocity is: " + collision.relativeVelocity.magnitude);
		Debug.Log ("Pressure is: " + pressure);
		if (Communicator.instance.touching) {
			setVibes (collision.contacts[0].thisCollider.name, setVibe);
		}
	}

	void setVibes(string name, short value) {
		switch (name) {
		case "RightHandThumb1":
		case "RightHandThumb2":
		case "RightHandThumb3":
		case "RightHandThumb4":
			Communicator.instance.vibes.thumb = value;
//			thumbText.text = Convert.ToString((short)value, 10);
			break;
		case "RightHandIndex1":
		case "RightHandIndex2":
		case "RightHandIndex3":
		case "RightHandIndex4":
			Communicator.instance.vibes.index = value;
//			indexText.text = Convert.ToString((short)value, 10);
			break;
		case "RightHandMiddle1":
		case "RightHandMiddle2":
		case "RightHandMiddle3":
		case "RightHandMiddle4":
			Communicator.instance.vibes.middle = value;
//			middleText.text = Convert.ToString((short)value, 10);
			break;
		case "RightHandRing1":
		case "RightHandRing2":
		case "RightHandRing3":
		case "RightHandRing4":
			Communicator.instance.vibes.ring = value;
//			ringText.text = Convert.ToString((short)value, 10);
			break;
		case "RightHandPinky1":
		case "RightHandPinky2":
		case "RightHandPinky3":
		case "RightHandPinky4":
			Communicator.instance.vibes.pinky = value;
//			pinkyText.text = Convert.ToString((short)value, 10);
			break;
		default:
			Communicator.instance.vibes.thumb = value;
			Communicator.instance.vibes.index = value;
			Communicator.instance.vibes.middle = value;
			Communicator.instance.vibes.ring = value;
			Communicator.instance.vibes.pinky = value;
//			thumbText.text = Convert.ToString((short)value, 10);
//			indexText.text = Convert.ToString((short)value, 10);
//			middleText.text = Convert.ToString((short)value, 10);
//			ringText.text = Convert.ToString((short)value, 10);
//			pinkyText.text = Convert.ToString((short)value, 10);
			break;
		}
	}

	short map(short x, int in_min, int in_max, int out_min, int out_max) {
		float slope = (float)(out_max - out_min)/(in_max-in_min);
		float stretched = slope * (x-in_min);
		short returnable = (short)(stretched);
		if (returnable > maxVibe)
			return maxVibe;
		if (returnable < 0)
			return 0;
		else
			return returnable;
	}
}
