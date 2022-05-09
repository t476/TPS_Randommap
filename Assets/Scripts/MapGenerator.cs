using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;
using System;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator instance;
    [Header("瓦片")]
    public GameObject tilePrefab;
    public Vector2 mapSize;
   // public Transform mapHolder;//这个在下次实例时要删除！！！
    [Range(0, 1)] public float outlinePrecent;//瓦片之间留有缝隙去调整
    [Header("障碍物")]
    public GameObject obstaclePrefab;
    //public float obstacleCount;
    public Color foregroundColor, backgroundColor;
    public float minHeight, maxHeight;
    public List<Coordiate> allTilesCoord = new List<Coordiate>();
    List<Coordiate> allOpenCoords;
    private Queue<Coordiate> shuffledQueue;//洗牌队列
    private Queue<Coordiate> shuffledOpenQueue;//洗除了障碍物以外的点的队列
    [Header("地图连通性")]
    [Range(0, 1)] public float obsPercent;
    private Coordiate mapCenter;//人物在中心点生成，这也是洪水起点
    bool[,] mapObstacles;//该位置是否含有障碍物
    //we can store allof the tiles .sothat we can access theis positions！！！
    Transform[,] tileMap;
   // Coordiate[] mapNotHaveObs;

    //随机地图最大尺寸
    public Vector2 mapMaxSize;
    [Header("地图边界墙")]
    public GameObject navMeshObsPrefab;
    public GameObject player;
    [Header("地图们")]
    public Map[] maps;
    Map currentMap;
    public int mapIndex;
    [Header("敌人颜色")]
    public Material m_material;
    public GameObject m_object;

    void Awake()
    {
        instance = this;
        Init();
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
         m_object= GameObject.FindGameObjectWithTag("enemyColor");
         m_material = m_object.GetComponent<Renderer>().sharedMaterial;
        m_material.color = foregroundColor;

    }
    private void Start()
    {
        //ChooseAMap();
        //FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
       // GenerateMap();
       // Init();

    }
    void OnNewWave(int waveNumber)
    {
        //mapIndex = waveNumber - 1;
        //if (GameObject.FindGameObjectWithTag("Player"))
        //    player.transform.position = getTileFromPosition(Vector3.zero).position + Vector3.up * 3;
        ChooseAMap();
        GenerateMap();
        //在这里更改enemy预制体的颜色 1直接改预制体2改材质球sharedmaterial选2
        m_material.color = foregroundColor;

     //   Init();


    }
    private void ChooseAMap()
    {
        currentMap = maps[UnityEngine.Random.Range(0, maps.Length)];//洗牌
        mapSize = currentMap._mapSize;
        minHeight = currentMap._minObsHeight;
        maxHeight = currentMap._maxObsHeight;
        foregroundColor = currentMap._foregroundColor;
        backgroundColor = currentMap._backgroundColor;
        mapCenter = currentMap.mapCenter;

    }
    /*直接用洗牌算法更好，和泛洪时避免取重复的点一样
    public Transform GetRandomOpenTile()
    {
        int getRandomOpenTileNum = UnityEngine.Random.Range(0, (int)(mapSize.x * mapSize.y * (1-obsPercent))-1);
        int num=0;
        for(int i = 0; i < mapSize.x - 1; i++)
        {
            for(int j = 0; j < mapSize.y-1; j++)
            {
                if (!mapObstacles[i, j])
                {
                    num++;
                    if (num == getRandomOpenTileNum)
                    {
                        Vector3 newPos = new Vector3(-mapSize.x / 2 + 0.5f + i, 0, -mapSize.y / 2 + 0.5f + j);
                        string Tilename=string.Format("111{0}",i+j);
                        Transform theOpenTile = transform.Find(Tilename);
                        return theOpenTile;
                    }
                }
            }
        }
        return null;
    }*/

    private void Init()
    {

        if (!GameObject.FindGameObjectWithTag("Player"))
            Instantiate(player, new Vector3(-mapSize.x / 2 + 0.5f + mapCenter.x, 0.5f, -mapSize.y / 2 + 0.5f + mapCenter.y), Quaternion.identity);
        else
        {
            //问题在于我的物体一开始就处于显示状态，这样移动只移动了MeshFilter,物体本身的Mesh并没有移动，依旧无用
            //player.SetActive(false);
            //   player.GetComponent<BoxCollider>().enabled=false;
            //为什么在这里不行
          //  player.transform.position= getTileFromPosition(Vector3.zero).position + Vector3.up * 0.5;
            //更改单个需要的操作
            //  Vector3 pos = player.transform.localPosition;
            //   player.transform.position= new Vector3(-mapSize.x / 2 + 0.5f + mapCenter.x, 0.5f, -mapSize.y / 2 + 0.5f + mapCenter.y);
            // player.transform.localPosition = pos;
            //  player.GetComponent<BoxCollider>().enabled = true;
            // player.SetActive(true);
        }
    }
    //use the Transform[,]
    public Transform getTileFromPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x + mapSize.x / 2 - 0.5f);
        int y = Mathf.RoundToInt(position.z + mapSize.y / 2 - 0.5f);
        x = Mathf.Clamp(x, 0,(int)mapSize.x - 1);
        y = Mathf.Clamp(y, 0, (int)mapSize.y - 1);
        return tileMap[x, y];
        
    }
    private void GenerateMap()
    {
        tileMap = new Transform[(int)mapSize.x,(int)mapSize.y];
        //重新生成空气边界墙，并摧毁之前的
       
        //重要!!!!,要重新在这里面初始化
        //是这里destoy之前的然后new了一个新的
        allTilesCoord = new List<Coordiate>();
        string holderName = "mapHolder";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
            
        }
        
        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;
        GameObject Wallup = Instantiate(navMeshObsPrefab, Vector3.forward * (mapMaxSize.y + mapSize.y) / 4, Quaternion.identity);
        //大小,要理解那張图，注意它的y才是他的高
        Wallup.transform.parent = mapHolder;
        Wallup.transform.localScale = new Vector3(mapSize.x, 5, mapMaxSize.y / 2 - mapSize.y / 2);
        GameObject Walldown = Instantiate(navMeshObsPrefab, Vector3.back * (mapMaxSize.y + mapSize.y) / 4, Quaternion.identity);
        Walldown.transform.parent = mapHolder;
        Walldown.transform.localScale = new Vector3(mapSize.x, 5, mapMaxSize.y / 2 - mapSize.y / 2);
        GameObject Wallleft = Instantiate(navMeshObsPrefab, Vector3.left * (mapMaxSize.x + mapSize.x) / 4, Quaternion.identity);
        Wallleft.transform.parent = mapHolder;
        Wallleft.transform.localScale = new Vector3(mapMaxSize.x / 2 - mapSize.x / 2, 5, mapSize.y);
        GameObject Wallright = Instantiate(navMeshObsPrefab, Vector3.right * (mapMaxSize.x + mapSize.x) / 4, Quaternion.identity);
        Wallright.transform.parent = mapHolder;
        Wallright.transform.localScale = new Vector3(mapMaxSize.x / 2 - mapSize.x / 2, 5, mapSize.y);

        //生成瓦片地图
        for (int i = 0; i < mapSize.x; i++)
        {
            for (int j = 0; j < mapSize.y; j++)
            {
                //在特定位置中生成Prefab，放在mapHolder下方,调增自身大小
                //以0，0点为中心,而不是以0，0点为起点位置
                Vector3 newPos = new Vector3(-mapSize.x / 2 + 0.5f + i, 0, -mapSize.y / 2 + 0.5f + j);
                GameObject spawnTile = Instantiate(tilePrefab, newPos, Quaternion.Euler(90, 0, 0));
                spawnTile.transform.SetParent(mapHolder);
                spawnTile.transform.localScale *= (1 - outlinePrecent);
                allTilesCoord.Add(new Coordiate(i, j));//?
                tileMap[i, j] = spawnTile.transform;
            }
        }

        allOpenCoords = new List<Coordiate>(allTilesCoord);//list的copy写法

        //障碍物生成
        shuffledQueue = new Queue<Coordiate>(Utilities.ShuffleCoords(allTilesCoord.ToArray()));//66666list转array，new 一个queue，IEnumerable//接口
        int obstacleCount =(int)( mapSize.x * mapSize.y * obsPercent);
        //初始化
        mapCenter = new Coordiate((int)mapSize.x / 2, (int)mapSize.y / 2);
        mapObstacles = new bool[(int)mapSize.x, (int)mapSize.y];

        int currentObsCount = 0;
        
        for (int i = 0; i < obstacleCount; i++)
        {
            Coordiate randomCoord = GetRandomCoord();
            //先假设可以加上这个点
            mapObstacles[randomCoord.x, randomCoord.y] = true;
            currentObsCount++;

            //条件：我们的randomcoord不能在中心点上,且洪水可通行
            if (randomCoord != mapCenter && MapIsFullyAccessible(mapObstacles, currentObsCount))
            {
                float obsHeight = UnityEngine.Random.Range(minHeight, maxHeight);

                Vector3 newPos = new Vector3(-mapSize.x / 2 + 0.5f + randomCoord.x, obsHeight / 2, -mapSize.y / 2 + 0.5f + randomCoord.y);
                GameObject spawnObs = Instantiate(obstaclePrefab, newPos, Quaternion.Euler(90, 0, 0));
                spawnObs.transform.SetParent(mapHolder);//spawn产生
                                                        // spawnObs.transform.localScale *= (1 - outlinePrecent);
                spawnObs.transform.localScale = new Vector3(1 - outlinePrecent, 1 - outlinePrecent, obsHeight);
                #region
                //折叠
                MeshRenderer meshRenderer = spawnObs.GetComponent<MeshRenderer>();
                Material material = meshRenderer.material;
                float colorPercent = randomCoord.y / mapSize.y;
                material.color = Color.Lerp(foregroundColor, backgroundColor, colorPercent);
                meshRenderer.material = material;
                #endregion
                allOpenCoords.Remove(randomCoord);
            }

            else
            {   //这样这个点就不会被加上啦~因为洗牌算法所以不需要标记这个点永久不可碰
                mapObstacles[randomCoord.x, randomCoord.y] = false;
                currentObsCount--;
                
            }

        }
        shuffledOpenQueue = new Queue<Coordiate>(Utilities.ShuffleCoords(allOpenCoords.ToArray()));
    }
    //洪泛DFS
    private bool MapIsFullyAccessible(bool[,] _mapObstacles, int _currentObsCount)
    {
        //这是嘛 ：标记点是否已经被检测过,和mapObstacles区别是，mO是用来标记这个点是否有障碍物的
        bool[,] mapFlags = new bool[_mapObstacles.GetLength(0),_mapObstacles.GetLength(1)];
        //所有可以行走的瓦片都放到这个队列通过先进先出挨个遍历
        //我的理解是先把所有设成可以行走然后去掉被选成障碍那些，看最新设置的这个障碍会不会使可以洪范到的点的数量没有应该的那么多
        Queue<Coordiate> queue = new Queue<Coordiate>();
        queue.Enqueue(mapCenter);
        mapFlags[mapCenter.x, mapCenter.y] = true;

        int accessibleCount = 1;//可以行走的瓦片数量，初始值为1 
        while (queue.Count > 0)
        {
            Coordiate currentTile = queue.Dequeue();
            //通过for获得当前瓦片上下左右相邻的四个瓦片
            for (int i = -1; i <=1; i++)
            {
                for (int j = -1; j <=1; j++)
                {
                    int neighborX = currentTile.x + i;
                    int neighborY = currentTile.y + j;
                    //除了我们这个是四边界要排除掉一些以外，还要做边界检测
                    if (i == 0 || j == 0)
                    {
                        if (neighborX >= 0 && neighborX < _mapObstacles.GetLength(0) && neighborY >= 0 && neighborY < _mapObstacles.GetLength(1))
                        {
                            //还要防止重复检测//后半句不是一定得么，洗牌算法保证了不会洗到同一张点来做障碍物啊
                            //oo傻了，这不是洗到的点是人家的相邻点
                            if (!mapFlags[neighborX, neighborY] && !_mapObstacles[neighborX, neighborY])
                            {
                                mapFlags[neighborX, neighborY] = true;
                                accessibleCount++;
                                queue.Enqueue(new Coordiate(neighborX, neighborY));
                            }
                        }
                    }

                }
            }
        }
        int jiasheaccessibleCnt = (int)(mapSize.x * mapSize.y - _currentObsCount);
        return accessibleCount == jiasheaccessibleCnt;
    }

    private Coordiate GetRandomCoord()
    {
        Coordiate randomCoord = shuffledQueue.Dequeue();
        shuffledQueue.Enqueue(randomCoord);//把它再排到最后
        return randomCoord;
    }
    public Transform GetRandomOpenTile()
    {
        Coordiate randomCoord = shuffledOpenQueue.Dequeue();
        shuffledQueue.Enqueue(randomCoord);//把它再排到最后

        return tileMap[randomCoord.x, randomCoord.y];
    }
}

//将所有瓦片位置用结构体写成整数坐标x，y的形式
//序列化好在窗口中显示
//人家没写在类MapG里，所以在别的代码里可以直接调用
[System.Serializable]
public struct Coordiate//坐标的意思
{
    public int x;
    public int y;
    public Coordiate(int _x, int _y)
    {
        this.x = _x;                                                                                                                                                                 
        this.y = _y;
    }
    //重载运算符
    public static bool operator !=(Coordiate _c1, Coordiate _c2)
    {
        return !(_c1 == _c2);
    }
    public static bool operator ==(Coordiate _c1, Coordiate _c2)
    {
        return (_c1.x == _c2.x) && (_c1.y == _c2.y);
    }
}
[System.Serializable]
public class Map
{
    public Vector2 _mapSize;//保护mapsize
    [Range(0, 1)] public float _obsPercent;
    //public int _seed;//伪随机数种子
    public float _minObsHeight, _maxObsHeight;
    public Color _foregroundColor, _backgroundColor;

    public Coordiate mapCenter
    {//有的类的数据是私有的，是封装起来的，所以为了读取和写入对应的私有数据，c#采用了关键字get和set，
        //其中get负责读取私有数据，set负责写入私有数据

        get
        {
            return new Coordiate((int)_mapSize.x / 2, (int)_mapSize.y / 2);
        }
    }


}




