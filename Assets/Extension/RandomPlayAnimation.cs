using System.Collections.Generic;
using UnityEngine;

namespace H.Library.Components
{
    [RequireComponent(typeof(Animation))]
    public class RandomPlayAnimation : MonoBehaviour
    {
        private Animation _anim = null;

        private List<string> _animationList = new List<string>();


        private void Awake()
        {
            _anim = GetComponent<Animation>();

            foreach (AnimationState state in _anim)
                _animationList.Add(state.name);
        }


        private void Start()
        {
            Play();
        }


        private void Play()
        {
            var animIdx = Random.Range(0, _animationList.Count);

            _anim.Play(_animationList[animIdx]);
        }
    }
}