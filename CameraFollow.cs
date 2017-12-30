using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	private GameObject player;
    //private float Z_Offset;
    public float Z_Offset_Start;
    public float Z_Offset_Scaler = 1.0f;

    public float followDist = 2.0f;
    public float cameraLerp = 1.0f;
    private Vector3 newPos;

	// Use this for initialization
	void Awake () {
		player = GameObject.FindGameObjectWithTag ("Player");
		Z_Offset_Start = player.transform.position.z - transform.position.z;
        //Z_Offset = Z_Offset_Start;
    }
	
	// Update is called once per frame
	void Update () {
		newPos = new Vector3(player.transform.position.x/followDist, player.transform.position.y/followDist, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, newPos, cameraLerp);
    }
}
