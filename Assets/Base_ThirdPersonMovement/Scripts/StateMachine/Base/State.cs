using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State
{
    public abstract void Enter();
    public abstract void Tick(float deltaTime);
    public virtual void FixedTick(float fixedDeltaTime) { }
    public abstract void Exit();

    // Returns the normalized time (0-1) of the relevant animator state, or 0 if not in a tagged state.
    protected float GetNormalizedTime(Animator animator, string tagToCheck = "Attack")
    {
        AnimatorStateInfo currentInfo = animator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo nextInfo = animator.GetNextAnimatorStateInfo(0);

        if (animator.IsInTransition(0) && nextInfo.IsTag(tagToCheck))
            return nextInfo.normalizedTime;
        if (!animator.IsInTransition(0) && currentInfo.IsTag(tagToCheck))
            return currentInfo.normalizedTime;

        return 0f;
    }
}
