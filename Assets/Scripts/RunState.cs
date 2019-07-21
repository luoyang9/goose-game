using UnityEngine;
using UnityEditor;

public class RunState : State {
    public RunState() { }

    public override void Update() {
        player.Run();
        if(Mathf.Abs(player.rBody.velocity.x) < 0.001) {
            machine.SwitchState<IdleState>();
        }
        if(Input.GetButtonDown(player.controller + "_Jump")) {
            machine.SwitchState<JumpState>();
        }
    }
}