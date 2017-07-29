using Equilibrium.Messages;
using Equilibrium.Players;
using Equilibrium.Sound;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Equilibrium
{
    public class GameManager : MonoBehaviour
    {
        public int MaxScore;
        public int MaxRounds;

        public GameState State { get; private set; }
        public int CurrentRound { get; private set; }

        public SoundManager Sound { get; private set; }
        public PlayerManager Players { get; private set; }

        private MessagePump _messagePump;
        private Transform _myTransform;

        private Dictionary<int, int> _playerPoints;
        private Dictionary<int, int> _playerRounds;

        private ParticleSystem _winParticles;

        private Dictionary<int, Dictionary<InputAction, string>> _inputMappings;

        public void Subscribe(object subscriber)
        {
            _messagePump.Subscribe(subscriber);
        }

        public void Unsubscribe(object subscriber)
        {
            _messagePump.Unsubscribe(subscriber);
        }

        public void AddScore(int playerId, int amount)
        {
            _playerPoints[playerId] += amount;

            if (_playerPoints[playerId] > MaxScore)
            {
                _playerPoints[playerId] = MaxScore;
            }

            if (_playerPoints[playerId] < -MaxScore)
            {
                _playerPoints[playerId] = -MaxScore;
            }

            Sound.PlaySound(amount > 0 ? SoundType.ScorePositive : SoundType.ScoreNegative);
            _messagePump.Publish(new ScoreUpdatedMessage());

            if (_playerPoints[playerId] != 0) return;

            _playerRounds[playerId]++;
            if (_playerRounds[playerId] >= MaxRounds)
            {
                GameWon(playerId);
            }
            else
            {
                RoundWon(playerId);
            }
        }

        public int GetPlayerPoints(int playerId)
        {
            return _playerPoints[playerId];
        }

        public int GetPlayerRounds(int playerId)
        {
            return _playerRounds[playerId];
        }

        private void GameSetup()
        {
            foreach (var id in Players.PlayerIds)
            {
                _playerPoints[id] = Players.GetPlayerStartPoints(id);
                _playerRounds[id] = 0;
            }
            CurrentRound = 0;
            _messagePump.Publish(new ScoreUpdatedMessage());
        }

        private void RoundSetup()
        {
            CurrentRound++;
            foreach (var id in Players.PlayerIds)
            {
                _playerPoints[id] = Players.GetPlayerStartPoints(id);
            }
            _messagePump.Publish(new ScoreUpdatedMessage());
        }

        private void RoundWon(int winningPlayerId)
        {
            var main = _winParticles.main;
            main.startColor = Players.GetPlayerColor(winningPlayerId);
            _winParticles.Play();

            State = GameState.RoundOver;
            _messagePump.Publish(new WinningGameStateChangeMessage(State, winningPlayerId));
            RoundSetup();
        }

        private void GameWon(int winningPlayerId)
        {
            State = GameState.GameOver;
            _messagePump.Publish(new WinningGameStateChangeMessage(State, winningPlayerId));
            GameSetup();
        }

        public Dictionary<InputAction, string> GetInputMappings(int playerId)
        {
            return _inputMappings[playerId];
        }

        private void Awake()
        {
            _messagePump = new MessagePump();

            _myTransform = GetComponent<Transform>();

            _playerPoints = new Dictionary<int, int>();
            _playerRounds = new Dictionary<int, int>();

            // InputManager
            _inputMappings = new Dictionary<int, Dictionary<InputAction, string>>
            {
                {1, new Dictionary<InputAction, string>
                {
                    {InputAction.HorizontalAxis, "Horizontal1"},
                    {InputAction.VerticalAxis, "Vertical1"},
                    {InputAction.Charge, "Thrust1"},
                    {InputAction.Brake, "Brake1"}
                }},
                {2, new Dictionary<InputAction, string>
                {
                    {InputAction.HorizontalAxis, "Horizontal2"},
                    {InputAction.VerticalAxis, "Vertical2"},
                    {InputAction.Charge, "Thrust2"},
                    {InputAction.Brake, "Brake2"}
                }}
            };

            var soundObject = _myTransform.Find("Sound");
            Sound = soundObject.GetComponent<SoundManager>();

            var playersObject = _myTransform.Find("Players");
            Players = playersObject.GetComponent<PlayerManager>();

            // ParticleManager
            var winParticles = GameObject.Find("WinParticles");
            _winParticles = winParticles.GetComponent<ParticleSystem>();
        }

        private void Start()
        {
            GameSetup();
            State = GameState.Title;
            _messagePump.Publish(new GameStateChangeMessage(State));
        }

        private void Update()
        {
            var newState = State;

            if (Input.GetButtonDown("Submit"))
            {
                switch (State)
                {
                    case GameState.Title:
                    case GameState.Pause:
                    case GameState.RoundOver:
                        newState = GameState.Play;
                        break;

                    case GameState.GameOver:
                        newState = GameState.Title;
                        break;

                    case GameState.Play:
                        newState = GameState.Pause;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else if (Input.GetButtonDown("Back"))
            {
                switch (State)
                {
                    case GameState.Title:
                        Application.Quit();
                        break;

                    case GameState.Pause:
                        newState = GameState.Title;
                        break;

                    case GameState.Play:
                    case GameState.RoundOver:
                    case GameState.GameOver:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (newState == State) return;

            State = newState;
            _messagePump.Publish(new GameStateChangeMessage(newState));
        }
    }
}
