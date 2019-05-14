using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class GestureHandler : MonoBehaviour
{

	private bool aiming = false;
	private bool grabbing = false;
	private int flatThresh = 20;
	private int curlThresh = 35;
	public GameObject interactionPoint;
//	public GameObject tracker;
	public GameObject laserPrefab;
	private GameObject laser;
	private Transform laserTransform;
	private Vector3 hitPoint;

	public Rigidbody arm, tracker;
	public Transform cameraRigTransform;
	public GameObject teleportReticlePrefab;
	private GameObject reticle;
	private Transform teleportReticleTransform;
	public Transform headTransform;
	public Vector3 teleportReticleOffset;
	public LayerMask teleportMask;
	private bool shouldTeleport;

	private string state = "None";

    // Start is called before the first frame update
    void Start()
    {
		laser = Instantiate(laserPrefab);
		laserTransform = laser.transform;

		reticle = Instantiate(teleportReticlePrefab);
		teleportReticleTransform = reticle.transform;
    }

    // Update is called once per frame
    void Update()
    {
		if (aiming) {
			UpdateRaycast();

			if (Abort()) {
				Reset();
			}

			if (Go() && shouldTeleport) {
				Teleport();
				Reset();
			}
		} else if (grabbing) {

		} else {
			if (Aiming()) {
				aiming = true;
				UpdateRaycast();
			} else if (Grabbing()) {
				grabbing = true;
			}
		}
    }

	void ShowProspectiveTeleport(RaycastHit hit) {
		// show laser
		laser.SetActive(true);
		laserTransform.position = Vector3.Lerp(interactionPoint.transform.position, hitPoint, 0.5f);
		laserTransform.LookAt(hitPoint);
		laserTransform.localScale = new Vector3(laserTransform.localScale.x,
												laserTransform.localScale.y,
												hit.distance);

		if (hit.collider.gameObject.layer == LayerMask.NameToLayer("CanTeleport")) {
			// show reticle
			reticle.SetActive(true);
			teleportReticleTransform.position = hitPoint + teleportReticleOffset;
			shouldTeleport = true;
		} else {
			reticle.SetActive(false);
			shouldTeleport = false;
		}

	}

	void Teleport() {
		shouldTeleport = false;
		Vector3 difference = cameraRigTransform.position - headTransform.position;
		difference.y = 0;
		cameraRigTransform.position = hitPoint + difference;
	}
	
	void UpdateRaycast() {
		RaycastHit hit;
		if (Physics.Raycast(interactionPoint.transform.position, interactionPoint.transform.forward, out hit, 100)) {
			hitPoint = hit.point;
			ShowProspectiveTeleport(hit);
		}
	}

	void Reset() {
		aiming = false;
		laser.SetActive(false);
		reticle.SetActive(false);
		state = "None";
	}
			

	bool Aiming() {
		// Aiming position is thumb, index, and middle extended
		// with ring and pinky curled
		if (Communicator.instance.inpkt.knuckles[0] < flatThresh &&
			Communicator.instance.inpkt.knuckles[1] < flatThresh &&
			Communicator.instance.inpkt.knuckles[2] > curlThresh &&
			Communicator.instance.inpkt.knuckles[3] > curlThresh &&
			Communicator.instance.inpkt.knuckles[4] > curlThresh) {
			state = "Aiming";
			return true;
		}
		return false;
	}

	bool Abort() {
		// Abort position is index, middle, ring, and pinky curled,
		// thumb extended
		if (Communicator.instance.inpkt.knuckles[0] < flatThresh &&
			Communicator.instance.inpkt.knuckles[1] < flatThresh &&
			Communicator.instance.inpkt.knuckles[2] < flatThresh &&
			Communicator.instance.inpkt.knuckles[3] < flatThresh &&
			Communicator.instance.inpkt.knuckles[4] < flatThresh) {
			state = "Aborting";
			return true;
		}
		return false;
	}

	bool Go() {
		// Go position is index and middle extended,
		// pinky, ring, and thumb curled
		if (Communicator.instance.inpkt.knuckles[0] > curlThresh &&
			Communicator.instance.inpkt.knuckles[1] < flatThresh &&
			Communicator.instance.inpkt.knuckles[2] > curlThresh &&
			Communicator.instance.inpkt.knuckles[3] > curlThresh &&
			Communicator.instance.inpkt.knuckles[4] > curlThresh) {
			state = "Going";
//			Debug.Log(state + ": " + Communicator.instance.inpkt.knuckles[0] + "," +
//				Communicator.instance.inpkt.knuckles[1] + "," +
//				Communicator.instance.inpkt.knuckles[2] + "," +
//				Communicator.instance.inpkt.knuckles[3] + "," +
//				Communicator.instance.inpkt.knuckles[4]);
			return true;
		}
		return false;
	}

	bool Grab() {
		if (Communicator.instance.inpkt.knuckles[0] > curlThresh &&
			Communicator.instance.inpkt.knuckles[1] > curlThresh &&
			Communicator.instance.inpkt.knuckles[2] > curlThresh &&
			Communicator.instance.inpkt.knuckles[3] > curlThresh &&
			Communicator.instance.inpkt.knuckles[4] > curlThresh) {
			state = "Grabbing";
			return true;
		}
		return false;
		
}
