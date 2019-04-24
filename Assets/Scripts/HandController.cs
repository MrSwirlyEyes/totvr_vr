/*
 * Hand controller class
 * 
 * Reads flex values from the communicator and assigns
 * hand model fingers to bend accordingly
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour {

//	int [] thumbRange		= { 90, 235 };
//	int [] indexRange		= { 236, 308 };
//	int [] middleRange		= { 236, 300 };
//	int [] ringRange		= { 190, 300 };
//	int [] pinkyRange		= { 119, 276 };
//
//	int [] fingerRange		= { 0, 90 };
//	int [] thRange			= { 0, 90 };

//	short maxRot = 90;
	/* For each finger, knuckle 1 is closest to the palm
	 * knuckle 3 right below the fingernail
	 */
	 // These public transforms are assigned to references
	 // to the Hand Model knuckles in Unity's configuration
	 // gui, as indicated in figure ##
	public Transform index1;
	public Transform index2;
	public Transform index3;

	public Transform middle1;
	public Transform middle2;
	public Transform middle3;

	public Transform ring1;
	public Transform ring2;
	public Transform ring3;

	public Transform pinky1;
	public Transform pinky2;
	public Transform pinky3;

	public Transform thumb1;
	public Transform thumb2;
	public Transform thumb3;


	/* Bend the fingers in accordance with the values read from hardware */
	void Update () {
		if (Communicator.instance.bending) {
//			int thumbRot = mapInvert (Communicator.instance.knuckles.thumb, thumbRange [0], thumbRange [1], thRange[0], thRange[1]);
//			int thumbRot = maxRot - Communicator.instance.inpkt.knuckles[0];
			int thumbRot = Communicator.instance.inpkt.knuckles[0];
			thumb1.localEulerAngles = new Vector3 (-thumbRot/2, thumbRot/4, thumbRot/3);
			thumb2.localEulerAngles = new Vector3 (-thumbRot/4, thumbRot/3, thumbRot/4);
			thumb3.localEulerAngles = new Vector3 (-thumbRot/4, thumbRot/4, thumbRot/3);

//			int indexRot = mapInvert (Communicator.instance.knuckles.index, indexRange [0], indexRange [1], fingerRange[0], fingerRange[1]);
//			int indexRot = maxRot - Communicator.instance.inpkt.knuckles[1];
			int indexRot = Communicator.instance.inpkt.knuckles[1];
			index1.localEulerAngles = new Vector3 (0, 50, indexRot);
			index2.localEulerAngles = new Vector3 (0, 0, indexRot);
			index3.localEulerAngles = new Vector3 (0, 0, indexRot/2);

//			int middleRot = mapInvert (Communicator.instance.knuckles.middle, middleRange [0], middleRange [1], fingerRange[0], fingerRange[1]);
//			int middleRot = maxRot - Communicator.instance.inpkt.knuckles[2];
			int middleRot = Communicator.instance.inpkt.knuckles[2];
			middle1.localEulerAngles = new Vector3 (0, 50, middleRot);
			middle2.localEulerAngles = new Vector3 (0, 0, middleRot);
			middle3.localEulerAngles = new Vector3 (0, 0, middleRot/2);

//			int ringRot = mapInvert (Communicator.instance.knuckles.ring, ringRange [0], ringRange [1], fingerRange[0], fingerRange[1]);
//			int ringRot = maxRot - Communicator.instance.inpkt.knuckles[3];
			int ringRot = Communicator.instance.inpkt.knuckles[3];
			ring1.localEulerAngles = new Vector3 (0, 50, ringRot);
			ring2.localEulerAngles = new Vector3 (0, 0, ringRot);
			ring3.localEulerAngles = new Vector3 (0, 0, ringRot/2);

//			int pinkyRot = mapInvert (Communicator.instance.knuckles.pinky, pinkyRange [0], pinkyRange [1], fingerRange[0], fingerRange[1]);
//			int pinkyRot = maxRot - Communicator.instance.inpkt.knuckles[4];
			int pinkyRot = Communicator.instance.inpkt.knuckles[4];
			pinky1.localEulerAngles = new Vector3 (0, 50, pinkyRot);
			pinky2.localEulerAngles = new Vector3 (0, 0, pinkyRot);
			pinky3.localEulerAngles = new Vector3 (0, 0, pinkyRot/2);
		}
	}

	/* map x from in range to out range, inverting so that higher values of
	 * x approach the minimum of out range.
	 * 
	 * Necessary to invert because rotation on the fingers increases as flex
	 * sensor readings decrease
	 * 
	 * TODO move to a utility class for DRY
	 */
	int mapInvert(int x, int in_min, int in_max, int out_min, int out_max) {
		float slope = (float)(out_max - out_min)/(in_max-in_min);
		float stretched = slope * (x-in_min);
		float b = ((float)out_max / in_min / slope);
		return out_max - (int)(stretched+b);
	}

	short map(short x, int in_min, int in_max, int out_min, int out_max) {
		float slope = (float)(out_max - out_min)/(in_max-in_min);
		float stretched = slope * (x-in_min);
		short returnable = (short)(stretched);
		if (returnable > out_max)
			return (short)out_max;
		if (returnable < out_min)
			return (short)out_min;
		else
			return returnable;
	}
}
