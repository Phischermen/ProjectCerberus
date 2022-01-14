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
    
    // Particle parameters
    private ParticleSystem.MainModule _mainModule;
    private ParticleSystem.EmissionModule _emissionModule;
    public ParticleSystem.MinMaxCurve startLifeTimeForFollowPreset = 0.5f;
    public ParticleSystem.MinMaxCurve startSpeedForFollowPreset = 0.5f;
    public ParticleSystem.MinMaxCurve startLifeTimeForCyclePreset = 1f;
    public ParticleSystem.MinMaxCurve startSpeedForCyclePreset = 1f;
    public ParticleSystem.MinMaxCurve rateOverDistanceForCyclePreset = 100f;
    
    private GameManager _manager;
    private ParticleSystem _particleSystem;
    private Cerberus _trackedDog;
    
    void Start()
    {
        _manager = FindObjectOfType<GameManager>();
        _particleSystem = GetComponent<ParticleSystem>();
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
        // Check if close enough to consider done with transit.
        var distance = Vector3.Distance(position, position1);
        if (distance < targetDistance && _inTransit)
        {
            _inTransit = false;
            SetFieldsToFollowPreset();
        }
    }
    
    private void SetFieldsToCyclePreset()
    {
        currentSpeed = cycleSpeed;
        _mainModule.startLifetime = startLifeTimeForCyclePreset;
        _mainModule.startSpeed = startSpeedForCyclePreset;
        _emissionModule.rateOverDistance = rateOverDistanceForCyclePreset;
    }

    private void SetFieldsToFollowPreset()
    {
        currentSpeed = followSpeed;
        _mainModule.startLifetime = startLifeTimeForFollowPreset;
        _mainModule.startSpeed = startSpeedForFollowPreset;
        _emissionModule.rateOverDistance = 0f;
    }
}