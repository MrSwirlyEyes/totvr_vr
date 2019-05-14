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

	public short temp_min = -100;
	public short temp_max = 100;

	private List<Collider> sources = new List<Collider>();

	void OnTriggerEnter(Collider other) {
		if (other.GetComponentInParent<HeatSource> ()) {
			Debug.Log("Adding Source " + other.name);
			sources.Add (other);
		}
	}



	void OnTriggerExit(Collider other) {
		if (other.GetComponentInParent<HeatSource> ()) {
			Debug.Log("Losing Source " + other.name);
			sources.Remove (other);
		}
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

//		Debug.Log("Num Sources: " + sources.Count);

		foreach (Collider source in sources) {
			temperature = source.GetComponentInParent<HeatSource> ().temperature;
			emissionDistance = source.GetComponentInParent<HeatSource> ().emissionDistance;

//			Debug.Log ("Dist: " + Vector3.Distance (thumb.transform.position, source.transform.position));
			Debug.Log(source.name + " : " + distToHeat(Vector3.Distance (index.transform.position, source.ClosestPointOnBounds(index.transform.position)), temperature, emissionDistance));
			thumbSum += distToHeat(Vector3.Distance (thumb.transform.position, source.ClosestPointOnBounds(thumb.transform.position)), temperature, emissionDistance);
			indexSum += distToHeat(Vector3.Distance (index.transform.position, source.ClosestPointOnBounds(index.transform.position)), temperature, emissionDistance);
			middleSum += distToHeat(Vector3.Distance (middle.transform.position, source.ClosestPointOnBounds(middle.transform.position)), temperature, emissionDistance);
			ringSum += distToHeat(Vector3.Distance (ring.transform.position, source.ClosestPointOnBounds(ring.transform.position)), temperature, emissionDistance);
			pinkySum += distToHeat(Vector3.Distance (pinky.transform.position, source.ClosestPointOnBounds(pinky.transform.position)), temperature, emissionDistance);

		}
			
		if (Communicator.instance.heating) {
			Communicator.instance.outpkt.heats[0] = map((short) (thumbSum),temp_min, temp_max, tec_min, tec_max);
			Communicator.instance.outpkt.heats[1] = map((short) (indexSum),temp_min, temp_max, tec_min, tec_max);
			Communicator.instance.outpkt.heats[2] = map((short) (middleSum),temp_min, temp_max, tec_min, tec_max);
			Communicator.instance.outpkt.heats[3] = map((short) (ringSum),temp_min, temp_max, tec_min, tec_max);
			Communicator.instance.outpkt.heats[4] = map((short) (pinkySum),temp_min, temp_max, tec_min, tec_max);
			thumbText.text = Convert.ToString(Communicator.instance.outpkt.heats[0], 10);
			indexText.text = Convert.ToString(Communicator.instance.outpkt.heats[1], 10);
			middleText.text = Convert.ToString(Communicator.instance.outpkt.heats[2], 10);
			ringText.text = Convert.ToString(Communicator.instance.outpkt.heats[3], 10);
			pinkyText.text = Convert.ToString(Communicator.instance.outpkt.heats[4], 10);
		}
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
		float stretched = slope * (x);
		short returnable = (short) stretched;

		if (returnable > out_max)
			return out_max;
		if (returnable < out_min)
			return out_min;
		else
			return returnable;
	}
}