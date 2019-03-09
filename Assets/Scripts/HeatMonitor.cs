using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeatMonitor : MonoBehaviour {

	public Collider thumb, index, middle, ring, pinky;
	public Text thumbText, indexText, middleText, ringText, pinkyText;
	public short maxTec = 4095;
	public short minTec = -4095;

	private List<Collider> sources = new List<Collider>();

	void OnTriggerEnter(Collider other) {
		if (other.GetComponentInParent<HeatSource> ()) {
			sources.Add (other);
			Debug.Log ("ADD SOURCE");
		}
	}



	void OnTriggerExit(Collider other) {
		sources.Remove (other);
		Debug.Log ("REMOVE SOURCE");
	}

	

	/* For each of the contributing sources, find its contribution to each finger.
	 * Right now, I have chosen to only compare to the fingertip collider
	 */
	void Update () {
		float thumbSum = 0;
		float indexSum = 0;
		float middleSum = 0;
		float ringSum = 0;
		float pinkySum = 0;

		int temperature; float emissionDistance;

		foreach (Collider source in sources) {
			temperature = source.GetComponentInParent<HeatSource> ().temperature;
			emissionDistance = source.GetComponentInParent<HeatSource> ().emissionDistance;

//			Debug.Log ("Dist: " + Vector3.Distance (thumb.transform.position, source.transform.position));

			thumbSum += distToHeat(Vector3.Distance (thumb.transform.position, source.ClosestPointOnBounds(thumb.transform.position)), temperature, emissionDistance);
			indexSum += distToHeat(Vector3.Distance (index.transform.position, source.ClosestPointOnBounds(index.transform.position)), temperature, emissionDistance);
			middleSum += distToHeat(Vector3.Distance (middle.transform.position, source.ClosestPointOnBounds(middle.transform.position)), temperature, emissionDistance);
			ringSum += distToHeat(Vector3.Distance (ring.transform.position, source.ClosestPointOnBounds(ring.transform.position)), temperature, emissionDistance);
			pinkySum += distToHeat(Vector3.Distance (pinky.transform.position, source.ClosestPointOnBounds(pinky.transform.position)), temperature, emissionDistance);
		}

		Communicator.instance.heats.thumb = map((short) (thumbSum / sources.Count), -100, 100, minTec, maxTec);
		Communicator.instance.heats.index = map((short) (indexSum / sources.Count), -100, 100, minTec, maxTec);
		Communicator.instance.heats.middle = map((short) (middleSum / sources.Count), -100, 100, minTec, maxTec);
		Communicator.instance.heats.ring = map((short) (ringSum / sources.Count), -100, 100, minTec, maxTec);
		Communicator.instance.heats.pinky = map((short) (pinkySum / sources.Count), -100, 100, minTec, maxTec);

		//Debug.Log ("Temperature on Thumb: " + (short)(thumbSum / sources.Count));
//		thumbText.text = Convert.ToString((short)(thumbSum / sources.Count), 10);
//		indexText.text = Convert.ToString((short)(indexSum / sources.Count), 10);
//		middleText.text = Convert.ToString((short)(middleSum / sources.Count), 10);
//		ringText.text = Convert.ToString((short)(ringSum / sources.Count), 10);
//		pinkyText.text = Convert.ToString((short)(pinkySum / sources.Count), 10);

	}

	float distToHeat(float dist, int temperature, float emit) {
		if (dist <= emit) {
			return dist*(-temperature/emit) + temperature;
		} else {
			return 0;
		}
	}

	short map(short x, int in_min, int in_max, int out_min, int out_max) {
		float slope = (float)(out_max - out_min)/(in_max-in_min);
		float stretched = slope * (x-in_min);
		short returnable = (short)(stretched);
		if (returnable > maxTec)
			return maxTec;
		if (returnable < minTec)
			return minTec;
		else
			return returnable;
	}
}