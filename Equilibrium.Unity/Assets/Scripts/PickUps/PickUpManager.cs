using System;
using System.Collections.Generic;
using System.Linq;
using Equilibrium.Messages;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Equilibrium.PickUps
{
    public class PickUpManager : MonoBehaviour, IMessageSubscriber<GameStateChangeMessage>
    {
        public float SpawnRate;
        public int StartSpawn;
        public int MaxSpawn;
        public float SpawnSpacing;

        private GameManager _gameManager;
        private Transform _myTransform;

        private List<PickUpComponent> _pickUps;

        public bool _isPaused;
        private GameState _oldState;
        public float _spawnCounter;

        public void Handle(GameStateChangeMessage message)
        {
            bool newIsPaused;
            switch (message.NewState)
            {
                case GameState.Title:
                case GameState.Pause:
                case GameState.RoundOver:
                case GameState.GameOver:
                    newIsPaused = true;
                    break;

                case GameState.Play:
                    switch (_oldState)
                    {
                        case GameState.Title:
                            GameSetup();
                            break;
                        case GameState.RoundOver:
                            RoundSetup();
                            break;
                        case GameState.Play:
                            break;
                        case GameState.Pause:
                            break;
                        case GameState.GameOver:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    newIsPaused = false;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            _oldState = message.NewState;

            if (newIsPaused == _isPaused) return;

            _isPaused = newIsPaused;
            foreach (var p in _pickUps)
            {
                p.SetActive(!_isPaused && p.IsSpawned);
            }

            
        }

        public void GameSetup()
        {
            RoundSetup();
        }

        public void RoundSetup()
        {
            _isPaused = true;
            foreach (var p in _pickUps)
            {
                p.IsSpawned = false;
                p.SetActive(false);
            }

            for (var i = 0; i < StartSpawn; i++)
            {
                SpawnRandom();
            }

            // TODO: Fix hack
            foreach (var p in _pickUps)
            {
                p.SetActive(false);
            }
        }

        private void SpawnRandom()
        {
            var existingTotal = _pickUps.Sum(p => !p.IsSpawned ? 0 : p.ScoreAmount);

            var points = 0;
            while (points == 0)
            {
                points = Random.Range(-4, 4);

                var randomness = Random.Range(0f, 1f);
                if (Math.Abs(points) == 1 && randomness < 0.8f)
                {
                    points = 0;
                    continue;
                }

                randomness = Random.Range(0f, 1f);
                if (Math.Abs(points + existingTotal) > 4 && randomness < 0.9f)
                {
                    points = 0;
                }
            }

            bool isValidPosition;
            Vector3 position;
            do
            {
                position = new Vector3(
                    Random.Range(-5f, 5f) * 10,
                    Random.Range(-2.5f, 2.5f) * 10,
                    0f
                );

                isValidPosition = _pickUps
                    .Where(p => p.IsSpawned)
                    .All(t => Vector3.Distance(position, t.transform.position) >= SpawnSpacing);
                
                if (_gameManager.Players.PlayerIds
                    .Any(p => Vector3.Distance(position, _gameManager.Players.GetPlayerPosition(p)) < SpawnSpacing))
                {
                    isValidPosition = false;
                }
            } while (!isValidPosition);

            Spawn(position, points);
        }

        private void Spawn(Vector2 position, int points)
        {
            var i = 0;
            while (_pickUps[i].IsSpawned)
            {
                i++;
                if (i == _pickUps.Count) return;
            }

            _pickUps[i].ScoreAmount = points;
            _pickUps[i].Position = position;
            _pickUps[i].IsSpawned = true;
            _pickUps[i].SetActive(true);
        }

        private void Awake()
        {
            _myTransform = GetComponent<Transform>();

            var prefab = Resources.Load("Prefabs/PickUp");

            _pickUps = new List<PickUpComponent>();
            for (var i = 0; i < MaxSpawn; i++)
            {
                var obj = Instantiate(prefab, _myTransform) as GameObject;
                var pickUp = obj.GetComponent<PickUpComponent>();

                pickUp.Id = i;

                _pickUps.Add(pickUp);
            }
        }

        public void Start()
        {
            var gameManagerObject = GameObject.FindGameObjectWithTag("GameManager");
            _gameManager = gameManagerObject.GetComponent<GameManager>();

            _gameManager.Subscribe(this);
            GameSetup();
        }

        private void Update()
        {
            if (_isPaused) return;

            var spawnCount = _pickUps.Sum(p => p.IsSpawned ? 1 : 0);
            if (spawnCount == _pickUps.Count) return;

            _spawnCounter += Time.unscaledDeltaTime;
            while (_spawnCounter >= SpawnRate)
            {
                SpawnRandom();
                _spawnCounter -= SpawnRate;
            }
        }
    }
}
