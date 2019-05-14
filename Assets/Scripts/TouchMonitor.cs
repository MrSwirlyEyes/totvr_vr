using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class TouchMonitor : MonoBehaviour {

	short pressure;

	short minVibe = 0;
	short maxVibe = 4095;
	private static short maxPre = 500;
	private static double roughnessScalar = 1.5;
	private short maxPressure = (short) (maxPre * roughnessScalar);
	float directionMagnitudeThreshold = 0.1f;

	public Text thumbText, indexText, middleText, ringText, pinkyText;
	public Text thumbDText, indexDText, middleDText, ringDText, pinkyDText;
	public Rigidbody armRb;

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
		setVibes ("", 0, 0);
	}

	void OnCollisionStay(Collision collision) {

		if (collision.impulse.magnitude != 0) {
//			Debug.Log("VEL: " + collision.GetContact(0).thisCollider.GetComponentInParent<Rigidbody>().velocity.magnitude);
//			Debug.Log("RELVEL: " + collision.relativeVelocity.magnitude);
//			Debug.Log("IMPULSE: " + collision.impulse.magnitude);
			pressure = (short)(collision.impulse / Time.fixedDeltaTime).magnitude;
//			Debug.Log("PRE: " + pressure);
		}

//		Debug.Log(armRb.velocity.magnitude);
		short direction = 0;
		if (armRb.velocity.magnitude > directionMagnitudeThreshold) {
//			Debug.Log("MOVING");
			direction = 1;

			HeatSource hs = collision.collider.GetComponentInParent<HeatSource>();
			if (hs) {
				pressure = (short) (maxPre * hs.roughness);
//				Debug.Log("POST: " + pressure);
			}
		}


		short setVibe = map(pressure, 0, maxPressure, minVibe, maxVibe);
		if (Communicator.instance.touching) {
			setVibes (collision.GetContact(0).thisCollider.name, setVibe, direction);
		}

//		short directionX = (short)Math.Round(collision.relativeVelocity.normalized.x);
//		short directionZ = (short)Math.Round(collision.relativeVelocity.normalized.z);
//		short direction = (short)((directionX+1)*(directionZ+2));
//		Debug.Log(collision.contacts[0].thisCollider.name + "  " + directionX + "  " + directionZ);
		// Debug.Log (collision.contacts[0].thisCollider.name + " hit " + collision.collider.name);
//		Debug.Log ("Impulse is: " + collision.impulse.magnitude);
//		Debug.Log ("Velocity is: " + collision.relativeVelocity.magnitude);
		// Debug.Log ("Pressure is: " + pressure);

	}

	void setVibes(string name, short value, short direction) {
		switch (name) {
		case "RightHandThumb1":
		case "RightHandThumb2":
		case "RightHandThumb3":
		case "RightHandThumb4":
			Communicator.instance.outpkt.vibes[0] = value;
			Communicator.instance.outpkt.dires[0] = direction;
			thumbText.text = Convert.ToString((short)value, 10);
			thumbDText.text = Convert.ToString((short)direction, 10);
			break;
		case "RightHandIndex1":
		case "RightHandIndex2":
		case "RightHandIndex3":
		case "RightHandIndex4":
			Communicator.instance.outpkt.vibes[1] = value;
			Communicator.instance.outpkt.dires[1] = direction;
			indexText.text = Convert.ToString((short)value, 10);
			indexDText.text = Convert.ToString((short)direction, 10);
			break;
		case "RightHandMiddle1":
		case "RightHandMiddle2":
		case "RightHandMiddle3":
		case "RightHandMiddle4":
			Communicator.instance.outpkt.vibes[2] = value;
			Communicator.instance.outpkt.dires[2] = direction;
			middleText.text = Convert.ToString((short)value, 10);
			middleDText.text = Convert.ToString((short)direction, 10);
			break;
		case "RightHandRing1":
		case "RightHandRing2":
		case "RightHandRing3":
		case "RightHandRing4":
			Communicator.instance.outpkt.vibes[3] = value;
			Communicator.instance.outpkt.dires[3] = direction;
			ringText.text = Convert.ToString((short)value, 10);
			ringDText.text = Convert.ToString((short)direction, 10);
			break;
		case "RightHandPinky1":
		case "RightHandPinky2":
		case "RightHandPinky3":
		case "RightHandPinky4":
			Communicator.instance.outpkt.vibes[4] = value;
			Communicator.instance.outpkt.dires[4] = direction;
			pinkyText.text = Convert.ToString((short)value, 10);
			pinkyDText.text = Convert.ToString((short)direction, 10);
			break;
		default:
			Communicator.instance.outpkt.vibes[0] = value;
			Communicator.instance.outpkt.vibes[1] = value;
			Communicator.instance.outpkt.vibes[2] = value;
			Communicator.instance.outpkt.vibes[3] = value;
			Communicator.instance.outpkt.vibes[4] = value;

			Communicator.instance.outpkt.dires[0] = direction;
			Communicator.instance.outpkt.dires[1] = direction;
			Communicator.instance.outpkt.dires[2] = direction;
			Communicator.instance.outpkt.dires[3] = direction;
			Communicator.instance.outpkt.dires[4] = direction;

			thumbText.text = Convert.ToString((short)value, 10);
			indexText.text = Convert.ToString((short)value, 10);
			middleText.text = Convert.ToString((short)value, 10);
			ringText.text = Convert.ToString((short)value, 10);
			pinkyText.text = Convert.ToString((short)value, 10);

			thumbDText.text = Convert.ToString((short)direction, 10);
			indexDText.text = Convert.ToString((short)direction, 10);
			middleDText.text = Convert.ToString((short)direction, 10);
			ringDText.text = Convert.ToString((short)direction, 10);
			pinkyDText.text = Convert.ToString((short)direction, 10);
			break;
		}
	}

	short map(short x, int in_min, int in_max, int out_min, int out_max) {
		if (x < in_min) {
			return (short) out_min;
		} else if (x > in_max) {
			return (short) out_max;
		} else {
			return (short) ((x - in_min) * ( (out_max - out_min)/(in_max - in_min) ));
		}
	}
}
