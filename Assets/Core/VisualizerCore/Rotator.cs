using UnityEngine;

// Lol rotate the visualizer to make it look better??
namespace Visualizer
{
    public class Rotator : MonoBehaviour
    {
        public float RotateSpeed = 20f;

        private void Update()
        {
            transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.z + RotateSpeed * Time.deltaTime);
        }
    }
}