using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Animator))]

public class ataque : MonoBehaviour
{
    
    float ataqueInput;
    private Animator animator;

    void Awake(){
        animator = GetComponent<Animator>();
    }


    // Update is called once per frame
    void Update()
    {
        ataqueInput = Input.GetAxisRaw("Submit");
        Debug.Log("el valor del input" + ataqueInput);

        if(ataqueInput > 0.01f){
            animator.SetFloat ("ataque", ataqueInput);
    } else {
            animator.SetFloat ("ataque", 0);
        }
    }
}
