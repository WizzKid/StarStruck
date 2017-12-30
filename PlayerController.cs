using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Data;
using Mono.Data.Sqlite;
using System.Data.SqlClient;
using UnityEngine.PostProcessing;

public class PlayerController : MonoBehaviour
{    
    // References in no particular order
    private TileController tileController;
    private AsteroidController collObj;
    private CameraShake cameraShake;
    private PlayerMovement playerMovement;
    private Vector3 v3Scale;
    private Rigidbody rb;
    
    // Score   
    private String connectionString;
    SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
    private List<Highscore> highscores = new List<Highscore>();
    private int scoreVal;

    [Header("Score")]
    public int scoreUpNum = 100;
    public GameObject scorePrefab;
    public Transform scoreParent;
    public int topRanks;
    public int savedScores;
    public InputField enterName;
    public GameObject nameDialouge;
    
    public Text scoreText;
    public int streakCount;
    public Text streakMultiplier;

    [Header("Mass gain/loss")]
    // red/green "damage"/"mass gain" indication
    public Color dmgFlashColor = new Color(1f, 0f, 0f, 0.1f);
    public Image damageImage;
    public float dmgFlashSpeed = 5.0f;
    public bool damaged;
    public Color massGainFlashColor = new Color(1f, 0f, 0f, 0.1f);
    public Image massGainImage;
    bool massGained;

    public static float massCollectionScaler = 3f;

    [Header("Particles")]
    // Particles
    public GameObject[] explosion;
	public GameObject explosion2;
    public GameObject distort;
    public GameObject stopWatchParticle;
    public GameObject titaniumShellParticle;
    public GameObject burnUpParticle;
    public GameObject burnUpParticle2;
    public GameObject Playerexplosion;
    public GameObject streakParticle1;
    public GameObject streakParticle2;
    public GameObject streakParticle3;

    // Pick ups
    private bool burnUp;
    private bool doubleBurnUp;
    private bool stopWatch;
    private MeshRenderer render;
    [Header("PickUps")]
    public float burnUpDuration = 6.0f;
    public float stopWatchDuration = 6.0f;
    public static bool titanium;  
    public Material titaniumMat;

    //Hazards
    private bool magneticField;
    private bool nebulaCloud;
    [Header("Hazards")]
    public float magneticFieldDuration = 5.0f;
    public float nebulaCloudDuration = 5.0f;

    [Header("Collision w/Target")]
    // Collision with Target
    private int movementMethod;
    private bool lerpToCenter;
    public float allowControlDelay = 1.8f;
        // Lerp variables
    private Vector3 startMarker;
    private Vector3 endMarker;
    public float lerpSpeed = 1.0f;
    private float startTime;
    private float journeyLength;
        // Event
    public delegate void TargetBoundary();
    public static event TargetBoundary TargetBoundary_Collision;
    public static event TargetBoundary Target_Collision;
    [Header("Particles to turn off")]
        // Space particles
    public GameObject spaceParticle;

    // Post processing at runtime
    private CameraRuntimePostProcessingChanger postProcessScript;

    //Sound
    public AudioPlayer Sound;

    // Use this for initialization
    void Awake()
    {
        builder.DataSource = "testhighscore.database.windows.net";
        builder.UserID = "DSelvia";
        builder.Password = "MeteorMen1";
        builder.InitialCatalog = "TestHighscore";
        
        tileController = GameObject.FindGameObjectWithTag("GameController").GetComponent<TileController>();
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody>();

        // Pick ups
        burnUp = false;
        doubleBurnUp = false;
        stopWatch = false;
        titanium = false;
        magneticField = false;
        nebulaCloud = false;

        // Target collision
        lerpToCenter = false;

        // Post processing at runtime
        postProcessScript = Camera.main.GetComponent<CameraRuntimePostProcessingChanger>();

        // Camera shake
        cameraShake = Camera.main.GetComponent<CameraShake>();
    }

    void Update()
    {
        ScoreStreakCounter();
        
        if (!massGained)
        {
            massGainImage.color = Color.Lerp(massGainImage.color, Color.clear, dmgFlashSpeed = Time.deltaTime);
        }
        
        if (!damaged)
        {
            damageImage.color = Color.Lerp(damageImage.color, Color.clear,  dmgFlashSpeed = Time.deltaTime*.5f);
        }

        if (lerpToCenter)
        {
            float distCovered = (Time.time - startTime) * lerpSpeed;
            float fracJourney = distCovered / journeyLength;
            transform.position = Vector3.Lerp(startMarker, endMarker, fracJourney);
        }
        
        damaged = false;
        massGained = false;
    }
    // Collisions
    void OnCollisionEnter(Collision other)
    {   // Asteroid
        if (other.gameObject.CompareTag("Asteroid"))
        {
            collObj = other.gameObject.GetComponent<AsteroidController>();
            // Player bigger than asteroid
            if (MassController.playerMass >= collObj.mass)
            {
                if (collObj.mass <= 1)
                {
                    MassController.playerMass++;
                }
                else {
                    MassController.playerMass += collObj.mass/massCollectionScaler;
                }
                if (burnUp)
                {
                    scoreVal += (scoreUpNum*2);
                }
                else
                {
                    scoreVal += scoreUpNum;
                }
                streakCount++;
                // Send camera shake info and start
                cameraShake.ShakeChanger(1f, 0.75f);
                CameraShake.start = true;
                // Screen flashes green
                massGainImage.color = massGainFlashColor;
                massGained = true;
            }
            // Player smaller than asteroid
            else if (MassController.playerMass < collObj.mass)
            {
                // Titanium allows gaining of 1 bigger asteroid
                if (titanium)
                {
                    if (burnUp)
                    {
                        scoreVal += 200;
                    }
                    else
                    {
                        scoreVal += 100;
                    }
                    titanium = false;

                    // Screen flashes green
                    massGainImage.color = massGainFlashColor;
                    massGained = true;

                    MassController.playerMass++;
                }
                else // Not in titanium shell
                {
                    if (MassController.playerMass < 5)
                    {
                        MassController.playerMass--;
                    }
                    else
                    {
                        MassController.playerMass -= MassController.playerMass / 5;
                    }
                    damaged = true;
                    damageImage.color = dmgFlashColor;
                }

                streakCount = 0;

                // Send bigger camera shake info and start
                cameraShake.ShakeChanger(2f, 1.75f);
                CameraShake.start = true;
            }

            // Random rotation
            rb.AddTorque(new Vector3(UnityEngine.Random.Range(-100.0f, 100.0f), UnityEngine.Random.Range(-100.0f, 100.0f), UnityEngine.Random.Range(-100.0f, 100.0f)));

            // Explosion particle
            int temp = UnityEngine.Random.Range(0,explosion.Length);
			Instantiate(explosion[temp], other.transform.position, Quaternion.identity);
            temp = UnityEngine.Random.Range(0, Sound.asteroidSfx.Length);
            Sound.PlayAsteroidSfx(temp); // Asteroid Sound

            // Update self size vector and call camera adjust
                //v3Scale = new Vector3(1 + MassController.playerMass / massScale_Fraction, 1 + (MassController.playerMass / massScale_Fraction), 1 + (MassController.playerMass / massScale_Fraction));

            Destroy(other.gameObject);

            scoreText.text = scoreVal.ToString();
        } // END Collision = Asteroid

        if (other.gameObject.CompareTag("BurnUp"))
        {
            if (burnUp)
            {
                scoreVal += scoreUpNum*2;
                doubleBurnUp = true;
            }
            else {
                StartCoroutine(BurnUpCoroutine());
            }
            GameObject burn = Instantiate(burnUpParticle, other.transform.position, Quaternion.identity) as GameObject;
            Destroy(burn, 2);
            Sound.PlayPickUpSfx(0); // Sound
            Destroy(other.gameObject);
            burnUp = true;
        }

        if (other.gameObject.CompareTag("StopWatch")) {
            if (!stopWatch)
            {
                GameObject timeStop = Instantiate(stopWatchParticle, other.transform.position, Quaternion.identity) as GameObject;
                Destroy(timeStop, 2);
                Sound.PlayPickUpSfx(2); // Sound
                Destroy(other.gameObject);
                Time.timeScale = 0.6f;
                stopWatch = true;
                StartCoroutine(StopWatchCoroutine());
            }
            else {
                StartCoroutine(StopWatchCoroutine());
            }
        }
        
        if (other.gameObject.CompareTag("RogueStar"))
        {
            if (!titanium) // Titanium shell not engaged
            {
                if (MassController.playerMass < 3)
                {
                    MassController.playerMass--;
                }
                else
                {
                    MassController.playerMass -= MassController.playerMass / 3;
                }
            } else // Titanium shell engaged
            {
                MassController.playerMass++;
                titanium = false;
            }
            cameraShake.ShakeChanger(2f, 1.75f);
            CameraShake.start = true;
            // Particle
            int temp = UnityEngine.Random.Range(1, explosion.Length);
            GameObject expl = Instantiate(explosion[temp], other.transform.position, Quaternion.identity) as GameObject;
            Destroy(expl, 2);
            Sound.PlayHazardSfx(5);
            Destroy(other.gameObject);
        }
    }
    // Triggers
    void OnTriggerEnter(Collider trig)
    {
        // Spawn new tile
        if (trig.gameObject.CompareTag("TileTrigger"))
        {
            tileController.NewTile();
        }
        // Collision with target boundary, centers player and disables input until collision with planet itself
        if (trig.gameObject.CompareTag("TargetBoundary"))
        {   // Disable controls and store which control method was being used
            if (playerMovement.mouse_Input)
            {
                playerMovement.mouse_Input = false;
                movementMethod = 1;
            }
            else if (playerMovement.mouse_Input_Physics)
            {
                playerMovement.mouse_Input_Physics = false;
                movementMethod = 2;
            }
            else if (playerMovement.joyStick_WASD_Input)
            {
                playerMovement.joyStick_WASD_Input = false;
                movementMethod = 3;
            }
            // Keeps track of beginning of time and distance for Lerp to center of screen
            startTime = Time.time;
            startMarker = transform.position;
            endMarker = Vector3.zero;
            journeyLength = Vector3.Distance(startMarker, endMarker);
            // Moves player to center of screen
            lerpToCenter = true;
            // Event is called to destroy all hazards on screen
                // Null reference check
                TargetBoundary temp = TargetBoundary_Collision;
            if (temp != null)
            {
                TargetBoundary_Collision();
            }
        }
        if (trig.gameObject.CompareTag("Target"))
        {
            // Player smaller than target mass = LOSE
            if (MassController.playerMass < MassController.static_targetMass)
            {
                TileController.lost = true;
				GameObject expl = Instantiate(Playerexplosion, transform.position, Quaternion.identity) as GameObject;
				Destroy(expl, 2);

                nameDialouge.SetActive(true);
                TileController.static_scrollSpeed = 0;
				tileController.DeathToPlanet ();

                GetComponent<PlayerModelChange>().enabled = false;
                GetComponent<CapsuleCollider>().enabled = false;
                GetComponent<SphereCollider>().enabled = false;

                // Turn off particles
                streakCount = 0;
                spaceParticle.SetActive(false);

                for (var i = 0; i < transform.childCount; i++)
                {
                    if (transform.GetChild(i).GetComponent<MeshRenderer>())
                    {
                        transform.GetChild(i).GetComponent<MeshRenderer>().enabled = false;
                    }
                }
                    // SceneManager.LoadScene("Game Over");
            }
            // Player bigger than or equal to target mass
            if (MassController.playerMass >= MassController.static_targetMass)
            {
                StartCoroutine(AllowControlDelay());                

                // Particle
                GameObject expl = Instantiate(explosion2, new Vector3(trig.transform.position.x, trig.transform.position.y, 0), Quaternion.identity) as GameObject;
                Destroy(expl, 2);
                // Destroy target
                Destroy(trig.transform.parent.gameObject);
                // Send big camera shake info and start
                cameraShake.ShakeChanger(4f, 2f);
                CameraShake.start = true;
                // Add mass of target
                MassController.playerMass += MassController.static_targetMass/3;
                    // Event is called on collision with planet to destroy asteroids and powerups
                    // Null reference check
                TargetBoundary temp = Target_Collision;
                if (temp != null)
                {
                    Target_Collision();
                }
                if (GameController.planetDestroyed == false)
                {
                    GameController.planetDestroyed = true;
                }
            } // END player > target
        } // END collision w/target

        // Titanium Shell
        if (trig.CompareTag("TitaniumShell"))
        {
            if (titanium)
            {
                scoreVal += scoreUpNum*2;
            }
            GameObject tiShell = Instantiate(titaniumShellParticle, trig.transform.position, Quaternion.identity) as GameObject;
            Destroy(tiShell, 2);
            Sound.PlayPickUpSfx(1); // Sound
            Destroy(trig.gameObject);
            titanium = true;
        }
        // Magnetic Field
        if (trig.CompareTag("MagneticField"))
        {
            Sound.PlayHazardSfx(1); // Sound
            StartCoroutine(MagneticFieldCoroutine());
        }
        // Nebula Cloud
        if (trig.CompareTag("NebulaCloud"))
        {
            Sound.PlayHazardSfx(0); // Sound
            StartCoroutine(NebulaCloudCoroutine());
        }
    }

    IEnumerator BurnUpCoroutine()
    {
        postProcessScript.bloomBool = true;
        burnUpParticle2.SetActive(true);
        yield return new WaitForSeconds(burnUpDuration);
        if (doubleBurnUp)
        {
            yield return new WaitForSeconds(burnUpDuration/2);
            doubleBurnUp = false;
        }
        burnUp = false;
        burnUpParticle2.SetActive(false);
        postProcessScript.bloomBool = false;
    }

    IEnumerator StopWatchCoroutine()
    {
        transform.GetComponent<PlayerMovement>().XYSpeed *= 2.7f;
        yield return new WaitForSeconds(stopWatchDuration);
        stopWatch = false;
        transform.GetComponent<PlayerMovement>().XYSpeed /= 2.7f;
        Time.timeScale = 1.0f;
    }

    IEnumerator MagneticFieldCoroutine() {
        GameObject[] asteroids = GameObject.FindGameObjectsWithTag("Asteroid");
        foreach (GameObject asteroid in asteroids)
        {
            asteroid.GetComponent<AsteroidController>().bloomLong = true;
        }
        yield return new WaitForSeconds(magneticFieldDuration);
        foreach (GameObject asteroid in asteroids)
        {
            if (asteroid)
            {
                asteroid.GetComponent<AsteroidController>().bloomLong = false;
            }
            else {
                continue;
            }
        }
    }

    IEnumerator NebulaCloudCoroutine() {
        postProcessScript.chromaticBool = true;
        postProcessScript.grainBool = true;
        distort.SetActive(true);
        yield return new WaitForSeconds(nebulaCloudDuration);
        postProcessScript.chromaticBool = false;
        postProcessScript.grainBool = false;
        distort.SetActive(false);
    }

    IEnumerator AllowControlDelay()
    {
        yield return new WaitForSeconds(allowControlDelay);

        lerpToCenter = false;
        switch (movementMethod)
        {
            case 1:
                playerMovement.mouse_Input = true;
                break;
            case 2:
                playerMovement.mouse_Input_Physics = true;
                break;
            case 3:
                playerMovement.joyStick_WASD_Input = true;
                break;
            default:
                Debug.Log("Input not reset after collision with planet");
                break;
        }
    }
    
    public void EnterName()
    {
        if (enterName.text != string.Empty)
        {
            //int score = UnityEngine.Random.Range(1, 1000);
            InsertScore(enterName.text, scoreVal);

            enterName.text = string.Empty;

            SceneManager.LoadScene("Highscore");

        }
    }
    
    private void GetScore()
    {
        highscores.Clear();
        // using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
        {
            //dbConnection.Open();
            connection.Open();
            //using (IDbCommand dbCmd = dbConnection.CreateCommand())
            using (SqlCommand command = connection.CreateCommand())
            {
                string sqlQuery = "SELECT [PlayerID], [name], [score], [date] FROM [dbo].[HighscoreTable] order by [score] desc, [date] desc";
                // dbCmd.CommandText = sqlQuery;
                command.CommandText = sqlQuery;
                // using (IDataReader reader = dbCmd.ExecuteReader())
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        highscores.Add(new Highscore(reader.GetInt32(0), reader.GetInt32(2), reader.GetString(1), reader.GetDateTime(3)));
                        Debug.Log(reader.GetString(1) + " " + reader.GetInt32(2));

                    }
                    // dbConnection.Close();
                    connection.Close();
                    reader.Close();
                }
            }
        }
    }
    private void InsertScore(string name, int newScore)
    {
        GetScore();
        int hsCount = highscores.Count;
        if (highscores.Count > 0)
        {
            Highscore lowestScore = highscores[highscores.Count - 1];
            if (lowestScore != null && savedScores > 0 && highscores.Count >= savedScores && newScore > lowestScore.Score)
            {
                DeleteScore(lowestScore.ID);
                hsCount--;
            }
        }
        if (hsCount < savedScores)
        {
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    //string sqlQuery = "INSERT INTO[dbo].[HighscoreTable] ( [name], [score], [date] ) VALUES (" + name + ", " + newScore + ", GETDATE())";
                    string sqlQuery = "INSERT INTO [dbo].[HighscoreTable] ( [name], [score] ) VALUES ('" + name + "', " + newScore + ")";
                    //string sqlQuery = String.Format("INSERT INTO[dbo].[HighscoreTable]( [name], [score]) VALUES('{0}', {1}", name, newScore);

                    command.CommandText = sqlQuery;
                    command.ExecuteScalar();
                    connection.Close();


                }
            }
        }

    }
    private void DeleteScore(int id)
    {
        using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
        {
            connection.Open();
            using (SqlCommand command = connection.CreateCommand())
            {
                string sqlQuery = String.Format("DELETE FROM [dbo].[HighscoreTable] WHERE [PlayerID] = {0}", id);
                command.CommandText = sqlQuery;
                command.ExecuteScalar();
                connection.Close();


            }
        }
    }

    private void ShowScores()
    {

        GetScore();
        foreach (GameObject score in GameObject.FindGameObjectsWithTag("Score"))
        {
            Destroy(score);
        }
        for (int i = 0; i < topRanks; i++)
        {
            if (i <= highscores.Count - 1)
            {
                GameObject tempObject = Instantiate(scorePrefab);
                Highscore tmpScore = highscores[i];

                tempObject.GetComponent<HighscoreScript>().SetScore(tmpScore.Name, tmpScore.Score.ToString(), "#" + (i + 1).ToString());
                tempObject.transform.SetParent(scoreParent);
                tempObject.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            }
        }
    }

    private void DeleteExtraScores()
    {
        GetScore();
        if (savedScores <= highscores.Count)
        {
            int deleteCount = highscores.Count - savedScores;
            highscores.Reverse();
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                for (int i = 0; i < deleteCount; i++)
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        string sqlQuery = String.Format("DELETE FROM [dbo].[HighscoreTable] WHERE Playerid = \"{0}\"", highscores[i].ID);
                        command.CommandText = sqlQuery;
                        command.ExecuteScalar();
                        connection.Close();

                    }
                }

            }
        }
    }
    
    public void ScoreStreakCounter()
    {
        if (streakCount >= 10 && streakCount < 20)
        {
            streakMultiplier.enabled = true;
            scoreUpNum = 200;
            streakMultiplier.text = ("2X");
            streakParticle1.SetActive(true);
        }
        else if (streakCount >= 20 && streakCount < 30)
        {
            streakParticle1.SetActive(false);
            streakMultiplier.text = ("3X");
            streakParticle2.SetActive(true);
            scoreUpNum = 300;
        }
        else if (streakCount >= 30)
        {
            streakParticle2.SetActive(false);
            streakMultiplier.text = ("4X");
            streakParticle3.SetActive(true);
            scoreUpNum = 400;
        }
        else if (streakCount < 10)
        {
            streakParticle1.SetActive(false);
            streakParticle2.SetActive(false);
           
            streakParticle3.SetActive(false);
            streakMultiplier.enabled = false;
            scoreUpNum = 100;
        }


    }

}