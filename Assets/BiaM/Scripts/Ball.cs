using Terresquall;
using UnityEngine;

namespace BiaM
{
    [RequireComponent(typeof(Rigidbody), typeof(ConstantForce))]
    public class Ball : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float forceMultiplier = 1f;

        private ConstantForce _constantForce;

        private void Awake()
        {
            _constantForce = GetComponent<ConstantForce>();
        }

        private void Update()
        {
            var input = VirtualJoystick.GetAxis();
            _constantForce.force = new Vector3(input.x, input.y, 1f) * forceMultiplier;
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log(other.gameObject.tag);
        }
    }
}