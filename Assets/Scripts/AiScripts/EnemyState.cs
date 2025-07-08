using UnityEngine;
public abstract class EnemyState
{
    protected EnemyRoamingAI enemy;
    protected EnemyStateMachine stateMachine;
    public EnemyState(EnemyRoamingAI enemy, EnemyStateMachine stateMachine)
    {
        this.enemy = enemy;
        this.stateMachine = stateMachine;
    }
    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
    public virtual void Exit() { }
}

public class EnemyStateMachine
{
    public EnemyState CurrentState { get; private set; }

    public void Initialize(EnemyState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    public void ChangeState(EnemyState newState)
    {
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public void Update() => CurrentState.Update();
    public void FixedUpdate() => CurrentState.FixedUpdate();
}