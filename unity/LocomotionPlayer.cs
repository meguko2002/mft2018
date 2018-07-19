 /// <summary>
/// 
/// </summary>

using UnityEngine;
using System;
using System.Collections;
  
[RequireComponent(typeof(Animator))]  

//Name of class must be name of file as well

public class LocomotionPlayer : MonoBehaviour {

    protected Animator animator;

    private float speed = 0;
    private float direction = 0;
    private Locomotion locomotion = null;
    [SerializeField] private float speed_val;
    [SerializeField] private float speed_val2;

    // Use this for initialization
    void Start () 
	{
        animator = GetComponent<Animator>();
        locomotion = new Locomotion(animator);
	}
    
	void Update () 
	{
        if (animator && Camera.main)
		{
            JoystickToEvents.Do(transform,Camera.main.transform, ref speed, ref direction);
            //locomotion.Do(speed * 6, direction * 180);
            locomotion.Do(speed * speed_val, direction * 180);
            animator.speed = speed_val2; //(xx f = 10.0f ìôÅj

            // º”ÀŸ£°
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            bool inWalkRun = state.IsName("Locomotion.WalkRun");
            if (inWalkRun)
            {
                transform.position += transform.TransformDirection(Vector3.forward * speed / 10);
            }
		}		
	}
}
