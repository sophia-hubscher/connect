using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Controller : MonoBehaviour
{
    public GameObject   copyLine;
    public GameObject[] shapeArray;
    public Material     defaultMat;
    public Material     correctMat;
    public Animator     finalSphereAnimator;
    public Animator     panelAnimator;
    public Animator     houseIconAnimator;

    float x1, x2, z1, z2;
    bool levelEnding = false;

    Shape firstShape;
    Line  ghostLine;

    List<Line>  lines  = new List<Line>();
    List<Shape> shapes = new List<Shape>();

    private void Start()
    {
        //create list of shape objects
        for (int i = 0; i < shapeArray.Length; i++)
        {
            shapes.Add(new Shape(shapeArray[i]));
        }

        //create temporary line
        ghostLine = new Line(Instantiate(copyLine));
    }

    private void Update()
    {
        //raycast!!
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //map mouse coords to world coords
        Vector3 mouseLoc = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            //if clicked
            if (Input.GetMouseButtonDown(0))
            {
                if (hit.transform.name.Equals("Cylinder(Clone)"))
                {
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (lines[i].SameLine(hit.transform.position))
                        {
                            DeleteLine(lines[i]);
                        }
                    }
                } else if (hit.transform.name.Equals("House"))
                {
                    StartCoroutine(LoadLevel(0));
                }
            }

            //if mouse is down
            if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0))
            {
                //if hit bomb, stop drawing the line
                if (Bomb.explode)
                {
                    firstShape = null;
                    ghostLine.Vanish();

                    //turn explosion off
                    Bomb.explode = false;
                }

                //if hit a shape, starting a new line
                if (hit.transform.CompareTag("selectable") && firstShape == null)
                {
                    for (int i = 0; i < shapes.Count; i++)
                    {
                        //find correct shape
                        if (shapes[i].SameShape(hit.transform.position))
                        {
                            firstShape = shapes[i];

                            //set line anchor
                            x1 = firstShape.GetPosition().x;
                            z1 = firstShape.GetPosition().z;

                            //FAKE FIX to remove flashing
                            x2 = firstShape.GetPosition().x;
                            z2 = firstShape.GetPosition().z;
                        }
                    }
                }

                if (firstShape != null)
                {
                    x2 = mouseLoc.x;
                    z2 = mouseLoc.z;

                    Draw(ghostLine, x1, x2, z1, z2);
                }
            }
            else //if mouse is up
            {
                if (hit.transform.CompareTag("selectable"))
                {
                    //loop through all shapes to find selected shape
                    for (int i = 0; i < shapes.Count; i++)
                    {
                        //find correct shape & check that a line is being drawn
                        if (shapes[i].SameShape(hit.transform.position) &&
                            firstShape != null)
                        {
                            //checks that line is not being drawn from a shape to itself
                            //& that line doesnt already exist
                            if (!firstShape.SameShape(hit.transform.position) &&
                                !LineExists(firstShape.GetPosition(), hit.transform.position))
                            {
                                CreateLine(firstShape, shapes[i]);

                                x2 = shapes[i].GetPosition().x;
                                z2 = shapes[i].GetPosition().z;

                                Draw(lines[lines.Count - 1], x1, x2, z1, z2);
                            }

                            firstShape = null;
                            ghostLine.Vanish();
                        }
                    }
                }
                else
                {
                    firstShape = null;
                    ghostLine.Vanish();
                }

                //play house icon animation
                if (hit.transform.name.Equals("House"))
                {
                    houseIconAnimator.SetBool("Hovered", true);
                } else
                {
                    houseIconAnimator.SetBool("Hovered", false);
                }

                //if drew a line that doesn't work, destroy it
                if (Bomb.explode)
                {
                DeleteLine(lines[lines.Count - 1]);

                //turn explosion off
                Bomb.explode = false;
                }
            }

            //check if level is complete
            bool levelComplete = true;

            //color all shapes on screen
            for (int i = 0; i < shapes.Count; i++)
            {
                if (shapes[i].IsCorrect())
                {
                    shapes[i].SetMaterial(correctMat);
                }
                else
                {
                    shapes[i].SetMaterial(defaultMat);

                    //if any shape is wrong, the level is not complete
                    levelComplete = false;
                }
            }

            if (levelComplete)
            {
                //TRASH FIX prevents level from increasing while coroutine runs
                if (!levelEnding)
                {
                    //load next level if it exists
                    if (SceneManager.sceneCountInBuildSettings >= SceneManager.GetActiveScene().buildIndex)
                    {
                        //load next level
                        MenuController.currentLevel++;
                    }
                    else
                    {
                        //load menu
                        MenuController.currentLevel = 0;
                    }

                    //play animations & load level
                    StartCoroutine(LoadLevel(MenuController.currentLevel));
                    levelEnding = true;
                }
            }
        }
    }

    void CreateLine(Shape shape1, Shape shape2)
    {
        lines.Add(new Line(shape1, shape2, Instantiate(copyLine)));

        //add line number to each shape
        shape1.AddLine();
        shape2.AddLine();
    }

    void Draw(Line line, float X1, float X2, float Z1, float Z2)
   {
        //find line length
        float lineLength = 0.5f * Mathf.Sqrt(Mathf.Pow((X2 - X1), 2) + Mathf.Pow((Z2 - Z1), 2));

        //find midpoints
        float xOut = (X1 + X2) / 2;
        float zOut = (Z1 + Z2) / 2;

        //draw line
        line.GetTransform().position   = new Vector3(xOut, 0f, zOut);
        line.GetTransform().localScale = new Vector3(1.5f, lineLength, 1.5f);

        line.GetObject().GetComponent<CapsuleCollider>().transform.position = new Vector3(xOut, 0f, zOut);
        line.GetObject().GetComponent<CapsuleCollider>().height = 2;

        //rotate line (i know its bad)
        line.GetTransform().LookAt(new Vector3(X2, 0, Z2), Vector3.forward);
        line.GetTransform().Rotate(0f, 90f, 0f, Space.World);
    }

    bool LineExists(Vector3 shape1Pos, Vector3 shape2Pos)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            if ((lines[i].GetShape1().SameShape(shape1Pos) &&
                 lines[i].GetShape2().SameShape(shape2Pos)) ||
                (lines[i].GetShape1().SameShape(shape2Pos) &&
                 lines[i].GetShape2().SameShape(shape1Pos)))
            {
                return true;
            }
        }

        return false;
    }

    void DeleteLine(Line line)
    {
        line.SubtractLines();

        Destroy(line.GetObject());
        lines.Remove(line);
    }

    IEnumerator LoadLevel(int level)
    {
        //wait
        yield return new WaitForSeconds(.1f);

        //begin blue sphere animation
        finalSphereAnimator.SetTrigger("Grow");

        //wait for sphere to grow
        yield return new WaitForSeconds(2.5f);

        //begin fade
        panelAnimator.SetTrigger("Fade Out");

        //wait for screen to fade
        yield return new WaitForSeconds(0.75f);

        //load next level
        SceneManager.LoadScene(level);

        //turn triggers off
        finalSphereAnimator.ResetTrigger("Grow");
        panelAnimator.ResetTrigger("Fade Out");
    }
}

public class Shape
{
    readonly int sides;
    public int lines;
    GameObject obj;

    public Shape(GameObject obj)
    {
        this.obj = obj;

        sides = int.Parse(obj.name);
        lines = 0;
    }

    public void AddLine()
    {
        lines++;
    }

    public void SubtractLine()
    {
        lines--;
    }

    public bool IsCorrect()
    {
        return sides == lines;
    }

    public bool SameShape(Vector3 pos)
    {
        //if same x & z returns true
        if (((int)pos.x == (int)obj.transform.position.x) &&
            ((int)pos.z == (int)obj.transform.position.z))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public Vector3 GetPosition()
    {
        return obj.transform.position;
    }

    public int GetSides()
    {
        return sides;
    }

    public void SetMaterial(Material material)
    {
        obj.GetComponent<MeshRenderer>().material = material;
    }
}

public class Line
{
    Shape shape1, shape2;
    GameObject obj;

    public Line(GameObject obj)
    {
        this.obj = obj;
    }

    public Line(Shape shape1, Shape shape2, GameObject obj)
    {
        this.shape1 = shape1;
        this.shape2 = shape2;
        this.obj = obj;
    }

    public Transform GetTransform ()
    {
        return obj.transform;
    }

    public GameObject GetObject()
    {
        return obj;
    }

    public Shape GetShape1()
    {
        return shape1;
    }

    public Shape GetShape2()
    {
        return shape2;
    }

    public void Vanish()
    {
        obj.transform.position   = new Vector3(0, 0, 0);
        obj.transform.localScale = new Vector3(0, 0, 0);
    }

    public bool SameLine(Vector3 pos)
    {
        //if same x & z returns true
        if (((int)pos.x == (int)obj.transform.position.x) &&
            ((int)pos.z == (int)obj.transform.position.z))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void SubtractLines()
    {
        shape1.SubtractLine();
        shape2.SubtractLine();
    }
}