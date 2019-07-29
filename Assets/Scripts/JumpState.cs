using UnityEngine;
using UnityEditor;

public class JumpState : State {
    public JumpState() { }

    public override void Update() {
        player.Airborne();
        if (player.rBody.velocity.y < 0) {
            machine.SwitchState<FallState>();
        }
    }

    public override void OnStateEnter() {
        base.OnStateEnter();
        player.Jump();
    }
}