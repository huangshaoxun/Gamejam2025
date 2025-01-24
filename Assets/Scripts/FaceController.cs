using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceController : MonoBehaviour
{
    public static FaceController Instance;

    public Animator animator;
    public Transform LeftEye;
    public Transform RightEye;
    public Vector3 EyeTarget;
    private Vector3 EyeVelocity;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        animator = GetComponent<Animator>();
    }

    public enum FaceType
    {
        Default,
        Thinking,
        Wow,
        BrainStorm,
        Question,
    }
    
    public void SetFaceType(FaceType type)
    {
        switch (type)
        {
            case FaceType.Default:
                animator.Play("Default");
                break;
            case FaceType.Thinking:
                //animator.Play("Thinking");
                break;
            case FaceType.Wow:
                animator.Play("Wow");
                break;
            case FaceType.BrainStorm:
                animator.Play("BrainStorm");
                break;
            case FaceType.Question:
                //animator.Play("Question");
                break;
            default:
                break;
        }
    }
    
    public void SetEyePos(Vector3 vec)
    {
        //EyeTarget = vec;
    }

    public void Update()
    {
        //RightEye.localPosition = LeftEye.localPosition = Vector3.SmoothDamp(LeftEye.localPosition, EyeTarget, ref EyeVelocity, 0.2f);
    }
}
