using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public class BoatController : MonoBehaviour {
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float rotationSpeed;


    private void FixedUpdate() {
        var input = UnityInputSystemSingleton.Instance.InputSystem.Player;
        if (input.Move.IsPressed()) {
            OnMove(input.Move.ReadValue<Vector2>());
        }
    }

    private void OnMove(Vector2 movement) {
        var dt = Time.deltaTime;
        // Debug.Log($"move: {movement}");

        if (movement.y != 0 ) {
            rb.AddForce(transform.forward * ( Mathf.Sign(movement.y) * movementSpeed* dt) , ForceMode
            .VelocityChange);
        }

        if (movement.x != 0) {
            rb.transform.Rotate(Vector3.up * (Mathf.Sign(movement.x)*rotationSpeed * dt), Space.World);
        }
    }

}
