using System.Collections;
using System.Collections.Generic;
using LFG.LevelEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private CharacterController _characterController;

    [SerializeField] private float _speed;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 moveVector = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            moveVector.z += 1;
        }

        if (Input.GetKey(KeyCode.S))
            moveVector.z -= 1;
        if (Input.GetKey(KeyCode.A))
            moveVector.x -= 1;
        if (Input.GetKey(KeyCode.D))
            moveVector.x += 1;
        Vector3 worldMoveVector = _characterController.transform.TransformDirection(moveVector);
        _characterController.Move(worldMoveVector * _speed * Time.deltaTime);
    }
}
