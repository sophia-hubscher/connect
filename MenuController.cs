using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public Animator panelAnimator;
    public Animator playAnimator;
    public Animator quitAnimator;

    public static int currentLevel = 1;

    // Update is called once per frame
    void Update()
    {
        //raycast!!
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            //if clicked
            if (Input.GetMouseButtonDown(0))
            {
                if (hit.transform.name.Equals("Play"))
                {
                    //load level
                    StartCoroutine(LoadLevel(currentLevel));
                }
                else if (hit.transform.name.Equals("Quit"))
                {
                    //quit game
                    Application.Quit();
                }
            }

            //if hovering
            if (!Input.GetMouseButton(0))
            {
                if (hit.transform.name.Equals("Play"))
                {
                    //play hover animation
                    playAnimator.SetBool("Hovered", true);

                    //turn off other hover animation
                    quitAnimator.SetBool("Hovered", false);
                }
                else if (hit.transform.name.Equals("Quit"))
                {
                    //play hover animation
                    quitAnimator.SetBool("Hovered", true);

                    //turn off other hover animation
                    playAnimator.SetBool("Hovered", false);
                }
                else
                {
                    //turn off all hover animations
                    playAnimator.SetBool("Hovered", false);
                    quitAnimator.SetBool("Hovered", false);
                }
            }
        }
    }

    IEnumerator LoadLevel(int level)
    {
        //begin fade
        panelAnimator.SetTrigger("Fade Out");

        //wait for screen to fade
        yield return new WaitForSeconds(0.75f);

        //load next level
        SceneManager.LoadScene(level);

        //turn fade trigger off
        panelAnimator.ResetTrigger("Fade Out");
    }
}