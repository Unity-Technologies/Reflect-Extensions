
using UnityEngine;

public interface IWindZone
{
    void OnWindDirectionChanged(Vector3 newDirection);

    void OnWindStrengthChanged(float newStrength);
}