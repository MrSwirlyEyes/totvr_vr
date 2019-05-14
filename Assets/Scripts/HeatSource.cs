using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatSource : MonoBehaviour {

	public int temperature = 100; // in celsius - 0 is freezing, 100 is boiling
	public float emissionDistance = 5; // game unit...still figuring out what that is.
	public float roughness = 1; // 0 means smooth, 1 means very rough, .5 means kinda rough 
}
