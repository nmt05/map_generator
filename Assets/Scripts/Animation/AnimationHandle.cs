using UnityEngine;

public class AnimationHandle : MonoBehaviour
{
    private Animator anim;
    private bool isDead = false;
    public bool IsDead => isDead;
    void Awake() => anim = GetComponent<Animator>();

    // Hàm public này có thể được gọi bởi bất kỳ script nào khác
    public void PlayAnimation(string triggerName)
    {
        anim.SetTrigger(triggerName);
    }
    public void Die()
    {
        anim.SetBool("IsDie", true);
        isDead = true;
        
    }
    public void SetFlag()
    {
        // if(!gameObject) return;
        anim.SetBool("IsDie", false); 
        isDead = false; 
    }

}
