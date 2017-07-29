using System;
using System.Collections.Generic;
using System.Linq;
using Equilibrium.Messages;
using UnityEngine;

namespace Equilibrium.Players
{
    public class PlayerManager : MonoBehaviour, IMessageSubscriber<GameStateChangeMessage>
    {
        private GameManager _gameManager;

        private Dictionary<int, PlayerComponent> _players;

        private bool _isPaused;
        private Dictionary<int, Vector3> _playerStartPositions;
        private Dictionary<int, Quaternion> _playerStartRotations;
        private Dictionary<int, Vector2> _playerVelocities;

        public int[] PlayerIds { get; private set; }

        public Color GetPlayerColor(int playerId)
        {
            return _players[playerId].Color;
        }

        public Vector2 GetPlayerPosition(int playerId)
        {
            return _players[playerId].Position;
        }

        public int GetPlayerStartPoints(int playerId)
        {
            return _players[playerId].StartPoints;
        }

        public void Handle(GameStateChangeMessage message)
        {
            bool newIsPaused;
            switch (message.NewState)
            {
                case GameState.Title:
                case GameState.Pause:
                    newIsPaused = true;
                    break;

                case GameState.RoundOver:
                    newIsPaused = true;
                    RoundSetup();
                    break;

                case GameState.GameOver:
                    newIsPaused = true;
                    GameSetup();
                    break;

                case GameState.Play:
                    newIsPaused = false;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (newIsPaused == _isPaused) return;

            _isPaused = newIsPaused;
            foreach (var p in _players)
            {
                if (_isPaused)
                {
                    _playerVelocities[p.Value.Id] = p.Value.Velocity;
                    p.Value.SetActive(false);
                }
                else
                {
                    p.Value.SetActive(true);
                    p.Value.Velocity = _playerVelocities[p.Value.Id];
                }
            }
        }

        private void GameSetup()
        {
            RoundSetup();
        }

        private void RoundSetup()
        {
            _isPaused = true;

            foreach (var p in _players)
            {
                p.Value.Position = _playerStartPositions[p.Value.Id];
                p.Value.Rotation = _playerStartRotations[p.Value.Id];
                p.Value.Velocity = Vector2.zero;

                _playerVelocities[p.Value.Id] = Vector2.zero;
                p.Value.SetActive(false);

                p.Value.ResetCharge();
                p.Value.EndBraking();
            }
        }

        private void Awake()
        {
            var playerObjects = GameObject.FindGameObjectsWithTag("Player");
            _players = new Dictionary<int, PlayerComponent>();
            _playerStartPositions = new Dictionary<int, Vector3>();
            _playerStartRotations = new Dictionary<int, Quaternion>();
            _playerVelocities = new Dictionary<int, Vector2>();
            foreach (var po in playerObjects)
            {
                var player = po.GetComponent<PlayerComponent>();
                _players[player.Id] = player;
                _playerStartPositions[player.Id] = player.Position;
                _playerStartRotations[player.Id] = player.Rotation;
                _playerVelocities[player.Id] = Vector2.zero;
            }

            PlayerIds = _players.Keys.OrderBy(id => id).ToArray();
        }

        private void Start()
        {
            var gameManagerObject = GameObject.FindGameObjectWithTag("GameManager");
            _gameManager = gameManagerObject.GetComponent<GameManager>();
            _gameManager.Subscribe(this);

            GameSetup();
        }
    }
}
