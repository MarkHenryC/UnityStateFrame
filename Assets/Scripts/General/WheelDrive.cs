using System;
using UnityEngine;

[Serializable]
public enum DriveType
{
    RearWheelDrive,
    FrontWheelDrive,
    AllWheelDrive
}

public class WheelDrive : MonoBehaviour
{
    [Tooltip("Maximum steering angle of the wheels")]
    public float maxAngle = 30f;
    [Tooltip("Maximum torque applied to the driving wheels")]
    public float maxTorque = 300f;
    public float reverseTorque = -100f;
    [Tooltip("Maximum brake torque applied to the driving wheels")]
    public float brakeTorque = 30000f;
    [Tooltip("If you need the visual wheels to be attached automatically, drag the wheel shape here.")]
    public GameObject wheelShape;

    [Tooltip("The vehicle's speed when the physics engine can use different amount of sub-steps (in m/s).")]
    public float criticalSpeed = 5f;
    [Tooltip("Simulation sub-steps when the speed is above critical.")]
    public int stepsBelow = 5;
    [Tooltip("Simulation sub-steps when the speed is below critical.")]
    public int stepsAbove = 1;
    [Tooltip("For when we've placed the wheels ourselves under the wheelcollider.")]
    public bool wheelsManuallyPlaced;

    [Tooltip("The vehicle's drive type: rear-wheels drive, front-wheels drive or all-wheels drive.")]
    public DriveType driveType;

    public float brakeDownReverseTime;
    public float maxForwardSpeed = 200f;
    public float maxReverseSpeed = -.95f;

    public Action<Collision> OnCollided;
    public Action<Collider> OnTriggeredEnter;
    public Action<Collider> OnTriggeredExit;

    public Transform steeringWheel;
    public float maxSteeringWheelAngle;

    private WheelCollider[] m_Wheels;
    private Vector3 steeringWheelAngle;
    private bool brakeDown, reversing, endReverse;
    private float brakeDownTime;

    private const float MIN_RPM = 10f;

    public void Stop( bool stop = true)
    {
        var rb = GetComponent<Rigidbody>();

        if (stop)
        {
            rb.Sleep();
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        else
            rb.constraints = RigidbodyConstraints.None;
                //RigidbodyConstraints.FreezeRotationX |
                //RigidbodyConstraints.FreezeRotationZ |
                //RigidbodyConstraints.FreezePositionY;
    }

    // Find all the WheelColliders down in the hierarchy.
    private void Start()
    {
        m_Wheels = GetComponentsInChildren<WheelCollider>();

        for (int i = 0; i < m_Wheels.Length; ++i)
        {
            WheelCollider wheel = m_Wheels[i];

            wheel.motorTorque = 0f;
            wheel.steerAngle = 0f;

            // Create wheel shapes only when needed.
            if (wheelShape != null)
            {
                GameObject ws = Instantiate(wheelShape);
                ws.transform.parent = wheel.transform;
            }
        }
    }

    // This is a really simple approach to updating wheels.
    // We simulate a rear wheel drive car and assume that the car is perfectly symmetric at local zero.
    // This helps us to figure our which wheels are front ones and which are rear.
    private void Update()
    {
        m_Wheels[0].ConfigureVehicleSubsteps(criticalSpeed, stepsBelow, stepsAbove);
        float angle = 0f;
        float steeringWheelAngle = 0f;
        float handBrake = 0f;
        float currentRpm = m_Wheels[0].rpm;

#if UNITY_EDITOR
        angle = maxAngle * Input.GetAxis("Horizontal");
        handBrake = Input.GetKey(KeyCode.X) ? brakeTorque : 0;
        steeringWheelAngle = maxSteeringWheelAngle * Input.GetAxis("Horizontal");
#else
        steeringWheelAngle = maxSteeringWheelAngle * QS.ControllerInput.Instance.UnitRotationZ;
        angle = maxAngle * QS.ControllerInput.Instance.UnitRotationZ;
        handBrake = (QS.ControllerInput.Instance.TouchIsDown ? brakeTorque : 0f);
#endif
        float torque = maxTorque * (QS.ControllerInput.Instance.TriggerIsDown ? 1f : 0f);

        if (endReverse)
        {
            Stop(false);
            endReverse = false;
        }

        if (handBrake != 0)
        {
            if (brakeDown) // if already down, accumulate
            {
                brakeDownTime += Time.deltaTime;
                if (brakeDownTime >= brakeDownReverseTime)
                {
                    if (currentRpm > maxReverseSpeed)
                        torque = reverseTorque;
                    else
                        torque = 0f;
                    reversing = true;
                }
            }
            else if (currentRpm < MIN_RPM) // new trigger; initialise
            {
                brakeDown = true;
                brakeDownTime = 0f;
            }
        }
        else
        {
            if (reversing) // quick stop for reverse
            {
                Stop();
                handBrake = brakeTorque;
                torque = 0f;
                endReverse = true;
                reversing = false;
            }
            brakeDown = false;            
        }
        
        if (endReverse)
            return;

        if (steeringWheel)
            steeringWheel.localRotation = Quaternion.Euler(0, 0, -steeringWheelAngle);

        foreach (WheelCollider wheel in m_Wheels)
        {
            // A simple car where front wheels steer while rear ones drive.
            if (wheel.transform.localPosition.z > 0)
                wheel.steerAngle = angle;

            if (wheel.transform.localPosition.z < 0)
            {
                if (endReverse)
                {
                    wheel.brakeTorque = Mathf.Infinity;
                    endReverse = false;
                }
                else if (reversing)
                    wheel.brakeTorque = 0f;
                else
                    wheel.brakeTorque = handBrake;
            }

            if (currentRpm > maxForwardSpeed)
                torque = 0f;

            if (wheel.transform.localPosition.z < 0 && driveType != DriveType.FrontWheelDrive)
            {
                wheel.motorTorque = torque;
            }

            if (wheel.transform.localPosition.z >= 0 && driveType != DriveType.RearWheelDrive)
            {
                wheel.motorTorque = torque;
            }            

            // Update visual wheels if any.
            if (wheelShape || wheelsManuallyPlaced)
            {
                Quaternion q;
                Vector3 p;
                wheel.GetWorldPose(out p, out q);

                // Assume that the only child of the wheelcollider is the wheel shape.
                Transform shapeTransform = wheel.transform.GetChild(0);
                shapeTransform.position = p;
                shapeTransform.rotation = q;
            }
        }
        //Debug.Log(m_Wheels[0].rpm);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (OnTriggeredEnter != null)
            OnTriggeredEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (OnTriggeredExit != null)
            OnTriggeredExit(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (OnCollided != null)
            OnCollided(collision);
    }
}
