using UnityEngine;

namespace Equilibrium
{
    public class ArenaComponent : MonoBehaviour
    {
        private SpriteRenderer _mySpriteRenderer;

        private float _time;
        private int _colorIndex;
        private readonly Color[] _colors =
        {
            Color.red,
            Color.cyan,
            Color.yellow,
            Color.blue,
            new Color(1f, .5f, 0f),
            Color.magenta,
            Color.green
        };

        private void Awake()
        {
            _mySpriteRenderer = GetComponent<SpriteRenderer>();
            _colorIndex = Random.Range(0, _colors.Length - 1);
        }

        private void Update()
        {
            _time += .01f * Time.unscaledDeltaTime;
            _mySpriteRenderer.color = new Color(
                Mathf.Lerp(_mySpriteRenderer.color.r, _colors[_colorIndex].r, _time),
                Mathf.Lerp(_mySpriteRenderer.color.g, _colors[_colorIndex].g, _time),
                Mathf.Lerp(_mySpriteRenderer.color.b, _colors[_colorIndex].b, _time)
            );

            if (_mySpriteRenderer.color == _colors[_colorIndex])
            {
                _colorIndex++;
                _time = 0f;
            }

            if (_colorIndex >= _colors.Length) _colorIndex = 0;
        }
    }
}
