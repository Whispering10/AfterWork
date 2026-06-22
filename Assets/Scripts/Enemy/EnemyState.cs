using UnityEngine;

public enum EnemyStates
{
    Idle,
    Run,
    Attack,
    Die
}

public class EnemyState
{
    public EnemyStates State = EnemyStates.Idle;

    private Animator animator;
    
    public EnemyState(Animator animator)
    {
        this.animator = animator;
    }

    public void Animate()
    {
        switch (State)
        {
            case EnemyStates.Die:
                {
                    animator.SetTrigger("Death");

                    break;
                }
            case EnemyStates.Attack:
                {
                    animator.SetTrigger("Attack");

                    break;
                }
            case EnemyStates.Run:
                {
                    animator.SetBool("Run", true);

                    break;
                }
            case EnemyStates.Idle:
                {
                    animator.SetBool("Run", false);

                    break;
                }
        }
    }
}
