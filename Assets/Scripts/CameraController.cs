using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour {
    [SerializeField] private Transform anchor;
    [SerializeField] private float lookSpeed = 5f;
    [SerializeField] private float camDistance = 5f;


    private void OnEnable() {
        Cursor.lockState = CursorLockMode.Locked;
        UnityInputSystemSingleton.Instance.InputSystem.Player.Look.performed += OnLook;
    }

    private void OnDisable() {
        Cursor.lockState = CursorLockMode.None;
    }


    private void FixedUpdate() {
        var dt = Time.deltaTime;
        var dist = Vector3.Distance(transform.position, anchor.position);
        if (camDistance < dist) {
            var diff = anchor.position - transform.position;
            diff.y = 0;
            transform.SetPositionAndRotation(
                transform.position +  diff.normalized * (Mathf.Abs(camDistance - dist)),
                transform.rotation);
        }
    }

    private void OnLook(InputAction.CallbackContext obj) {
        var dt = Time.deltaTime;
        var movement = obj.ReadValue<Vector2>();

        var val = Mathf.Sign(movement.x) * dt * 10f * lookSpeed;
        transform.RotateAround(anchor.position , Vector3.up ,val  );

    }
}
