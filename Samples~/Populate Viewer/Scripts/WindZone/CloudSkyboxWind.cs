using UnityEngine;

public class CloudSkyboxWind : MonoBehaviour, IWindZone
{
	[SerializeField] Material skyboxMaterial = default;
	[SerializeField] float scroll1Multiplier = 0.6f;
	[SerializeField] float scroll2Multiplier = 0.3f;
	[SerializeField] float scroll1Up = 0.08f;
	[SerializeField] float scroll2Up = 0.02f;
	[SerializeField] AnimationCurve biasCurve = new AnimationCurve(new Keyframe[2] { new Keyframe(0, -2, 0, 0), new Keyframe(1, 2, 0, 0) });

	readonly string SCROLLSPEED_PROPNAME_1 = "_Scroll1", SCROLLSPEED_PROPNAME_2 = "_Scroll2", NOISEBIAS_PROPNAME = "_NoiseBias";

	Vector3 direction;
	float strength;

	float bias = 0.5f;
	public float Bias
	{
		get => bias;
		set
		{
			if (Mathf.Approximately(bias, value))
				return;

			bias = value;

			skyboxMaterial?.SetFloat(NOISEBIAS_PROPNAME, biasCurve.Evaluate(bias));
		}
	}

	private void Start()
	{
		skyboxMaterial?.SetFloat(NOISEBIAS_PROPNAME, biasCurve.Evaluate(bias));
	}

	public void OnWindDirectionChanged(Vector3 newDirection)
	{
		direction = newDirection;
		UpdateMaterialProperties();
	}

	public void OnWindStrengthChanged(float newStrength)
	{
		strength = newStrength;
		UpdateMaterialProperties();
	}

	private void UpdateMaterialProperties()
	{
		skyboxMaterial?.SetVector(SCROLLSPEED_PROPNAME_1, direction * -strength * scroll1Multiplier + Vector3.up * scroll1Up);
		skyboxMaterial?.SetVector(SCROLLSPEED_PROPNAME_2, direction * -strength * scroll2Multiplier + Vector3.up * scroll2Up);
	}
}