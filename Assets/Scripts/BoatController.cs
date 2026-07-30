using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public class BoatController : MonoBehaviour {
    [SerializeField] private float speed;

    private void OnEnable() {
        UnityInputSystemSingleton.Instance.InputSystem.Player.Move.performed += OnMove;
    }

    private void OnMove(InputAction.CallbackContext obj) {
        var movement = obj.ReadValue<Vector2>();
        Debug.Log($"move: {movement}");
    }

}
