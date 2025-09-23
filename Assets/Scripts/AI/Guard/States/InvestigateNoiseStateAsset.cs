using UnityEngine;

[CreateAssetMenu(menuName = "AI/Guard/States/Investigate")]
public class InvestigateNoiseState : StateAsset
{
    [Header("Movement")]
    public float moveSpeed = 2.0f;
    public float stopDistance = 0.25f;

    [Header("Timing")]
    public float maxInvestigateTime = 4f;
    public float lingerTime = 1.0f;

    [Header("Flow")]
    public StateAsset resumeState;

    public override IState CreateRuntime(GuardBrain brain) => new Runtime(this, brain);

    private class Runtime : IState
    {
        private readonly InvestigateNoiseState asset;
        private readonly GuardBrain brain;
        private Vector2 target;
        private float startedAt;
        private float reachedAt;
        private bool hasTarget;

        public Runtime(InvestigateNoiseState asset, GuardBrain brain)
        {
            this.asset = asset;
            this.brain = brain;
        }

        public void Enter()
        {
            startedAt = Time.time;
            hasTarget = brain.Ctx.Noise.HasSignal;
            target = hasTarget ? brain.Ctx.Noise.Point : (Vector2)brain.transform.position;
            brain.Ctx.ClearNoise();
        }

        public void Tick()
        {
            if (!hasTarget) { SwitchBack(); return; }

            bool arrived = brain.Ctx.Mover.MoveTowards(
                target,
                asset.moveSpeed,               // <-- use state-local speed
                asset.stopDistance);

            if (arrived)
            {
                if (reachedAt <= 0f) reachedAt = Time.time;
                if (Time.time - reachedAt >= asset.lingerTime) SwitchBack();
                return;
            }

            if (Time.time - startedAt >= asset.maxInvestigateTime)
                SwitchBack();
        }

        public void Exit() { }

        private void SwitchBack()
        {
            if (asset.resumeState != null)
                brain.Switch(asset.resumeState);
        }
    }
}
