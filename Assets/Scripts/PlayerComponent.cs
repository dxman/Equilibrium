using System;
using System.Collections.Generic;
using UnityEngine;

namespace Equilibrium
{
    public class PlayerComponent : MonoBehaviour
    {
        public int Id;
        public float ThrustStrength;
        public float RotateSpeed;
        public float ChargeSpeed;

        private GameManager _gameManager;
        private SoundManager _soundManager;
        private Transform _myTransform;
        private Rigidbody2D _myRigidbody2D;
        private Transform _mySpriteTransform;
        private Transform _myChargeBarTransform;

        private Dictionary<InputAction, string> _inputMappings;
        private Vector3 _facing;
        private float _chargePercent;
        private bool _isCharging;
        private bool _isBraking;

        private void Idle()
        {
            _chargePercent = 0f;
            _isCharging = false;
        }

        private void BeginCharging()
        {
            _isCharging = true;
        }

        private void Thrust()
        {
            _myRigidbody2D.AddForce(_facing * _chargePercent * ThrustStrength, ForceMode2D.Impulse);
            _soundManager.PlaySound(SoundType.Thrust);
            Idle();
        }

        private void UpdateCharging()
        {
            if (_isCharging)
            {
                _chargePercent += ChargeSpeed * Time.deltaTime;
                if (_chargePercent > 1f)
                {
                    _chargePercent = 1f;
                }
            }

            var chargeBarLocalScale = _myChargeBarTransform.localScale;
            chargeBarLocalScale.x = 10f * _chargePercent;
            _myChargeBarTransform.localScale = chargeBarLocalScale;
        }

        public void BeginBraking()
        {
            _isBraking = true;
            _myRigidbody2D.drag = 8f;
        }

        public void EndBraking()
        {
            _isBraking = false;
            _myRigidbody2D.drag = .5f;
        }

        public float XInput;
        public float YInput;
        public bool IsChargeHeld;

        private void Awake()
        {
            var gm = GameObject.FindGameObjectWithTag("GameManager");
            _gameManager = gm.GetComponent<GameManager>();
            _soundManager = gm.GetComponent<SoundManager>();

            _myTransform = GetComponent<Transform>();
            _myRigidbody2D = GetComponent<Rigidbody2D>();

            _mySpriteTransform = _myTransform.Find("Sprite");
            if (_mySpriteTransform == null)
            {
                Debug.LogError($"Initialization error: Player {Id} does not have a child named Sprite.");
                return;
            }

            _myChargeBarTransform = _mySpriteTransform.Find("ChargeBar");
            if (_myChargeBarTransform == null)
            {
                Debug.LogError($"Initialization error: Player {Id} does not have a child named ChargeBar.");
                return;
            }

            _inputMappings = _gameManager.GetInputMappings(Id);

            _isCharging = false;
            _isBraking = false;
        }

        private void Update()
        {
            if (Math.Abs(XInput) > 0.1f || Math.Abs(YInput) > 0.1f)
            {
                var heading = Mathf.Atan2(
                    -XInput,
                    -YInput
                );

                var target = Quaternion.Euler(0f, 0f, heading * Mathf.Rad2Deg);
                _mySpriteTransform.rotation = Quaternion.RotateTowards(
                    _mySpriteTransform.rotation,
                    target,
                    RotateSpeed * Time.deltaTime);

                _facing = _mySpriteTransform.rotation * Vector3.up;
                if (_facing == Vector3.zero)
                {
                    _facing = Vector3.up;
                }
            }

            if (IsChargeHeld)
            {
                if (!_isCharging)
                {
                    BeginCharging();
                }
            }
            else if (_isCharging)
            {
                Thrust();
            }

            UpdateCharging();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                _soundManager.PlaySound(SoundType.CollidePlayer);
            }
            else
            {
                _soundManager.PlaySound(SoundType.CollideWall);
            }
        }
    }
}
