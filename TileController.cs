using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TileController : MonoBehaviour 
{
	// Variables for wave spawner
	/*
	public GameObject hazard;
	public Vector3 spawnValues;
	public int hazardCount;
	public float spawnWait;
	public float startWait;
	public float waveWait;
	*/

	List<GameObject> tiles = new List<GameObject> ();

	private GameObject currentTile;
	private GameObject RandomTile;
	public GameObject spawnTile;
	private int tileCount;
	public int tileSpawnLimit;
	//tile creation index
	private int i;
	//tile destroy index
	private int z;
	//Random int for tiles
	private int randomInt;
    private GameObject[] currentTileType;
	public GameObject[] tileTypes_firstStage;
    public GameObject[] tileTypes_secondStage;
    public GameObject[] tileTypes_thirdStage;
    public GameObject[] tileTypes_finalStage;
    public GameObject[] tileTypes_calm;

    public float newTileCoolDown = 1.0f;

	private Vector3 Z_Offset = new Vector3(0,0,100.0f);
	private bool isRunning;

    public static float static_scrollSpeed = -23.0f;
    public static float static_defaultScrollSpeed = -23.0f;
    public float scrollSpeed = 20.0f;
    public float scrollLerpSpeed = 0.1f;

    public static bool lost;

    // Comeback mechanic
    [Header("Comback Mechanic")]
    public Slider distance;
    private bool comeBack;
    public float comeBackCD;

    // Target destroyed, spawn calm tile
    private bool TargetCalmAllowed;

    void Awake ()
	{
		// Start list with spawn tile
		tiles.Add(spawnTile);
		currentTile = spawnTile;
		// Runs tileSpawnLimit times, creates and adds tiles to game and list
		for (i=1; i < tileSpawnLimit+1; i++) 
		{
			// Randomize tile
			randomInt = Random.Range (0, tileTypes_firstStage.Length);

			// Spawn instance of random tile with location of gameObjects
			currentTile = Instantiate(tileTypes_firstStage[randomInt],currentTile.transform.position + Z_Offset,Quaternion.identity);

			// Add new tile to list
			tiles.Add(currentTile);
		}

		//StartCoroutine (SpawnWaves ());
		isRunning = false;

        //Initialize scroll speed from inspector value
        static_scrollSpeed = -scrollSpeed;
        static_defaultScrollSpeed = static_scrollSpeed;

        //Tells tile scroller to stop readjusting if player lost to planet
        lost = false;

        // Comeback is true until it is used, then goes on cooldown
        comeBack = true;

        // Target destroyed calm spawn
        TargetCalmAllowed = true;
	}

    void Update()
    {
        if (static_scrollSpeed != static_defaultScrollSpeed && !lost)
        {
            static_scrollSpeed = Mathf.Lerp(static_scrollSpeed, static_defaultScrollSpeed, Time.deltaTime * scrollLerpSpeed);
        }
        if (GameController.planetDestroyed && TargetCalmAllowed)
        {
            TargetCalmAllowed = false;
            randomInt = Random.Range(0, tileTypes_calm.Length);
            GameObject tempTile = Instantiate(tileTypes_calm[randomInt], new Vector3 (0,0,100), Quaternion.identity);
            tempTile.GetComponentInChildren<BoxCollider>().enabled = false;
            StartCoroutine(TargetDestroyedCalmSpawnCoroutine());
        }

        // Comeback mechanic
        if (MassController.playerMass < MassController.static_targetMass / 2 && distance.value > 0.6f && comeBack) // Player losing and comeback is available
        {
            comeBack = false;
            GameObject[] tempTile = tileTypes_calm;
            int rand = Random.Range(0, tempTile.Length);
            StartCoroutine(ComeBackCoroutine());
            Instantiate(tempTile[rand], currentTile.transform.position + Z_Offset, Quaternion.identity);
        }
    }

    // New random tile, spawn + add to list, delete old tile and remove from list
    public void NewTile()
	{	// isRunning is a check to make sure run twice from collision checking errors
		if (!isRunning) {
			isRunning = true;

			//Add new tile to list
			tiles.Add (currentTile);

            //Select tile array based on player mass
            if (MassController.playerMass < 20)
            {
                currentTileType = tileTypes_firstStage;
            }
            else if (MassController.playerMass < 400)
            {
                currentTileType = tileTypes_secondStage;
                static_defaultScrollSpeed = -28.5f;
            }
            else if (MassController.playerMass < 8000)
            {
                currentTileType = tileTypes_thirdStage;
                static_defaultScrollSpeed = -30f;
            }
            else if (MassController.playerMass < 160000)
            {
                currentTileType = tileTypes_finalStage;
                static_defaultScrollSpeed = -31f;
            }
            else
            {
                currentTileType = tileTypes_finalStage;
                static_defaultScrollSpeed = -31.5f;
            }

            //Randomize tile
            randomInt = Random.Range(0, currentTileType.Length);

			//Spawn instance of random tile at end of former tile
			currentTile = Instantiate (currentTileType [randomInt], currentTile.transform.position + Z_Offset, Quaternion.identity);

			//destroy old tile
			Destroy (tiles [0]);
			tiles.RemoveAt (0);

			// Allow function call again
			StartCoroutine (NewTileDelay());
		}
	}

	IEnumerator NewTileDelay() {
		yield return new WaitForSeconds (newTileCoolDown);
		isRunning = false;
	}
    // Player dies at planet, destroy tiles
	public void DeathToPlanet()
	{
		for (int g = 1; g < tiles.Count; g++) {
			Destroy (tiles [g]);
		}
		tiles.Clear();
	}

    IEnumerator ComeBackCoroutine()
    {
        yield return new WaitForSeconds(comeBackCD);
        comeBack = true;
    }

    IEnumerator TargetDestroyedCalmSpawnCoroutine()
    {
        yield return new WaitForSeconds(10.0f);
        TargetCalmAllowed = true;
    }

    // Spawns waves of objects from a spawn position
    // not functional yet, not needed yet either
    /*
	IEnumerator SpawnWaves ()
	{
		yield return new WaitForSeconds (startWait);
		while (true) 
		{
			for (int j = 0; j < hazardCount; j++) {	
				Vector3 spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
				Quaternion spawnRotation = Quaternion.identity;
				Instantiate (hazard, spawnPosition, spawnRotation);
				yield return new WaitForSeconds (spawnWait);
			}
			yield return new WaitForSeconds (waveWait);
		}
	}
	*/
}
