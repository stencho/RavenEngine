using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raven.Graphics.Skybox {
    public class SkyBoxTesselator {
        /// <summary>
        /// Creates a skybox with the scale and the facing order in the skybox image 
        /// Front, Back, Right, Left, Top, Bottom 
        /// is defined as ... 
        /// 0 1 2  from the upper part of the image across
        /// 3 4 5  from the lower part of the image across
        /// You may change the order to handle different skyboxes orders
        /// </summary>
        public void PrivateCreateSkyboxFromCrossImage(out VertexPositionNormalColorUv[] vertices, out int[] indices, float scale, int Front, int Back, int Right, int Left, int Top, int Bottom) {
            VertexPositionNormalColorUv[] vertices_sborss = new VertexPositionNormalColorUv[24];
            int[] indices_sborss = new int[36];

            var front = 4 * Front;
            var back = 4 * Back;
            var right = 4 * Right;
            var left = 4 * Left;
            var top = 4 * Top;
            var bottom = 4 * Bottom;

            bool flipuUv_V = true;

            Vector3 LT_f = new Vector3(-1f, -1f, -1f) * scale;
            Vector3 LB_f = new Vector3(-1f, 1f, -1f) * scale;
            Vector3 RT_f = new Vector3(1f, -1f, -1f) * scale;
            Vector3 RB_f = new Vector3(1f, 1f, -1f) * scale;

            Vector3 LT_b = new Vector3(-1f, -1f, 1f) * scale;
            Vector3 LB_b = new Vector3(-1f, 1f, 1f) * scale;
            Vector3 RT_b = new Vector3(1f, -1f, 1f) * scale;
            Vector3 RB_b = new Vector3(1f, 1f, 1f) * scale;

            Vector3[] p = new Vector3[]
            {
                LT_f, LB_f, RT_f, RB_f,
                LT_b, LB_b, RT_b, RB_b
            };

            int i = 0;
            // front;
            vertices_sborss[i].Position = p[0]; i++; //left
            vertices_sborss[i].Position = p[1]; i++;
            vertices_sborss[i].Position = p[2]; i++; //right
            vertices_sborss[i].Position = p[3]; i++;
            // back is mirrored front
            vertices_sborss[i].Position = p[6]; i++; // mirrored
            vertices_sborss[i].Position = p[7]; i++;
            vertices_sborss[i].Position = p[4]; i++; // mirrored
            vertices_sborss[i].Position = p[5]; i++;
            // right
            vertices_sborss[i].Position = p[2]; i++;
            vertices_sborss[i].Position = p[3]; i++;
            vertices_sborss[i].Position = p[6]; i++;
            vertices_sborss[i].Position = p[7]; i++;
            // left
            vertices_sborss[i].Position = p[4]; i++;
            vertices_sborss[i].Position = p[5]; i++;
            vertices_sborss[i].Position = p[0]; i++;
            vertices_sborss[i].Position = p[1]; i++;
            // top
            vertices_sborss[i].Position = p[4]; i++;
            vertices_sborss[i].Position = p[0]; i++;
            vertices_sborss[i].Position = p[6]; i++;
            vertices_sborss[i].Position = p[2]; i++;
            // bottom mirrored
            vertices_sborss[i].Position = p[1]; i++;
            vertices_sborss[i].Position = p[5]; i++;
            vertices_sborss[i].Position = p[3]; i++;
            vertices_sborss[i].Position = p[7]; i++;

            Vector2 tupeBuvwh = new Vector2(.25000000f, .33333333f); // i might just delete this one
            Vector2 currentuvWH = tupeBuvwh;
            Vector2 uvStart = Vector2.Zero;
            Vector2 uvEnd = Vector2.Zero;

            for (int j = 0; j < 6; j++) {
                // face represents which set of 4 vertices we assign a set of texture coordinates to
                // we can move them by simply moving face number
                int face = 0;
                if (j == 0) {
                    face = front; //front
                    uvStart = new Vector2(currentuvWH.X * 1f, currentuvWH.Y * 1f);
                    uvEnd = uvStart + currentuvWH;
                }
                if (j == 1) {
                    face = back; // back
                    uvStart = new Vector2(currentuvWH.X * 3f, currentuvWH.Y * 1f);
                    uvEnd = uvStart + currentuvWH;
                }
                if (j == 2) {
                    face = right; // right
                    uvStart = new Vector2(currentuvWH.X * 2f, currentuvWH.Y * 1f);
                    uvEnd = uvStart + currentuvWH;
                }
                if (j == 3) {
                    face = left; // left
                    uvStart = new Vector2(currentuvWH.X * 0f, currentuvWH.Y * 1f);
                    uvEnd = uvStart + currentuvWH;
                }
                if (j == 4) {
                    face = top; // top
                    uvStart = new Vector2(currentuvWH.X * 1f, currentuvWH.Y * 0f);
                    uvEnd = uvStart + currentuvWH;
                }
                if (j == 5) {
                    face = bottom; // bottom
                    uvStart = new Vector2(currentuvWH.X * 1f, currentuvWH.Y * 2f);
                    uvEnd = uvStart + currentuvWH;
                }
                if (flipuUv_V) { float y = uvStart.Y; uvStart.Y = uvEnd.Y; uvEnd.Y = y; }
                vertices_sborss[face + 0].TextureCoordinateA = new Vector2(uvStart.X, uvStart.Y);
                vertices_sborss[face + 1].TextureCoordinateA = new Vector2(uvStart.X, uvEnd.Y);
                vertices_sborss[face + 2].TextureCoordinateA = new Vector2(uvEnd.X, uvStart.Y);
                vertices_sborss[face + 3].TextureCoordinateA = new Vector2(uvEnd.X, uvEnd.Y);
            }

            Vector3 center = new Vector3(0, 0, 0);
            for (int j = 0; j < 24; j++) {
                vertices_sborss[j].Normal = Vector3.Normalize(vertices_sborss[j].Position - center);
                vertices_sborss[j].Color = Color.White;
            }

            //
            i = 0;
            int k = 0; // front
            indices_sborss[i] = k + 0; i++;
            indices_sborss[i] = k + 1; i++;
            indices_sborss[i] = k + 2; i++;
            indices_sborss[i] = k + 2; i++;
            indices_sborss[i] = k + 1; i++;
            indices_sborss[i] = k + 3; i++;

            k = 4; // back
            indices_sborss[i] = k + 0; i++;
            indices_sborss[i] = k + 1; i++;
            indices_sborss[i] = k + 2; i++;
            indices_sborss[i] = k + 2; i++;
            indices_sborss[i] = k + 1; i++;
            indices_sborss[i] = k + 3; i++;

            k = 8; // right
            indices_sborss[i] = k + 0; i++;
            indices_sborss[i] = k + 1; i++;
            indices_sborss[i] = k + 2; i++;
            indices_sborss[i] = k + 2; i++;
            indices_sborss[i] = k + 1; i++;
            indices_sborss[i] = k + 3; i++;

            k = 12; // left
            indices_sborss[i] = k + 0; i++;
            indices_sborss[i] = k + 1; i++;
            indices_sborss[i] = k + 2; i++;
            indices_sborss[i] = k + 2; i++;
            indices_sborss[i] = k + 1; i++;
            indices_sborss[i] = k + 3; i++;

            k = 16; // top
            indices_sborss[i] = k + 0; i++;
            indices_sborss[i] = k + 1; i++;
            indices_sborss[i] = k + 2; i++;
            indices_sborss[i] = k + 2; i++;
            indices_sborss[i] = k + 1; i++;
            indices_sborss[i] = k + 3; i++;

            k = 20; // bottom
            indices_sborss[i] = k + 0; i++;
            indices_sborss[i] = k + 1; i++;
            indices_sborss[i] = k + 2; i++;
            indices_sborss[i] = k + 2; i++;
            indices_sborss[i] = k + 1; i++;
            indices_sborss[i] = k + 3; i++;

            vertices = vertices_sborss;
            indices = indices_sborss;
        }
        // Tesselate a skycube into a sphere based on distance and normals. 
        // All created points should be based on the distance * the normal.
        //    
        //    A skybox it turns out on the first prototype has some special considerations.
        //    This forced me to scrap my simpler design due to each face having its own disjointed uv areas.
        //    In so doing i decided to expound on the quads treat them as meshes and expand them in place.
        //    This is primarily done by simple remapping however that required quite a bit of thought.
        //
        //    We will tesselate a face by simply expanding it.
        //    Well start off by precalculating all the variables we might need.
        //
        //    The first major step is we will remap edges to edges.
        //
        //    //
        //    // such that this
        //    //
        //    //    0          2 
        //    //   LT ------ RT
        //    //   |          |  
        //    //   |1         |3 
        //    //   LB ------ RB
        //
        //    //
        //    // is mapped to the below when it is tesselated 1 time.
        //    //
        //    //    0          3          6
        //    //   LT ----- LTcRT ----- RT
        //    //   |          |          |
        //    //   |1         |4         |7
        //    //   LTcLB -- cent ------ RBcRT
        //    //   |          |          |
        //    //   |2         |5         |8
        //    //   LB ----- LBcRB ----- RB
        //    //
        //
        //    If we do it this way we simply expand on each face and remmap it. 
        //    As if each quad face is a mesh. 
        //    Since we are using quads we can subdivide them in this manner.
        //
        //    Recalculating the mesh... this wont be actual local (vertice) tesselation.
        //    It's more like remapping a a mesh... i would of liked to but it would be impractical in this case
        //    Though i believe the proper term is (quad subdivision) or quad tesselation.
        //    
        //    For a skybox with such a six side image... 
        //    To map that to a sphere we must keep seperate faces seperate.
        //    This way is pretty much a must in this case. 
        //    We however wish for unobvious reasons to keep the entire skybox vertices and indice sets each in a single array of their own.
        //    In the same two arrays and we can treat it as a single vertice set.
        //

        public void Subdivide(VertexPositionNormalColorUv[] source_vertices, int[] source_indices, out VertexPositionNormalColorUv[] dest_vertices, out int[] dest_indices, int numberOfSubdivisions, float distance) {
            Subdivide(source_vertices, source_indices, Vector3.Zero, distance, numberOfSubdivisions, out dest_vertices, out dest_indices);
        }

        // provided this is based on a skybox it can be subdivided again
        public void Subdivide(VertexPositionNormalColorUv[] source_vertices, int[] source_indices, Vector3 origin, float distance, int numberOfSubdivisions, out VertexPositionNormalColorUv[] dest_vertices, out int[] dest_indices) {
            VertexPositionNormalColorUv[] sv = source_vertices;
            int[] si = source_indices;

            int Faces = 6;

            int s_quads = si.Length / 6;
            int s_quadsPerFace = s_quads / 6;
            int s_indicesPerFace = s_quadsPerFace * 6;
            int s_totalIndices = s_indicesPerFace * Faces;
            int s_totalVertices = sv.Length;
            int s_verticesPerFace = sv.Length / Faces;
            int s_verticesPerDirection = (int)Math.Sqrt((double)s_verticesPerFace);
            int sw = s_verticesPerDirection;
            int sh = s_verticesPerDirection;

            // well be moving the edges outwards in this case as we recalculate vertices
            // inside this just means im retesselating a mesh not really tesselating vertices
            // though ultimately it gives the same result 
            // vertices are normally simpler but here we have a non contigous texture with 6 parts
            // each part has its own uv set thus its own mesh that we must tesselate individually
            // as the uv coordinates cannot interpolate across texels directly in the non contiguous skybox image
            int d_verticesPerDirection = s_verticesPerDirection + numberOfSubdivisions;
            int d_verticesPerFace = d_verticesPerDirection * d_verticesPerDirection;
            int d_totalVertices = d_verticesPerFace * Faces;
            int d_quadsPerFace = (d_verticesPerDirection - 1) * (d_verticesPerDirection - 1);
            int d_quads = d_quadsPerFace * 6; // coincidwntally 6 faces 
            int d_indicesPerFace = d_quadsPerFace * 6; // 6 indices per quad
            int d_totalIndices = d_indicesPerFace * Faces;

            VertexPositionNormalColorUv[] dv = new VertexPositionNormalColorUv[d_totalVertices];
            int[] di = new int[d_totalIndices];
            int dw = d_verticesPerDirection;
            int dh = d_verticesPerDirection;


            // Maping to the face surface or quad edge vertices
            //
            // because quads are vertically oriented this means our loops will be x y not y x ordered
            // so later on ill have to either change the order of the cube or keep in mind
            // that this particular sphere mesh is x y ordered
            // i could however make seperate function to swap order i probably should have already
            // but not in this class as input is expected in the x y order a swaped order would 
            // screw this all up and so whatever order our quad draw is in this function must follow
            // in order to properly tesselate the faces
            // y + x * height; were height is verticesPerDirection
            int s_lt_Edge = 0;
            int s_lb_Edge = s_verticesPerDirection - 1;
            int s_rt_Edge = (s_verticesPerDirection - 1) * s_verticesPerDirection;
            int s_rb_Edge = s_verticesPerFace - 1;

            int d_lt_Edge = 0;
            int d_lb_Edge = d_verticesPerDirection - 1;
            int d_rt_Edge = (d_verticesPerDirection - 1) * d_verticesPerDirection;
            int d_rb_Edge = d_verticesPerFace - 1;

            // now here we have the decision to make do we loop the source or the destination
            // i have been to this point only considered expanding the mesh;
            // however later and for other things i may want to deflate it
            // in either case looping destination or source we must calculate the opposite.
            // Its the sides however that matters

            for (int f = 0; f < Faces; f++) {
                // the index that denotes the top left of the entire face
                int s_startVertice = s_verticesPerFace * f;
                int s_startIndice = s_indicesPerFace * f;
                int d_startVertice = d_verticesPerFace * f;
                int d_startIndice = d_indicesPerFace * f;

                int s = s_startVertice;
                int d = d_startVertice;

                // calculate were the corners are on the source face and destination face
                s_lt_Edge = s + 0;
                s_lb_Edge = s + s_verticesPerDirection - 1;
                s_rt_Edge = s + (s_verticesPerDirection - 1) * s_verticesPerDirection;
                s_rb_Edge = s + s_verticesPerFace - 1;

                d_lt_Edge = d + 0;
                d_lb_Edge = d + d_verticesPerDirection - 1;
                d_rt_Edge = d + (d_verticesPerDirection - 1) * d_verticesPerDirection;
                d_rb_Edge = d + d_verticesPerFace - 1;

                // normalize and redistance the source corner positions before setting them
                sv[s_lt_Edge].Position = Vector3.Normalize(sv[s_lt_Edge].Position) * distance;
                sv[s_lb_Edge].Position = Vector3.Normalize(sv[s_lb_Edge].Position) * distance;
                sv[s_rt_Edge].Position = Vector3.Normalize(sv[s_rt_Edge].Position) * distance;
                sv[s_rb_Edge].Position = Vector3.Normalize(sv[s_rb_Edge].Position) * distance;

                // sets the corners destination vertices to recalculate the entire quad mesh
                dv[d_lt_Edge] = sv[s_lt_Edge];
                dv[d_lb_Edge] = sv[s_lb_Edge];

                dv[d_rt_Edge] = sv[s_rt_Edge];
                dv[d_rb_Edge] = sv[s_rb_Edge];

                // the left lines empty vertices are lerped down, thru the face edges between top bottom known endpoints
                for (int j = 1; j < d_verticesPerDirection - 1; j++) {
                    int dindex = d_lt_Edge + j;
                    float lerptime = (float)(j) / (float)(d_verticesPerDirection - 1); // - d_lt_Edge
                    dv[dindex] = LerpSubDivisionVeritces(dv[d_lt_Edge], dv[d_lb_Edge], lerptime, distance, origin);
                }
                // the right lines empty vertices are lerped down, thru the face edges between top bottom known endpoints
                for (int j = 1; j < d_verticesPerDirection - 1; j++) {
                    int dindex = d_rt_Edge + j;
                    float lerptime = (float)(j) / (float)(d_verticesPerDirection - 1);//j / (d_verticesPerDirection - d_rt_Edge);
                    dv[dindex] = LerpSubDivisionVeritces(dv[d_rt_Edge], dv[d_rb_Edge], lerptime, distance, origin);
                }
                // we fill across
                for (int down = 0; down < d_verticesPerDirection; down++) {
                    int fromLeft = d_lt_Edge + down;
                    int toRight = d_rt_Edge + down;

                    for (int across = 1; across < d_verticesPerDirection - 1; across++) {
                        int creationindex = fromLeft + across * d_verticesPerDirection;
                        float lerptime = (float)(across) / (float)(d_verticesPerDirection - 1);
                        dv[creationindex] = LerpSubDivisionVeritces(dv[fromLeft], dv[toRight], lerptime, distance, origin);
                    }
                }
            }


            // We have are vertices setup now we must set the indices to them as triangles
            int dIndice_Index = 0;
            for (int f = 0; f < Faces; f++) {
                int d_startFaceOffset = d_verticesPerFace * f;
                // we move across
                for (int across = 0; across < d_verticesPerDirection - 1; across++) {
                    // we move down
                    for (int down = 0; down < d_verticesPerDirection - 1; down++) {

                        // we now must find the 4 vertices of each quad in this mesh
                        // our loop procedes in the order of down then left to right
                        // basically this
                        // int index0 = x * stride + y + startOffset;
                        //
                        int vindex0 = ((across + 0) * d_verticesPerDirection) + (down + 0) + d_startFaceOffset;
                        int vindex1 = ((across + 0) * d_verticesPerDirection) + (down + 1) + d_startFaceOffset;
                        int vindex2 = ((across + 1) * d_verticesPerDirection) + (down + 0) + d_startFaceOffset;
                        int vindex3 = ((across + 1) * d_verticesPerDirection) + (down + 1) + d_startFaceOffset;

                        // set the indices
                        di[dIndice_Index] = vindex0; dIndice_Index++;
                        di[dIndice_Index] = vindex1; dIndice_Index++;
                        di[dIndice_Index] = vindex2; dIndice_Index++;

                        di[dIndice_Index] = vindex2; dIndice_Index++;
                        di[dIndice_Index] = vindex1; dIndice_Index++;
                        di[dIndice_Index] = vindex3; dIndice_Index++;
                    }
                }
            }

            // well repeat this process when generating a new face surface
            // to set the originals to the edges.
            dest_vertices = dv;
            dest_indices = di;
        }

        /// <summary>
        /// This method lerps between two custom vertices by the specified percentage. 
        /// The position is found by the normal * the distance from the origin specified.
        /// </summary>
        public VertexPositionNormalColorUv LerpSubDivisionVeritces
            (
            VertexPositionNormalColorUv from,
            VertexPositionNormalColorUv to,
            float lerpPercentage,
            float distance,
            Vector3 origin
            ) {
            VertexPositionNormalColorUv B = new VertexPositionNormalColorUv();
            B.Color = new Color(
                (byte)(from.Color.R + (to.Color.R - from.Color.R) * lerpPercentage),
                (byte)(from.Color.G + (to.Color.G - from.Color.G) * lerpPercentage),
                (byte)(from.Color.B + (to.Color.B - from.Color.B) * lerpPercentage),
                (byte)(from.Color.A + (to.Color.A - from.Color.A) * lerpPercentage)
                );
            Vector3 d = (from.Normal + ((to.Normal - from.Normal) * lerpPercentage));
            B.Normal = Vector3.Normalize(d);
            B.Position = origin + B.Normal * distance;
            B.TextureCoordinateA = from.TextureCoordinateA + ((to.TextureCoordinateA - from.TextureCoordinateA) * lerpPercentage);
            return B;
        }


    }

    // Ill just be placing these as inner classes for now 
    // So i can proof test this fast and not mess up my working project
    // i still have more to do on it.
public struct VertexPositionNormalColorUv : IVertexType {
    public Vector3 Position;
    public Color Color;
    public Vector3 Normal;
    public Vector2 TextureCoordinateA;

    public static VertexDeclaration VertexDeclaration = new VertexDeclaration
    (
          new VertexElement(VertexElementByteOffset.PositionStartOffset(), VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
          new VertexElement(VertexElementByteOffset.OffsetColor(), VertexElementFormat.Color, VertexElementUsage.Color, 0),
          new VertexElement(VertexElementByteOffset.OffsetVector3(), VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
          new VertexElement(VertexElementByteOffset.OffsetVector2(), VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
    );
    VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
}
/// <summary>
/// This is actually a helper struct
/// </summary>
public struct VertexElementByteOffset {
    public static int currentByteSize = 0;
    public static int PositionStartOffset() { currentByteSize = 0; var s = sizeof(float) * 3; currentByteSize += s; return currentByteSize - s; }
    public static int Offset(float n) { var s = sizeof(float); currentByteSize += s; return currentByteSize - s; }
    public static int Offset(Vector2 n) { var s = sizeof(float) * 2; currentByteSize += s; return currentByteSize - s; }
    public static int Offset(Color n) { var s = sizeof(int); currentByteSize += s; return currentByteSize - s; }
    public static int Offset(Vector3 n) { var s = sizeof(float) * 3; currentByteSize += s; return currentByteSize - s; }
    public static int Offset(Vector4 n) { var s = sizeof(float) * 4; currentByteSize += s; return currentByteSize - s; }

    public static int OffsetFloat() { var s = sizeof(float); currentByteSize += s; return currentByteSize - s; }
    public static int OffsetColor() { var s = sizeof(int); currentByteSize += s; return currentByteSize - s; }
    public static int OffsetVector2() { var s = sizeof(float) * 2; currentByteSize += s; return currentByteSize - s; }
    public static int OffsetVector3() { var s = sizeof(float) * 3; currentByteSize += s; return currentByteSize - s; }
    public static int OffsetVector4() { var s = sizeof(float) * 4; currentByteSize += s; return currentByteSize - s; }
    }
}



