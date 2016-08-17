using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Collections.Generic;

namespace ProjectorGaletes
{
    class ModelGraphics
    {
        public Vector2 _modelOrigin, _modelPosition, _modelScale;

        public Vector2 modelOrigin
        {
            get { return _modelOrigin; }
            set { _modelOrigin = value; }
        }

        public Vector2 modelPosition
        {
            get { return _modelPosition; }
            set { _modelPosition = value; }
        }

        public Vector2 modelScale
        {
            get { return _modelScale; }
            set { _modelScale = value; }
        }

        private float _modelRotation;

        public float modelRotation
        {
            get { return _modelRotation; }
            set { _modelRotation = value; }
        }

        private PancakeCoil _pancakeCoil;

        public PancakeCoil pancakeCoil
        {
            get { return _pancakeCoil; }
            set { _pancakeCoil = value; }
        }

        private int _currentTurn;

        public int currentTurn
        {
            get { return _currentTurn; }
            set { _currentTurn = value; }
        }

        private float _blinkAlpha;

        public float blinkAlpha
        {
            get { return _blinkAlpha; }
            set { _blinkAlpha = value; }
        }


        public ModelGraphics(Vector2 origin, Vector2 position, Vector2 scale, float rotation, PancakeCoil coil)
        {
            modelOrigin = origin;
            modelPosition = position;
            modelScale = scale;
            modelRotation = rotation;
            pancakeCoil = coil;
            blinkAlpha = 255;
            currentTurn = 0;

        }

        public void Redraw()
        {
            DrawMandrel(Color.Green, Vector2.Zero, _modelScale, _modelRotation, Color.Green, _modelOrigin, _pancakeCoil);
            DrawTurns(Color.Black, Vector2.Zero, _modelScale, _modelRotation, _modelOrigin, _pancakeCoil, _currentTurn);
            DrawCoilBoundary(Color.Red, Vector2.Zero, _modelScale, _modelRotation, _modelOrigin, _pancakeCoil);
            DrawCircle(3, Vector2.Zero, new Vector2(1f, 1f), _modelOrigin, Color.Blue);
            DrawCircunference(3.5f, Vector2.Zero, new Vector2(1f, 1f), _modelOrigin, 0, Color.Red);
            //DrawCross(new Vector2(mandril.E + 20, mandril.C + 20), Vector2.Zero, new Vector2(0.2f, 0.2f), origin, Color.Black);
            DrawRectangle(60f, 60f, Vector2.Zero, new Vector2(1f, 1f), _modelOrigin, Color.Red, (byte)blinkAlpha);
        }

        public static void DrawTurns(Color color, Vector2 position, Vector2 scale, float rotation, Vector2 origin, PancakeCoil coil, int maxTurns = -1)
        {
            int C = coil.mandrel.C;
            int E = coil.mandrel.E;
            int Ri = coil.mandrel.Ri;
            float H = coil.mandrel.H;
            int HR = coil.mandrel.HR;
            int F = coil.mandrel.F;

            double auxA, auxB, auxD, auxL;
            int auxAlpha, auxBeta;

            Vector2 arcCenter = new Vector2(0, 0);
            List<Vector2> vertices = new List<Vector2>();

            //Se galete inversa faz mirror em X
            if (coil.sentido == -1) { scale.X = -scale.X; }

            if (maxTurns == -1) { maxTurns = coil.nrEsp; }

            for (int esp = 1; esp <= maxTurns; esp++)
            {
                float R = Ri + coil.Hmed_R * esp;


                /*
                 *  auxA = E / 2 - F - Ri;
                    auxB = Ri - HR + H;
                 * 
                 */
                auxA = E / 2 - F + coil.Hmed_G * esp - R;
                auxB = R - HR + H;
                auxD = Math.Sqrt(auxA * auxA + auxB * auxB);
                auxL = Math.Sqrt(auxD * auxD - R * R);

                if (auxL == 0)
                {
                    auxAlpha = 90;
                }
                else
                {
                    auxAlpha = (int)(Math.Atan(R / auxL) * 180 / 3.1416);
                }

                auxBeta = (int)(Math.Atan(auxA / auxB) * 180 / 3.1416);

                // Load geometry array
                if (esp == 1)
                {
                    //vertices.Add(new Vector2 ((float)-F, C/2+coil.Hmed_A1*esp));
                    vertices.Add(new Vector2((float)-F, C / 2 + coil.mandrel.H));
                }

                arcCenter.X = E / 2 + (coil.Hmed_G * esp) - R;
                arcCenter.Y = C / 2 + coil.Hmed_A2 * esp - R;
                DrawArc(ref vertices, arcCenter, 90, 0, R);

                arcCenter.X = E / 2 + (coil.Hmed_G * esp) - R;
                arcCenter.Y = -(C / 2 + coil.Hmed_B * esp) + R;
                DrawArc(ref vertices, arcCenter, 0, -90, R);

                arcCenter.X = -(E / 2 + (coil.Hmed_G * esp) - R);
                arcCenter.Y = -(C / 2 + coil.Hmed_B * esp) + R;
                DrawArc(ref vertices, arcCenter, -90, -180, R);

                arcCenter.X = -(E / 2 + (coil.Hmed_G * esp) - R);
                arcCenter.Y = C / 2 + coil.Hmed_A * esp - R + HR;
                DrawArc(ref vertices, arcCenter, -180, -180 - auxAlpha - auxBeta, R);

                vertices.Add(new Vector2((float)-F, (float)C / 2 + H + coil.Hmed_A1 * esp));
            }

            DrawMultiLine(vertices, color, position, scale, rotation, origin, continuous: false);

        }

        public static void DrawCrossOverLocation(Color fillColor, Vector2 position, Vector2 scale, float rotation, Color color, Vector2 origin, PancakeCoil coil) 
        {
            
        }

        public static void DrawMandrel(Color fillColor, Vector2 position, Vector2 scale, float rotation, Color color, Vector2 origin, PancakeCoil coil)
        {
            int C, E, Ri, HR, F;
            float H;


            C = coil.mandrel.C;
            E = coil.mandrel.E;
            Ri = coil.mandrel.Ri;
            H = coil.mandrel.H;
            HR = coil.mandrel.HR;
            F = coil.mandrel.F;

 
            Vector2 arcCenter = new Vector2(0, 0);

            double auxA, auxB, auxD, auxL;
            float auxAlpha, auxBeta;

            //Se galete inversa faz mirror em X
            if (coil.sentido == -1) { scale.X = -scale.X; }
           

            auxA = E / 2 - F - Ri;
            auxB = Ri - HR + H;
            auxD = Math.Sqrt(auxA * auxA + auxB * auxB);
            auxL = Math.Sqrt(auxD * auxD - Ri * Ri);

            if (auxL == 0)
            {
                auxAlpha = 90;
            }
            else
            {
                auxAlpha = (int)(Math.Atan(Ri / auxL) * 180 / 3.1416);
            }

            auxBeta = (int)(Math.Atan(auxA / auxB) * 180 / 3.1416);

            // Load geometry array
            List<Vector2> vertices = new List<Vector2>();

            vertices.Add(new Vector2((float)E / 2, 0f));

            arcCenter.X = E / 2 - Ri;
            arcCenter.Y = -C / 2 + Ri;
            DrawArc(ref vertices, arcCenter, 0, -90, Ri);

            arcCenter.X = -E / 2 + Ri;
            arcCenter.Y = -C / 2 + Ri;
            DrawArc(ref vertices, arcCenter, -90, -180, Ri);

            arcCenter.X = -E / 2 + Ri;
            arcCenter.Y = C / 2 - Ri + HR;
            DrawArc(ref vertices, arcCenter, -180, -180 - auxAlpha - auxBeta, Ri);

            vertices.Add(new Vector2((float)-F, (float)C / 2 + H));
            vertices.Add(new Vector2((float)-F, (float)C / 2));

            arcCenter.X = E / 2 - Ri;
            arcCenter.Y = C / 2 - Ri;
            DrawArc(ref vertices, arcCenter, 90, 0, Ri);

            DrawMultiLine(vertices, color, position, scale, rotation, origin);

        }

        public static void DrawCoilBoundary(Color color, Vector2 position, Vector2 scale, float rotation, Vector2 origin, PancakeCoil coil)
        {
            int K = coil.K;
            int W = coil.W;
            int Re = coil.Re;
            float H = coil.mandrel.H;
            int HR = coil.mandrel.HR;
            int F = coil.mandrel.F;
            Vector2 arcCenter = new Vector2(0, 0);



            // Load geometry array
            List<Vector2> vertices = new List<Vector2>();

            //Se galete inversa faz mirror em X
            if (coil.sentido == -1) { scale.X = -scale.X; }

            //vertices.Add(new Vector2 ((float)E/2, 0f));
            arcCenter.X = W / 2 - Re;
            arcCenter.Y = K / 2 - Re;
            DrawArc(ref vertices, arcCenter, 90, 0, Re);

            arcCenter.X = W / 2 - Re;
            arcCenter.Y = -K / 2 + Re;
            DrawArc(ref vertices, arcCenter, 0, -90, Re);

            arcCenter.X = -W / 2 + Re;
            arcCenter.Y = -K / 2 + Re;
            DrawArc(ref vertices, arcCenter, -90, -180, Re);

            arcCenter.X = -W / 2 + Re;
            arcCenter.Y = K / 2 - Re + HR;
            DrawArc(ref vertices, arcCenter, -180, -270, Re);

            DrawMultiLine(vertices, color, position, scale, rotation, origin);

        }

        public static void DrawArc(ref List<Vector2> vertices, Vector2 center, float startAngle, float endAngle, float radius)
        {
            float Cx = center.X;
            float Cy = center.Y;
            float inc;
            //int index = vertices.Length-1;

            if (endAngle > startAngle)
            {
                inc = +1f;
            }
            else if (endAngle < startAngle)
            {
                inc = -1f;
            }
            else
            {
                return; //startAngle = endAngle do nothing
            }

            for (float i = startAngle; i >= endAngle; i += inc)
            {
                double degInRad = (i) * Math.PI / 180;
                vertices.Add(new Vector2((float)(Cx + Math.Cos(degInRad) * radius), (float)(Cy + Math.Sin(degInRad) * radius)));
            }
        }

        public static void DrawCircle(float raio, Vector2 position, Vector2 scale, Vector2 origin, Color fillColor)
        {

            List<Vector2> vertex = new List<Vector2>();

            for (int i = 0; i <= 360; i++)
            {
                double degInRad = (i) * 3.1416 / 180;
                vertex.Add(new Vector2((float)(Math.Cos(degInRad) * raio), (float)(Math.Sin(degInRad) * raio)));
            }

            DrawPolygon(vertex, fillColor, position, scale, 0, origin);

        }

        public static void DrawCircunference(float raio, Vector2 position, Vector2 scale, Vector2 origin, float rotation, Color color)
        {

            List<Vector2> vertex = new List<Vector2>();

            for (int i = 0; i <= 360; i++)
            {
                double degInRad = (i) * 3.1416 / 180;
                vertex.Add(new Vector2((float)(Math.Cos(degInRad) * raio), (float)(Math.Sin(degInRad) * raio)));
            }

            DrawMultiLine(vertex, color, position, scale, rotation, origin);

        }

        public static void DrawCross(Vector2 size, Vector2 position, Vector2 scale, Vector2 origin, Color color)
        {
            /*
            Vector2 vertice;

            GL.Begin(PrimitiveType.Lines);
            GL.Color3(color);

            vertice = new Vector2((float)-size.X / 2, (float)0);
            GL.Vertex2(ApplyTransformation(vertice, origin, scale, position,0));

            vertice = new Vector2((float)+size.X / 2, (float)0);

            GL.Vertex2(ApplyTransformation(vertice, origin, scale, position,0));

            vertice = new Vector2((float)0, (float)-size.Y / 2);

            GL.Vertex2(ApplyTransformation(vertice, origin, scale, position,0));

            vertice = new Vector2((float)0, (float)size.Y / 2);

            GL.Vertex2(ApplyTransformation(vertice, origin, scale, position,0));

            //GL.Vertex2 (vertice);

            GL.End();
             * */

        }

        public static void DrawRectangle(float width, float height, Vector2 position, Vector2 scale, Vector2 origin, Color fillColor, byte alpha)
        {
            List<Vector2> vertex = new List<Vector2>();


            vertex.Add(new Vector2((float)(-width / 2), (float)(-height / 2)));
            vertex.Add(new Vector2((float)(-width / 2), (float)(height / 2)));
            vertex.Add(new Vector2((float)(width / 2), (float)(height / 2)));
            vertex.Add(new Vector2((float)(width / 2), (float)(-height / 2)));

            DrawPolygon(vertex, fillColor, position, scale, 0, origin, alpha);

        }

        public static void DrawMultiLine(List<Vector2> vertex, Color color, Vector2 position, Vector2 scale, float rotation, Vector2 origin, bool continuous = true)
        {
            GL.PushMatrix();
            
            GL.Translate(origin.X + position.X, origin.Y + position.Y, 0);     //Poderá ser necessário alterar a ordem das transformações
            GL.Rotate(rotation, 0f, 0f, 1.0f);
            GL.Scale(scale.X, scale.Y, 0f);
            
            if (continuous)
            {
                GL.Begin(PrimitiveType.LineLoop);
            }
            else
            {
                GL.Begin(PrimitiveType.LineStrip);
            }
            GL.Color3(color);

            foreach (Vector2 vertice in vertex)
            {
                GL.Vertex2(vertice);
            }
            GL.End();
            GL.PopMatrix();
        }

        public static void DrawPolygon(List<Vector2> vertex, Color color, Vector2 position, Vector2 scale, float rotation, Vector2 origin, byte alpha = (byte)255)
        {
            GL.PushMatrix();


            GL.Translate(origin.X + position.X, origin.Y + position.Y, 0);     //Poderá ser necessário alterar a ordem das transformações
            GL.Rotate(rotation, 0f, 0f, 1.0f);
            GL.Scale(scale.X, scale.Y, 0f);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Begin(PrimitiveType.Polygon);
            GL.Color4((byte)color.R, (byte)color.G, (byte)color.B, alpha);

            foreach (Vector2 vertice in vertex)
            {
                GL.Vertex2(vertice);
            }

            GL.End();
            GL.PopMatrix();
            GL.Disable(EnableCap.Blend);

        }

        public static void Begin(int screenWidth, int screenHeight)
        {
            /*
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, screenWidth, screenHeight, 0, -1, 1);
             */
        }
    }
}

