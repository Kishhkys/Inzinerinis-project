using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void RotateToPointer(Vector2 lookDirection)
    {
        Vector3 scale = transform.localScale;
        if (lookDirection.x > 0)
        {
            scale.x = 1;
        }
        else if (lookDirection.x < 0)
        {
            scale.x = -1;
        }
        transform.localScale = scale;
    }

    public void PlayAnimation(Vector2 movementInput)
    {
        animator.SetBool("Running", movementInput.magnitude > 0);

    }
}
