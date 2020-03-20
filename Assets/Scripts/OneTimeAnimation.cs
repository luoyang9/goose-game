using UnityEngine;
using System.Collections;

public class OneTimeAnimation : StateMachineBehaviour {
    public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        Destroy(animator.gameObject);
    }
}
