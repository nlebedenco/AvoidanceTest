using UnityEngine;
using System.Collections;


public class MateAgent : MonoBehaviour
{
    enum MateState
    {
        None = 0,
        Safe,
        Panic,
        Scared,
        Patrol
    }

    MateState _state;
    MateState State
    {
        get { return _state; }
        set
        {
            if (_state != value)
            {
                OnExitState(_state);
                _state = value;
                OnEnterState(_state);
            }
        }
    }
    
    public Transform whatToAvoid;
    public float intendedSpeed = 200f;
    public float intendedDrag = 5f;
    public float patrolSpeed = 50f;
    public float minSafetyDistance = 10f;
    public float minClosestMateDistance = 15f;
    public float minQueenDistance = 20f;
    public float minPatrolPointDistance = 10f;

    public float maxFear = 1f;
    public float fearDecay = 0.05f;

    public bool isFearContagious = true;

    float actualDelayToCalmDown = 0;

    Transform[] patrolPoints;
    Transform nextPatrolPoint;

    Rigidbody rb;

    int patrolPointIndex = 0;

    [ReadOnly]
    public float fear = 0;

    #region Unity Events

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

	void Start()
    {
        State = MateState.Safe;
        var points = GameObject.FindGameObjectsWithTag("PatrolPoint");
        patrolPoints = new Transform[points.Length];
        for (int i  = 0; i < points.Length; ++i)
        {
            patrolPoints[i] = points[i].transform;
        }

        patrolPointIndex = Random.Range(0, patrolPoints.Length - 1);
    }

    void Update()
    {
        UpdateState();
    }

    void OnCollisionStay(Collision collision)
    {
        if (isFearContagious)
        {
            MateAgent mate = collision.gameObject.GetComponent<MateAgent>();
            if (mate != null)
                mate.Scare(fear);
        }
    }

    #endregion

    Transform GetNextPatrolPoint()
    {
        if (patrolPoints.Length > 0)
        {
            patrolPointIndex = (patrolPointIndex + 1) % patrolPoints.Length;
            return patrolPoints[patrolPointIndex];
        }
        return null;
    }

    bool IsEnemyTooClose()
    {
        return (whatToAvoid != null && Vector3.Distance(transform.position, whatToAvoid.position) < minSafetyDistance);
    }

    MateAgent FindClosestMate()
    {
        MateAgent closest = null;
        float minDistance = float.PositiveInfinity;
        var mates = GameObject.FindGameObjectsWithTag("Mate");

        foreach (GameObject go in mates)
        {
            var mate = go.GetComponent<MateAgent>();
            if (mate != null && mate != this)
            {
                float distance = Vector3.Distance(transform.position, mate.transform.position);
                if (distance < minDistance)
                {
                    closest = mate;
                    minDistance = distance;
                }
            }
        }

        return closest;
    }

    void RunAwayFromThreat()
    {
        rb.AddForce((transform.position - whatToAvoid.position).normalized * intendedSpeed * Time.deltaTime, ForceMode.VelocityChange);
    }

    void RunAwayRandomly()
    {
        // Run randomly while scared
        Quaternion randomRotation = Quaternion.AngleAxis(Random.Range(0, 180), Vector3.up);
        Vector3 newDirection = randomRotation * transform.forward;
        rb.AddForce(newDirection.normalized * intendedSpeed * Time.deltaTime, ForceMode.VelocityChange);
    }

    void Scare(float amount)
    {
        switch (State)
        {
            case MateState.None:
                break;
            case MateState.Safe:
                fear = amount / 2;
                State = MateState.Scared;
                break;
            case MateState.Panic:
                break;
            case MateState.Scared:
                break;
            case MateState.Patrol:
                fear = amount / 2;
                State = MateState.Scared;
                break;
            default:
                break;
        }
    }

    #region State Machine 

    void UpdateState()
    {
        switch (State)
        {
            case MateState.None:
                State = MateState.Safe;
                break;
            case MateState.Safe:
                if (IsEnemyTooClose())
                {
                    State = MateState.Panic;
                }
                else
                {
                    var targetMate = SpawnMates.Queen == null ? FindClosestMate() :
                                        SpawnMates.Queen == this ? FindClosestMate() : SpawnMates.Queen;
                    if (targetMate != null)
                    {
                        if (Vector3.Distance(transform.position, targetMate.transform.position) > minClosestMateDistance)
                        {
                            rb.AddForce((targetMate.transform.position - transform.position).normalized * intendedSpeed * Time.deltaTime, ForceMode.VelocityChange);
                        }
                        else
                        {
                            State = MateState.Patrol;
                        }
                    }
                }
                break;
            case MateState.Panic:
                RunAwayFromThreat();
                if (!IsEnemyTooClose())
                    State = MateState.Scared;                
                break;
            case MateState.Scared:
                fear -= (fear * fearDecay * Time.deltaTime);
                if (fear < 1e-6f)
                    fear = 0;

                if (IsEnemyTooClose())
                {
                    State = MateState.Panic;
                }
                else if (fear > 0)
                {
                    RunAwayRandomly();
                }
                else
                {
                    State = MateState.Safe;
                }
                break;
            case MateState.Patrol:
                if (IsEnemyTooClose())
                {
                    State = MateState.Panic;
                }
                else
                {
                    if (nextPatrolPoint == null)
                        nextPatrolPoint = GetNextPatrolPoint();

                    if ((nextPatrolPoint != null) && (Vector3.Distance(transform.position, nextPatrolPoint.position) < minPatrolPointDistance))
                        nextPatrolPoint = GetNextPatrolPoint();

                    if (nextPatrolPoint != null)
                        rb.AddForce((nextPatrolPoint.position - transform.position).normalized * patrolSpeed * Time.deltaTime, ForceMode.VelocityChange);
                }
                
                break;
        }
    }

    void OnEnterState(MateState value)
    {
        switch (value)
        {
            case MateState.None:
                break;
            case MateState.Safe:
                break;
            case MateState.Panic:
                fear = maxFear;
                RunAwayFromThreat();
                break;
            case MateState.Scared:
                RunAwayRandomly();
                break;
            case MateState.Patrol:
                break;
        }

    }

    void OnExitState(MateState value)
    {
        switch (value)
        {
            case MateState.None:
                break;
            case MateState.Safe:
                break;
            case MateState.Panic:
                break;
            case MateState.Scared:
                break;
            case MateState.Patrol:
                break;
        }
    }

    #endregion
}
