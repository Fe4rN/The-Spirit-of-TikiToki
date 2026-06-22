using UnityEngine;

namespace TikiToki.Gameplay
{
    public class SunsetLogic : MonoBehaviour
    {
        [Header("Time Configuration")]
        public float durationMinutes = 4f;

        [Header("Rotation Angles (X Axis)")]
        public float startAngle = 30f;
        public float endAngle = -170f;

        private float _elapsedTime = 0f;
        private float _durationSeconds;

        private float _fixedY;
        private float _fixedZ;

        private GameManager _gameManager;

        void Start()
        {
            _gameManager = GameManager.Instance;

            if (_gameManager != null)
            {
                _durationSeconds = _gameManager.maxTime;
            }
            else
            {
                _durationSeconds = durationMinutes * 60f;
            }

            _fixedY = transform.eulerAngles.y;
            _fixedZ = transform.eulerAngles.z;

            transform.rotation = Quaternion.Euler(startAngle, _fixedY, _fixedZ);
        }

        void Update()
        {
            if (_elapsedTime < _durationSeconds)
            {
                _elapsedTime += Time.deltaTime;

                float t = _elapsedTime / _durationSeconds;
                float currentX = Mathf.Lerp(startAngle, endAngle, t);

                transform.rotation = Quaternion.Euler(currentX, _fixedY, _fixedZ);
            }
            else
            {
                transform.rotation = Quaternion.Euler(endAngle, _fixedY, _fixedZ);
            }
        }
    }
}
