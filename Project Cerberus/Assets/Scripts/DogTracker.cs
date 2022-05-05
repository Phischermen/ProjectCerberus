/*
 * DogTracker follows the currently controlled dog, and manages its own particle system.
 */

using UnityEngine;

public class DogTracker : MonoBehaviour
{
    public float followSpeed;
    public float cycleSpeed;
    private float currentSpeed;
    public float targetDistance;
    private bool _inTransit;
    private float lastTimeInTransit;

    // Particle parameters
    private ParticleSystem.MainModule _mainModule;
    private ParticleSystem.EmissionModule _emissionModule;
    [Header("Particles")] public ParticleSystem.MinMaxCurve startLifeTimeForFollowPreset = 0.5f;
    public ParticleSystem.MinMaxCurve startSpeedForFollowPreset = 0.5f;
    public ParticleSystem.MinMaxCurve startLifeTimeForCyclePreset = 1f;
    public ParticleSystem.MinMaxCurve startSpeedForCyclePreset = 1f;
    public ParticleSystem.MinMaxCurve rateOverDistanceForCyclePreset = 100f;

    // Trail parameters
    // TODO: Configure IDE to not auto format my code like this.

    [Header("Trail")] public float timeForFollowPreset;
    public float timeForCyclePreset;
    public float widthMultiplier;

    private GameManager _manager;
    private ParticleSystem _particleSystem;
    private TrailRenderer _trailRenderer;
    private Cerberus _trackedDog;

    void Start()
    {
        _manager = FindObjectOfType<GameManager>();
        _particleSystem = GetComponent<ParticleSystem>();
        _trailRenderer = GetComponent<TrailRenderer>();
        _emissionModule = _particleSystem.emission;
        _mainModule = _particleSystem.main;
        // Initialize tracked dog.
        _trackedDog = _manager.currentCerberus;
        SetFieldsToFollowPreset();
    }

    void Update()
    {
        // Check to see if player has cycled dog.
        if (_manager.currentCerberus != _trackedDog)
        {
            _trackedDog = _manager.currentCerberus;
            _inTransit = true;
            SetFieldsToCyclePreset();
        }

        // Ease in to trackedDog's position.
        var position = transform.position;
        var position1 = _trackedDog.transform.position;
        transform.position = Vector3.Lerp(position, position1, currentSpeed);
        // Scale based on transit status.
        Vector3 targetSize;
        if (_inTransit)
        {
            targetSize = Vector3.one;
            _trailRenderer.widthMultiplier = widthMultiplier;
        }
        else
        {
            var f = Mathf.Sin((lastTimeInTransit - Time.time) * 0.5f + 1.5f);
            _trailRenderer.widthMultiplier = widthMultiplier * f;
            targetSize = Vector3.one * f;
        }
        transform.localScale = Vector3.Lerp(transform.localScale, targetSize, 0.6f);
        // Check if close enough to consider done with transit.
        var distance = Vector3.Distance(position, position1);
        if (distance < targetDistance && _inTransit)
        {
            _inTransit = false;
            lastTimeInTransit = Time.time;
            SetFieldsToFollowPreset();
        }
    }

    private void SetFieldsToCyclePreset()
    {
        currentSpeed = cycleSpeed;
        _mainModule.startLifetime = startLifeTimeForCyclePreset;
        _mainModule.startSpeed = startSpeedForCyclePreset;
        _emissionModule.rateOverDistance = rateOverDistanceForCyclePreset;
        // Activate trail.
        _trailRenderer.emitting = true;
        _trailRenderer.time = timeForCyclePreset;
    }

    private void SetFieldsToFollowPreset()
    {
        currentSpeed = followSpeed;
        _mainModule.startLifetime = startLifeTimeForFollowPreset;
        _mainModule.startSpeed = startSpeedForFollowPreset;
        _emissionModule.rateOverDistance = 0f;

        _trailRenderer.time = timeForFollowPreset;
    }
}