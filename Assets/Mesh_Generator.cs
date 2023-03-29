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
        outlines.Clear();
        checkedVertices.Clear();
        triangleDictionary.Clear();

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

        createWallMesh();
    }

    public MeshFilter walls;

    void createWallMesh()
    {
        calculateMeshOutlines();

        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        Mesh wallMesh = new Mesh();
        float wallHeight = 5;

        foreach(List<int> outline in outlines)
        {
            for(int i=0; i<outline.Count-1; ++i)
            {
                int startIndex = wallVertices.Count;
                wallVertices.Add(vertices[outline[i]]); //Left vertex
                wallVertices.Add(vertices[outline[i+1]]); //Right vertex
                wallVertices.Add(vertices[outline[i]] - Vector3.up * wallHeight); //Bottom left vertex
                wallVertices.Add(vertices[outline[i+1]] - Vector3.up * wallHeight); //Bottom right vertex

                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);

                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 0);
            }
        }

        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();
        walls.mesh = wallMesh;
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

    Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
    struct Triangle
    {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;
        int[] vertices;

        public Triangle(int A, int B, int C)
        {
            vertexIndexA = A;
            vertexIndexB = B;
            vertexIndexC = C;

            vertices = new int[3];
            vertices[0] = A;
            vertices[1] = B;
            vertices[2] = C;
        }

        public int this[int i]
        {
            get { return vertices[i]; }
        }

        public bool contains(int vertexIndex)
        {
            return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
        }

    }

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
                    meshFromPoint(sq.centerLeft, sq.centerBottom, sq.bottomLeft);
                    break;
                }
            case 2:
                {
                    meshFromPoint(sq.bottomRight, sq.centerBottom, sq.centerRight);
                    break;
                }
            case 4:
                {
                    meshFromPoint(sq.topRight, sq.centerRight, sq.centerTop);
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
                    checkedVertices.Add(sq.topLeft.vertexIndex);
                    checkedVertices.Add(sq.topRight.vertexIndex);
                    checkedVertices.Add(sq.bottomLeft.vertexIndex);
                    checkedVertices.Add(sq.bottomRight.vertexIndex);
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

        Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
        addTriangleDictionary(triangle.vertexIndexA, triangle);
        addTriangleDictionary(triangle.vertexIndexB, triangle);
        addTriangleDictionary(triangle.vertexIndexC, triangle);
    }

    void addTriangleDictionary(int vertexIndexKey, Triangle triangle)
    {
        if (triangleDictionary.ContainsKey(vertexIndexKey))
        {
            triangleDictionary[vertexIndexKey].Add(triangle);
        }
        else
        {
            List<Triangle> trianglelist = new List<Triangle>();
            trianglelist.Add(triangle);
            triangleDictionary.Add(vertexIndexKey, trianglelist);
        }
    }

    List<List<int>> outlines = new List<List<int>>();
    HashSet<int> checkedVertices = new HashSet<int>();
    bool isOutlineEdge(int vertexA, int vertexB)
    {
        List<Triangle> trianglesContainingVertexA = triangleDictionary[vertexA];
        int sharedTriangleCount = 0;
        for(int i = 0; i < trianglesContainingVertexA.Count; i++)
        {
            if (trianglesContainingVertexA[i].contains(vertexB))
            {
                sharedTriangleCount++;
                if(sharedTriangleCount > 1)
                {
                    break;
                }
            }
        }
        return sharedTriangleCount == 1;
    }

    void calculateMeshOutlines()
    {
        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
        {
            if(!checkedVertices.Contains(vertexIndex))
            {
                int newOutlineVertex = getConnectedOutlineVertex(vertexIndex);
                if(newOutlineVertex != -1)
                {
                    checkedVertices.Add(vertexIndex);
                    List<int> newOutline = new List<int>();
                    newOutline.Add(vertexIndex);
                    outlines.Add(newOutline);
                    followOutline(newOutlineVertex, outlines.Count-1);
                    outlines[outlines.Count-1].Add(vertexIndex);
                }
            }
        }
    }

    void followOutline(int vertexIndex, int outlineIndex)
    {
        outlines[outlineIndex].Add(vertexIndex);
        checkedVertices.Add(vertexIndex);
        int nextVertexIndex = getConnectedOutlineVertex(vertexIndex);
        if(nextVertexIndex != -1)
        {
            followOutline(nextVertexIndex, outlineIndex);
        }
    }

    int getConnectedOutlineVertex(int vertexIndex)
    {
        List<Triangle> triangleContaingVertex = triangleDictionary[vertexIndex];
        for(int  i=0; i < triangleContaingVertex.Count; i++)
        {
            Triangle triangle = triangleContaingVertex[i];
            for(int j=0; j < 3; j++)
            {
                int vertexB = triangle[j];
                if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB))
                {
                    if (isOutlineEdge(vertexIndex, vertexB))
                    {
                        return vertexB;
                    }
                }
            }
        }
        return -1;
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
