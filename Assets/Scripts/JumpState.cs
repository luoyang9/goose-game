using UnityEngine;
using UnityEditor;

public class JumpState : State {
    public JumpState() { }

    public override void Update() {
        if(Input.GetAxis(player.controller + "_Vertical") < -0.5f && player.rBody.velocity.y < 5f) {
            player.HardFall();
        } else {
            player.Airborne();
        }
        if(player.rBody.velocity.y < 0) {
            machine.SwitchState<FallState>();
        }
    }

    public override void OnStateEnter() {
        base.OnStateEnter();
        player.Jump();
    }

    public override void OnStateExit() {
        base.OnStateExit();
    }
}