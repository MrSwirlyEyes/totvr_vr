/* This should be attached to the Camera Rig. It is global to the game, and serves as the
 * channel connecting game information to the device on the other side (probably an Arduino).
 * Any script that would like to send information to the hardware must have a variable internal
 * to the Communicator representing it (e.g. the VibeValues struct vibes), and can update its
 * values by accessing the global instance (e.g. Communicator.instance.vibes.index = value; ).
 */

using UnityEngine;
using System;
using System.Collections;
using System.Reflection;
using System.IO.Ports;

public class Communicator : MonoBehaviour {

	public static Communicator instance = null;
	public string port = "COM6";
	public int baudrate = 9600;
	private SerialPort stream;


	/* are we connected to hardware? */
	public bool communicating = true;

	/* are we bending hand model fingers in accordance with flex sensors? */
	public bool bending = true;

	/* are we detecting pressure when to touch an in-game object? */
	public bool touching = true;

	/* are we detecting the temperature emission of in-game objects? */
	public bool heating = true;

	private int numSensors = 10;

	/* Stores flex sensor values received from glove to be applied to hand model knuckles */
	public struct KnuckleValues {
		public int index, middle, ring, pinky, thumb;
	}

	/* Stores in-game pressure to be sent to glove vibe motors */
	public struct VibeValues {
		public short index, middle, ring, pinky, thumb;
	}

	/* stores in-game heat to be sent to glove TECss */
	public struct HeatValues {
		public short index, middle, ring, pinky, thumb;
	}

	public struct DirectionValues {
		public short index, middle, ring, pinky, thumb, wrist;
	}

	public KnuckleValues knuckles;
	public VibeValues vibes;
	public HeatValues heats;
	public DirectionValues dires;

	/* tracks whether we are waiting for a response from the glove, to ensure clean handoffs
	 * we exchange between reading and writing - attempting both concurrently caused significant
	 * delay in Unity's performance
	 */
	private bool reading = false;

	
	/* Set the global instance of the Communicator and open the stream to the hardwware */
	void Start () {
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);
		}

		if (communicating) {
			Open ();
		}

		setDefaults ();
	}


	/* Read from and Write to the Hardware, alternatingly (aka we won't write if we're trying to read
	 * or vice versa).
	 */
	void Update() {

		if (communicating && !reading) {
			WriteToArduino ();
			reading = true;
			StartCoroutine (
				ReadFromArduino (handleData)
			);
		}
	}


	/* Open a stream to to given port */
	public void Open() {
		stream = new SerialPort (port, baudrate);
		stream.ReadTimeout = 10;
		stream.Open ();
	}


	/* Close the stream */
	public void Close() {
		stream.Close ();
	}


	/* Write bytes to the hardware */
	public void WriteToArduino() {
		short[] sendValues = new short[] {	vibes.thumb, vibes.index, vibes.middle, vibes.ring, vibes.pinky,
											heats.thumb, heats.index, heats.middle, heats.ring, heats.pinky,
											/* dires.thumb, dires.index, dires.middle, dires.ring, dires.pinky, dires.wrist */ };
		byte[] bytes = new byte[numSensors * sizeof(short)];
		Buffer.BlockCopy (sendValues, 0, bytes, 0, bytes.Length);
		Debug.Log ("Writing");
		stream.Write(bytes,0,bytes.Length);
//		stream.BaseStream.Flush ();
	}



	/* Write a string to the hardware */
	public void WriteToArduino(string message) {
		stream.WriteLine(message);
		//		stream.BaseStream.Flush ();
	}



	/* Async function so that game continues while we wait for the hardware
	 * to send flex data
	 */
	public IEnumerator ReadFromArduino (Action<string> callback) {
		string dataString = null;
		int attempts = 0;

		while (true) {
			try {
				/* read a string from the stream */
				dataString = stream.ReadLine ();
			} catch (TimeoutException) {
				dataString = null;
			}

			/* if we didn't get a reading, then yield so we can try again next frame
			 * if successful, send the string for handling
			 */
			if (dataString != null) {
				callback (dataString);
				reading = false;
				yield break;
			} else {
				Debug.Log("noread");
				if (++attempts > 50) {
					reading = false;
					yield break;
				}
				yield return new WaitForSeconds (0.05f);
			}
		}
	}



	/* callback from read, which populates the variables with received data.
	 * TODO relocate mapping to the Hand Controller. Having it here is not SRPy.
	 */
	void handleData(string inData) {
		char[] delimiters = { ',' };
		string[] values = inData.Split (delimiters);
		Debug.Log("Receiving " + inData);

		try {
			knuckles.thumb	= System.Convert.ToInt32 (values [0]);
			knuckles.index	= System.Convert.ToInt32 (values [1]);
			knuckles.middle	= System.Convert.ToInt32 (values [2]);
			knuckles.ring	= System.Convert.ToInt32 (values [3]);
			knuckles.pinky	= System.Convert.ToInt32 (values [4]);
		} catch {
		}
//		Debug.Log ("Index: " + knuckles.index);
	}



	/* Initialize game env until we receive/write first hardware interaction */
	void setDefaults() {
		knuckles.thumb = 40;
		knuckles.index = 40;
		knuckles.middle = 40;
		knuckles.ring = 40;
		knuckles.pinky = 40;

		vibes.thumb = 0;
		vibes.index = 0;
		vibes.middle = 0;
		vibes.ring = 0;
		vibes.pinky = 0;
	}

} /* END COMMUNICATOR CLASS */