using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Subsystems;
using UnityEngine.XR; // Getting the touchpad input for the player 
using UnityEngine.XR.Interaction.Toolkit;



public class ContinuousMovement : MonoBehaviour
{
    public float speed = 1;    // This helps to control the speed of the character
    public XRNode inputSource; // This allows us tp select a particular input source in the Unity inspector tab, in this case we have chosen the left hand.
    public float deadZone = 0.25f;
    public float additionalHeight = 0.2f;
    

    
    private XRRig rig; // To access the players head, so that we move/rotate in the direction that the head is facing 
    private Vector2 inputAxis; // 
    private CharacterController character; // The CharacterController will manage how we can move the when colliding with an object, e.g. stairs or slopes

    // Start is called before the first frame update
    void Start()
    {
        character = GetComponent<CharacterController>(); 
        rig = GetComponent<XRRig>(); // This is where we can access the head rotation function of the rig

    }

    // Update is called once per frame
    void Update()
    {
        // We can listen to an input by accessing the device, and we can access a device using the characteristics of the device using the following code:
        /* InputDevices.GetDevicesWithCharacteristics();*/

        // However another way to access a device is through using the XRNode, this is more straight forward than the the code in line 35
        InputDevice device = InputDevices.GetDeviceAtXRNode(inputSource); 
        device.TryGetFeatureValue(CommonUsages.primary2DAxis, out inputAxis); // We want to listen to the touchpad/Joystick for the movement 

    }
    // For the actual movement of the character we will do in the FixedUpdate function 
    private void FixedUpdate()
    {
        // Checking to see if its outside the Deadzone
        if (inputAxis.magnitude < deadZone)
        {
            //  Vector2 example = new Vector2(0, 0f);
            //  inputAxis = example; (This is one way of creating the deadzone)

            // This is a simpler way of writing the same cdoe for creating the deadzone
            inputAxis = Vector2.zero;
        }
        // This allows us to move in the direction we are facing in the game/ VR World
        Quaternion headYaw = Quaternion.Euler(0, rig.cameraGameObject.transform.eulerAngles.y, 0); 
        Vector3 direction = headYaw * new Vector3(inputAxis.x, 0, inputAxis.y);

        CapsuleFollowHeadSet();
        // Need to find a way to stop player from continuously moving we the speed is set to more than zero and the joystick has not been triggered  
        character.Move(direction * Time.fixedDeltaTime*speed);

    }
    // The character controller needs o follow the headset, other wise it we will phase through the obsticales in the game
    void CapsuleFollowHeadSet()
    {
        character.height = rig.cameraInRigSpaceHeight + additionalHeight;
        Vector3 capsuleCenter = transform.InverseTransformPoint(rig.cameraGameObject.transform.position); // 
        capsuleCenter = new Vector3(capsuleCenter.x, character.height / 2 + character.skinWidth , capsuleCenter.z);
    }
}
