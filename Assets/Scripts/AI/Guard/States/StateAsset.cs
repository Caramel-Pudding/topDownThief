using UnityEngine;

public interface IState
{
    void Enter();
    void Tick();
    void Exit();
}

public abstract class StateAsset : ScriptableObject
{
    public abstract IState CreateRuntime(GuardBrain brain);
}
