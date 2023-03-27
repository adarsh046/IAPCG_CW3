using UnityEngine;

public class Mesh_Generator : MonoBehaviour
{
    public Color cavesColour;
    public Color waterColour;

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
        }
    }

    public squareGrid sqG;
    public void meshGenerate(int[,] level, float squareSize)
    {
        sqG = new squareGrid(level, squareSize);
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
            for(int i= 0; i< nodeCountx; ++i) 
            {
                for (int j = 0; j < nodeCounty; ++j)
                {
                    Vector3 pos = new Vector3(-levelWidth * 0.5f + i * squareSize + squareSize * 0.5f, 0, -levelHeight * 0.5f + j * squareSize + squareSize * 0.5f);
                    controllerNodes[i, j] = new controllerNode(pos, level[i,j] == 1, squareSize);
                }
            }

            varSquare = new square[nodeCountx-1, nodeCounty-1];
            //Going through each square in the "square" array and setting each one to a new square
            for (int i = 0; i < nodeCountx-1; ++i)
            {
                for (int j = 0; j < nodeCounty-1; ++j)
                {
                    varSquare[i,j] = new square(controllerNodes[i, j+1], controllerNodes[i+1, j+1], controllerNodes[i+1, j], controllerNodes[i, j]);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if(sqG!= null)
        {
            for (int i = 0; i < sqG.varSquare.GetLength(0); ++i)
            {
                for (int j = 0; j < sqG.varSquare.GetLength(1); ++j)
                {
                    if(sqG.varSquare[i, j].topLeft.isActive)
                        Gizmos.color = cavesColour;
                    else
                        Gizmos.color = waterColour;
                    Gizmos.DrawCube(sqG.varSquare[i, j].topLeft.position, Vector3.one * 0.4f);

                    if (sqG.varSquare[i, j].topRight.isActive)
                        Gizmos.color = cavesColour;
                    else
                        Gizmos.color = waterColour;
                    Gizmos.DrawCube(sqG.varSquare[i, j].topRight.position, Vector3.one * 0.4f);

                    if (sqG.varSquare[i, j].bottomLeft.isActive)
                        Gizmos.color = cavesColour;
                    else
                        Gizmos.color = waterColour;
                    Gizmos.DrawCube(sqG.varSquare[i, j].bottomLeft.position, Vector3.one * 0.4f);

                    if (sqG.varSquare[i, j].topRight.isActive)
                        Gizmos.color = cavesColour;
                    else
                        Gizmos.color = waterColour;
                    Gizmos.DrawCube(sqG.varSquare[i, j].topRight.position, Vector3.one * 0.4f);

                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(sqG.varSquare[i, j].centerTop.position, Vector3.one * 0.1f);
                    Gizmos.DrawCube(sqG.varSquare[i, j].centerBottom.position, Vector3.one * 0.1f);
                    Gizmos.DrawCube(sqG.varSquare[i, j].centerLeft.position, Vector3.one * 0.1f);
                    Gizmos.DrawCube(sqG.varSquare[i, j].centerRight.position, Vector3.one * 0.1f);
                }
            }
        }
    }
}
