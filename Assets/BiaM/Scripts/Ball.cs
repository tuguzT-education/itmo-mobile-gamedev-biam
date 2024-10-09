using Terresquall;
using UnityEngine;

namespace BiaM
{
    [RequireComponent(typeof(Rigidbody), typeof(ConstantForce))]
    public class Ball : MonoBehaviour
    {
        private ConstantForce _constantForce;

        private void Awake()
        {
            _constantForce = GetComponent<ConstantForce>();
        }

        private void Update()
        {
            _constantForce.force = VirtualJoystick.GetAxis();
        }
    }
}