/* this is a map manager class that manages the map 
 * once it receive an order from GameManger it spawns a map 
 * also it calculates where are the surfaces
 * so that they can be used to spawn objects
 */



//using System;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MapManager : NetworkBehaviour {
    public MapGenerator[] mapGeneratorPrefabs;
    public GameManager GM;
    public GameObject blackHolePrefab;
    public GameObject deathPlatformPrefab;

    [HideInInspector] public List<Vector3[]> surfacesInGlobalSpace;
    [HideInInspector] public List<Vector3[]> spreadSurfacesInGlobalSpace;
    private List<MapGenerator> platforms;

    public bool isBusyMakingMap = true;
    public bool finishedCreatingPlatforms = false;


    private int lastUsedIndex = 0;
    private Vector3 lastPlatformEnd = Vector3.zero;

    MapGenerator generatedMapBase;

    private int randomMapPrefabIndex = 0;

    bool MMinited = false;
    private void makeSureInit() {
        if (MMinited)
            return;
        MMinited = true;
        surfacesInGlobalSpace = new List<Vector3[]>();
        spreadSurfacesInGlobalSpace = new List<Vector3[]>();
        platforms = new List<MapGenerator>();

    }




    private bool createdDeathBarrier = false;
    private bool syncedTheMaps = false;
    float lastSpawnTime = 0f;
    void Update() {
        if (!isServer)
            return;
        if (!isBusyMakingMap && !finishedCreatingPlatforms &&Time.time -lastSpawnTime>=0.1f) {
            lastSpawnTime = Time.time;
            if (!createdDeathBarrier) {
                createdDeathBarrier = true;
                createDeathBarier();
            }
            MapGenerator MG = addPlatform(GM.currentRules);
            if(MG)
                platforms.Add(MG);

        } else if (finishedCreatingPlatforms && !syncedTheMaps&&isPlatformsOnly) {
            //Debug.Log("platforms only have finished");
            syncedTheMaps = true;
            NetworkServer.Destroy(generatedMapBase.gameObject);
            //Debug.Log("finished making all map with platforms");
            mapFinishedGettingCreated();

        } else if (finishedCreatingPlatforms && !syncedTheMaps) {
            //Debug.Log("normal have finished");
            syncedTheMaps = true;
            //Debug.Log("finished making all map with platforms");
            mapFinishedGettingCreated();

        } else if (finishedCreatingPlatforms && syncedTheMaps && !bossSyncedTheMaps) {
            bossSyncedTheMaps = true;
            //Debug.Log("finished making all map with platforms");
            bossMapFinishedGettingCreated();

        }

        if(GM.currentRules!=null&&!GM.currentRules.isCircle && blackHolePrefab != null) {
            NetworkServer.Destroy(deathBarrierObj);
            deathBarrierObj = null;
        }




    }



    MapGenerator generateMapBase(Vector3 position, Rules rules) {
        isBusyMakingMap = true;
       GameObject GO= Instantiate(mapGeneratorPrefabs[randomMapPrefabIndex].gameObject, position,Quaternion.identity);
        GO.transform.parent = transform;
        MapGenerator MG = GO.GetComponent<MapGenerator>();
        MG.setRules(rules);
       // MG.length = 500f;
       // MG.totalSurfaceVerts = 500;
        MG.updateMap(mapFinishedUpdating);
        NetworkServer.Spawn(GO);

        //RpcUpdateBaseMapOnClients(GO);

        return GO.GetComponent<MapGenerator>();
    }

    [ClientRpc]public void RpcUpdateBaseMapOnClients(GameObject mapGO) {
        MapGenerator MG = mapGO.GetComponent<MapGenerator>();
        if (!MG) {
            Debug.Log("MapGenerator component not found");
            return;
        }
        Debug.Log("updating map base on clients");
        MG.updateMap();
    }

    private MapGenerator addPlatform(Rules rules = null) {//returns null if it can't add
        isBusyMakingMap = true;
        float jumpHeight = generatedMapBase.jumpHeight + 1;
        Vector3[] surfaceVerts = generatedMapBase.getSpreadSurfaceVertsInGlobalSpace();//in local space 


        Vector3 spawnPosition;

        int randomIndexOffset = Random.Range(4, 20);

        do {
            if (randomIndexOffset + lastUsedIndex >= surfaceVerts.Length) {
                finishedCreatingPlatforms = true;
                isBusyMakingMap = false;
                return null;

            }
            //Debug.Log(spawnPosition);
            spawnPosition = Vector3.up * (jumpHeight + generatedMapBase.thickness);
            spawnPosition += surfaceVerts[randomIndexOffset + lastUsedIndex];
            lastUsedIndex = randomIndexOffset + lastUsedIndex;
        } while (lastPlatformEnd.x > spawnPosition.x);//we didn't pass over the last platform


        //       Debug.Log(generatedMapBase.transform.position);

        GameObject GO = Instantiate(mapGeneratorPrefabs[randomMapPrefabIndex].gameObject, spawnPosition, Quaternion.identity);
        GO.transform.parent = transform;

        MapGenerator MG = GO.GetComponent<MapGenerator>();
        MG.setRules(rules);
        MG.seed += Random.Range(1, 50);
        MG.length = Random.Range(5, 50);
        MG.totalSurfaceVerts = (int)MG.length;
        MG.Radius += MG.jumpHeight;

        //Vector3[] vertsToAvoid = spreadSurfacesInGlobalSpace[0];
        Vector3[] vertsToAvoid = generatedMapBase.getSpreadSurfaceVertsInGlobalSpace();
        MG.updateMapPlatform(vertsToAvoid, mapFinishedUpdating);
        lastPlatformEnd = MG.getSpreadSurfaceVertsInGlobalSpace()[MG.totalSurfaceVerts - 1];



        NetworkServer.Spawn(GO);
        return MG;


    }

    [ClientRpc]public void RpcUpdateMapPlatformOnClients(GameObject mapGO) {
        MapGenerator MG = mapGO.GetComponent<MapGenerator>();
        if (!MG) {
            Debug.Log("MapGenerator component not found");
            return;
        }
        MG.updateMap();
    }





    public void mapFinishedUpdating(MapGenerator MG) {
        surfacesInGlobalSpace.Add(MG.getSurfaceVertsInGlobalSpace());
        spreadSurfacesInGlobalSpace.Add(MG.getSpreadSurfaceVertsInGlobalSpace());
        isBusyMakingMap = false;



    }
    public void bossMapFinishedGettingCreated() {
        if (GM.currentRules.isCircle) {
            circlizeAllMaps();
        }
        CancelInvoke();
        Invoke("syncAllMaps", 0.5f);
        //Invoke("respawnAllPlayers", 2f);
    }
    public void mapFinishedGettingCreated() {
        if (GM.currentRules.isCircle) {
            circlizeAllMaps();
        }
        CancelInvoke();
        Invoke("syncAllMaps", 0.5f);
        //Invoke("respawnAllPlayers", 2f);
    }
    //tells all the clients to update their maps to the same as the server's
    public void syncAllMaps() {
        //Debug.Log("syncing the maps with the clients");
        //generatedMapBase.RpcSyncVerts(generatedMapBase.mainMesh.vertices);
        if (!isPlatformsOnly) {
            //Debug.Log("syncing only platforms");
            if (!generatedMapBase) {
               
                return;
            }
            generatedMapBase.RpcSyncSeedOnBase(GM.currentRules.seed, GM.currentRules.length,
                    GM.currentRules.jumpHeight, GM.currentRules.isCircle);
        }
        StartCoroutine(syncMapCo());

        respawnAllPlayers();
    }

    IEnumerator syncMapCo() {
        int index = 0;
        foreach (MapGenerator m in platforms) {
            index++;
            if (!m) {
                Debug.Log("map object null: " + index);
                continue;
            }
            if (!m.mainMesh) {
                Debug.Log("main mesh on map is null");
                continue;
            }

            m.RpcSyncVerts(m.mainMesh.vertices);
           // Debug.Log("syncing platform");
            yield return new WaitForSeconds(0.1f);
        }

    }



        //basicly distort the space, apply the distortion to the verticies 
        public void circlizeAllMaps() {
        surfacesInGlobalSpace.Clear();
        //Debug.Log("cirulizing the maps");
        float radius = generatedMapBase.length / Mathf.PI / 2;
        Vector3 begin = generatedMapBase.transform.position;
        Vector3 end = generatedMapBase.RightEdge;

        Vector3[] globalBaseVerts = generatedMapBase.getSurfaceVertsInGlobalSpace();

        Vector3[] rotatedBaseVerts = calcCircularVerts(globalBaseVerts, radius, begin, end);
        //generatedMapBase.RpcSyncVerts(rotatedBaseVerts);
        generatedMapBase.createMapFromSurfacePlane(rotatedBaseVerts);
        generatedMapBase.transform.position = Vector3.zero;
        surfacesInGlobalSpace.Add(rotatedBaseVerts);
        foreach (MapGenerator m in platforms) {
            Vector3[] globalVerts = m.getSurfaceVertsInGlobalSpace();
            Vector3[] rotatedVerts = calcCircularVerts(globalVerts, radius, begin, end);
            m.createMapFromSurfacePlane(rotatedVerts);

            m.transform.position = Vector3.zero;
            surfacesInGlobalSpace.Add(rotatedVerts);
        }


    }


    public void createNewMap() {
        //Debug.Log("creating new map");
        isPlatformsOnly = false;
        randomMapPrefabIndex = Random.Range(0, mapGeneratorPrefabs.Length);
        syncedTheMaps = false;
        makeSureInit();
        deleteOldMap();

        //respawnAllPlayers();
        //create new map

        generatedMapBase = generateMapBase(transform.position,GM.currentRules);
        finishedCreatingPlatforms = false;
        createdDeathBarrier = false;

    }
    public bool isPlatformsOnly = false;
    public void createNewMapPlatformsOnly() {
        isPlatformsOnly = true;
        //Debug.Log("creating new map");

        randomMapPrefabIndex = Random.Range(0, mapGeneratorPrefabs.Length);
        syncedTheMaps = false;
        makeSureInit();
        deleteOldMap();

        //respawnAllPlayers();
        //create new map

        generatedMapBase = generateMapBase(transform.position, GM.currentRules);
        finishedCreatingPlatforms = false;
        createdDeathBarrier = false;

    }
    bool bossSyncedTheMaps = true;
    public void createNewMapBaseOnly() {
        isPlatformsOnly = false;
        randomMapPrefabIndex = Random.Range(0, mapGeneratorPrefabs.Length);
        syncedTheMaps = true;
        bossSyncedTheMaps = false;
        makeSureInit();
        deleteOldMap();


        //respawnAllPlayers();
        //create new map

        generatedMapBase = generateMapBase(transform.position, GM.currentRules);
        finishedCreatingPlatforms = true;
        createdDeathBarrier = true;

    }

    private void respawnAllPlayers() {
        foreach(GameObject p in GM.players) {
            if (!p)
                continue;
            PlayerConnectionObject PCO = p.GetComponent<PlayerConnectionObject>();
            if (!PCO || !PCO.PC)
                continue;

            Vector3 randomPosition = getRandomPositionAboveMap();
            PCO.PC.RpcTeleportObject(randomPosition);
            
        }
    }

    private void deleteOldMap() {

        if (!generatedMapBase)
            return;
        NetworkServer.Destroy(generatedMapBase.gameObject);
        generatedMapBase = null;
        foreach(MapGenerator MG in platforms) {
            if (!MG)
                continue;
            NetworkServer.Destroy(MG.gameObject);
        }
        platforms.Clear();
        surfacesInGlobalSpace.Clear();
        isBusyMakingMap = false;


        lastPlatformEnd = Vector3.zero;
        lastUsedIndex = 0;
    }

    public Vector3 getRandomPosition() {
        if (surfacesInGlobalSpace.Count == 0 || !finishedCreatingPlatforms) {
            return new Vector3(50,50);
        }
        //beacause map base is always bigger 
        bool shouldBeOnBase = Random.Range(0f, 1f) < 0.8 ? true : false;

        int randomSurfaceIndex = 0;
        if (!shouldBeOnBase && surfacesInGlobalSpace.Count >1) {
            randomSurfaceIndex = Random.Range(1, surfacesInGlobalSpace.Count);
        }
        if (isPlatformsOnly) {
            randomSurfaceIndex = Random.Range(1, surfacesInGlobalSpace.Count);
        }

        Vector3[] surfaceVerts = surfacesInGlobalSpace[randomSurfaceIndex];

        int randomVertIndex = Random.Range(0, surfaceVerts.Length);

        Vector3 randomPosition = surfaceVerts[randomVertIndex];

        return randomPosition;
    }

    public Vector3 getRandomPositionAboveMap() {
        Vector3 randomPosition = getRandomPosition();
        if (randomPosition == Vector3.zero)
            return randomPosition;

        randomPosition += GravitySystem.instance.getUpDirection(randomPosition) * 2;
        return randomPosition;

    }

    public Vector3 getMapEndPosition() {
        if (!generatedMapBase)
            return Vector3.zero;

        Vector3[] surfaceVerts= generatedMapBase.getSurfaceVertsInGlobalSpace();

        if (surfaceVerts.Length <= 0) {
            Debug.Log("couldn't get map end");
            return Vector3.zero;
        }

        return surfaceVerts[surfaceVerts.Length-1];


    }
    GameObject deathBarrierObj;
        public void createDeathBarier() {
        if (GM.currentRules.isCircle) {
            if (deathBarrierObj)
                return;
            //add black hole
            //Debug.Log("spawned black hole");
            deathBarrierObj = Instantiate(blackHolePrefab, Vector3.zero, Quaternion.identity);

            NetworkServer.Spawn(deathBarrierObj);
        } else {
            //add death platform
            //Debug.Log("spawned death platform");
            //Vector3 rightend = generatedMapBase.RightEdge;
            //Vector3 middle = new Vector3(rightend.x / 2, -100, 0);

            //GameObject GO = Instantiate(deathPlatformPrefab, middle, Quaternion.identity);
            //Vector3 tmpScale = GO.transform.localScale;
            //tmpScale.x = rightend.x;
            //GO.transform.localScale = tmpScale;
            //NetworkServer.Spawn(GO);
        }
    }




    private Vector3[] calcCircularVerts(Vector3[] vertsInGlobal,float radius,Vector3 start,Vector3 end) {
        Vector3[] verts = vertsInGlobal;//to shorten the name
        //Debug.Log("rotating chosen object");

        for (int i = 0; i < verts.Length; i++) {
            float height = radius +( verts[i].y -start.y);//clalculate distance from center
            Vector3 to = Vector3.up * height;

            //calculate rotation
            float theta =  verts[i].x / (end.x-start.x) * 360f;

            //rotate clockwise
            to = Quaternion.Euler(0, 0,-theta) * to;

            //Debug.Log("before: " + verts[i] + " after: " + to);
            verts[i] = to;
        }
        
        return verts;

    }




    private void OnDrawGizmos() {
        //Debug.Log("drawing gizmos on map manager");
        // Gizmos.DrawSphere(Vector3.zero, 5f);

       // float i = 1;
       // foreach (Vector3 v in generatedMapBase.GetComponent<MapGenerator>().mainMesh.vertices) {
       //    Gizmos.DrawSphere(v, 0.5f*i);
           
        //    i+=0.01f;
        //}
    }

}
