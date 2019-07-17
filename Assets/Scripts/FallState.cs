using UnityEngine;
using UnityEditor;

public class FallState : State
{
	public FallState() { }

	public override void Update() {
        if(player.grounded) {
            machine.SwitchState<IdleState>();
            return;
        }
        if(Input.GetKey(KeyCode.S)) {
            player.HardFall();
        } else {
            player.Airborne();
        }
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
	}
}