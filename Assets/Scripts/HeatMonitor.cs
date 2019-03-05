using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeatMonitor : MonoBehaviour {

	public Collider thumb, index, middle, ring, pinky;
	public Text thumbText, indexText, middleText, ringText, pinkyText;
	public short tec_max = 4095;
	public short tec_min = -4095;

	private List<Collider> sources = new List<Collider>();

	void OnTriggerEnter(Collider other) {
		if (other.GetComponentInParent<HeatSource> ()) {
			sources.Add (other);
		}
	}



	void OnTriggerExit(Collider other) {
		sources.Remove (other);
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

		Communicator.instance.heats.thumb = map((short) (thumbSum / sources.Count),temp_min, temp_max, tec_min, tec_max);
		Communicator.instance.heats.index = map((short) (indexSum / sources.Count),temp_min, temp_max, tec_min, tec_max);
		Communicator.instance.heats.middle = map((short) (middleSum / sources.Count),temp_min, temp_max, tec_min, tec_max);
		Communicator.instance.heats.ring = map((short) (ringSum / sources.Count),temp_min, temp_max, tec_min, tec_max);
		Communicator.instance.heats.pinky = map((short) (pinkySum / sources.Count),temp_min, temp_max, tec_min, tec_max);

		Debug.Log ("Temperatures: " + Communicator.instance.heats.thumb + ',' 
									+ Communicator.instance.heats.index + ','
									+ Communicator.instance.heats.middle + ','
									+ Communicator.instance.heats.ring + ','
									+ Communicator.instance.heats.pinky);
		
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

	short map(short x, short in_min, short in_max, short out_min, short out_max) {
		float slope = (float)(out_max - out_min)/(in_max-in_min);
		float stretched = slope * (x-in_min);
		short returnable = (short) stretched;

		if (returnable > out_max)
			return out_max;
		if (returnable < out_min)
			return out_min;
		else
			return returnable;
	}
}