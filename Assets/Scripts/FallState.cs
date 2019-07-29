using UnityEngine;
using UnityEditor;

public class FallState : State {

    public FallState() { }

	public override void Update() {;
        if (Mathf.Abs(player.rBody.velocity.y) < 0.001) {
            machine.SwitchState<IdleState>();
            return;
        }
        player.Airborne();
	}
}