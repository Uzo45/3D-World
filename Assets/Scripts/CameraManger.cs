using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManger : MonoBehaviour
{
    InputManager inputManager;

    public Transform targetTransform;   //object that will be followed
    public Transform cameraPivot;       //object camera uses to pivot
    public Transform cameraTransform;   //transform of the actual camera object in the scene
    public LayerMask collosionLayers;   //Layers used to for the camera to collide with
    private float defaultPosition;
    private Vector3 cameraFollowVelocity = Vector3.zero;
    private Vector3 cameraVectorPosition;

    [SerializeField] private float cameraCollosionOffset = 0.2f;    //how much the camera will jump off of objects its colliding with
    [SerializeField] private float minimumCollosionOffset = 0.2f;
    [SerializeField] private float cameraCollosionRadius = 0.2f;
    [SerializeField] float cameraFollowSpeed = 0.2f;
    [SerializeField] float cameraSensivity = 2f;    //cameraLookSpeed and cameraPivotSpeed

    private float lookAngle;
    private float pivotAngle;
    public float minimumPivotAngle = -35;
    public float maximumPivotAngle = 35;

    private void Awake()
    {
        inputManager = FindAnyObjectByType<InputManager>();
        targetTransform = FindAnyObjectByType<PlayerManager>().transform;
        defaultPosition = cameraTransform.localPosition.z;
    }

    public void HandleAllCameraMovement()
    {
        FollowTarget();
        RotateCamera();
        HandleCameraCollisions();
    }

    private void FollowTarget()
    {
        Vector3 targetPosition = Vector3.SmoothDamp(
            transform.position, 
            targetTransform.position, 
            ref cameraFollowVelocity, 
            cameraFollowSpeed
            );

        transform.position = targetPosition;
    }

    private void RotateCamera()
    {
        Vector3 rotation;
        Quaternion targetRotation;

        lookAngle = lookAngle + (inputManager.cameraInputX * /*cameraLookSpeed*/cameraSensivity);
        pivotAngle = pivotAngle + (inputManager.cameraInputY * /*cameraPivotSpeed*/cameraSensivity);
        pivotAngle = Mathf.Clamp(pivotAngle, minimumPivotAngle, maximumPivotAngle);

        rotation = Vector3.zero;
        rotation.y = lookAngle;
        targetRotation = Quaternion.Euler(rotation);
        transform.rotation = targetRotation;

        rotation = Vector3.zero;
        rotation.x = pivotAngle;
        targetRotation = Quaternion.Euler(rotation);
        cameraPivot.localRotation = targetRotation;
    }

    private void HandleCameraCollisions()
    {
        float targetPosition = defaultPosition;
        RaycastHit hit;
        Vector3 direction = cameraTransform.position - cameraPivot.position;
        direction.Normalize();

        if(Physics.SphereCast
            (cameraPivot.transform.position, cameraCollosionRadius, direction, out hit, Mathf.Abs(targetPosition), collosionLayers))
        {
            float distance = Vector3.Distance(cameraPivot.position, hit.point);
            targetPosition =- (distance * cameraCollosionOffset);
        }

        if(Mathf.Abs(targetPosition) < minimumCollosionOffset)
        {
            targetPosition = targetPosition - minimumCollosionOffset;
        }

        cameraVectorPosition.z = Mathf.Lerp(cameraTransform.localPosition.z, targetPosition, 0.2f);
        cameraTransform.localPosition = cameraVectorPosition;
    }
}
