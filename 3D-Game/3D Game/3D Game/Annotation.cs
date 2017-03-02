using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace _3D_Game
{
    abstract class Annotation
    {
        protected Annotation()
        {
            if (!IsChange)
            {
                AllAnnotations.Add(this);
            }
        }

        protected static BasicEffect BasicEffect { get; set; }

        public static Color? GlobalLineColor = Color.Black;
        public static Color? GlobalFillColor;
        public static Vector3 Position;
        public SpriteFont Font;
        public abstract bool IsChange { get; }
        public static void InitializeAnnotationSystem(GraphicsDevice d)
        {
            BasicEffect = new BasicEffect(d);
        }
        public SpriteBatch spriteBatch;

        public static List<Annotation> AllAnnotations = new List<Annotation>();
        public static List<AttributeChangeAnnotation> AllAttributeAnnotations = new List<AttributeChangeAnnotation>();

        public static void DrawAllAnnotations(Matrix view, Matrix projection)
        {
            BasicEffect.View = view;
            BasicEffect.Projection = projection;
            BasicEffect.World = Matrix.Identity;
            BasicEffect.VertexColorEnabled = true;
            BasicEffect.LightingEnabled = false;
            foreach (Annotation a in AllAnnotations)
            {
                {
                    a.Draw();
                }
            }
            foreach (AttributeChangeAnnotation b in AllAttributeAnnotations)
            {
                b.Draw();
            }

        }

        public abstract void Draw();
        //protected static Matrix CurrentDrawMatrix { get; set; }
        static void ResetAttributes()
        {
        }
    }

    abstract class AttributeChangeAnnotation : Annotation
    {
        protected AttributeChangeAnnotation(List<Annotation> children)
        {
            Children = children;
            AllAttributeAnnotations.Add(this);
        }
        public override bool IsChange { get { return true; } }
        List<Annotation> Children { get; set; }
        protected void DrawChildren()
        {
            //foreach (Annotation c in Children)
            //{
            //    c.Draw();
            //}
            Children[0].Draw();
        }
    }

    class LineAnnotation : Annotation
    {
        VertexPositionColor[] vertices { get; set; }
        Color LineColor;
        public override bool IsChange { get { return false; } }
        public LineAnnotation(Vector3 start, Vector3 end)
        {
            vertices = new VertexPositionColor[2] {
                new VertexPositionColor(start, GlobalLineColor.GetValueOrDefault(Color.Black)),
                new VertexPositionColor(end, GlobalLineColor.GetValueOrDefault(Color.Black))
            };
        }

        public override void Draw()
        {
            vertices[0].Color = GlobalLineColor.GetValueOrDefault(Color.Black);
            vertices[1].Color = GlobalLineColor.GetValueOrDefault(Color.Black);
            foreach (EffectPass pass in BasicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                BasicEffect.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>
                    (PrimitiveType.LineList, vertices, 0, 1);
            }
        }

    }


    class TranslationAnnotation : AttributeChangeAnnotation
    {
        public TranslationAnnotation(Vector3 translation, List<Annotation> children)
            : base(children)
        {
            this.Translation = translation;
            int j = 0;
            //if (AllAnnotations.Count > 0)
            //{
                //children.Remove(children.First<Annotation>());
                //AllAnnotations.Add(this);
            //}
            foreach (Annotation child in children)
            {
                if (AllAnnotations.Remove(child))
                    j++;
            }
            if (children.Count > j)
            {
                for (int i = 0; i < children.Count - j; i++)
                    //AllAttributeAnnotations.RemoveAt(AllAttributeAnnotations.Count - i - 1);
                    AllAttributeAnnotations.RemoveAt(i);
            }
            
        }

        public TranslationAnnotation(Vector3 translation, params Annotation[] children)
            : this(translation, new List<Annotation>(children))
        { }

        public Vector3 Translation { get; set; }

        public override void Draw()
        {
            Matrix saved = BasicEffect.World;
            BasicEffect.World = Matrix.CreateTranslation(Translation) * BasicEffect.World;
            DrawChildren();
            BasicEffect.World = saved;
        }
    }

    class RotationAnnotation : AttributeChangeAnnotation
    {
        public float Angle;
        public Vector3 Axis;

        public RotationAnnotation(Vector3 myaxis, float myangle, List<Annotation> children)
            : base(children)
        {
            this.Axis = myaxis;
            this.Angle = myangle;
            int j = 0;
            foreach (Annotation child in children)
            {
                if (AllAnnotations.Remove(child))
                    j++;
            }
            if (children.Count > j)
            {
                for (int i = 0; i < children.Count - j; i++)
                    //AllAttributeAnnotations.RemoveAt(AllAttributeAnnotations.Count - i - 1);
                    AllAttributeAnnotations.RemoveAt(i);
            }
        }

        public RotationAnnotation(Vector3 myaxis, float myangle, params Annotation[] children)
            : this(myaxis, myangle, new List<Annotation>(children))
        { }

        public override void Draw()
        {
            Matrix saved = BasicEffect.World;
            BasicEffect.World = Matrix.CreateFromAxisAngle(Axis, Angle) * BasicEffect.World;
            DrawChildren();
            BasicEffect.World = saved;
        }
    }

    class ScaleAnnotation : AttributeChangeAnnotation
    {
        public float Scale;

        public ScaleAnnotation(float scale, List<Annotation> children)
            : base(children)
        {
            this.Scale = scale;
            int j = 0;
            foreach (Annotation child in children)
                {
                    if(AllAnnotations.Remove(child))
                        j++;
                }
                if (children.Count > j)
                {
                    for (int i = 0; i < children.Count-j; i++)
                        //AllAttributeAnnotations.RemoveAt(AllAttributeAnnotations.Count-i-1);
                        AllAttributeAnnotations.RemoveAt(i);
                }
        }

        public ScaleAnnotation(float scale, params Annotation[] children)
            : this(scale, new List<Annotation>(children))
        { }

        public override void Draw()
        {
            Matrix saved = BasicEffect.World;
            BasicEffect.World = Matrix.CreateScale(Scale) * BasicEffect.World;
            DrawChildren();
            BasicEffect.World = saved;
        }



    }

    class ColorAnnotation : AttributeChangeAnnotation
    {
        public Color LineColor;
        public Color FillColor;

        public ColorAnnotation(Color lineColor, Color fillColor, List<Annotation> children)
            : base(children)
        {
                LineColor = lineColor;
                FillColor = fillColor;
                int j=0;
                foreach (Annotation child in children)
                {
                    if (AllAnnotations.Remove(child))
                        j++;
                }
                if (children.Count > j)
                {
                    for (int i = 0; i < children.Count - j; i++)
                        //AllAttributeAnnotations.RemoveAt(AllAttributeAnnotations.Count - i - 1);
                        AllAttributeAnnotations.RemoveAt(i);
                }
        }

        public ColorAnnotation(Color lineColor, Color fillColor, params Annotation[] children)
            : this(lineColor, fillColor, new List<Annotation>(children))
        {
        }

        public override void Draw()
        {
 	        //Matrix saved = BasicEffect.World;
            Color? oldLineColor = GlobalLineColor;
            Color? oldFillColor = GlobalFillColor;
            GlobalLineColor = LineColor;
            GlobalFillColor = FillColor;
            DrawChildren();
            GlobalLineColor = oldLineColor;
            GlobalFillColor = oldFillColor;
            //BasicEffect.World = saved;
        }
    }

    //class FillColorAnnotation : AttributeChangeAnnotation
    //{
    //    public Color FillColor;
    //    PrimitiveType.
    //}

    class CircleAnnotation : Annotation
    {
        public override bool IsChange { get { return false; } }
        VertexPositionColor[] vertices { get; set; }
        VertexPositionColor[] lineverts { get; set; }
        Vector3 Position;
        float Radius;
        Vector3 Normal;

        // new input for painting color.
        //public Color FillColor;
        //public Color LineColor;

    public CircleAnnotation(float radius, Vector3 startPosition, Vector3 normal)
        {
            Vector3 b1 = Vector3.Cross(normal, Vector3.UnitY);
            float length = b1.Length();
            if (length < 0.05)
            {
                b1 = Vector3.Cross(normal, Vector3.UnitX);
                b1.Normalize();
            }
            else
                b1 = b1 * (1f / length);
            Vector3 b2 = Vector3.Cross(b1, normal);

            vertices = new VertexPositionColor[153];
            
            //new code to draw triangle strips
            vertices[0].Position = startPosition;

           
                vertices[0].Color = GlobalFillColor.GetValueOrDefault(Color.Black);
            

            for (int i = 1; i < vertices.Length - 1; i++)
            {
                if (i % 3 == 0)
                {
                    vertices[i].Position = startPosition;
                    
                    vertices[i].Color = GlobalFillColor.GetValueOrDefault(Color.Black);
                   
                }
                else if ((i >= 3) && (vertices[i - 1].Position == startPosition))
                {
                    vertices[i].Position = vertices[i-2].Position;
                   
                    
                    //vertices[i].Color = GlobalFillColor.GetValueOrDefault(Color.Transparent);
                   
                }
                else
                {
                    float angle = (float)(((float)i / (vertices.Length - 1)) * Math.PI * 2);
                    vertices[i].Position = (b1 * (float)Math.Cos(angle) * radius) + (b2 * (float)Math.Sin(angle) * radius) + startPosition;
                    
                    //vertices[i].Color = GlobalFillColor.GetValueOrDefault(Color.Transparent);
                    
                }
            }
            vertices[vertices.Length - 1] = vertices[1];
            vertices[vertices.Length - 2] = vertices[149];
            vertices[vertices.Length - 3] = vertices[0];


            lineverts = new VertexPositionColor[50];
            for (int i = 0; i < lineverts.Length - 1; i++)
            {
                float angle = (float)(((float)i / (lineverts.Length - 1)) * Math.PI * 2);
                lineverts[i].Position = (b1 * (float)Math.Cos(angle) * radius) + (b2 * (float)Math.Sin(angle) * radius) + startPosition;
                
                //lineverts[i].Color = GlobalLineColor.GetValueOrDefault(Color.Transparent);
            }
            lineverts[lineverts.Length - 1] = lineverts[0];
        }


    public CircleAnnotation(float radius, Vector3 startPosition, Vector3 normal, Color lineColor)
    {
        GlobalLineColor = lineColor;
            Vector3 b1 = Vector3.Cross(normal, Vector3.UnitY);
            float length = b1.Length();
            if (length < 0.05)
            {
                b1 = Vector3.Cross(normal, Vector3.UnitX);
                b1.Normalize();
            }
            else
                b1 = b1 * (1f / length);
            Vector3 b2 = Vector3.Cross(b1, normal);

            vertices = new VertexPositionColor[153];

            //new code to draw triangle strips
            vertices[0].Position = startPosition;
            //vertices[0].Color = fillColor;

            for (int i = 1; i < vertices.Length - 1; i++)
            {
                if (i % 3 == 0)
                {
                    vertices[i].Position = startPosition;
                    //vertices[i].Color = fillColor;

                }
                else if ((i >= 3) && (vertices[i - 1].Position == startPosition))
                {
                    vertices[i].Position = vertices[i - 2].Position;
                    //vertices[i].Color = fillColor;
                }
                else
                {
                    float angle = (float)(((float)i / (vertices.Length - 1)) * Math.PI * 2);
                    vertices[i].Position = (b1 * (float)Math.Cos(angle) * radius) + (b2 * (float)Math.Sin(angle) * radius) + startPosition;
                    //vertices[i].Color = fillColor;
                }
            }
            vertices[vertices.Length - 1] = vertices[1];
            vertices[vertices.Length - 2] = vertices[149];
            vertices[vertices.Length - 3] = vertices[0];

            lineverts = new VertexPositionColor[50];
            for (int i = 0; i < lineverts.Length - 1; i++)
            {
                float angle = (float)(((float)i / (lineverts.Length - 1)) * Math.PI * 2);
                lineverts[i].Position = (b1 * (float)Math.Cos(angle) * radius) + (b2 * (float)Math.Sin(angle) * radius) + startPosition;
                lineverts[i].Color = lineColor;
            }
            lineverts[lineverts.Length - 1] = lineverts[0];
        
    }
    
        public override void Draw()
    {
        for (int i = 0; i < lineverts.Length; i++)
        {
            lineverts[i].Color = GlobalLineColor.GetValueOrDefault(Color.Black);
        }
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].Color = GlobalFillColor.GetValueOrDefault(Color.Transparent);
        }
            foreach (EffectPass pass in BasicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                BasicEffect.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, vertices, 0, 51, VertexPositionColor.VertexDeclaration);
                BasicEffect.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, lineverts, 0, lineverts.Length - 1);
            }
        }
    }

    

    class TextAnnotation : Annotation
    {
        public override bool IsChange { get { return false; } }
        public Vector2 Position;
        public String Words;
        public Color TextColor;
        //SpriteBatch spriteBatch;

        public TextAnnotation(String words, Vector2 position, ContentManager content)
        {
            Position = position;
            Words = words;
            TextColor = GlobalLineColor.GetValueOrDefault(Color.Transparent);
            Font = content.Load<SpriteFont>("textAnnotation");
        }

        public TextAnnotation(String words, Vector2 position)
        {
            Position = position;
            Words = words;
            //TextColor = GlobalLineColor.GetValueOrDefault(Color.Transparent);
            //Font = content.Load<SpriteFont>("textAnnotation");
        }

        public override void Draw()
        {
            
            foreach (EffectPass pass in BasicEffect.CurrentTechnique.Passes)
            {
                TextColor = GlobalLineColor.GetValueOrDefault(Color.Black);
                //new TextAnnotation(Words, Position);
                pass.Apply();
                
                spriteBatch = new SpriteBatch(BasicEffect.GraphicsDevice);
                spriteBatch.Begin();
                spriteBatch.DrawString(Font, Words, Position, TextColor);
                spriteBatch.End();
            }
        }
    }


}
