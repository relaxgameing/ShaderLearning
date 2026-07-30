using System;
using UnityEngine;

public class UnityInputSystemSingleton : MonoBehaviour {

    private static UnityInputSystemSingleton _instance;
    public static UnityInputSystemSingleton Instance {
        get {
            if (_instance == null) {
                _instance = FindAnyObjectByType<UnityInputSystemSingleton>();

                if (_instance != null) {
                    return _instance;
                }

                GameObject ob = new GameObject("UnityInputSystem");
                _instance = ob.AddComponent<UnityInputSystemSingleton>();
            }

            return _instance;
        }
    }

    private void Awake() {
        if (_instance == null) {
            _instance = this;
            DontDestroyOnLoad(this);
        }
        else {
            Destroy(this.gameObject);
        }
    }

    private UnityInputSystem _inputSystem;
    public UnityInputSystem InputSystem => _inputSystem ??= new UnityInputSystem();

    private void OnEnable() {
        InputSystem.Enable();
    }

    private void OnDisable() {
        InputSystem.Disable();
    }
}
