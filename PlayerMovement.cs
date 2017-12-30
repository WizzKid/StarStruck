using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

	Rigidbody rb;

    // Mouse follow
	public bool mouse_Input;
    public float mouse_Input_Speed = 40.0f;

    // Physics mouse follow
    public bool mouse_Input_Physics;
    // Physics speed modifiers
        public float physics_Speed_Mod = 2.2f;
        public float physics_CursorDistance_Slow = 2.0f;
        public float physics_CursorRange_Slow = 1.2f;

    // Joystick, WASD, and mobile*(eventually)
    public bool joyStick_WASD_Input;
        // Joystick speed modifier
        public float Joystick_Speed_Mod = 3.0f;
        public float Joystick_Drag = 1.05f;

    // Rigidbody variables
    private float h;
	private float v;
    Vector3 movement;
    public float XYSpeed = 20.0f;
    public float XYSpeedLimit = 20.0f;

    // Screen border variables
    private float dist;
	private float leftBorder;
	private float rightBorder;
	private float bottomBorder;
	private float topBorder;
	private bool leftOOB;
	private bool rightOOB;
	private bool bottomOOB;
	private bool topOOB;

    // Boost
    public float boostMultiplier = 3.5f;
    private bool boost;
    public float boostDuration = 1.0f;
    public float boostCD = 2.0f;
    private int movementMethod;
    private float boostDistance;
    private float distanceTravelled = 0;
    private Vector3 lastPosition;
    private bool ended;

    // MOBILE
    public bool mobileMode;
    public Vector2 startPos;
    public Vector2 direction;
    public bool directionChosen;

    // Use this for initialization
    void Awake () {
		rb = GetComponent<Rigidbody>();
		SetScreenBorder ();

		leftOOB = false;
		rightOOB = false;
		bottomOOB = false;
        topOOB = false;

        // Random rotation
        rb.rotation = Random.rotation;
        rb.AddTorque(new Vector3(Random.Range(-100.0f, 100.0f), Random.Range(-100.0f, 100.0f), Random.Range(-100.0f, 100.0f)));

        // Boost
        boost = false;
        ended = false;
    }
	
	// Update is called once per frame
	void Update () {

        if (InputDetector.PS4_Controller == 1)
        {
            mouse_Input = false;
            joyStick_WASD_Input = true;
        }
        else if (InputDetector.Xbox_One_Controller == 1)
        {
            mouse_Input = false;
            joyStick_WASD_Input = true;
        }
        else if (InputDetector.Mouse == 1)
        {
            print("Mouse input activated");
            mouse_Input = true;
            joyStick_WASD_Input = false;
        }
        // Mobile
        else if (mobileMode)
        {
            mouse_Input = true;
            joyStick_WASD_Input = false;

            // Track a single touch as a direction control.
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                // Handle finger movements based on touch phase.
                switch (touch.phase)
                {
                    // Record initial touch position.
                    case TouchPhase.Began:
                        startPos = touch.position;
                        directionChosen = false;
                        break;

                    // Determine direction by comparing the current touch position with the initial one.
                    case TouchPhase.Moved:
                        direction = touch.position - startPos;
                        break;

                    // Report that a direction has been chosen when the finger is lifted.
                    case TouchPhase.Ended:
                        directionChosen = true;
                        break;
                }
            }
            if (directionChosen)
            {
                if (direction.x < 0.1f && direction.x > -0.1f && !boost)
                {
                    boost = true;
                    ended = false;
                    XYSpeed *= boostMultiplier;
                    StartCoroutine(BoostCD());
                }
            }
        }

        if (mouse_Input && Input.GetKeyDown(KeyCode.Mouse0) && !boost || InputDetector.PS4_Controller == 1 && Input.GetKeyDown(KeyCode.Joystick1Button1) && !boost || InputDetector.Xbox_One_Controller == 1 && Input.GetKeyDown(KeyCode.Joystick1Button0) && !boost)
        {
            boost = true;
            ended = false;
            XYSpeed *= boostMultiplier;
            
            // Cooldown
            StartCoroutine(BoostCD());
        }
    }

	// Handles all physics movement
	void FixedUpdate ()
	{
        // Cursor follow - physics, but sets velocity every frame rather than maintaining and adding to it
        if (mouse_Input)
        {
            if (mobileMode)
            {
                // Get postion of mouse
                Vector3 newPos = Input.GetTouch(0).position;
                // Find distance between player and camera in z
                newPos.z = dist;
                // Translate vector to world space
                newPos = Camera.main.ScreenToWorldPoint(newPos);
                // Stops X/Y movement if on mouse cursor
                if (Vector3.Distance(newPos, transform.position) < 0.25f)
                {
                    // Very close to cursor speed based on distance
                    rb.velocity = ((newPos - transform.position).normalized * (XYSpeed * mouse_Input_Speed * Time.deltaTime) * Vector3.Distance(newPos, transform.position));
                }
                else if (Vector3.Distance(newPos, transform.position) < 1f)
                {
                    // Near cursor slow
                    rb.velocity = ((newPos - transform.position).normalized * (XYSpeed * mouse_Input_Speed * Time.deltaTime) / 1.7f);
                }
                else
                {
                    // Normal speed
                    rb.velocity = ((newPos - transform.position).normalized * (XYSpeed * mouse_Input_Speed * Time.deltaTime));
                }
            }
            else {
                // Get postion of mouse
                Vector3 newPos = Input.mousePosition;
                // Find distance between player and camera in z
                newPos.z = dist;
                // Translate vector to world space
                newPos = Camera.main.ScreenToWorldPoint(newPos);
                // Stops X/Y movement if on mouse cursor
                if (Vector3.Distance(newPos, transform.position) < 0.25f)
                {
                    // Very close to cursor speed based on distance
                    rb.velocity = ((newPos - transform.position).normalized * (XYSpeed * mouse_Input_Speed * Time.deltaTime) * Vector3.Distance(newPos, transform.position));
                }
                else if (Vector3.Distance(newPos, transform.position) < 1f)
                {
                    // Near cursor slow
                    rb.velocity = ((newPos - transform.position).normalized * (XYSpeed * mouse_Input_Speed * Time.deltaTime) / 1.7f);
                }
                else
                {
                    // Normal speed
                    rb.velocity = ((newPos - transform.position).normalized * (XYSpeed * mouse_Input_Speed * Time.deltaTime));
                }
            }
        }

        // Cursor follow - physics based, addForce
        if (mouse_Input_Physics)
        {
            // Get postion of mouse
            Vector3 newPos = Input.mousePosition;
            // Find distance between player and camera in z
            newPos.z = dist;
            // Translate vector to world space
            newPos = Camera.main.ScreenToWorldPoint(newPos);
            // Vector 3 Direction
            Vector3 direction = (newPos - transform.position).normalized;
            if (rb.velocity.x < XYSpeedLimit || rb.velocity.y < XYSpeedLimit)
            {
                // Add force to player in vector towards world space mouse with desired z
                rb.AddForce(transform.position + direction * XYSpeed * physics_Speed_Mod);
            }
            // Slows down player if near mouse to stop orbiting.
            if (Vector3.Distance(newPos, transform.position) < physics_CursorDistance_Slow)
            {
                rb.velocity = rb.velocity / physics_CursorRange_Slow;
                // Stops X/Y movement if on mouse cursor
                if (Vector3.Distance(newPos, transform.position) < 0.25f)
                {
                    rb.velocity = new Vector3(0, 0, rb.velocity.z);
                }
            }
        }

        // WASD movement - addForce with speed limits
        if (joyStick_WASD_Input)
        {
            // Keeps player on the horizontal axis which is mapped to the "a" and "d" keys. Left is negative. Right is positive.
            h = Input.GetAxisRaw("Horizontal");
            // Zero out input if trying to go out of bounds "OOB"
            if (leftOOB && h < 0.0f)
            {
                h = 0.0f;
            }
            else if (rightOOB && h > 0.0f)
            {
                h = 0.0f;
            }

            // Keeps player on the vertical axis in this case z space. "w" and "s" but does not allow for y space movement which would be a jump.
            v = Input.GetAxisRaw("Vertical");
            // Zero out input if trying to go out of bounds "OOB"
            if (bottomOOB && v < 0.0f)
            {
                v = 0.0f;
            }
            else if (topOOB && v > 0.0f)
            {
                v = 0.0f;
            }

            // Add force if under speed limit
            if (h > 0 && rb.velocity.x < XYSpeedLimit || h < 0 && rb.velocity.x > -XYSpeedLimit)
            {
                movement.Set(h * XYSpeed * Joystick_Speed_Mod, 0, 0);
                rb.AddForce(movement, ForceMode.Force);
            }
            if (v > 0 && rb.velocity.y < XYSpeedLimit || v < 0 && rb.velocity.y > -XYSpeedLimit)
            {
                movement.Set(0, v * XYSpeed * Joystick_Speed_Mod, 0);
                rb.AddForce(movement, ForceMode.Force);
            }
            // Drag
            rb.velocity = new Vector3(rb.velocity.x / Joystick_Drag, rb.velocity.y / Joystick_Drag, rb.velocity.z);
        }
        
        // Keep speed within limits
        // x
        if (rb.velocity.x > XYSpeedLimit)
        {
            rb.velocity = new Vector3(XYSpeedLimit, rb.velocity.y, rb.velocity.z);
        }
        else if (rb.velocity.x < -XYSpeedLimit)
        {
            rb.velocity = new Vector3(-XYSpeedLimit, rb.velocity.y, rb.velocity.z);
        }

        // y
        if (rb.velocity.y > XYSpeedLimit)
        {
            rb.velocity = new Vector3(rb.velocity.x, XYSpeedLimit, rb.velocity.z);
        }
        else if (rb.velocity.y < -XYSpeedLimit)
        {
            rb.velocity = new Vector3(rb.velocity.x, -XYSpeedLimit, rb.velocity.z);
        }
        
        // z
        if (transform.position.z < -1.0f  &&  rb.velocity.z < 0 || transform.position.z > 1.0f && rb.velocity.z > 0)
        {
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, -rb.velocity.z);
        }
        else {
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z / 1.2f);
        }

        if (transform.position.z == 0.0f) {
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, 0.0f);
        }

        // Screen border restriction
        // Vector pointing out of bounds is zeroed out
        if ((transform.position.x <= leftBorder) && rb.velocity.x < 0f)
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, rb.velocity.z);
            leftOOB = true;
        }
        else if (transform.position.x > leftBorder)
        {
            leftOOB = false;
        }
        if ((transform.position.x >= rightBorder) && rb.velocity.x > 0f)
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, rb.velocity.z);
            rightOOB = true;
        }
        else if (transform.position.x < rightBorder)
        {
            rightOOB = false;
        }
        if ((transform.position.y <= bottomBorder) && rb.velocity.y < 0f)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            bottomOOB = true;
        }
        else if (transform.position.y > bottomBorder)
        {
            bottomOOB = false;
        }
        if ((transform.position.y >= topBorder) && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            topOOB = true;
        }
        else if (transform.position.y < topBorder)
        {
            topOOB = false;
        }
    }

    IEnumerator BoostCD()
    {
        yield return new WaitForSeconds(boostDuration);

        XYSpeed /= boostMultiplier;
        
        yield return new WaitForSeconds(boostCD);
        boost = false;
    }

    // Call to update new screen border
    public void SetScreenBorder () {
		dist = (transform.position - Camera.main.transform.position).z;
		leftBorder = Camera.main.ViewportToWorldPoint (new Vector3 (0, 0, dist)).x + transform.localScale.x;
		rightBorder = Camera.main.ViewportToWorldPoint (new Vector3 (1, 0, dist)).x - transform.localScale.x;
		bottomBorder = Camera.main.ViewportToWorldPoint (new Vector3 (0, 0, dist)).y + transform.localScale.y;
		topBorder = Camera.main.ViewportToWorldPoint (new Vector3 (0, 1, dist)).y - transform.localScale.y;
	}
}
