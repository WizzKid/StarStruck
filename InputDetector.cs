using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputDetector : MonoBehaviour {

    // Controller input
    public static int Xbox_One_Controller = 0;
    public static int PS4_Controller = 0;
    public static int Mouse = 0;
    private string[] names;

    // Use this for initialization
    void Awake () {
        // Use correct input method
        InvokeRepeating("TestInput", 0, 2.0f);
    }

    public void TestInput()
    {
        names = Input.GetJoystickNames();

        if (names.Length > 0)
        {
            for (int x = 0; x < names.Length; x++)
            {
                if (names[x].Length <= 20)
                {
                    print("PS4 CONTROLLER IS CONNECTED");
                    PS4_Controller = 1;
                    Xbox_One_Controller = 0;
                }
                if (names[x].Length >= 32)
                {
                    print("XBOX ONE OR 360 CONTROLLER IS CONNECTED");
                    PS4_Controller = 0;
                    Xbox_One_Controller = 1;
                }
                if (names[x].Length < 2)
                {
                    PS4_Controller = 0;
                    Xbox_One_Controller = 0;
                    Mouse = 1;
                }
            }
        }
    }
}
