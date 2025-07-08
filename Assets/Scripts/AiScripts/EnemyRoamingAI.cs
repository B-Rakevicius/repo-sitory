using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyRoamingAI : MonoBehaviour
{
    [Header("References")]
    public List<Transform> players = new List<Transform>();
    private NavMeshAgent agent;
    private EnemyStateMachine stateMachine;
    [Header("State Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float minIdleTime = 1f;
    [SerializeField] private float maxIdleTime = 3f;
    [SerializeField] private float minRoamDistance = 5f;
    [SerializeField] private float maxRoamDistance = 15f;
    [SerializeField] private float playerRoamPref = 0.1f;
    [SerializeField] private float targetSwitchCooldown = 2f;
    private Transform currentTarget;
    private float lastTargetSwitchTime;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        stateMachine = new EnemyStateMachine();
        DoDelayAction(2.5f);
    }
    void DoDelayAction(float delayTime)
    {
        StartCoroutine(DelayAction(delayTime));
    }
    IEnumerator<WaitForSeconds> DelayAction(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in playerObjects)
        {
            players.Add(player.transform);
        }
    }
    private void Start() => stateMachine.Initialize(new IdleState(this, stateMachine));
    private void Update() => stateMachine.Update();
    private void FixedUpdate() => stateMachine.FixedUpdate();
    public Transform GetNearestPlayer()
    {
        if (players.Count == 0) return null;
        Transform nearest = null;
        float minDistance = Mathf.Infinity;
        foreach (Transform player in players)
        {
            if (player == null) continue;
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = player;
            }
        }
        return nearest;
    }
    public bool IsInDetectionRange()
    {
        foreach (Transform player in players)
        {
            if (player != null && Vector3.Distance(transform.position, player.position) <= detectionRange)
            {
                return true;
            }
        }
        return false;
    }
    public bool IsInAttackRange()
    {
        return currentTarget != null &&
               Vector3.Distance(transform.position, currentTarget.position) <= attackRange;
    }
    public void UpdateCurrentTarget()
    {
        if (Time.time < lastTargetSwitchTime + targetSwitchCooldown) return;
        Transform nearest = GetNearestPlayer();
        if (nearest != currentTarget)
        {
            currentTarget = nearest;
            lastTargetSwitchTime = Time.time;
        }
    }
    public Vector3 GetRoamPos()
    {
        Vector3 randomDirection = Random.insideUnitSphere.normalized;
        randomDirection.y = 0;
        if (players.Count > 0 && Random.value < playerRoamPref)
        {
            Transform randomPlayer = players[Random.Range(0, players.Count)];
            if (randomPlayer != null)
            {
                Vector3 toPlayer = (randomPlayer.position - transform.position).normalized;
                randomDirection = Vector3.Lerp(randomDirection, toPlayer, 0.5f).normalized;
            }
        }
        float distance = Random.Range(minRoamDistance, maxRoamDistance);
        Vector3 targetPosition = transform.position + randomDirection * distance;
        NavMeshHit hit;
        int attempts = 0;
        while (attempts < 5)
        {
            if (NavMesh.SamplePosition(targetPosition, out hit, maxRoamDistance, NavMesh.AllAreas))
                return hit.position;
            distance *= 0.8f;
            targetPosition = transform.position + randomDirection * distance;
            attempts++;
        }
        return transform.position;
    }
    // ( united ) states ( of america hehe ) 
    public class IdleState : EnemyState
    {
        private float idleDuration;
        private float idleTimer;
        public IdleState(EnemyRoamingAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
            idleDuration = Random.Range(enemy.minIdleTime, enemy.maxIdleTime);
        }
        public override void Enter()
        {
            idleTimer = 0;
            enemy.agent.ResetPath();
            enemy.UpdateCurrentTarget();
        }
        public override void Update()
        {
            if (enemy.IsInDetectionRange())
            {
                stateMachine.ChangeState(new ChaseState(enemy, stateMachine));
                return;
            }
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleDuration)
            {
                stateMachine.ChangeState(new RoamState(enemy, stateMachine));
            }
        }
    }
    public class RoamState : EnemyState
    {
        public RoamState(EnemyRoamingAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }
        public override void Enter()
        {
            enemy.agent.SetDestination(enemy.GetRoamPos());
            enemy.UpdateCurrentTarget();
        }
        public override void Update()
        {
            if (enemy.IsInDetectionRange())
            {
                stateMachine.ChangeState(new ChaseState(enemy, stateMachine));
                return;
            }
            if (!enemy.agent.pathPending && enemy.agent.remainingDistance < 0.5f)
            {
                stateMachine.ChangeState(new IdleState(enemy, stateMachine));
            }
        }
    }
    public class ChaseState : EnemyState
    {
        public ChaseState(EnemyRoamingAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }
        public override void Enter()
        {
            enemy.UpdateCurrentTarget();
            if (enemy.currentTarget != null)
            {
                enemy.agent.SetDestination(enemy.currentTarget.position);
            }
        }
        public override void Update()
        {
            enemy.UpdateCurrentTarget();

            if (!enemy.IsInDetectionRange())
            {
                stateMachine.ChangeState(new IdleState(enemy, stateMachine));
                return;
            }
            if (enemy.IsInAttackRange())
            {
                stateMachine.ChangeState(new AttackState(enemy, stateMachine));
                return;
            }
            if (enemy.currentTarget != null)
            {
                enemy.agent.SetDestination(enemy.currentTarget.position);
            }
        }
    }
    public class AttackState : EnemyState
    {
        public AttackState(EnemyRoamingAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }
        public override void Enter()
        {
            enemy.agent.ResetPath();
            Debug.Log( this + " just did an attack animation !");
            // attack anim here !!!!!!!!!!!!
        }
        public override void Update()
        {
            enemy.UpdateCurrentTarget();
            if (enemy.currentTarget == null || !enemy.IsInAttackRange())
            {
                stateMachine.ChangeState(new ChaseState(enemy, stateMachine));
                return;
            }
            Vector3 direction = (enemy.currentTarget.position - enemy.transform.position).normalized;
            enemy.transform.rotation = Quaternion.Slerp(
                enemy.transform.rotation,
                Quaternion.LookRotation(direction),
                10f * Time.deltaTime
            );
            Debug.Log(" did 0 dmg to a player !");
            // attack dmg here !!!!!!!!!!
        }
    }
}