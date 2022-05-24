using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public static bool explode = false;

    public Animator bombAnimator;

    private void Start()
    {
        bombAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.name.Equals("Cylinder(Clone)"))
        {
            explode = true;

            //play animation
            StartCoroutine(PlayExplosionAnim());
        }
    }

    IEnumerator PlayExplosionAnim()
    {
        //play animation
        bombAnimator.SetBool("Exploding", true);

        //wait for screen to fade
        yield return new WaitForSeconds(0.417f);

        //play animation
        bombAnimator.SetBool("Exploding", false);
    }
}