/*
 * Hand controller class
 * 
 * Reads flex values from the communicator and assigns
 * hand model fingers to bend accordingly
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

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
//	public Rigidbody index1;
//	public Rigidbody index2;
//	public Rigidbody index3;
//
//	public Rigidbody middle1;
//	public Rigidbody middle2;
//	public Rigidbody middle3;
//
//	public Rigidbody ring1;
//	public Rigidbody ring2;
//	public Rigidbody ring3;
//
//	public Rigidbody pinky1;
//	public Rigidbody pinky2;
//	public Rigidbody pinky3;
//
//	public Rigidbody thumb1;
//	public Rigidbody thumb2;
//	public Rigidbody thumb3;

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

	public GameObject tracker;
	protected const float VelocityMagic = 6000f;
	protected const float AngularVelocityMagic = 50f;
	public const float ExpectedDeltaTime = 0.0111f;
	protected const float MaxVelocityChange = 10f;
	protected const float MaxAngularVelocityChange = 50f;

	private int changeThreshold = 7;
	private int thumbRot = 0;
	private int indexRot = 0;
	private int middleRot = 0;
	private int ringRot = 0;
	private int pinkyRot = 0;

	public Rigidbody rb;
	private Vector3 posDel;

	/* Bend the fingers in accordance with the values read from hardware */
	void Update () {
		updateLinearVelocity(tracker.transform.position, rb.position, rb);
		updateAngularVelocity(tracker.transform.rotation, rb.rotation, rb);

		if (Communicator.instance.bending) {
			if (Mathf.Abs(thumbRot - Communicator.instance.inpkt.knuckles[0]) > changeThreshold) {
				thumbRot = Communicator.instance.inpkt.knuckles[0];
				updateRotation(Quaternion.Euler(-thumbRot/2, thumbRot/4, thumbRot/3), thumb1.rotation, thumb1);
				updateRotation(Quaternion.Euler(-thumbRot/4, thumbRot/3, thumbRot/4), thumb2.rotation, thumb2);
				updateRotation(Quaternion.Euler(-thumbRot/4, thumbRot/4, thumbRot/3), thumb3.rotation, thumb3);
			}

			if( Mathf.Abs(indexRot - Communicator.instance.inpkt.knuckles[1]) > changeThreshold) {
				indexRot = Communicator.instance.inpkt.knuckles[1];
				updateRotation(Quaternion.Euler(0, 50, indexRot), index1.rotation, index1);
				updateRotation(Quaternion.Euler(0, 0, indexRot), index2.rotation, index2);
				updateRotation(Quaternion.Euler(0, 0, indexRot/2), index3.rotation, index3);
			}

			if ( Mathf.Abs(middleRot - Communicator.instance.inpkt.knuckles[2]) > changeThreshold) {
				middleRot = Communicator.instance.inpkt.knuckles[2];
				updateRotation(Quaternion.Euler(0, 50, middleRot), middle1.rotation, middle1);
				updateRotation(Quaternion.Euler(0, 0, middleRot), middle2.rotation, middle2);
				updateRotation(Quaternion.Euler(0, 0, middleRot/2), middle3.rotation, middle3);
			}

			if ( Mathf.Abs(ringRot - Communicator.instance.inpkt.knuckles[3]) > changeThreshold) {
				ringRot = Communicator.instance.inpkt.knuckles[3];
				updateRotation(Quaternion.Euler(0, 50, ringRot), ring1.rotation, ring1);
				updateRotation(Quaternion.Euler(0, 0, ringRot), ring2.rotation, ring2);
				updateRotation(Quaternion.Euler(0, 0, ringRot/2), ring3.rotation, ring3);
			}

			if ( Mathf.Abs(pinkyRot - Communicator.instance.inpkt.knuckles[4]) > changeThreshold) {
				pinkyRot = Communicator.instance.inpkt.knuckles[4];
				updateRotation(Quaternion.Euler(0, 50, pinkyRot), pinky1.rotation, pinky1);
				updateRotation(Quaternion.Euler(0, 0, pinkyRot), pinky2.rotation, pinky2);
				updateRotation(Quaternion.Euler(0, 0, pinkyRot/2), pinky3.rotation, pinky3);
			}
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

	Vector3 updateLinearVelocity(Vector3 targetPosition, Vector3 currentPosition, Rigidbody object_) {
		float velocityMagic = VelocityMagic / (Time.deltaTime / ExpectedDeltaTime);

		Vector3 positionDelta = targetPosition - currentPosition;
		if (positionDelta.magnitude > 1) {
			object_.MovePosition(targetPosition);
		} else {
			Vector3 velocityTarget = (positionDelta * velocityMagic) * Time.deltaTime;
			if (float.IsNaN(velocityTarget.x) == false)
			{
				object_.velocity = Vector3.MoveTowards(object_.velocity, velocityTarget, MaxVelocityChange);
			}
		}

		return positionDelta;
	}

	void updateAngularVelocity(Quaternion targetRotation, Quaternion currentRotation, Rigidbody object_) {
		float angularVelocityMagic = AngularVelocityMagic / (Time.deltaTime / ExpectedDeltaTime);
		Quaternion rotationDelta = targetRotation * Quaternion.Inverse(currentRotation);

		float angle;
		Vector3 axis;
		rotationDelta.ToAngleAxis(out angle, out axis);

		if (angle > 180)
			angle -= 360;

		if (angle != 0) {
			Vector3 angularTarget = angle * axis;
			if (float.IsNaN(angularTarget.x) == false) {
				angularTarget = (angularTarget * angularVelocityMagic) * Time.deltaTime;
				object_.angularVelocity = Vector3.MoveTowards(object_.angularVelocity, angularTarget, MaxAngularVelocityChange);
			}
		}
	}

	void updateRotation(Quaternion targetRotation, Quaternion currentRotation, Transform object_) {
		object_.localRotation = targetRotation;
	}
}
