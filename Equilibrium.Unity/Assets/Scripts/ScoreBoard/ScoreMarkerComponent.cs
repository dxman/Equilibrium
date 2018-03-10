using UnityEngine;

namespace Equilibrium.ScoreBoard
{
    public class ScoreMarkerComponent : MonoBehaviour
    {
        private Transform _myTransform;
        private SpriteRenderer _mySpriteRenderer;

        private Vector3 _targetPosition;

        public Color Color
        {
            get { return _mySpriteRenderer.color; }

            set { _mySpriteRenderer.color = value; }
        }

        public void SetScore(int points)
        {
            _targetPosition.x = points * 10;
        }

        private void Awake()
        {
            _myTransform = GetComponent<Transform>();
            _mySpriteRenderer = GetComponent<SpriteRenderer>();

            _targetPosition = _myTransform.localPosition;
        }

        private void Update()
        {
            _myTransform.localPosition = Vector3
                .MoveTowards(_myTransform.localPosition, _targetPosition, 100 * Time.unscaledDeltaTime);
        }
    }
}
