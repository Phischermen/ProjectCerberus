/*
 * The camera controller has 3 modes: fixed point, scrolling, and cinematic. The game primarily uses fixed point,
 * because it captures the whole level, but certain scenes use scrolling, for when the level is particularly big.
 * If the player wants a bigger view, they can hold SPACE to switch from fixed point to scrolling. Cinematic mode is
 * used for cutscenes, and it disables the camera from updating.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PuzzleCameraController : MonoBehaviour
{
    public static PuzzleCameraController i;

    private Vector3 _position;
    private float _shake = 0f;
    private float _maxShake = 1f;

    private Camera _camera;
    private PuzzleContainer _puzzleContainer;
    private GameManager _gameManager;

    public Vector3 desiredPosition;
    public float desiredSize;

    public CameraMode currentCameraMode = CameraMode.FixedPointMode;

    public enum CameraMode
    {
        FixedPointMode,
        ScrollingMode,
        CinematicMode
    }

    private void Awake()
    {
        _position = transform.position;
        _camera = GetComponent<Camera>();
        _puzzleContainer = FindObjectOfType<PuzzleContainer>();
        _gameManager = FindObjectOfType<GameManager>();
    }

    private void Start()
    {
        i = this;
        SetDesiredSizeAndPositionForFixedPointMode();
    }

    void Update()
    {
        switch (currentCameraMode)
        {
            case CameraMode.CinematicMode:
                break;
            case CameraMode.ScrollingMode:
                SetDesiredSizeAndPositionForScrollingMode();
                goto case CameraMode.FixedPointMode; //Fall through
            case CameraMode.FixedPointMode:
                _position = Vector3.Lerp(_position, desiredPosition, 0.05f);
                _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, desiredSize, 0.1f);
                break;
            default:
                break;
        }

        transform.position = _position + new Vector3(Random.Range(-_shake, _shake), Random.Range(-_shake, _shake));
        _shake = Mathf.Max(0f, _shake - Time.deltaTime);
    }

    public void AddShake(float shake)
    {
        _shake = Mathf.Min(_shake + shake, _maxShake);
    }

    public Vector3 GetPosition()
    {
        return _position;
    }

    public void SetPosition(Vector3 position)
    {
        _position = position;
    }

    public void SetCameraMode(CameraMode mode)
    {
        currentCameraMode = mode;
        if (mode == CameraMode.FixedPointMode)
        {
            SetDesiredSizeAndPositionForFixedPointMode();
        }
        else if (mode == CameraMode.ScrollingMode)
        {
            SetDesiredSizeAndPositionForScrollingMode();
        }
    }

    public void ToggleCameraMode()
    {
        if (currentCameraMode == CameraMode.CinematicMode) return;
        if (currentCameraMode == CameraMode.FixedPointMode)
        {
            currentCameraMode = CameraMode.ScrollingMode;
        }
        else
        {
            currentCameraMode = CameraMode.FixedPointMode;
            SetDesiredSizeAndPositionForFixedPointMode();
        }
    }

    public void SetDesiredSizeAndPositionForScrollingMode()
    {
        var currentCerberusCell = _gameManager.currentCerberus.position;
        var positionOfCell =
            _puzzleContainer.tilemap.layoutGrid.CellToWorld(new Vector3Int(currentCerberusCell.x, currentCerberusCell.y,
                0));
        desiredPosition = new Vector3(positionOfCell.x, positionOfCell.y, -10f);
        desiredSize = 5f;
    }

    public void SetDesiredSizeAndPositionForFixedPointMode()
    {
        var bounds = _puzzleContainer.tilemap.localBounds;
        var boundsAspect = bounds.extents.x / bounds.extents.y;
        desiredPosition =
            new Vector3(bounds.center.x, bounds.center.y, -10f);
        if (boundsAspect < _camera.aspect)
        {
            desiredSize = bounds.extents.y;
        }
        else
        {
            desiredSize = bounds.extents.x / _camera.aspect;
        }
    }

    public void GotoDesiredPositionAndSize()
    {
        _position = desiredPosition;
        transform.position = _position;
        _camera.orthographicSize = desiredSize;
    }
}