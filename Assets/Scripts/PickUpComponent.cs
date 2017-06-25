using System;
using UnityEngine;

namespace Equilibrium
{
    public class PickUpComponent : MonoBehaviour
    {
        public int Id;
        public int ScoreAmount;
        public PickUpManager PickUpManager;

        public Color PositiveColor;
        public Color NegativeColor;

        private GameManager _gameManager;
        private SoundManager _soundManager;

        private Transform _myTransform;
        private SpriteRenderer _mySpriteRenderer;
        private TextMesh _myTextMesh;

        private void UpdateScore()
        {
            var sign = ScoreAmount > 0 ? "+" : "-";
            _myTextMesh.text = sign + Math.Abs(ScoreAmount);

            var color = ScoreAmount >= 0 ? PositiveColor : NegativeColor;
            _mySpriteRenderer.color = color;
            _myTextMesh.color = color;
        }

        private void Awake()
        {
            var gm = GameObject.FindGameObjectWithTag("GameManager");
            _gameManager = gm.GetComponent<GameManager>();
            _soundManager = gm.GetComponent<SoundManager>();

            _myTransform = GetComponent<Transform>();

            var sprite = _myTransform.Find("Sprite");
            if (sprite == null)
            {
                Debug.LogError($"Initialization error: Pick Up {Id} does not have a child named Sprite.");
                return;
            }

            var outline = sprite.Find("Outline");
            if (outline == null)
            {
                Debug.LogError($"Initialization error: Sprite for Pick Up {Id} does not have a child named Outline.");
                return;
            }

            _mySpriteRenderer = outline.GetComponent<SpriteRenderer>();
            if (_mySpriteRenderer == null)
            {
                Debug.LogError($"Initialization error: Outline for Pick Up {Id} does not have a SpriteRenderer component.");
                return;
            }

            var text = sprite.Find("Text");
            if (text == null)
            {
                Debug.LogError($"Initialization error: Sprite for Pick Up {Id} does not have a child named Text.");
                return;
            }

            _myTextMesh = text.GetComponent<TextMesh>();
            if (_myTextMesh == null)
            {
                Debug.LogError($"Initialization error: Text for Pick Up {Id} does not have a TextMesh component.");
                return;
            }

            UpdateScore();
        }

        private void OnEnable()
        {
            UpdateScore();
        }

        private void OnTriggerEnter2D(Collider2D collider2D)
        {
            var player = collider2D.gameObject.GetComponent<PlayerComponent>();
            if (player == null) return;

            _gameManager.AddScore(player.Id, ScoreAmount);
            if (ScoreAmount > 0)
            {
                _soundManager.PlaySound(SoundType.ScorePositive);
            }
            else
            {
                _soundManager.PlaySound(SoundType.ScoreNegative);
            }

            gameObject.SetActive(false);
        }
    }
}
