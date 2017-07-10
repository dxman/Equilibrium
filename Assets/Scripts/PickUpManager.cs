using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Equilibrium
{
    public class PickUpManager : MonoBehaviour
    {
        public float SpawnRate;
        public int StartSpawn;
        public int MaxSpawn;
        public float SpawnSpacing;

        private Transform _myTransform;
        private Transform[] _playerTransforms;

        private List<GameObject> _pickUpObjects;
        private List<PickUpComponent> _pickUps;
        private float _spawnCounter;

        public void SeedPickups()
        {
            foreach (var pickup in _pickUpObjects)
            {
                pickup.SetActive(false);
            }

            for (var i = 0; i < StartSpawn; i++)
            {
                SpawnPickUp();
            }
        }

        private void SpawnPickUp()
        {
            var i = 0;
            while (_pickUpObjects[i].activeSelf)
            {
                i++;
                if (i == _pickUpObjects.Count) return;
            }

            var existingTotal = _pickUps.Sum(p => (p.Id == _pickUps[i].Id || !p.gameObject.activeSelf)
                ? 0
                : p.ScoreAmount);

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

            _pickUps[i].ScoreAmount = points;

            bool isValidLocation;
            Vector3 position;
            do
            {
                position = new Vector3(
                    Random.Range(-5f, 5f) * 10,
                    Random.Range(-2.5f, 2.5f) * 10,
                    0f
                );

                isValidLocation = _pickUpObjects
                    .Where((t, j) => i != j && t.activeSelf)
                    .All(t => !(Vector3.Distance(position, t.transform.position) < SpawnSpacing));

                if (_playerTransforms
                    .Any(t => Vector3.Distance(position, t.position) < SpawnSpacing))
                {
                    isValidLocation = false;
                }
            } while (!isValidLocation);
            

            _pickUpObjects[i].transform.position = position;
            _pickUpObjects[i].SetActive(true);
        }

        private void Awake()
        {
            _myTransform = GetComponent<Transform>();

            var players = GameObject.FindGameObjectsWithTag("Player");
            _playerTransforms = players.Select(p => p.GetComponent<Transform>()).ToArray();

            var prefab = Resources.Load("Prefabs/PickUp");

            _pickUpObjects = new List<GameObject>();
            _pickUps = new List<PickUpComponent>();
            for (var i = 0; i < MaxSpawn; i++)
            {
                var obj = Instantiate(prefab, _myTransform) as GameObject;
                var pickUp = obj.GetComponent<PickUpComponent>();

                pickUp.Id = i;
                pickUp.PickUpManager = this;

                _pickUpObjects.Add(obj);
                _pickUps.Add(pickUp);
            }
        }

        private void Update()
        {
            _spawnCounter += Time.deltaTime;
            while (_spawnCounter >= SpawnRate)
            {
                SpawnPickUp();
                _spawnCounter -= SpawnRate;
            }
        }
    }
}
