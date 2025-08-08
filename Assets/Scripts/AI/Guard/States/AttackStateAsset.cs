using UnityEngine;
using TopDownThief.Interfaces;

[CreateAssetMenu(menuName = "AI/Guard/States/Attack")]
public class AttackStateAsset : StateAsset
{
    [Header("Transitions")]
    public StateAsset chaseState;

    public override IState CreateRuntime(GuardBrain brain) => new AttackRuntime(brain, this);

    private class AttackRuntime : IState
    {
        private readonly GuardBrain brain;
        private readonly AttackStateAsset asset;
        private float nextAttackTime;

        public AttackRuntime(GuardBrain brain, AttackStateAsset asset)
        {
            this.brain = brain;
            this.asset = asset;
        }

        public void Enter()
        {
            nextAttackTime = 0f;
        }

        public void Tick()
        {
            var ctx = brain.Ctx;
            var p = ctx.Player;
            if (!p) { brain.Switch(asset.chaseState); return; }

            float dist = Vector2.Distance(ctx.Self.position, p.position);
            if (dist > ctx.Config.attackRange * 1.1f)
            {
                brain.Switch(asset.chaseState);
                return;
            }

            FaceTowards(p.position, ctx.Self);

            if (Time.time >= nextAttackTime)
            {
                var dmg = p.GetComponentInParent<IDamageable>();
                if (dmg != null)
                {
                    dmg.TakeDamage(1); // можно вынести в Config
                    nextAttackTime = Time.time + ctx.Config.attackCooldown;
                }
            }
        }

        public void Exit() { }

        private static void FaceTowards(Vector2 target, Transform self)
        {
            var dir = (target - (Vector2)self.position).normalized;
            if (dir.sqrMagnitude > 1e-6f)
                self.right = new Vector3(dir.x, dir.y, 0f);
        }
    }
}
