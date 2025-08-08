using UnityEngine;

[CreateAssetMenu(menuName = "AI/Guard/States/Chase")]
public class ChaseStateAsset : StateAsset
{
    [Header("Transitions")]
    public StateAsset attackState;
    public StateAsset backToPatrolState;

    public override IState CreateRuntime(GuardBrain brain) => new ChaseRuntime(brain, this);

    private class ChaseRuntime : IState
    {
        private readonly GuardBrain brain;
        private readonly ChaseStateAsset asset;
        private float lostTimer;

        public ChaseRuntime(GuardBrain brain, ChaseStateAsset asset)
        {
            this.brain = brain;
            this.asset = asset;
        }

        public void Enter() { lostTimer = 0f; }

        public void Tick()
        {
            var ctx = brain.Ctx;
            var p = ctx.Player;
            if (!p) { brain.Switch(asset.backToPatrolState); return; }

            bool visible = ctx.Perception && ctx.Perception.TryGetVisibility(p, out _);
            if (!visible)
            {
                lostTimer += Time.deltaTime;
                if (lostTimer >= ctx.Config.loseTime) { brain.Switch(asset.backToPatrolState); return; }
            }
            else lostTimer = 0f;

            var pos = (Vector2)p.position;
            ctx.Mover.MoveTowards(pos, ctx.Config.chaseSpeed);
            FaceTowards(pos, ctx.Self);

            if (Vector2.Distance(ctx.Self.position, p.position) <= ctx.Config.attackRange)
            {
                brain.Switch(asset.attackState);
            }
        }

        public void Exit() { }

        private static void FaceTowards(Vector2 target, Transform self)
        {
            var dir = (target - (Vector2)self.position).normalized;
            if (dir.sqrMagnitude > 1e-6f) self.right = new Vector3(dir.x, dir.y, 0f);
        }
    }
}
