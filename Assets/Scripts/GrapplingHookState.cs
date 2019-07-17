using UnityEngine;
using UnityEditor;

public class GrapplingHookState : State {

    public GrapplingHookState() { }

    public override void Update() {
        player.AutoRappel();
        if(player.reachedHook()) {
            machine.gameObject.GetComponent<RopeSystem>().ResetRope();
            machine.SwitchState<FallState>();
        }
    }

    public override void OnStateEnter() {
        base.OnStateEnter();
    }

    public override void OnStateExit() {
        base.OnStateExit();
    }
}