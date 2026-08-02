using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour {
    [SerializeField] private Transform anchor;
    [SerializeField] private float anchorRadius = 10f;
    [SerializeField] private float anchorAngle = 0f;
    [SerializeField] private float lookSpeed = 5f;

    private Camera _cam;

    private void Awake() {
        _cam = GetComponent<Camera>();
        _cam.depthTextureMode |= DepthTextureMode.Depth;
    }

    private void OnEnable() {
        Cursor.lockState = CursorLockMode.Locked;
        UnityInputSystemSingleton.Instance.InputSystem.Player.Look.performed += OnLook;
    }

    private void OnDisable() {
        Cursor.lockState = CursorLockMode.None;
    }


    private void LateUpdate() {
        UpdatePosition();
    }

    private void UpdatePosition() {
        var angle = Mathf.Deg2Rad * anchorAngle;
        var pos = anchor.position + new Vector3(anchorRadius * Mathf.Cos(angle), 0f,
            anchorRadius * Mathf.Sin(angle));


        this.transform.position = new Vector3(pos.x, this.transform.position.y, pos.z);
        this.transform.LookAt(anchor.position);
    }

    private void OnLook(InputAction.CallbackContext obj) {
        var dt = Time.deltaTime;
        var movement = obj.ReadValue<Vector2>();

        var val = Mathf.Sign(movement.x) * dt * 10f * lookSpeed;
        // transform.RotateAround(anchor.position , Vector3.up ,val  );
        anchorAngle += val;
    }

    private void OnValidate() {
        UpdatePosition();
    }
}
