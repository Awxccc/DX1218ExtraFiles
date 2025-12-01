using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ObjectAnimator : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public void Hit()
    {
        if (animator != null)
        {
            animator.ResetTrigger("Hit");
            animator.SetTrigger("Hit");
        }
    }
    public void HitOnce()
    {
        if (animator != null)
        {
            animator.SetBool("HitOnce", true);
        }
    }

    public void Break()
    {
        if (animator != null) animator.SetBool("IsBroken", true);
    }
}