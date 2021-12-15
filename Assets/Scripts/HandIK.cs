using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandIK : MonoBehaviour
{
    [SerializeField]
    private Transform leftHandObj;

    [SerializeField]
    private Transform attachLeft;

    [Range(0, 1)] public float leftHandPositionWeight = 1.0f;
    [Range(0, 1)] public float leftHandRotationWeight = 1.0f;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (leftHandObj)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandPositionWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandRotationWeight);
            if (attachLeft)
            {
                animator.SetIKPosition(AvatarIKGoal.LeftHand, attachLeft.position);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, attachLeft.rotation);
            }
        }
    }
}
