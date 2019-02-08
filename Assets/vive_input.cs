using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class vive_input : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if(SteamVR_Input._default.inActions.Teleport.GetStateDown(SteamVR_Input_Sources.Any)) {
		print("Teleport down");
	}
    }
}
