using UnityEngine;

namespace _GAME.Scripts.Battle.Level.Runner
{
    public class Asteroid: MonoBehaviour
    {
        [SerializeField] private Vector3 spinAxis = Vector3.up;
        [SerializeField] private float spinDegPerSec = 25f;

        [SerializeField] private float driftAmplitude = 0.3f;
        [SerializeField] private float driftFrequency = 0.45f;

        private Vector3 _baseLocalPos;
        private float _seed;

        private void Awake()
        {
            _baseLocalPos = transform.localPosition;
            _seed = Random.value * 1000f;

            if (spinAxis.sqrMagnitude < 0.0001f)
                spinAxis = Vector3.up;
            spinAxis.Normalize();
        }

        public void SetupRandom(Vector2 spinDegPerSecRange, Vector2 driftAmplitudeRange, Vector2 driftFrequencyRange)
        {
            spinDegPerSec = Random.Range(spinDegPerSecRange.x, spinDegPerSecRange.y);
            driftAmplitude = Random.Range(driftAmplitudeRange.x, driftAmplitudeRange.y);
            driftFrequency = Random.Range(driftFrequencyRange.x, driftFrequencyRange.y);

            spinAxis = Random.onUnitSphere;
        }

        private void Update()
        {
            transform.Rotate(spinAxis, spinDegPerSec * Time.deltaTime, Space.Self);

            float t = (Time.time + _seed) * driftFrequency;
            float dx = Mathf.Sin(t) * driftAmplitude;
            float dy = Mathf.Sin(t * 1.37f) * driftAmplitude * 0.6f;
            float dz = Mathf.Sin(t * 0.73f) * driftAmplitude * 0.4f;

            transform.localPosition = _baseLocalPos + new Vector3(dx, dy, dz);
        }

    }
}