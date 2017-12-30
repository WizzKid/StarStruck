using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public Texture2D mouseTexture;

    // For when we call player death
    public float deathTimer;

    // UI variables
    public Text playerMassText;
    public Text targetMassText;
    public Slider distanceSlider;

    private string[] SIPrefix = new string[] { "BIG", "U", "Q", "B", "Y", "Z", "E", "P", "T", "G", "M" };
    private int currentPlayerSIPrefix = 10;
    private int currentTargetSIPrefix = 10;

    // Target Variables
    public GameObject Player;
    public GameObject[] Target;
    public GameObject[] RandomTarget;
    private GameObject currentTargetGameObj;
    public static int currentTarget;
    private bool randomTargetsBool;

    public float targetScaling = 3.0f;

    private float targetStartingDistance;
    public static bool planetDestroyed;

    // HUD
    [Header("HUD")]
    public GameObject[] planetUI;
    public GameObject[] alienUI;

    public Image playerMass1;
    public Image playerMass2;
    public Image playerMass3;
    public Image playerMass4;
    public Image playerMass5;
    public Image playerMass6;
    private float currentMass;

    // Sound
    public AudioPlayer Sound;
    private int currentSound;
    

    // Use this for initialization
    void Awake () {
        Cursor.lockState = CursorLockMode.None;
        Cursor.SetCursor(mouseTexture, new Vector2(mouseTexture.width / 2, mouseTexture.height / 2), CursorMode.Auto);

        currentTarget = 0;
        currentTargetGameObj = Instantiate(Target[currentTarget], new Vector3(0.0f, 0.0f, 1200.0f), Quaternion.identity);

        // Set target Mass UI first time
        targetMassText.text = MassController.displayed_TargetMass.ToString() + SIPrefix[currentTargetSIPrefix];

        targetStartingDistance = currentTargetGameObj.transform.position.z - Player.transform.position.z;
        AsteroidController.currentLarge = 200;
        planetDestroyed = false;
        randomTargetsBool = false;
        currentMass = MassController.playerMass;
        playerMassText.text = MassController.displayed_PlayerMass.ToString() + SIPrefix[currentPlayerSIPrefix];
    }

    // Checks for input to lock mouse to screen
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    // Update is called once per frame
    void FixedUpdate () {
        //Debug.Log("Player Mass = " + MassController.playerMass + " Displayed = " + MassController.displayed_PlayerMass + SIPrefix[currentPlayerSIPrefix] + " Target Mass = " + MassController.static_targetMass + " Displayed = " + MassController.displayed_TargetMass + SIPrefix[currentTargetSIPrefix]);

        if (planetDestroyed)
        {
            if (currentTarget <= 3)
            {
                currentSound = 2; // Sound for terrestrial planet collision
            }
            else if (currentTarget <= 7)
            {
                currentSound = 3; // Sound for gas planet collision
            }
            else {
                currentSound = 4; // Sound for sun collision
            }
            Sound.PlayHazardSfx(currentSound); // Sound

            // Old UI off before setting new target
            if (!randomTargetsBool) // Target UI
            {
                planetUI[currentTarget].SetActive(false);
            }
            else { // Alien UI
                alienUI[currentTarget].SetActive(false);
            }

            // Select which target to spawn
            if (currentTarget + 1 >= Target.Length) // Turn on random alien target selection
            {
                randomTargetsBool = true; // This never needs to get set to false
            }
            if (randomTargetsBool) // Set alien target
            {
                int temp = currentTarget;
                while (currentTarget == temp) // Random alien target but not the same alien target
                {
                    currentTarget = Random.Range(0, RandomTarget.Length);
                }
            }
            else // Set regular target
            {
                currentTarget++;
            }

            if (!randomTargetsBool) // In order target
            {
                currentTargetGameObj = Instantiate(Target[currentTarget], new Vector3(0.0f, 0.0f, 1200.0f), Quaternion.identity);
                targetStartingDistance = Target[currentTarget].transform.position.z - Player.transform.position.z;
                // New UI on
                planetUI[currentTarget].SetActive(true);
            }
            else { // Random target
                currentTargetGameObj = Instantiate(RandomTarget[currentTarget], new Vector3(0.0f, 0.0f, 1200.0f), Quaternion.identity);
                targetStartingDistance = RandomTarget[currentTarget].transform.position.z - Player.transform.position.z;
                // New UI on
                alienUI[currentTarget].SetActive(true);
            }

            if (targetScaling < 3.2f)
            {
                targetScaling += 0.02f;
            }

            // Set target mass and UI
            MassController.static_targetMass = MassController.playerMass * targetScaling; // Balance this when tiles scale in mass tiers
                MassController.displayed_TargetMass = MassController.static_targetMass; // Update target mass

                // Make SIPrefix go up and number decrease by 1000 if over 1000
                currentTargetSIPrefix = 10;
                while (MassController.displayed_TargetMass > 1000 && currentTargetSIPrefix > 0)
                {
                    MassController.displayed_TargetMass /= 1000;
                    currentTargetSIPrefix--;
                }
                // Round to 1 decimal
                MassController.displayed_TargetMass = Mathf.Round(MassController.displayed_TargetMass * 10) / 10; 

                // Update target Mass UI
                targetMassText.text = MassController.displayed_TargetMass.ToString() + SIPrefix[currentTargetSIPrefix];

            // Complete
            planetDestroyed = false;
        } // END:: planet destroyed

        if (currentMass != MassController.playerMass)
        {
            // Set Display mass values
            SetDisplayedMassValues();
            // Only show 1 decimal
            MassController.displayed_PlayerMass = Mathf.Round(MassController.displayed_PlayerMass * 10) / 10;

            // Set UI
            // Mass
            playerMassText.text = MassController.displayed_PlayerMass.ToString() + SIPrefix[currentPlayerSIPrefix];
        }
         
            // Distance
            distanceSlider.value = 1.0f - (currentTargetGameObj.transform.position.z - Player.transform.position.z) / targetStartingDistance;

        // Set player img for bottom left
        SetPlayerImg();

        // Player died, no highscore
        if (MassController.playerMass <= 0)
        {
            StartCoroutine(DeathDelay());
        }
    }

    void SetDisplayedMassValues()
    {
        MassController.displayed_PlayerMass = MassController.playerMass;

        int temp = 10;
        // player mass | growing
        while (MassController.displayed_PlayerMass > 1000.0f && currentPlayerSIPrefix > 0)
        {
            MassController.displayed_PlayerMass /= 1000.0f;
            temp--;
        }
        // player mass | shrinking
        if (MassController.displayed_PlayerMass <= 0 && currentPlayerSIPrefix != 10)
        {
            MassController.displayed_PlayerMass *= 1000.0f;
            temp++;
        }

        currentPlayerSIPrefix = temp;
    }

    void SetPlayerImg()
    {
        if (MassController.playerMass > MassController.static_targetMass)
        {
            playerMass1.enabled = false;
            playerMass2.enabled = false;
            playerMass3.enabled = false;
            playerMass4.enabled = false;
            playerMass5.enabled = false;
            playerMass6.enabled = true;
        }

        else if (MassController.playerMass > (MassController.static_targetMass * 5) / 6)
        {
            playerMass1.enabled = false;
            playerMass2.enabled = false;
            playerMass3.enabled = false;
            playerMass4.enabled = false;
            playerMass5.enabled = true;
            playerMass6.enabled = false;
        }

        else if (MassController.playerMass > (MassController.static_targetMass * 2) / 3)
        {
            playerMass1.enabled = false;
            playerMass2.enabled = false;
            playerMass3.enabled = false;
            playerMass4.enabled = true;
            playerMass5.enabled = false;
            playerMass6.enabled = false;
        }

        else if (MassController.playerMass > (MassController.static_targetMass) / 2)
        {
            playerMass1.enabled = false;
            playerMass2.enabled = false;
            playerMass3.enabled = true;
            playerMass4.enabled = false;
            playerMass5.enabled = false;
            playerMass6.enabled = false;
        }

        else if (MassController.playerMass > (MassController.static_targetMass) / 3)
        {
            playerMass1.enabled = false;
            playerMass2.enabled = true;
            playerMass3.enabled = false;
            playerMass4.enabled = false;
            playerMass5.enabled = false;
            playerMass6.enabled = false;
        }

        else {
            playerMass1.enabled = true;
            playerMass2.enabled = false;
            playerMass3.enabled = false;
            playerMass4.enabled = false;
            playerMass5.enabled = false;
            playerMass6.enabled = false;
        }
    }

    // In case we want a death delay after the player dies, call this public function from the player death before disabling
    public IEnumerator DeathDelay()
    {
        Debug.Log("DeathDelay start");
        yield return new WaitForSeconds(deathTimer);
        SceneManager.LoadScene(0);
    }
}
