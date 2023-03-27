using UnityEngine;
using System.Collections.Generic;
using System.Drawing;

public class Mesh_Generator : MonoBehaviour
{
    public UnityEngine.Color cavesColour;
    public UnityEngine.Color waterColour;

    public class intermediateNode
    {
        public Vector3 position;
        public int vertexIndex = -1;

        public intermediateNode(Vector3 pos)
        {
            position = pos;
        }
    }

    public class controllerNode : intermediateNode
    {
        public bool isActive;
        public intermediateNode above;
        public intermediateNode right;

        public controllerNode(Vector3 pos, bool active, float squareSize) : base(pos)
        {
            isActive = active;
            above = new intermediateNode(position + Vector3.forward * squareSize * 0.5f);
            right = new intermediateNode(position + Vector3.right * squareSize * 0.5f);

        }
    }

    public class square
    {
        public controllerNode topRight, topLeft, bottomRight, bottomLeft;
        public intermediateNode centerTop, centerBottom, centerLeft, centerRight;
        public int configurations;

        public square(controllerNode tL, controllerNode tR, controllerNode bR, controllerNode bL)
        {
            topLeft = tL;
            topRight = tR;
            bottomLeft = bL;
            bottomRight = bR;

            centerTop = topLeft.right;
            centerRight = bottomRight.above;
            centerBottom = bottomLeft.right;
            centerLeft = bottomLeft.above;

            //Setting up configuration of squares in the grid
            if (topLeft.isActive)
                configurations += 8;
            if (topRight.isActive)
                configurations += 4;
            if (bottomRight.isActive)
                configurations += 2;
            if (bottomLeft.isActive)
                configurations += 1;
        }
    }

    public squareGrid sqG;
    public void meshGenerate(int[,] level, float squareSize)
    {
        sqG = new squareGrid(level, squareSize);

        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int i = 0; i < sqG.varSquare.GetLength(0); ++i)
        {
            for (int j = 0; j < sqG.varSquare.GetLength(1); ++j)
            {
                triangulateSquare(sqG.varSquare[i, j]);
            }
        }

        //Setting up the mesh filter and renderer
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    public class squareGrid
    {
        public square[,] varSquare;
        public squareGrid(int[,] level, float squareSize)
        {
            int nodeCountx = level.GetLength(0);
            int nodeCounty = level.GetLength(1);
            float levelWidth = nodeCountx * squareSize;
            float levelHeight = nodeCounty * squareSize;

            controllerNode[,] controllerNodes = new controllerNode[nodeCountx, nodeCounty];
            for (int i = 0; i < nodeCountx; ++i)
            {
                for (int j = 0; j < nodeCounty; ++j)
                {
                    Vector3 pos = new Vector3(-levelWidth * 0.5f + i * squareSize + squareSize * 0.5f, 0, -levelHeight * 0.5f + j * squareSize + squareSize * 0.5f);
                    controllerNodes[i, j] = new controllerNode(pos, level[i, j] == 1, squareSize);
                }
            }

            varSquare = new square[nodeCountx - 1, nodeCounty - 1];
            //Going through each square in the "square" array and setting each one to a new square
            for (int i = 0; i < nodeCountx - 1; ++i)
            {
                for (int j = 0; j < nodeCounty - 1; ++j)
                {
                    varSquare[i, j] = new square(controllerNodes[i, j + 1], controllerNodes[i + 1, j + 1], controllerNodes[i + 1, j], controllerNodes[i, j]);
                }
            }
        }
    }

   /* private void OnDrawGizmos()
    {
        if (sqG != null)
        {
            for (int i = 0; i < sqG.varSquare.GetLength(0); ++i)
            {
                for (int j = 0; j < sqG.varSquare.GetLength(1); ++j)
                {
                    if (sqG.varSquare[i, j].topLeft.isActive)
                        //Gizmos.color = cavesColour;
                        continue;
                    else
                        Gizmos.color = waterColour;
                    Gizmos.DrawCube(sqG.varSquare[i, j].topLeft.position, Vector3.one * 0.4f);

                    if (sqG.varSquare[i, j].topRight.isActive)
                        //Gizmos.color = cavesColour;
                        continue;
                    else
                        Gizmos.color = waterColour;
                    Gizmos.DrawCube(sqG.varSquare[i, j].topRight.position, Vector3.one * 0.4f);

                    if (sqG.varSquare[i, j].bottomLeft.isActive)
                        //Gizmos.color = cavesColour;
                        continue;
                    else
                        Gizmos.color = waterColour;
                    Gizmos.DrawCube(sqG.varSquare[i, j].bottomLeft.position, Vector3.one * 0.4f);

                    if (sqG.varSquare[i, j].topRight.isActive)
                        //Gizmos.color = cavesColour;
                        continue;
                    else
                        Gizmos.color = waterColour;
                    Gizmos.DrawCube(sqG.varSquare[i, j].topRight.position, Vector3.one * 0.4f);

                    Gizmos.color = UnityEngine.Color.black;
                    Gizmos.DrawCube(sqG.varSquare[i, j].centerTop.position, Vector3.one * 0.1f);
                    Gizmos.DrawCube(sqG.varSquare[i, j].centerBottom.position, Vector3.one * 0.1f);
                    Gizmos.DrawCube(sqG.varSquare[i, j].centerLeft.position, Vector3.one * 0.1f);
                    Gizmos.DrawCube(sqG.varSquare[i, j].centerRight.position, Vector3.one * 0.1f);
                }
            }
        }
    }*/

    List<Vector3> vertices;
    List<int> triangles;
    void triangulateSquare(square sq)
    {
        switch (sq.configurations)
        {
            case 0:
                break;

            //only 1 point selected
            case 1:
                {
                    meshFromPoint(sq.centerBottom, sq.bottomLeft, sq.centerLeft);
                    break;
                }
            case 2:
                {
                    meshFromPoint(sq.centerRight, sq.bottomRight, sq.centerBottom);
                    break;
                }
            case 4:
                {
                    meshFromPoint(sq.centerTop, sq.topRight, sq.centerRight);
                    break;
                }
            case 8:
                {
                    meshFromPoint(sq.topLeft, sq.centerTop, sq.centerLeft);
                    break;
                }

            //2 points selected
            case 3:
                {
                    meshFromPoint(sq.centerRight, sq.bottomRight, sq.bottomLeft, sq.centerLeft);
                    break;
                }
            case 6:
                {
                    meshFromPoint(sq.centerTop, sq.topRight, sq.bottomRight, sq.centerBottom);
                    break;
                }
            case 9:
                {
                    meshFromPoint(sq.topLeft, sq.centerTop, sq.centerBottom, sq.bottomLeft);
                    break;
                }
            case 12:
                {
                    meshFromPoint(sq.topLeft, sq.topRight, sq.centerRight, sq.centerLeft);
                    break;
                }
            case 5:
                {
                    meshFromPoint(sq.centerTop, sq.topRight, sq.centerRight, sq.centerBottom, sq.bottomLeft, sq.centerLeft);
                    break;
                }
            case 10:
                {
                    meshFromPoint(sq.topLeft, sq.centerTop, sq.centerRight, sq.bottomRight, sq.centerBottom, sq.centerLeft);
                    break;
                }

            //3 points selected
            case 7:
                {
                    meshFromPoint(sq.centerTop, sq.topRight, sq.bottomRight, sq.bottomLeft, sq.centerLeft);
                    break;
                }
            case 11:
                {
                    meshFromPoint(sq.topLeft, sq.centerTop, sq.centerRight, sq.bottomRight, sq.bottomLeft);
                    break;
                }
            case 13:
                {
                    meshFromPoint(sq.topLeft, sq.topRight, sq.centerRight, sq.centerBottom, sq.bottomLeft);
                    break;
                }
            case 14:
                {
                    meshFromPoint(sq.topLeft, sq.topRight, sq.bottomRight, sq.centerBottom, sq.centerLeft);
                    break;
                }

            //4 points selected
            case 15:
                {
                    meshFromPoint(sq.topLeft, sq.topRight, sq.bottomRight, sq.bottomLeft);
                    break;
                }
        }
    }

    void assignVertices(intermediateNode[] points)
    {
        for(int i=0; i<points.Length; ++i)
        {
            if (points[i].vertexIndex == -1)
            {
                points[i].vertexIndex = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }

    void createTriagnle(intermediateNode a, intermediateNode b, intermediateNode c)
    {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);
    }

    void meshFromPoint(params intermediateNode[] points)
    {
        assignVertices(points);

        if(points.Length>=3) 
            createTriagnle(points[0], points[1], points[2]);
        if(points.Length>=4)
            createTriagnle(points[0], points[2], points[3]);
        if(points.Length>=5)
            createTriagnle(points[0], points[3], points[4]);
        if(points.Length>=6)
            createTriagnle(points[0], points[4], points[5]);
    }
}
