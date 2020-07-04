using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class xWalkFeedback : StateMachineBehaviour
{
    public AudioClip leftFoot, rightFoot, skid, roll;
    private Transform hostObject;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        hostObject = animator.transform;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}


    public void LeftFoot()
    {
        LeanAudio.playClipAt(leftFoot, hostObject.position);
    }

    public void RightFoot()
    {
        LeanAudio.playClipAt(rightFoot, hostObject.position);
    }

    public void Skid()
    {
        LeanAudio.playClipAt(skid, hostObject.position);
    }

    public void Roll()
    {
        LeanAudio.playClipAt(roll, hostObject.position);
    }
}
