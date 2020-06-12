using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ExposeAnimatorBool : MonoBehaviour
{
    [SerializeField] string animatorBoolParameterName = "isOn";
    int hashId;
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        hashId = Animator.StringToHash(animatorBoolParameterName);
    }

    public void SetAnimatorBool (bool state)
    {
        animator.SetBool(hashId, state);
    }
}