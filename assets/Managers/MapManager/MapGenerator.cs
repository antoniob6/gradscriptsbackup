/*
 * this file generates a map randomly
 * it uses the construsts a mesh from scratch accroding to the design in the project 
 * it construsts vertices, triangle and UVs, by using mathmatics
 * 
 */
using System;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class MapGenerator: NetworkBehaviour {
    public GameObject collisionBoxPrefab;
    public delegate void VoidFunctionDelegate();
    public delegate void MapGeneratorFunctionDelegate(MapGenerator MG);
    //public GameManager GM;
    public Vector3 RightEdge;


    


    [SyncVar]public float thickness = 2f;
    [SyncVar]public float length = 50f;
    [SyncVar]public int totalSurfaceVerts = 50; // 2 minimum
    [SyncVar]public int seed=0;
    // public float stepHeight = 2f;
    [SyncVar]public float jumpHeight = 1.5f;
    [SyncVar]public int vertsPerPlatform=4;
    [SyncVar]public float Radius = 4;
    [SyncVar]public Boolean isCircle= true;

    private int resZ = 2;



    public Mesh mainMesh;
    private Vector3[] topSurface;
    private Vector3[] topSurfaceSpread;
    private Vector3 positionSpread;
    private int oldSeed;


    private bool finished = false;
    // Use this for initialization
    //private EdgeCollider2D edgeCollider;
    private PolygonCollider2D edgeCollider;
    void Start () {
        MeshFilter filter = gameObject.GetComponent<MeshFilter>();
        mainMesh = filter.mesh;

        //edgeCollider = gameObject.GetComponent<EdgeCollider2D>();

        oldSeed = seed;
    }

   public  void setRules(Rules rules){
    length =rules.length; 
    seed = rules.seed;
    jumpHeight = rules.jumpHeight;
    Radius = rules.Radius;
    isCircle = rules.isCircle;

    totalSurfaceVerts = (int)length;
    }


    //  [ClientRpc]public void RpcUpdateMap() {
    //     Debug.Log("clients recvied the server update signal");
    //      updateMap();
    //  }

    [SyncVar(hook = "onChange")] public bool createBaseMapVerts = false;
    public void onChange(bool v) {
       // Debug.Log("bool on client");
        createBaseMapVerts = v;
       // updateMap();
    }

    public void updateMap(MapGeneratorFunctionDelegate callback =null) {
        createBaseMapVerts = true;
      //  if (isServer) {
        //    Debug.Log("server is updating the map on the clients");
         //   RpcUpdateMap();
       // }

        if (!mainMesh)
            mainMesh = gameObject.GetComponent<MeshFilter>().mesh;
        createMesh(mainMesh);
        positionSpread = transform.position;
        topSurfaceSpread = new Vector3[totalSurfaceVerts];
        for (int i = 0; i < totalSurfaceVerts; i++) {
            topSurfaceSpread[i] = mainMesh.vertices[i];
        }

        if (isCircle) {
            //turnIntoCircle(mainMesh, mainMesh.vertices[0], Radius);
        }
        makeCollision(mainMesh.vertices);
        mainMesh.RecalculateBounds();
        topSurface = new Vector3[totalSurfaceVerts];
        for (int i = 0; i < totalSurfaceVerts; i++) {
            topSurface[i] = mainMesh.vertices[i];
        }

        if (callback != null) {
            callback(this);
        }

    }




    public void updateMapPlatform(Vector3[] surfaceToAvoidGlobal, MapGeneratorFunctionDelegate callback = null) {
        if (!mainMesh)
            mainMesh = gameObject.GetComponent<MeshFilter>().mesh;
        Vector3[] surfaceToAvoidLocal = tranlateVertsToLocalSpace(surfaceToAvoidGlobal);
        Vector3[] surfaceVerts= createSurfacePlaneThatAvoids(surfaceToAvoidLocal,totalSurfaceVerts);

        
        Mesh mesh=createMeshFromSurfacePlane(surfaceVerts);

        positionSpread = transform.position;
        topSurfaceSpread = new Vector3[totalSurfaceVerts];
        for (int i = 0; i < totalSurfaceVerts; i++) {
            topSurfaceSpread[i] = mesh.vertices[i];
        }

        //if (isCircle)
           // turnIntoCirclePlatform(mesh,  Radius, baseMG);
        makeCollision(mesh.vertices);


        topSurface = new Vector3[totalSurfaceVerts];
        for (int i = 0; i < totalSurfaceVerts; i++) {
            topSurface[i] = mesh.vertices[i];
        }

        if (callback != null) {
            callback(this);
        }
        assignMainMesh(mesh);

    }

    private void assignMainMesh(Mesh mesh) {
        mainMesh.vertices = mesh.vertices;

        mainMesh.uv = mesh.uv;

        mainMesh.triangles = mesh.triangles;
        mainMesh.RecalculateBounds();
    }


    private void turnIntoCirclePlatform(Mesh mesh,  float radius,MapGenerator MG) {
        Vector3[] currVerts = MG.getSurfaceVertsInGlobalSpace();
        //Vector3[] spreadVerts = MG.getSpreadSurfaceVertsInGlobalSpace();
        float anglePercent = transform.position.x / MG.RightEdge.x;
//        Debug.Log("angle percent: " + anglePercent);
//        Debug.Log("position: " + transform.position.x);
        int spawnVertIndex = (int)(anglePercent * (currVerts.Length - 1));
        Vector3 spawnLoc = currVerts[spawnVertIndex];
        transform.position = spawnLoc;

        float rotationAmount = Vector3.SignedAngle( spawnLoc,Vector3.up,Vector3.back);
      //  Debug.Log(rotationAmount);
            transform.Rotate(0, 0, rotationAmount);
/*
        Vector3[] verts = mesh.vertices;

        float startingAngle = -transform.position.x / (baseSurfaceGlobal[baseSurfaceGlobal.Length - 1].x- baseSurfaceGlobal[0].x) * 360f;

        //Debug.Log("starting angle: "+startingAngle);
        //Debug.Log("position: " + transform.position.x);
        Debug.Log("end base x: " + (baseSurfaceGlobal[baseSurfaceGlobal.Length - 1].x - baseSurfaceGlobal[0].x));
        for (int i = 0; i < verts.Length; i++) {
            Vector3 to = Vector3.up * (verts[i].y + radius);
            //Debug.Log(to);
            float angle =  -verts[i].x / (baseSurfaceGlobal[baseSurfaceGlobal.Length - 1].x - baseSurfaceGlobal[0].x) * 360f+startingAngle;
            to = Quaternion.Euler(0, 0, angle) * to;
            verts[i] = to;

           // Debug.Log("angle: " + angle);
        }
        transform.position = Vector3.zero;
        mesh.vertices = verts;
*/
    }


    private Mesh createMeshFromSurfacePlane(Vector3[] surface) {
        Mesh mesh=new Mesh();
        Vector3[] vertices = createMeshVerticesFromSurface(surface);
       // currentSurface = vertices;
        #region uvsAndTriangles
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int v = 0; v < resZ; v++) {
            for (int u = 0; u < totalSurfaceVerts; u++) {
                // uvs[u + v * totalSurfaceVerts] = new Vector2((float)u / (totalSurfaceVerts - 1), (float)v / (resZ - 1));
                //uvs[u + v * totalSurfaceVerts] = new Vector2((float)u * 0.1f, (float)v * 0.1f);
                uvs[u + v * totalSurfaceVerts] = new Vector2(vertices[u + v * totalSurfaceVerts].x * 0.1f, vertices[u + v * totalSurfaceVerts].y * 0.1f);


            }
        }


        int nbFaces = (totalSurfaceVerts - 1) * (resZ - 1);
        int[] triangles = new int[nbFaces * 6];
        int t = 0;
        for (int faceZ = 0; faceZ < resZ - 1; faceZ++) {
            for (int faceX = 0; faceX < totalSurfaceVerts - 1; faceX++) {
                int i = faceX + faceZ * totalSurfaceVerts;

                triangles[t++] = i + totalSurfaceVerts;
                triangles[t++] = i;
                triangles[t++] = i + 1;


                triangles[t++] = i + totalSurfaceVerts;
                triangles[t++] = i + 1;
                triangles[t++] = i + totalSurfaceVerts + 1;

            }
        }
        #endregion
        mesh.vertices = vertices;

        mesh.uv = uvs;

        mesh.triangles = triangles;
        return mesh;


    }

    public void createMapFromSurfacePlane(Vector3[] surface) {
        Mesh m= createMeshFromSurfacePlane(surface);
        makeCollision(m.vertices);
        assignMainMesh(m);

    }

    private Vector3[] createMeshVerticesFromSurface(Vector3[] surface) {
        float angle = 180;
        Vector3[] vertices = new Vector3[surface.Length * 2];
        for(int i = 0; i < surface.Length; i++) {
            vertices[i] = surface[i];
        }
        
        for (int x = 2; x < surface.Length; x++) {//generate bottom surface verticies


            Vector2 to = new Vector2(vertices[x].x - vertices[x - 1].x, vertices[x].y - vertices[x - 1].y);
            Vector2 from = new Vector2(vertices[x - 2].x - vertices[x - 1].x, vertices[x - 2].y - vertices[x - 1].y);
            angle = Vector2.SignedAngle(to, from);
            float desiredAngle;
            if (angle >= 0)
                desiredAngle = angle / 2;
            else
                desiredAngle = angle / 2 + 180;


            Vector2 bottomVertLoc = new Vector2(to.x, to.y);
            bottomVertLoc.Normalize();
            bottomVertLoc = bottomVertLoc * thickness;
            bottomVertLoc = Quaternion.Euler(0, 0, desiredAngle) * bottomVertLoc;//rotate vector

            if (x == 2) {
                vertices[surface.Length] = surface[0] - (Vector3)bottomVertLoc;
            }
            if (x == surface.Length - 1) {
                vertices[surface.Length * 2 - 1] = surface[surface.Length - 1] - (Vector3)bottomVertLoc;
            }

            bottomVertLoc = (Vector2)vertices[x - 1] - bottomVertLoc;
            vertices[(x - 1) + surface.Length] = bottomVertLoc;



        }
       
     //   Debug.Log(vertices[surface.Length * 2 - 1]);
        return vertices;
    }


    Vector3[] baseVertsGizmos;
    public void createMesh(Mesh mesh)
    {
        mesh.Clear();
        Vector3[] vertices = new Vector3[totalSurfaceVerts * resZ];

        generateSurfaceVerts(vertices,jumpHeight,vertsPerPlatform,seed, jumpHeight);
        baseVertsGizmos = vertices;
        vertices=createAllVerticesFromSurface(vertices);

        Vector2[] uvs = new Vector2[vertices.Length];
        for (int v = 0; v < resZ; v++){
            for (int u = 0; u < totalSurfaceVerts; u++) {
                //uvs[u + v * totalSurfaceVerts] = new Vector2((float)u / (totalSurfaceVerts - 1), (float)v / (resZ - 1));
                // uvs[u + v * totalSurfaceVerts] = new Vector2(u * 0.1f, v * 0.1f);
                uvs[u + v * totalSurfaceVerts] = new Vector2(vertices[u + v * totalSurfaceVerts].x * 0.1f, vertices[u + v * totalSurfaceVerts].y * 0.1f);
            }
        }


        int nbFaces = (totalSurfaceVerts - 1) * (resZ - 1);
        int[] triangles = new int[nbFaces * 6];
        int t = 0;
        for (int faceZ =0; faceZ < resZ-1; faceZ++){
            for (int faceX = 0; faceX < totalSurfaceVerts- 1; faceX++)
            {
                int i = faceX + faceZ * totalSurfaceVerts;
                
                triangles[t++] = i + totalSurfaceVerts;
                triangles[t++] = i;
                triangles[t++] = i + 1;


                triangles[t++] = i + totalSurfaceVerts;
                triangles[t++] = i + 1;
                triangles[t++] = i + totalSurfaceVerts + 1;
 
            }
        }

        mesh.vertices = vertices;

        mesh.uv = uvs;

        mesh.triangles = triangles;

        

    }

    private Vector3[] createAllVerticesFromSurface(Vector3[] surfaceVertices) {
        float angle = 180;
        for (int x = 2; x < totalSurfaceVerts; x++) {//generate bottom surface verticies


            Vector2 to = new Vector2(surfaceVertices[x].x - surfaceVertices[x - 1].x, surfaceVertices[x].y - surfaceVertices[x - 1].y);
            Vector2 from = new Vector2(surfaceVertices[x - 2].x - surfaceVertices[x - 1].x, surfaceVertices[x - 2].y - surfaceVertices[x - 1].y);
            angle = Vector2.SignedAngle(to, from);
            if (Math.Abs(angle) <= 0.1) {
                Debug.Log("divide by zero");
                return surfaceVertices;
            }
            float desiredAngle;
            if (angle >= 0)
                desiredAngle = angle / 2;
            else
                desiredAngle = angle / 2 + 180;


            Vector2 bottomVertLoc = new Vector2(to.x, to.y);
            bottomVertLoc.Normalize();
            bottomVertLoc = bottomVertLoc * thickness;
            bottomVertLoc = Quaternion.Euler(0, 0, desiredAngle) * bottomVertLoc;//rotate vector

            if (x == 2) {
                surfaceVertices[totalSurfaceVerts] = surfaceVertices[0] - (Vector3)bottomVertLoc;
            }
            if (x == totalSurfaceVerts - 1) {
                surfaceVertices[totalSurfaceVerts * 2 - 1] =
                    surfaceVertices[totalSurfaceVerts - 1] - (Vector3)bottomVertLoc;

                surfaceVertices[totalSurfaceVerts * 2 - 2] =
                    surfaceVertices[totalSurfaceVerts - 1] - (Vector3)bottomVertLoc;
                //Debug.Log("last vert: " + surfaceVertices[totalSurfaceVerts * 2 - 1]);
                //Debug.Log("index: " + (totalSurfaceVerts * 2 - 1) + " total: " + surfaceVertices.Length);
            }
            //Vector3 localDownOffset = bottomVertLoc;
            bottomVertLoc = (Vector2)surfaceVertices[x - 1] - bottomVertLoc;
            surfaceVertices[(x - 2) + totalSurfaceVerts] = bottomVertLoc;



        }
        //surfaceVertices[totalSurfaceVerts * resZ - 1] = new Vector3(surfaceVertices[totalSurfaceVerts - 1].x, surfaceVertices[totalSurfaceVerts - 1].y - thickness);
        //Debug.Log("last vert: " + surfaceVertices[totalSurfaceVerts * 2 - 1]);

        //for(int i = 0; i <surfaceVertices.Length;i++) {
        //    if (surfaceVertices[i].sqrMagnitude <= 0.5) {
        //        if(i<totalSurfaceVerts)
        //            Debug.Log("found zero 1nd row: " + i);
        //        else
        //            Debug.Log("found zero 2nd row: " + i+", total: " + surfaceVertices.Length);
        //    }


        //}
        return surfaceVertices;
    }
 


    public void makeCollision(Vector3[] verts)
    {

       Vector2[] colliderPath = new Vector2[totalSurfaceVerts*2+resZ*2-1];
       
        int index = 0;
        for (int i = 0; i < totalSurfaceVerts; i++){//top surface edges
             colliderPath[index] = new Vector2(verts[i].x, verts[i].y);
             index++;
        }
        for (int i = 0; i < resZ-1; i++)//right surface edges
        {
            colliderPath[index] = new Vector2(verts[i*totalSurfaceVerts+totalSurfaceVerts-1].x, verts[i * totalSurfaceVerts + totalSurfaceVerts - 1].y);
            index++;
        }
        for (int i = 0; i < totalSurfaceVerts; i++)
        {//bottom surface edges
            colliderPath[index] = new Vector2(verts[totalSurfaceVerts*resZ-1-i].x, verts[totalSurfaceVerts * resZ - 1-i].y);
            index++;
        }
        for (int i = resZ - 1; i >=0; i--)//left surface edges
        {
            colliderPath[index] = new Vector2(verts[i * totalSurfaceVerts ].x, verts[i * totalSurfaceVerts ].y);
            index++;
        }
        if (!edgeCollider) {
            edgeCollider = gameObject.GetComponent<PolygonCollider2D>();
            //edgeCollider = gameObject.GetComponent<EdgeCollider2D>();
            //print("looking for collider componenet");
        }
        edgeCollider.points = colliderPath;
        edgeCollider.pathCount = 1;
        edgeCollider.SetPath(0, colliderPath);

        //edgeCollider.points = colliderPath;
    }

    private void generateSurfaceVerts(Vector3[] vertices, float stepHight, int stepWidth, int seed,float jumpHeight)
    {
        Random.InitState(seed);
        int x = 0;
        float unitWidth = ((float)1 / (totalSurfaceVerts - 1)) * length;
        int prvDirection = 0;
        Vector3 prvVert = new Vector3(0, 0, 0);
        for (; x < totalSurfaceVerts; )
        {
            
           
            int paltformDirection= Random.Range(0, 5);

            if (paltformDirection == 0 || paltformDirection == prvDirection)
            {
               prvVert= platformStraight(vertices, prvVert, stepWidth, totalSurfaceVerts, Vector3.right * unitWidth, ref x);
            }
            else if (paltformDirection == 1)//up
            {

                    prvVert = platformStraight(vertices, prvVert, stepWidth, totalSurfaceVerts,
                        Vector3.up * (jumpHeight / (float)stepWidth) + Vector3.right * unitWidth,
                        ref x);
                
            }
            else if (paltformDirection == 2)//dn
            {

                prvVert = platformStraight(vertices, prvVert, stepWidth, totalSurfaceVerts,
Vector3.down * (jumpHeight / (float)stepWidth) + Vector3.right * unitWidth,
ref x);
              
                
            }
            else if (paltformDirection == 3&&prvDirection!=4)//wall up
            {
                prvVert = platformStraight(vertices, prvVert, stepWidth/3, totalSurfaceVerts, Vector3.right * unitWidth, ref x);
                prvVert = platformStraight(vertices, prvVert, stepWidth / 3, totalSurfaceVerts, Vector3.up * jumpHeight, ref x);
                prvVert = platformStraight(vertices, prvVert, stepWidth / 3, totalSurfaceVerts, Vector3.right * unitWidth, ref x);

                
            }
            else if (paltformDirection == 4 && prvDirection != 3)//wall dn
            {

                prvVert = platformStraight(vertices, prvVert, stepWidth / 3, totalSurfaceVerts, Vector3.right * unitWidth, ref x);
                prvVert = platformStraight(vertices, prvVert, stepWidth / 3, totalSurfaceVerts, Vector3.down * jumpHeight, ref x);
                prvVert = platformStraight(vertices, prvVert, stepWidth / 3, totalSurfaceVerts, Vector3.right * unitWidth, ref x);

            }
            //Debug.Log(prvVert);
            // float groundLevel = Mathf.PerlinNoise(x * 0.1f, x * x * 0.3f);
            prvDirection = paltformDirection;
        
        }
        vertices[totalSurfaceVerts] = new Vector3(vertices[0].x, vertices[0].y - thickness);

        RightEdge = vertices[totalSurfaceVerts-1];
        

    }
    private Vector3 platformStraight(Vector3[] vertices,Vector3 prvVert,int numOfVerts,int maxVertIndex,
                                        Vector3 step, ref int x)
    {
        for (int i = 0; i < numOfVerts && x < maxVertIndex; i++)
        {

            vertices[x] = prvVert + step;
            prvVert = vertices[x];
            x++;
        }
        return prvVert;
    }
    private void turnIntoCircle(Mesh mesh, Vector3 origin,float radius,float hightScale=1.0f){
        Vector3[] verts = mesh.vertices;
        for(int i=0;i< verts.Length; i++) {
            Vector3 to=  Vector3.up*((verts[i].y - origin.y)* hightScale + radius);
            to = Quaternion.Euler(0, 0,- verts[i].x/(verts[verts.Length - 1].x-verts[0].x) * 360f) * to ;
            verts[i] = to;
        }
        mesh.vertices = verts;

    }

    public Vector3[] getSurfaceVerts() {
        Vector3[] surfVerts = new Vector3[totalSurfaceVerts];
        for (int i = 0; i < topSurface.Length; i++)
            surfVerts[i] = topSurface[i] + transform.position;
        return surfVerts;
    }
    public Vector3[] getSurfaceVertsInGlobalSpace() {
        Vector3[] surfVerts = new Vector3[totalSurfaceVerts];
        for (int i = 0; i < topSurface.Length; i++)
            surfVerts[i] = topSurface[i]+transform.position;
        return surfVerts;
    }
    public Vector3[] getSpreadSurfaceVertsInGlobalSpace() {
        Vector3[] surfVerts = new Vector3[totalSurfaceVerts];
        for (int i = 0; i < topSurfaceSpread.Length; i++)
            surfVerts[i] = topSurfaceSpread[i] + positionSpread;
        return surfVerts;
    }
    public Vector3[] tranlateVertsToLocalSpace(Vector3[] verts) {
        Vector3[] surfVerts = new Vector3[verts.Length];
        for (int i = 0; i < verts.Length; i++)
            surfVerts[i] = verts[i] - transform.position;
        return surfVerts;
    }
    public Vector3[] tranlateVertsToGlobalSpace(Vector3[] verts) {
        Vector3[] surfVerts = new Vector3[verts.Length];
        for (int i = 0; i < verts.Length; i++)
            surfVerts[i] = verts[i] + transform.position;
        return surfVerts;
    }



    private Vector3[] preventedCollision;
    private int pcindex = 0;


    Vector3 translationCorrection = Vector3.zero;
    public Vector3[] createSurfacePlaneThatAvoids(Vector3[] surfaceToAvoid,int numOfVerts) {
        preventedCollision = new Vector3[100];

        if(minDistance(transform.position,surfaceToAvoid) < jumpHeight){//if we start too low
            
            //Debug.Log("map starting too low");
            if (collisionBoxPrefab) {
                GameObject GO = Instantiate(collisionBoxPrefab, transform.position, Quaternion.identity);
                GO.transform.SetParent(this.transform);
                GO.transform.localScale *= 2f;
                GO = Instantiate(collisionBoxPrefab, transform.position+ Vector3.up * jumpHeight, Quaternion.identity);
                GO.transform.SetParent(this.transform);
                GO.transform.localScale *= 3f;
            }

            //transform.position += Vector3.up * jumpHeight;//doesn't work because position is calculated from outside
            translationCorrection = Vector3.up * jumpHeight;
        }

        Vector3[] vertices = new Vector3[numOfVerts];
        Random.InitState(seed);
        int x = 0;
        float unitWidth = ((float)1 / (totalSurfaceVerts - 1)) * length;
        int prvDirection = 0;
        Vector3 prvVert = new Vector3(0, 0, 0);
        for (; x < vertices.Length;) {

           

            int platformDirection = Random.Range(0, 5);


            Vector3[] predictedSection =  createPlatformSurface(prvVert,
                        Vector3.down * (jumpHeight/vertsPerPlatform) + Vector3.right * unitWidth,vertsPerPlatform);
            //Vector3 predictedVert = predictedSection[predictedSection.Length-1]+Vector3.down*thickness;
            foreach (Vector3 predictedVert in predictedSection) {
                if ((minDistance(predictedVert+ Vector3.down * thickness, surfaceToAvoid) < jumpHeight)) {
                    preventedCollision[pcindex] = predictedVert;
                    pcindex++;
                    //Debug.Log("prevented collision: "+ predictedVert);
                    if (collisionBoxPrefab) {
                         Instantiate(collisionBoxPrefab, predictedVert +transform.position , Quaternion.identity).transform.SetParent(this.transform);
                     
                    }
                    platformDirection = 1;//curve up
                    break;
                } else {
                    if (collisionBoxPrefab) {
                       GameObject GO= Instantiate(collisionBoxPrefab, predictedVert + transform.position, Quaternion.identity);
                        GO.transform.SetParent(this.transform);
                        GO.transform.localScale*= 0.5f;
                    }
                }


            }

            if (platformDirection == 0 || platformDirection == prvDirection && platformDirection!= 1) {
                prvVert = platformStraight(vertices, prvVert, vertsPerPlatform, numOfVerts, Vector3.right * unitWidth, ref x);
            } else if (platformDirection == 1)//up
              {

                prvVert = platformStraight(vertices, prvVert, vertsPerPlatform, numOfVerts,
                    Vector3.up * (jumpHeight / (float)vertsPerPlatform) + Vector3.right * unitWidth,
                    ref x);

            } else if (platformDirection == 2)//dn
              {

                prvVert = platformStraight(vertices, prvVert, vertsPerPlatform, numOfVerts,
Vector3.down * (jumpHeight / (float)vertsPerPlatform) + Vector3.right * unitWidth,
ref x);


            } else if (platformDirection == 3 && prvDirection != 4)//wall up
              {
                prvVert = platformStraight(vertices, prvVert, vertsPerPlatform / 3, numOfVerts, Vector3.right * unitWidth, ref x);
                prvVert = platformStraight(vertices, prvVert, vertsPerPlatform / 3, numOfVerts, Vector3.up * jumpHeight, ref x);
                prvVert = platformStraight(vertices, prvVert, vertsPerPlatform / 3, numOfVerts, Vector3.right * unitWidth, ref x);


            } else if (platformDirection == 4 && prvDirection != 3)//wall dn
              {

                prvVert = platformStraight(vertices, prvVert, vertsPerPlatform / 3, numOfVerts, Vector3.right * unitWidth, ref x);
                prvVert = platformStraight(vertices, prvVert, vertsPerPlatform / 3, numOfVerts, Vector3.down * jumpHeight, ref x);
                prvVert = platformStraight(vertices, prvVert, vertsPerPlatform / 3, numOfVerts, Vector3.right * unitWidth, ref x);

            }
            //Debug.Log(prvVert);
            // float groundLevel = Mathf.PerlinNoise(x * 0.1f, x * x * 0.3f);
            prvDirection = platformDirection;

        }
        // vertices[vertices.Length-1] = new Vector3(vertices[0].x, vertices[0].y - length);
        if(translationCorrection!=Vector3.zero)
            correctTranslation(vertices, translationCorrection);
        RightEdge = vertices[totalSurfaceVerts - 1];
        return vertices;
    }
    void correctTranslation(Vector3[] vertices,Vector3 translationCorrection) {
        for(int i=0;i<vertices.Length;i++) {
            vertices[i] += translationCorrection;
        }
    }


    private Vector3[] createPlatformSurface(Vector3 origin,Vector3 direction, int vertCount, float stepSize=1) {
        Vector3[] temp = new Vector3[vertCount];
        for (int i = 0; i < temp.Length; i++) {
            temp[i] = origin+ direction * stepSize * i;
        }
        return temp;
    }
    private int getClosestPoint(Vector3 position, Vector3[] points) {

        int closest=-1;
        float minDist = Mathf.Infinity;
        int i = 0;
        foreach (Vector3 t in points) {
            float dist = Vector3.SqrMagnitude(position-t);
            if (dist < minDist) {
                closest = i;
                minDist = dist;
            }
            i++;
        }
        return closest;
    }
    private float minDistance(Vector3 position, Vector3[] points) {
        return Vector3.Magnitude(position - points[getClosestPoint(position, points)]);
    }

    [ClientRpc]
    public void RpcSyncSeedOnBase(int _seed, float _length, float _jumpHeight, bool _isCircle) {
        //Debug.Log("updating base by the seed");

        if (!mainMesh) {
            mainMesh = gameObject.GetComponent<MeshFilter>().mesh;
            Debug.Log("main mesh couldn't be found, looking again.");
        }

        seed = _seed;
        length = _length;
        totalSurfaceVerts = (int)_length;
        jumpHeight = _jumpHeight;

        updateMap();//till here is a ready to use flat map
        if (_isCircle) {//circlize the ready map

            float radius = length / Mathf.PI / 2;
            Vector3 begin = transform.position;
            Vector3 end = RightEdge;
            Vector3[] globalBaseVerts = mainMesh.vertices;
            Vector3[] rotatedBaseVerts = calcCircularVerts(globalBaseVerts, radius, begin, end);
            createAndAssignMeshFromAllVertsWithCollisions(rotatedBaseVerts);
        }

    }
    public void createAndAssignMeshFromAllVertsWithCollisions(Vector3[] surfaceVertsInLocalSpace) {
        Vector3[] allVertsInLocalSpace = createAllVerticesFromSurface(surfaceVertsInLocalSpace);
        mainMesh.vertices = allVertsInLocalSpace;
        mainMesh.uv = calculateUVs(allVertsInLocalSpace);
        mainMesh.triangles = calculateTriangles(allVertsInLocalSpace);

        makeCollision(mainMesh.vertices);
        mainMesh.RecalculateBounds();
    }
    public void createAndAssignMeshFromSurfaceWithCollisions(Vector3[] surfaceVertsInLocalSpace) {
        Vector3[] allVertsInLocalSpace = createAllVerticesFromSurface(surfaceVertsInLocalSpace);
        mainMesh.vertices = allVertsInLocalSpace;
        mainMesh.uv = calculateUVs(allVertsInLocalSpace);
        mainMesh.triangles = calculateTriangles(allVertsInLocalSpace);

        makeCollision(mainMesh.vertices);
        mainMesh.RecalculateBounds();
    }
    
    //should be called for all the maps by the map manager
    [ClientRpc]public void RpcSyncVerts(Vector3[] allVertsInLocalSpace) {
        if (!mainMesh) {
            mainMesh = gameObject.GetComponent<MeshFilter>().mesh;
            Debug.Log("main mesh couldn't be found, looking again.");
        }


        //Vector3[] allVertsInLocalSpace = tranlateVertsToLocalSpace(allVertsInGlobalSpace);

        mainMesh.vertices = allVertsInLocalSpace;
        mainMesh.uv = calculateUVs(allVertsInLocalSpace);
        mainMesh.triangles = calculateTriangles(allVertsInLocalSpace);

        makeCollision(mainMesh.vertices);
        mainMesh.RecalculateBounds();
    }

    private Vector3[] calcCircularVerts(Vector3[] vertsInGlobal, float radius, Vector3 start, Vector3 end) {
        Vector3[] verts = vertsInGlobal;//to shorten the name
        //Debug.Log("rotating chosen object");

        for (int i = 0; i < verts.Length; i++) {
            float height = radius + (verts[i].y - start.y);//clalculate distance from center
            Vector3 to = Vector3.up * height;

            //calculate rotation
            if (end.x - start.x <= 0.1f) {
                Debug.Log("divide by zero");
                return vertsInGlobal;
            }

            float theta = verts[i].x / (end.x - start.x) * 360f;

            //rotate clockwise
            to = Quaternion.Euler(0, 0, -theta) * to;

            //Debug.Log("before: " + verts[i] + " after: " + to);
            verts[i] = to;
        }

        return verts;

    }


    private int[] calculateTriangles(Vector3[] allVertsInLocalSpace) {
        int nbFaces = (totalSurfaceVerts - 1) * (resZ - 1);
        int[] triangles = new int[nbFaces * 6];
        int t = 0;
        for (int faceZ = 0; faceZ < resZ - 1; faceZ++) {
            for (int faceX = 0; faceX < totalSurfaceVerts - 1; faceX++) {
                int i = faceX + faceZ * totalSurfaceVerts;

                triangles[t++] = i + totalSurfaceVerts;
                triangles[t++] = i;
                triangles[t++] = i + 1;


                triangles[t++] = i + totalSurfaceVerts;
                triangles[t++] = i + 1;
                triangles[t++] = i + totalSurfaceVerts + 1;

            }
        }
        //Debug.Log("triangles calculated: " + triangles.Length);
        return triangles;
    }

    private Vector2[] calculateUVs(Vector3[] allVertsInLocalSpace) {
        Vector2[] uvs = new Vector2[allVertsInLocalSpace.Length];
        for (int v = 0; v < resZ; v++) {
            for (int u = 0; u < totalSurfaceVerts; u++) {
                uvs[u + v * totalSurfaceVerts] = new Vector2(allVertsInLocalSpace[u + v * totalSurfaceVerts].x * 0.1f,
                                        allVertsInLocalSpace[u + v * totalSurfaceVerts].y * 0.1f);
            }
        }
        //Debug.Log("uvs calculated: "+ uvs.Length);
        return uvs;
    }
    /*
    private void OnDrawGizmosSelected() {
        foreach (Vector3 v in baseVertsGizmos) {
            Gizmos.DrawSphere(v, 0.2f);
        }
        foreach (Vector3 v in preventedCollision) {
            Gizmos.DrawSphere(v, 0.2f);
        }
        Debug.Log("drawing gizmos");

        /*
        Vector3[] GS1CurrentSurface = tranlateVertsToGlobalSpace(topSurface);
        Vector3[] GSCurrentSurface = tranlateVertsToGlobalSpace(preventedCollision);
        foreach (Vector3 v in GSCurrentSurface) {
            Gizmos.DrawSphere(v, 0.2f);
        }
        foreach (Vector3 v in GS1CurrentSurface) {
            Gizmos.DrawCube(v, new Vector3(0.1f, 0.1f, 0.1f));
        }
        */
    //}

    private void OnDrawGizmos() {
        //Debug.Log("drawing gizmos on map generator");
        // Gizmos.DrawSphere(Vector3.zero, 5f);
        // foreach (Vector3 v in generatedMap.GetComponent<MapGenerator>().getSurfaceVerts()) {
        // Gizmos.DrawSphere(v, 0.5f);

        // }
    }






}
