using UnityEngine;
using UnityEditor;

public class HookPullState : State {

    public HookPullState() { }

    public override void Update() {
        player.AutoRappel();
        if (player.ReachedHook()) {
            machine.SwitchState<HookEndState>();
        }
    }
}