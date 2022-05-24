using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawLine : MonoBehaviour
{
    public GameObject   line;
    public GameObject[] shapes;

    public float lineWidth = 1.5f;
    public Text  lineText;

    float x1, z1, x2, z2 = 0;
    float lineLength     = 0;
    float xOut, zOut     = 0;
    int   lineIndex      = 0;
    int   totalLines     = 0;
    int   linesLeft      = 0;
    int   counter        = 0;
    bool  newLine        = true;

    List<GameObject> lines = new List<GameObject>();

    private void Start()
    {
        //count total number of lines needed
        for (int i = 0; i < shapes.Length; i++)
        {
            totalLines += int.Parse(shapes[i].transform.name);
        }

        totalLines /= 2;
        linesLeft = totalLines;
    }

    // Update is called once per frame
    void Update()
    {
        //raycast!!
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //map mouse coords to world coords
        Vector3 mouseLoc = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //click to destroy lines
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                string lineName = hit.transform.name;

                if (lineName.StartsWith("Line", System.StringComparison.Ordinal))
                {
                    DestroyLine(int.Parse(lineName.Substring(lineName.Length - 1)));
                    linesLeft++;
                }
            }
        }

        //if mouse button is down
        if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0))
        {
            //set start & end coords of line
            if (newLine)
            {
                //set initial x & z
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.transform.CompareTag("selectable"))
                    {
                        CreateNewLine();

                        //if clicked object, set x & z to center of object
                        SetXAndZ(1, hit.transform.position);
                        SetXAndZ(2, hit.transform.position);
                    }
                }
            }
            else
            {
                //set end of line to mouse position
                x2 = mouseLoc.x;
                z2 = mouseLoc.z;
            }
                              
            //draw unfinished line
            if (lines.Count - 1 >= lineIndex)
            {
                Draw();
            }
        } else
        {
            //if currently drawing a line
            if (!newLine)
            {
                //check if hit second object
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    //if hit object that wasnt the first object clicked
                    if (hit.transform.CompareTag("selectable") &&
                        ((int)x1 != (int)hit.transform.position.x &&
                         (int)x2 != (int)hit.transform.position.x))
                    {
                        //if clicked object, set x & z to center of object
                        SetXAndZ(2, hit.transform.position);

                        Draw();

                        linesLeft--;
                        lineIndex++;
                    }
                    else
                    {
                        DestroyLine(lineIndex);
                    }
                }

                newLine = true;
            }
        }

        //update line text
        lineText.text = "";

        for (int i = 0; i < linesLeft; i++)
        {
            lineText.text += "┃";
        }
    }

    private void SetXAndZ(int number, Vector3 position)
    {
        if (number == 1)
        {
            x1 = position.x;
            z1 = position.z;
        } else if (number == 2)
        {
            x2 = position.x;
            z2 = position.z;
        }
    }

    private void CreateNewLine ()
    {
        //create new line
        lines.Add(Instantiate(line));
        lines[lines.Count - 1].name = "Line " + (counter);
        counter++;

        newLine = false;
    }

    private void Draw()
    {
        //find line length
        lineLength = 0.5f * Mathf.Sqrt(Mathf.Pow((x2 - x1), 2) + Mathf.Pow((z2 - z1), 2));

        //find midpoints
        xOut = (x1 + x2) / 2;
        zOut = (z1 + z2) / 2;

        //draw line
        lines[lineIndex].transform.position = new Vector3(xOut, 0f, zOut);
        lines[lineIndex].transform.localScale = new Vector3(lineWidth, lineLength, lineWidth);

        //rotate line (i know its bad)
        lines[lineIndex].transform.LookAt(new Vector3(x2, 0, z2), Vector3.forward);
        lines[lineIndex].transform.Rotate(0f, 90f, 0f, Space.World);
    }

    private void DestroyLine(int index)
    {
        //destroys line
        Destroy(lines[index]);
        lines.Remove(lines[index]);
    }
}