using System.Collections.Generic;
using Equilibrium.Messages;
using UnityEngine;

namespace Equilibrium.ScoreBoard
{
    public class ScoreBoardManager : MonoBehaviour, IMessageSubscriber<ScoreUpdatedMessage>
    {
        private GameManager _gameManager;

        private Transform _myTransform;

        private Dictionary<int, ScoreMarkerComponent> _scoreMarkers;

        public void Handle(ScoreUpdatedMessage message)
        {
            UpdateScoreBoard();
        }

        public void GameSetup()
        {
            RoundSetup();
        }

        public void RoundSetup()
        {
            UpdateScoreBoard();
        }

        private void UpdateScoreBoard()
        {
            foreach (var marker in _scoreMarkers)
            {
                marker.Value.SetScore(_gameManager.GetPlayerPoints(marker.Key));
            }
        }

        private void Awake()
        {
            _myTransform = GetComponent<Transform>();

            _scoreMarkers = new Dictionary<int, ScoreMarkerComponent>();
        }

        private void Start()
        {
            var gameManagerObject = GameObject.FindGameObjectWithTag("GameManager");
            _gameManager = gameManagerObject.GetComponent<GameManager>();

            var startPosition = _myTransform.position + new Vector3(0f, 3f, -1f);
            var prefab = Resources.Load("Prefabs/Marker");
            foreach (var id in _gameManager.Players.PlayerIds)
            {
                var obj = Instantiate(prefab, startPosition, Quaternion.identity, _myTransform) as GameObject;
                _scoreMarkers[id] = obj.GetComponent<ScoreMarkerComponent>();
                _scoreMarkers[id].Color = _gameManager.Players.GetPlayerColor(id);
            }

            _gameManager.Subscribe(this);
            UpdateScoreBoard();
        }
    }
}
