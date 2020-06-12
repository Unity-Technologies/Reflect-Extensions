using UnityEngine;

[RequireComponent(typeof(WindZone))]
public class WindZoneNotifications : MonoBehaviour
{
    IWindZone[] windZoneSubscribers;

    WindZone windZone;

    Vector3 windDirection = Vector3.down;
    public Vector3 WindDirection
    {
        get => windDirection;
        set
        {
            if (Vector3.Angle(windDirection, value) < 0.01f)
                return;

            windDirection = value;

            foreach (IWindZone i in windZoneSubscribers)
                i.OnWindDirectionChanged(windDirection);
        }
    }

    float windStrength = -1;
    public float WindStrength
	{
        get => windStrength;
        set
		{
            if (Mathf.Approximately(windStrength, value))
                return;

            windStrength = value;

            foreach (IWindZone i in windZoneSubscribers)
                i.OnWindStrengthChanged(windStrength);
        }
	}

    private void Awake()
    {
        windZoneSubscribers = GetComponentsInChildren<IWindZone>();
        enabled = windZoneSubscribers.Length != 0;

        windZone = GetComponent<WindZone>();
    }

    private void Update()
    {
        WindStrength = windZone.windMain;
        WindDirection = transform.forward;
    }
}