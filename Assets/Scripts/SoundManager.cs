using System;
using System.Collections.Generic;
using UnityEngine;

namespace Equilibrium
{
    public class SoundManager : MonoBehaviour
    {
        public AudioClip Thrust;
        public AudioClip ScorePositive;
        public AudioClip ScoreNegative;
        public AudioClip CollideWall;
        public AudioClip CollidePlayer;

        private AudioSource _myAudioSource;

        public void PlaySound(SoundType soundType)
        {
            switch (soundType)
            {
                case SoundType.Thrust:
                    _myAudioSource.PlayOneShot(Thrust);
                    break;

                case SoundType.ScorePositive:
                    _myAudioSource.PlayOneShot(ScorePositive);
                    break;

                case SoundType.ScoreNegative:
                    _myAudioSource.PlayOneShot(ScoreNegative);
                    break;

                case SoundType.CollideWall:
                    _myAudioSource.PlayOneShot(CollideWall);
                    break;
                
                case SoundType.CollidePlayer:
                    _myAudioSource.PlayOneShot(CollidePlayer);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(soundType), soundType, null);
            }
        }

        private void Awake()
        {
            _myAudioSource = GetComponent<AudioSource>();
        }
    }
}
