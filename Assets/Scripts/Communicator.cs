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
using UnityEngine.UI;
using System.Runtime.InteropServices;


public class Communicator : MonoBehaviour {

	public static Communicator instance = null;
	public string port = "COM6";
	public int baudrate = 9600;
	private SerialPort stream;

	public GameObject tracker = null;
	public GameObject touchGlove = null;

	/* are we connected to hardware? */
	public bool communicating = true;

	/* are we bending hand model fingers in accordance with flex sensors? */
	public bool bending = true;

	/* are we detecting pressure when to touch an in-game object? */
	public bool touching = true;

	/* are we detecting the temperature emission of in-game objects? */
	public bool heating = true;

//	private int numSensors = 5;

	public bool calibrate = true;
	public Text instructions;

	/* Stores flex sensor values received from glove to be applied to hand model knuckles */
	public struct KnuckleValues {
		public int[] knuckles;

		public KnuckleValues(int size){
			knuckles = new int[size];
		}
	}

	/* Stores in-game pressure to be sent to glove vibe motors */
//	public struct VibeValues {
//		public short index, middle, ring, pinky, thumb;
//	}
//
//	/* stores in-game heat to be sent to glove TECss */
//	public struct HeatValues {
//		public short index, middle, ring, pinky, thumb;
//	}
//
//	public struct DirectionValues {
//		public short index, middle, ring, pinky, thumb;
//	}

	public struct TouchGlovePkt {
		public short msgType;
		public short[] vibes;
		public short[] heats;
		public short[] dires;

		public TouchGlovePkt(int size) {
			msgType = 0;
			vibes = new short[size];
			heats = new short[size];
			dires = new short[size];
		}
	}

	public TouchGlovePkt outpkt = new TouchGlovePkt(5);
	public KnuckleValues inpkt = new KnuckleValues(5);

//	public KnuckleValues knuckles;
//	public VibeValues vibes;
//	public HeatValues heats;
//	public DirectionValues dires;

	/* tracks whether we are waiting for a response from the glove, to ensure clean handoffs
	 * we exchange between reading and writing - attempting both concurrently caused significant
	 * delay in Unity's performance
	 */
	private bool reading = false;
	private int calibration;

	
	/* Set the global instance of the Communicator and open the stream to the hardware */
	void Start () {

		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);
		}

		if (calibrate) {
			calibration = -1;
		} else {
			calibration = 4;
		}

//		FixedJoint fx = tracker.AddComponent<FixedJoint>();
//		fx.breakForce = 50000;
//		fx.breakTorque = 50000;
//		fx.connectedBody = touchGlove.GetComponent<Rigidbody>();

		if (communicating) {
			Open ();
		}
		instructions.text = "Greetings";
		setDefaults ();
	}


	/* Read from and Write to the Hardware, alternatingly (aka we won't write if we're trying to read
	 * or vice versa).
	 */
	void Update() {
		if (calibration == -1) {
			Debug.Log("Sending Max Calibrate");
			instructions.text = "Hold your hand flat";
			WriteToArduino((short) 256);
			calibration++;
			StartCoroutine (
				ReadFromArduino (emptyFunction, 0)
			);

		// skip 0 - 0 waits for readfromarduino to finish
		} else if (calibration == 0 || calibration == 2) {
			Debug.Log(calibration + " Waiting for Arduino");
		} else if (calibration == 1) {
			Debug.Log("Sending Min Calibrate");
			instructions.text = "Make a fist";
			WriteToArduino((short) 128);
			calibration++;
			StartCoroutine (
				ReadFromArduino (emptyFunction, 0)
			);
		
		// skip 2 - 2 waits for readfromarduino to finish
		} else if (calibration == 3) {
			instructions.text = "";
			calibration++;
		} else if (calibration == 4) {
			if (communicating && !reading) {
				short cmd = 64; // (short)48879;
				WriteToArduino (cmd); // 0xBEEF in base 10
				reading = true;
				StartCoroutine (
					ReadFromArduino (handleData, 50)
				);
			}
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

	public short getDirectionByte() {
		int directions = 0;
		for (int i = 0; i < outpkt.dires.Length; i++) {
			directions += outpkt.dires[i]*(2^i);
		}
		return (short) directions;
	}

	/* Write bytes to the hardware */
	public void WriteToArduino(short msgType) {
//		Debug.Log("heats: " + outpkt.heats[0] + "," + outpkt.heats[1] + "," + outpkt.heats[2] + "," + outpkt.heats[3] + "," + outpkt.heats[4]);
//		Debug.Log("vibes: " + outpkt.vibes[0] + "," + outpkt.vibes[1] + "," + outpkt.vibes[2] + "," + outpkt.vibes[3] + "," + outpkt.vibes[4]);
//		Debug.Log("dires: " + outpkt.dires[0] + "," + outpkt.dires[1] + "," + outpkt.dires[2] + "," + outpkt.dires[3] + "," + outpkt.dires[4]);

		outpkt.msgType = msgType;
		short directions = getDirectionByte();

//		int size = Marshal.SizeOf(outpkt);
//		byte[] bytes = new byte[size];
//
//		IntPtr ptr = Marshal.AllocHGlobal(size);
//		Marshal.StructureToPtr(outpkt, ptr, true);
//		Marshal.Copy(ptr, bytes, 0, size);
//		Marshal.FreeHGlobal(ptr);



		short[] sendValues = new short[] {	msgType,
			outpkt.vibes[0], outpkt.vibes[1], outpkt.vibes[2], outpkt.vibes[3], outpkt.vibes[4],
			outpkt.heats[0], outpkt.heats[1], outpkt.heats[2], outpkt.heats[3], outpkt.heats[4],
			directions
											/*dires.thumb, dires.index, dires.middle, dires.ring, dires.pinky*/
										 };
		int size = sendValues.Length * sizeof(short);
		byte[] bytes = new byte[size];
		Buffer.BlockCopy (sendValues, 0, bytes, 0, bytes.Length);
		stream.Write(bytes,0,size);

//		Debug.Log ("Writing " + sendValues[11] + ',' + sendValues[12] + ',' + sendValues[13] + ',' + sendValues[14] + ',' + sendValues[15]);
	}



	/* Write a string to the hardware */
	public void WriteToArduino(string message) {
		stream.WriteLine(message);
	}



	/* Async function so that game continues while we wait for the hardware
	 * to send flex data
	 */
	public IEnumerator ReadFromArduino (Action<string> callback, int maxAttempts) {
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
				//stream.Flush();
				yield break;
			} else {
				Debug.Log("noread");
				if (maxAttempts > 0 && ++attempts > maxAttempts) {
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

		if (values.Length == 5) {
			try {
				for (int i = 0; i < values.Length; i++) {
					inpkt.knuckles[i] = System.Convert.ToInt32 (values [i]);
				}
//				knuckles.index	= System.Convert.ToInt32 (values [1]);
//				knuckles.middle	= System.Convert.ToInt32 (values [2]);
//				knuckles.ring	= System.Convert.ToInt32 (values [3]);
//				knuckles.pinky	= System.Convert.ToInt32 (values [4]);
			} catch {
			}
		}
		Debug.Log("Receiving " + inData);

//		Debug.Log ("Index: " + knuckles.index);
	}



	/* Initialize game env until we receive/write first hardware interaction */
	void setDefaults() {
		for (int i = 0; i < inpkt.knuckles.Length; i++) {
			inpkt.knuckles[i] = 40;
			outpkt.vibes[i] = 0;
			outpkt.heats[i] = 0;
			outpkt.dires[i] = 0;
		}

//		knuckles.thumb = 40;
//		knuckles.index = 40;
//		knuckles.middle = 40;
//		knuckles.ring = 40;
//		knuckles.pinky = 40;
	}

	void emptyFunction(string inData) {
		Debug.Log(calibration + " From Arduino: " + inData);
		calibration++;
	}

} /* END COMMUNICATOR CLASS */