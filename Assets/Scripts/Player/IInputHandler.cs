using TargetSystem;
using UnityEngine;

namespace Player
{
    public interface IInputHandler
    {
        public void OnClickGround(Vector3 worldPoint);
        public void OnClickTarget(Targetable target);
    }
}